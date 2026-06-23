//========================================================= Библиотеки ================================================================//
using System.Windows.Input;   // Интерфейс ICommand и базовые типы команд
using Work_Practice.Views;    // Окна заданий (Task1Window, Task2Window, Task3Window)
using Work_Practice.Commands; // Реализация команд (DelegateCommand)

namespace Work_Practice.ViewModels   // Пространство имён для ViewModel
{
	//========================================================= Класс ViewModel главного окна – навигация по заданиям ================================================================//
	public class MainViewModel
	{
		//========================================================= Свойства команд ================================================================//
		public ICommand OpenTask1Command { get; }    // Команда открытия окна задания 1
		public ICommand OpenTask2Command { get; }    // Команда открытия окна задания 2
		public ICommand OpenTask3Command { get; }    // Команда открытия окна задания 3

		//========================================================= Конструктор ================================================================//
		public MainViewModel()
		{
			// Инициализация команд с привязкой методов открытия окон
			OpenTask1Command = new DelegateCommand(OpenTask1); // Привязка к OpenTask1
			OpenTask2Command = new DelegateCommand(OpenTask2); // Привязка к OpenTask2
			OpenTask3Command = new DelegateCommand(OpenTask3); // Привязка к OpenTask3
		}

		//========================================================= Методы открытия окон заданий ================================================================//
		private void OpenTask1() => new Task1Window().ShowDialog(); // Открытие окна задания 1 (модально)
		private void OpenTask2() => new Task2Window().ShowDialog(); // Открытие окна задания 2 (модально)
		private void OpenTask3() => new Task3Window().ShowDialog(); // Открытие окна задания 3 (модально)
	}
}