using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LocalizationHelper.Core;
using LocalizationHelper.Core.IElements;

namespace LocalizationHelper.WPF {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {

		private ToggleButton activeLocalizableButton;
		private ToggleButton activeSubclassButton;

		public MainWindow() {
			InitializeComponent();
			Init();
			InvalidateVisual();
		}

		private void Init() {
			WindowViewModel model = DataContext as WindowViewModel;
			LocalizablesGrid.Children.Clear();

			for (int i = 0; i < model.Localizables.Count; i++) {
				Localizable l = model.Localizables[i];
				ToggleButton b = new() { Content = l.Name };
				int index = i;
				b.Click += (s, e) => {
					if (activeLocalizableButton != null) {
						activeLocalizableButton.IsChecked = false;
						SubClassesGrid.Children.Clear();
					}
					if (activeLocalizableButton == b && b.IsChecked.HasValue && !b.IsChecked.Value) {
						activeLocalizableButton = null;
						return;
					}
					model.OnLocalizableButtonClicked(index);
					PopulateSubclasses(l);
					activeLocalizableButton = b;
				};
				LocalizablesGrid.Children.Add(b);
			}
		}

		private void PopulateSubclasses(Localizable localizable) {
			WindowViewModel model = DataContext as WindowViewModel;
			List<InnerClass> innerClasses = localizable.ClassFile.GetInnerClasses();
			SubClassesGrid.Children.Clear();

			foreach (InnerClass ic in innerClasses) {
				ToggleButton b = new() { Content = ic.Name };
				b.Click += (s, e) => {
					if (activeSubclassButton != null) {
						activeSubclassButton.IsChecked = false;
					}
					if (activeSubclassButton == b && b.IsChecked.HasValue && !b.IsChecked.Value) {
						activeSubclassButton = null;
						return;
					}
					model.OnSubClassButtonClicked(ic.Name);
					activeSubclassButton = b;
				};
				SubClassesGrid.Children.Add(b);
			}
		}
	}
}
