using LocalizationHelper.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;

namespace LocalizationHelper.WPF {
	public class WindowViewModel : INotifyPropertyChanged {
		private string outputText;
		private string inputText;
		private ICommand searchClicked;
		private ICommand defineClicked;
		private ICommand actionCommand;

		public string OutputText { get => outputText; set { outputText = value; Notify(nameof(OutputText)); } }
		public string InputText { get => inputText; set { inputText = value; Notify(nameof(InputText)); } }

		public ICommand SearchClicked { get => searchClicked; set { searchClicked = value; Notify(nameof(SearchClicked)); } }
		public ICommand DefineClicked { get => defineClicked; set { defineClicked = value; Notify(nameof(DefineClicked)); } }
		public ICommand ActionCommand { get => actionCommand; set { actionCommand = value; Notify(nameof(ActionCommand)); } }

		public List<Localizable> Localizables { get; }

		public event PropertyChangedEventHandler PropertyChanged;

		private const string CONFIG_PATH = "private/locales.txt";

		private InputType currentInputType;
		private readonly Main localizationCore;
		private readonly MemoryStream ms = new();

		private void Notify(string name) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		public WindowViewModel() {
			SearchClicked = new Command(SearchAction);
			DefineClicked = new Command(DefineAction);
			ActionCommand = new Command(DoAction);
			Localizables = GetLocalizables(CONFIG_PATH);
			localizationCore = new Main(Localizables);
		}

		private static List<Localizable> GetLocalizables(string cfgPath) {
			List<Localizable> ret = new();
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

		public void OnLocalizableButtonClicked(int index) {
			OutputText = localizationCore.Handle("sl " + Localizables[index].Shortcut);
		}

		public void OnSubClassButtonClicked(string subclassName) {
			OutputText = localizationCore.Handle("sc " + subclassName);
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
}
