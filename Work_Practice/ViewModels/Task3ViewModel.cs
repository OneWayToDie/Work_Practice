//========================================================= Библиотеки ================================================================//
using System;                          // Базовые типы (double, int, Random, Math)
using System.Collections.Generic;       // Коллекции (List, LinkedList, IEnumerable)
using System.Collections.ObjectModel;   // ObservableCollection<T>
using System.ComponentModel;            // INotifyPropertyChanged
using System.Linq;                      // LINQ (Enumerable.Range, Select, ToArray)
using System.Runtime.CompilerServices; // CallerMemberName
using System.Windows.Input;             // ICommand
using Work_Practice.Commands;           // DelegateCommand
using Work_Practice.Models;             // TypeOfNumber, SinglyLinkedList
using Work_Practice.Services;           // Сервисы (ProductDataService и др.)
using Work_Practice.Views;              // AppDialog

namespace Work_Practice.ViewModels
{
	//========================================================= ViewModel для задания 3 (односвязный список) ================================================================//
	public class Task3ViewModel : INotifyPropertyChanged
	{
		//========================================================= Поля ================================================================//
		private static readonly Random rand = new Random();           // Генератор случайных чисел для создания списков

		//--- Режимы отображения ---
		private bool isCustomViewSelected = true;                     // Выбран ли режим "Собственная реализация"

		//--- Тип генерируемых чисел ---
		private TypeOfNumber selectedNumberType = TypeOfNumber.Integer; // Тип чисел (Integer или Double)

		//--- Собственная реализация списка ---
		private SinglyLinkedList<double> customList = new SinglyLinkedList<double>();        // Собственный односвязный список
		private ObservableCollection<double> customListItems = new ObservableCollection<double>(); // Исходные элементы (до переноса)
		private ObservableCollection<double> customListResultItems = new ObservableCollection<double>(); // Элементы после переноса

		//--- Реализация через LinkedList<T> ---
		private LinkedList<double> builtInList = new LinkedList<double>();                  // Встроенный LinkedList
		private ObservableCollection<double> builtInListItems = new ObservableCollection<double>(); // Исходные элементы (до переноса)
		private ObservableCollection<double> builtInListResultItems = new ObservableCollection<double>(); // Элементы после переноса

		//--- Общие свойства UI ---
		private string numbersInput = "";                               // Строка для ручного ввода чисел
		private string nValue = "";                                    // Количество чисел для генерации

