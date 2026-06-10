using System;
using System.Windows.Input;
using Work_Practice.Views;

namespace Work_Practice.ViewModels
{
	public class MainViewModel
	{
		public ICommand OpenTask1Command { get; }
		public ICommand OpenTask2Command { get; }
		public ICommand OpenTask3Command { get; }

		public MainViewModel()
		{
			OpenTask1Command = new DelegateCommand(OpenTask1);
			OpenTask2Command = new DelegateCommand(OpenTask2);
			OpenTask3Command = new DelegateCommand(OpenTask3);
		}

		private void OpenTask1() => new Task1Window().ShowDialog();
		private void OpenTask2() => new Task2Window().ShowDialog();
		private void OpenTask3() => new Task3Window().ShowDialog();
	}

	public class DelegateCommand : ICommand
	{
		private readonly Action _execute;
		private readonly Func<bool> _canExecute;

		public DelegateCommand(Action execute, Func<bool> canExecute = null)
		{
			_execute = execute;
			_canExecute = canExecute;
		}

		public bool CanExecute(object parameter) => _canExecute == null || _canExecute();
		public void Execute(object parameter) => _execute();
		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}
	}
}