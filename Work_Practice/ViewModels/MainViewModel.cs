using System.Windows.Input;
using Work_Practice.Views;
using Work_Practice.Commands;

namespace Work_Practice.ViewModels
{
	// ViewModel главного окна — навигация по заданиям
	public class MainViewModel
	{
		public ICommand OpenTask1Command { get; }
		public ICommand OpenTask2Command { get; }
		public ICommand OpenTask3Command { get; }

		// Конструктор — привязка команд открытия окон
		public MainViewModel()
		{
			OpenTask1Command = new DelegateCommand(OpenTask1);
			OpenTask2Command = new DelegateCommand(OpenTask2);
			OpenTask3Command = new DelegateCommand(OpenTask3);
		}

		// Открытие окна задания 1
		private void OpenTask1() => new Task1Window().ShowDialog();
		// Открытие окна задания 2
		private void OpenTask2() => new Task2Window().ShowDialog();
		// Открытие окна задания 3
		private void OpenTask3() => new Task3Window().ShowDialog();
	}
}