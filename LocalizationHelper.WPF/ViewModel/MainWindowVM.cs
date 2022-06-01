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
using System.Windows.Input;

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
	
	[Notify]
	public ICommand ActionCommand { get; set; }

	public ObservableCollection<Localizable> Localizables { get; } = new();
	[Notify]
	public ObservableCollection<InnerClass> Subclasses { get; set; } = new();
	[Notify]
	public ObservableCollection<DataGridItemVM> LocalizedTexts { get; set; } = new();
	
	private const string CONFIG_PATH = "private/locales.txt";

	private InputType currentInputType;
	private readonly Main localizationCore;

	public MainWindowVM() {
		SearchClicked = new Command(SearchAction);
		DefineClicked = new Command(DefineAction);
		ActionCommand = new Command(DoAction);
		LocalizableClickCommand = new Command(LocalizableClickAction);
		SubclassClickCommand = new Command(SubclassClickAction);
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
		Localizable it = value as Localizable;
		OutputText = localizationCore.Handle("sl " + it.Shortcut);
		Subclasses = it.ClassFile.GetInnerClasses().ToObservable();
	}
	
	public void SubclassClickAction(object value) {
		InnerClass it = value as InnerClass;
		OutputText = localizationCore.Handle("sc " + it.Name);
		LocalizedTexts = it.Internals.OfType<IDLineDefinition>().Select(s => new DataGridItemVM() { PropertyName = s.ID.ToString(), ValueCZ = "CZ", ValueEN = "EN" }).ToObservable();
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

	private string BuildCommand() {
		throw new NotImplementedException();
	}

	#endregion
}