		//========================================================= Публичные свойства ================================================================//
		public bool IsCustomViewSelected
		{
			get => isCustomViewSelected;
			set { isCustomViewSelected = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsBuiltInViewSelected)); }
		}
		public bool IsBuiltInViewSelected
		{
			get => !isCustomViewSelected;
			set { isCustomViewSelected = !value; OnPropertyChanged(); OnPropertyChanged(nameof(IsCustomViewSelected)); }
		}

		public TypeOfNumber SelectedNumberType
		{
			get => selectedNumberType;
			set { selectedNumberType = value; OnPropertyChanged(); }
		}

		public ObservableCollection<double> CustomListItems
		{
			get => customListItems;
			set { customListItems = value; OnPropertyChanged(); }
		}
		public ObservableCollection<double> CustomListResultItems
		{
			get => customListResultItems;
			set { customListResultItems = value; OnPropertyChanged(); }
		}

		public ObservableCollection<double> BuiltInListItems
		{
			get => builtInListItems;
			set { builtInListItems = value; OnPropertyChanged(); }
		}
		public ObservableCollection<double> BuiltInListResultItems
		{
			get => builtInListResultItems;
			set { builtInListResultItems = value; OnPropertyChanged(); }
		}

		public string NumbersInput
		{
			get => numbersInput;
			set { numbersInput = value; OnPropertyChanged(); }
		}
		public string NValue
		{
			get => nValue;
			set { nValue = value; OnPropertyChanged(); }
		}

		//========================================================= Команды ================================================================//
		public ICommand CreateRandomListCommand { get; }   // Команда генерации случайного списка
		public ICommand LoadFromStringCommand { get; }      // Команда загрузки из строки
		public ICommand MoveThirdToFrontCommand { get; }    // Команда переноса третьего элемента

		//========================================================= Конструктор ================================================================//
		public Task3ViewModel()
		{
			CreateRandomListCommand = new DelegateCommand(CreateRandomList); // Привязка команд
			LoadFromStringCommand = new DelegateCommand(LoadFromString);
			MoveThirdToFrontCommand = new DelegateCommand(MoveThirdToFront);
		}

		//========================================================= Генерация случайного списка ================================================================//
		private void CreateRandomList()
		{
			if (!int.TryParse(NValue, out int n) || n <= 0)               // Проверка ввода N
			{
				AppDialog.ShowWarning("Введите положительное целое число N.");
				return;
			}
			if (n > 500)                                                   // Ограничение на 500 элементов
			{
				AppDialog.ShowWarning("Количество чисел не должно превышать 500.");
				return;
			}
			double[] randomNumbers;

			if (SelectedNumberType == TypeOfNumber.Integer)               // Если целые числа
			{
				randomNumbers = Enumerable.Range(0, n).Select(_ => (double)rand.Next(1, 101)).ToArray(); // Генерация от 1 до 100
			}
			else                                                           // Если действительные
			{
				randomNumbers = Enumerable.Range(0, n).Select(_ => Math.Round(rand.NextDouble() * 100, 2)).ToArray(); // 0-100 с двумя знаками
			}

			string typeName = SelectedNumberType == TypeOfNumber.Integer ? "целых" : "действительных";

			if (IsCustomViewSelected)                                      // Если выбран режим "Собственная реализация"
			{
				customList.Clear();                                        // Очистка собственного списка
				CustomListItems.Clear();                                   // Очистка отображаемой коллекции (исходной)
				CustomListResultItems.Clear();                             // Очистка коллекции результата
				foreach (double num in randomNumbers)                     // Заполнение списка
				{
					customList.Add(num);                                   // Добавление в собственный список
					CustomListItems.Add(num);                              // Добавление в отображаемую коллекцию
				}
				AppDialog.ShowInfo($"Создан случайный список (собственная реализация) из {n} {typeName} чисел.");
			}
			else                                                           // Если выбран режим LinkedList<T>
			{
				builtInList.Clear();                                       // Очистка встроенного списка
				BuiltInListItems.Clear();                                  // Очистка отображаемой коллекции (исходной)
				BuiltInListResultItems.Clear();                            // Очистка коллекции результата
				foreach (double num in randomNumbers)                     // Заполнение списка
				{
					builtInList.AddLast(num);                              // Добавление в LinkedList
					BuiltInListItems.Add(num);                             // Добавление в отображаемую коллекцию
				}
				AppDialog.ShowInfo($"Создан случайный список (LinkedList<T>) из {n} {typeName} чисел.");
			}
		}

		//========================================================= Загрузка списка из текстового ввода ================================================================//
		private void LoadFromString()
		{
			if (string.IsNullOrWhiteSpace(NumbersInput))                  // Проверка пустого ввода
			{
				AppDialog.ShowWarning("Введите числа через запятую или пробел.");
				return;
			}
			string[] parts = NumbersInput.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries); // Разделение на части
			List<double> parsedNumbers = new List<double>();
			foreach (string part in parts)                                // Парсинг каждого числа
			{
				if (double.TryParse(part, out double val))
					parsedNumbers.Add(val);
				else
				{
					AppDialog.ShowError($"Некорректное число: {part}"); // Ошибка при неверном формате
					return;
				}
			}
			if (parsedNumbers.Count == 0) return;                        // Если нет чисел – выход
			if (parsedNumbers.Count > 500)                                // Проверка лимита
			{
				AppDialog.ShowWarning("Количество чисел не должно превышать 500.");
				return;
			}

			if (IsCustomViewSelected)                                     // Загрузка в собственную реализацию
			{
				customList.Clear();
				CustomListItems.Clear();
				CustomListResultItems.Clear();
				foreach (double num in parsedNumbers)
				{
					customList.Add(num);
					CustomListItems.Add(num);
				}
				AppDialog.ShowInfo($"Загружено {parsedNumbers.Count} чисел (собственная реализация).");
			}
			else                                                           // Загрузка в LinkedList
			{
				builtInList.Clear();
				BuiltInListItems.Clear();
				BuiltInListResultItems.Clear();
				foreach (double num in parsedNumbers)
				{
					builtInList.AddLast(num);
					BuiltInListItems.Add(num);
				}
				AppDialog.ShowInfo($"Загружено {parsedNumbers.Count} чисел (LinkedList<T>).");
			}
		}

		//========================================================= Перенос третьего элемента в начало ================================================================//
		private void MoveThirdToFront()
		{
			if (IsCustomViewSelected)                                     // Для собственной реализации
			{
				if (customList.Head?.Next?.Next != null)                 // Если есть как минимум 3 элемента
				{
					// Синхронизируем "До" с текущим состоянием списка
					CustomListItems.Clear();
					foreach (double item in customList.ToList())
						CustomListItems.Add(item);

					// Копируем "До" в "После" (для отображения начального состояния)
					CustomListResultItems.Clear();
					foreach (double item in CustomListItems)
						CustomListResultItems.Add(item);

					bool success = customList.MoveThirdToFront();        // Вызов метода переноса
					if (success)
					{
						// Обновляем "После" результатом переноса
						CustomListResultItems.Clear();
						foreach (double item in customList.ToList())
							CustomListResultItems.Add(item);
						AppDialog.ShowInfo("Собственная реализация: третий элемент перенесён в начало.");
					}
					else
					{
						CustomListResultItems.Clear();                   // Очищаем, если не удалось
						AppDialog.ShowWarning("Собственная реализация: в списке меньше трёх элементов. Операция невозможна.");
					}
				}
				else
					AppDialog.ShowWarning("Собственная реализация: в списке меньше трёх элементов. Операция невозможна.");
			}
			else                                                           // Для встроенного LinkedList
			{
				if (builtInList.Count >= 3)                               // Если элементов >= 3
				{
					// Синхронизируем "До" с текущим состоянием списка
					BuiltInListItems.Clear();
					foreach (double item in builtInList)
						BuiltInListItems.Add(item);

					// Копируем "До" в "После"
					BuiltInListResultItems.Clear();
					foreach (double item in BuiltInListItems)
						BuiltInListResultItems.Add(item);

					LinkedListNode<double> thirdNode = builtInList.First?.Next?.Next; // Третий узел
					if (thirdNode != null)
					{
						double value = thirdNode.Value;                  // Сохраняем значение
						builtInList.Remove(thirdNode);                  // Удаляем узел
						builtInList.AddFirst(value);                     // Вставляем в начало
																		 // Обновляем "После" результатом переноса
						BuiltInListResultItems.Clear();
						foreach (double item in builtInList)
							BuiltInListResultItems.Add(item);
						AppDialog.ShowInfo("LinkedList<T>: третий элемент перенесён в начало.");
					}
					else
						AppDialog.ShowError("LinkedList<T>: не удалось найти третий элемент.");
				}
				else
					AppDialog.ShowWarning($"LinkedList<T>: в списке {builtInList.Count} элементов. Операция требует минимум 3 элемента.");
			}
		}

		//========================================================= Реализация INotifyPropertyChanged ================================================================//
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}