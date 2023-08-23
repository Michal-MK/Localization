using ERecept.Core.Annotations;
using LocalizationHelper.Core;
using LocalizationHelper.Core.IElements;
using LocalizationHelper.Core.IElements.Code;
using LocalizationHelper.WPF.Extensions;
using LocalizationHelper.WPF.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Humanizer;
using LocalizationHelper.Core.IElements.Text;
using TextCopy;

namespace LocalizationHelper.WPF.ViewModel;

public class MainWindowVM : BaseViewModel {

	[Notify]
	public string OutputText { get; set; }

	[Notify]
	public string InputText { get; set; }

	public ICommand LocalizableClickCommand { get; set; }
	public ICommand SubclassClickCommand { get; set; }

	public ICommand SearchClicked { get; }
	public ICommand DefineClicked { get; }

	public ICommand AddAboveCommand { get; }
	public ICommand AddBelowCommand { get; }
	public ICommand CreateCodeCommand { get; }

	[Notify]
	public ICommand ActionCommand { get; set; }

	public ICommand SaveCommand { get; set; }

	public ObservableCollection<Localizable> Localizables { get; }

	[Notify]
	public ObservableCollection<InnerClass> Subclasses { get; set; } = new();

	[Notify]
	public ObservableCollection<DataGridItemVM> LocalizedTexts { get; set; } = new();

	[Notify]
	public Localizable ActiveLocalizable { get; set; }

	public InnerClass ActiveInnerClass { get; set; }

	private const string CONFIG_PATH = "private/locales.txt";

	private InputType currentInputType;
	private readonly Main localizationCore;

	public MainWindowVM() {
		SearchClicked = new Command(SearchAction);
		DefineClicked = new Command(DefineAction);
		ActionCommand = new Command(DoAction);
		AddAboveCommand = new Command(AddAboveAction);
		AddBelowCommand = new Command(AddBelowAction);
		CreateCodeCommand = new Command(CreateCodeAction);
		LocalizableClickCommand = new Command(LocalizableClickAction);
		SubclassClickCommand = new Command(SubclassClickAction);
		SaveCommand = new Command(SaveAction);
		Localizables = GetLocalizables(CONFIG_PATH);
		localizationCore = new Main(Localizables.ToList());
	}

	private static ObservableCollection<Localizable> GetLocalizables(string cfgPath) {
		ObservableCollection<Localizable> ret = new();
		List<string> region = new();

		string[] lines = File.ReadAllLines(cfgPath);

		foreach (string line in lines) {
			if (string.IsNullOrEmpty(line)) continue;

			region.Add(line);

			if (!line.StartsWith(";")) continue;

			ret.Add(Localizable.Parse(region));
			region.Clear();
		}
		return ret;
	}

	#region Actions

	public void LocalizableClickAction(object value) {
		ActiveLocalizable = (Localizable)value;
		OutputText = localizationCore.Handle("sl " + ActiveLocalizable.Shortcut);
		Subclasses = ActiveLocalizable.ClassFile.GetInnerClasses().ToObservable();
	}

	public void SubclassClickAction(object value) {
		ActiveInnerClass = (InnerClass)value;
		OutputText = localizationCore.Handle("sc " + ActiveInnerClass.Name);
		LocalizedTexts = ActiveInnerClass.Internals.OfType<IDLineDefinition>()
			.Select(s => new DataGridItemVM() {
				PropertyName = s,
				Localizations = ActiveLocalizable.LangFiles
					.ToDictionary(k => k.Key,
								  v => v.Value.GetAllDefs().SingleOrDefault(ss => ss.ID == s.ID) ??
									   new IDLineLocalization(ActiveLocalizable.GetLangSectionForInnerClass(ActiveInnerClass, v.Key), s.ID ?? 0, ""))
			})
			.ToObservable();
	}

	private void AddAboveAction(object item) {
		DataGridItemVM row = (DataGridItemVM)item;
		int idx = LocalizedTexts.IndexOf(row);
		CreateNewEntry(idx, row.PropertyName, true, "");
	}

	private void AddBelowAction(object item) {
		DataGridItemVM row = (DataGridItemVM)item;
		int idx = LocalizedTexts.IndexOf(row);
		CreateNewEntry(idx + 1, row.PropertyName, false, "");
	}
	
	private void CreateCodeAction(object _) {
		StringBuilder sb = new();
		sb.AppendLine("#region Localization");
		sb.AppendLine();
		sb.AppendLine("[Notify]");
		sb.AppendLine("public Locale L { get; } = new();");
		sb.AppendLine();
		sb.AppendLine("public class Locale {");
		sb.AppendLine("public LocaleProvider L => LocaleProvider.Instance;");

		foreach (IDLineDefinition def in ActiveInnerClass.GetAllDefs()) {
			sb.Append("public string ");

			sb.Append(def.Name.ToLower().Pascalize());

			sb.Append(" => L.Get(");

			sb.Append(def.GetFullyQualifiedName());

			sb.Append(");");
			sb.AppendLine();
		}

		sb.AppendLine("}");
		sb.AppendLine();
		sb.AppendLine("#endregion");
		
		new Clipboard().SetText(sb.ToString());
	}

	private void SearchAction(object _) {
		currentInputType = InputType.Search;
	}

	private void DefineAction(object _) {
		currentInputType = InputType.Definition;
	}

	private void DoAction(object _) {
		string command = BuildCommand();

		localizationCore.Handle(command);
	}

	private void SaveAction(object _) {
		ActiveLocalizable.Save();
	}

	private string BuildCommand() {
		throw new NotImplementedException();
	}

	#endregion

	public void CreateNewEntry(int index, IDLineDefinition it, bool above, string content) {
		int classIndex = GetActualIndex(index, it, ActiveInnerClass);
		IDLineDefinition newDef = new(ActiveLocalizable.ClassFile, content, GetID(), ActiveInnerClass);
		ActiveInnerClass.Internals.Insert(classIndex, newDef);
		DataGridItemVM toInsert = new() {
			PropertyName = newDef,
			Localizations = ActiveLocalizable.LangFiles.Values
				.ToDictionary(k => k.FileName,
							  v => {
								  LangSection section = ActiveLocalizable.GetLangSectionForInnerClass(ActiveInnerClass, v.FileName);
								  if (section is null) {
									  ActiveLocalizable.LangFiles.ForEach(f => {
										  f.Value.Sections.Add(new LangSection(f.Value, "#" + ActiveInnerClass.Name));
									  });
								  }
								  section = ActiveLocalizable.GetLangSectionForInnerClass(ActiveInnerClass, v.FileName);
								  IDLineLocalization loc = new (section, newDef.ID ?? 0, "");
								  section.Definitions.Insert(Math.Min(section.Definitions.Count, index), loc);
								  return loc;
							  })
		};
		LocalizedTexts.Insert(index, toInsert);
	}

	private int GetActualIndex(int index, IDLineDefinition relative, InnerClass innerClass) {
		int skips = 0;
		foreach (ICodeElement innerClassInternal in innerClass.Internals) {
			if (innerClassInternal is IDLineDefinition a && a == relative) {
				return index + skips;
			}
			if (innerClassInternal is InnerClass) {
				skips++;
			}
			if (innerClassInternal is StdLine) {
				skips++;
			}
		}
		return index + skips;
	}

	private static int GetID() {
		return Random.Shared.Next(1_000_000, 99_999_999);
	}
}