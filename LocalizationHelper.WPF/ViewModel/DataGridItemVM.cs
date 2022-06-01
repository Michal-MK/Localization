using LocalizationHelper.WPF.ViewModel.Base;

namespace LocalizationHelper.WPF.ViewModel;
public class DataGridItemVM : BaseViewModel {
	public string PropertyName { get; set; }

	public string ValueCZ { get; set; }
	public string ValueEN { get; set; }
}
