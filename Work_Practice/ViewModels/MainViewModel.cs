using System.Windows.Input;
using Work_Practice.Views;
using Work_Practice.Commands;

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
}