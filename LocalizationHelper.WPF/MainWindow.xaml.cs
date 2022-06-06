using System;
using System.Collections.Generic;
using LocalizationHelper.WPF.ViewModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using LocalizationHelper.Core.IElements.Text;

namespace LocalizationHelper.WPF;

public partial class MainWindow : Window {

	public MainWindowVM Context => DataContext as MainWindowVM;

	public MainWindow() {
		InitializeComponent();
	}

	private readonly Dictionary<string, string> fileToFullPath = new();

	private void Root_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
		Context.PropertyChanged += Context_PropertyChanged;
	}

	private void Context_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
		if (e.PropertyName == nameof(Context.LocalizedTexts)) {
			fileToFullPath.Clear();
			DataGridControl.Columns.Clear();
			DataGridControl.Columns.Add(new DataGridTextColumn() { Header = "Property Name", Binding = new Binding(nameof(DataGridItemVM.PropertyName) + "." + nameof(DataGridItemVM.PropertyName.Name)) });
			if (Context.LocalizedTexts.Count <= 0) {
				DataGridControl.Columns.Add(new DataGridTextColumn() { Header = "Empty!" });
				return;
			}
			foreach (var item in Context.LocalizedTexts[0].Localizations.Keys) {
				string fileName = Path.GetFileName(item);
				fileToFullPath.Add(fileName, item);
				DataGridControl.Columns.Add(new DataGridTextColumn() { Header = fileName, Binding = new Binding(nameof(DataGridItemVM.Localizations) + "[" + item + "]." + nameof(IDLineLocalization.Value)) });
			}
		}
	}

	private void DataGridControl_OnAddingNewItem(object? sender, AddingNewItemEventArgs e) {
		Console.WriteLine(e.NewItem);
	}

	private void DataGridControl_OnCellEditEnding(object? sender, DataGridCellEditEndingEventArgs e) {
		DataGridItemVM data = ((DataGridItemVM)e.Row.Item);
		Console.WriteLine(data.PropertyName);
		string content = (e.EditingElement as TextBox).Text;
		Console.WriteLine(content);
		string header = e.Column.Header.ToString();
		bool propCol = DataGridControl.Columns[0] == e.Column;
		if (propCol) {
			data.PropertyName.Name = content;
		}
		else {
			data.Localizations[fileToFullPath[header]].Value = content;
		}
	}

	private void DataGridControl_OnLoadingRow(object sender, DataGridRowEventArgs e) {
		e.Row.ContextMenu = new ContextMenu {
			Items = {
				new MenuItem() { Header = "Add Above", Command = Context.AddAboveCommand, CommandParameter = e.Row.Item },
				new MenuItem() { Header = "Add Below", Command = Context.AddBelowCommand, CommandParameter = e.Row.Item },
			}
		};
	}
}