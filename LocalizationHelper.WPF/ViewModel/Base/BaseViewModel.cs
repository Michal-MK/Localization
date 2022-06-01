using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LocalizationHelper.WPF.ViewModel.Base;

public class BaseViewModel : INotifyPropertyChanged {
	public event PropertyChangedEventHandler PropertyChanged;

	public void Notify([CallerMemberName] string name = null) {
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}
