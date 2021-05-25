using System;
using System.Windows.Input;

namespace LocalizationHelper.WPF {
	public class Command : ICommand {
		public event EventHandler CanExecuteChanged;

		private readonly Action<object> toExecute;

		public Command(Action<object> toExec) {
			toExecute = toExec;
		}

		public bool CanExecute(object parameter) {
			return true;
		}

		public void Execute(object parameter) {
			toExecute(parameter);
		}
	}
}
