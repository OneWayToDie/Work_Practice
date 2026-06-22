using System;
using System.Windows.Input;

namespace Work_Practice.Commands
{
	// Реализация ICommand для MVVM привязки команд
	public class DelegateCommand : ICommand
	{
		private readonly Action execute;
		private readonly Func<bool> canExecute;

		// Конструктор — делегаты выполнения и проверки
		public DelegateCommand(Action execute, Func<bool> canExecute = null)
		{
			this.execute = execute;
			this.canExecute = canExecute;
		}

		public bool CanExecute(object parameter) => canExecute == null || canExecute();
		public void Execute(object parameter) => execute();
		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}
	}
}
