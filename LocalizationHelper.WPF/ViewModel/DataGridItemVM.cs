using LocalizationHelper.WPF.ViewModel.Base;
using System.Collections.Generic;
using LocalizationHelper.Core.IElements.Code;
using LocalizationHelper.Core.IElements.Text;

namespace LocalizationHelper.WPF.ViewModel;
public class DataGridItemVM : BaseViewModel {
	public IDLineDefinition PropertyName { get; set; }

	public Dictionary<string, IDLineLocalization> Localizations { get; set; } = new();
}