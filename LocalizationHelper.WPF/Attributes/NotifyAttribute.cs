using AspectInjector.Broker;
using LocalizationHelper.WPF.Extensions;
using LocalizationHelper.WPF.ViewModel.Base;
using System;
using System.Linq;
using System.Reflection;
using LocalizationHelper.Core;

namespace ERecept.Core.Annotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
[Injection(typeof(NotifyAspect))]
public class NotifyAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
[Injection(typeof(NotifyAspect))]
public class NotifyWithCallbackAttribute : Attribute {
	public NotifyWithCallbackAttribute(string updateMethodName) => UpdateCall = updateMethodName;
	public string UpdateCall { get; }
}

[AttributeUsage(AttributeTargets.Property)]
[Injection(typeof(NotifyAspect))]
public class NotifyAlsoAttribute : Attribute {
	public NotifyAlsoAttribute(params string[] notifyAlso) => NotifyAlso = notifyAlso;
	public string[] NotifyAlso { get; }
}

[Aspect(Scope.Global)]
public class NotifyAspect {
	private const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

	[Advice(Kind.After, Targets = Target.Public | Target.Setter)]
	public void AfterSetter(
		[Argument(Source.Instance)] object source,
		[Argument(Source.Name)] string propName,
		[Argument(Source.Triggers)] Attribute[] triggers
	) {

		MethodInfo? notifyMethod = source.GetType().GetMethod(nameof(BaseViewModel.Notify), BINDING_FLAGS);

		if (triggers.OfType<NotifyAttribute>().Any() || triggers.OfType<NotifyAlsoAttribute>().Any()) {
			if (notifyMethod != null) {
				notifyMethod.Invoke(source, new object[] { propName });
			}
		}

		triggers.OfType<NotifyWithCallbackAttribute>()
				.ForEach(a => {
					if (notifyMethod != null) {
						notifyMethod.Invoke(source, new object[] { propName });
					}
					source.GetType().GetMethod(a.UpdateCall, BINDING_FLAGS)
						  ?.Invoke(source, new object[] { });
				});

		triggers.OfType<NotifyAlsoAttribute>()
				.ForEach(a => {
					foreach (string additional in a.NotifyAlso ?? new string[] { }) {
						notifyMethod?.Invoke(source, new object[] { additional });
					}
				});
	}
}