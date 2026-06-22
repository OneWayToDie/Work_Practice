using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Work_Practice.Commands;
using Work_Practice.Models;
using Work_Practice.Services;
using Work_Practice.Views;

namespace Work_Practice.ViewModels
{
	public class Task3ViewModel : INotifyPropertyChanged
	{
		private static readonly Random rand = new Random();
		// --- Режимы отображения ---
		private bool isCustomViewSelected = true;
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

		// --- Тип генерируемых чисел (теперь просто TypeOfNumber) ---
		private TypeOfNumber selectedNumberType = TypeOfNumber.Integer;
		public TypeOfNumber SelectedNumberType
		{
			get => selectedNumberType;
			set { selectedNumberType = value; OnPropertyChanged(); }
		}

		// --- Собственная реализация списка ---
		private SinglyLinkedList<double> customList = new SinglyLinkedList<double>();
		private ObservableCollection<double> customListItems = new ObservableCollection<double>();
		public ObservableCollection<double> CustomListItems
		{
			get => customListItems;
			set { customListItems = value; OnPropertyChanged(); }
		}
		private ObservableCollection<double> customListResultItems = new ObservableCollection<double>();
		public ObservableCollection<double> CustomListResultItems
		{
			get => customListResultItems;
			set { customListResultItems = value; OnPropertyChanged(); }
		}

		// --- Реализация через LinkedList<T> ---
		private LinkedList<double> builtInList = new LinkedList<double>();
		private ObservableCollection<double> builtInListItems = new ObservableCollection<double>();
		public ObservableCollection<double> BuiltInListItems
		{
			get => builtInListItems;
			set { builtInListItems = value; OnPropertyChanged(); }
		}
		private ObservableCollection<double> builtInListResultItems = new ObservableCollection<double>();
		public ObservableCollection<double> BuiltInListResultItems
		{
			get => builtInListResultItems;
			set { builtInListResultItems = value; OnPropertyChanged(); }
		}

		// --- Общие свойства UI ---
		private string numbersInput = "";
		public string NumbersInput
		{
			get => numbersInput;
			set { numbersInput = value; OnPropertyChanged(); }
		}

		private string nValue = "";
		public string NValue
		{
			get => nValue;
			set { nValue = value; OnPropertyChanged(); }
		}

		// --- Команды ---
		public ICommand CreateRandomListCommand { get; }
		public ICommand LoadFromStringCommand { get; }
		public ICommand MoveThirdToFrontCommand { get; }

		// Конструктор — привязка команд задания 3
		public Task3ViewModel()
		{
			CreateRandomListCommand = new DelegateCommand(CreateRandomList);
			LoadFromStringCommand = new DelegateCommand(LoadFromString);
			MoveThirdToFrontCommand = new DelegateCommand(MoveThirdToFront);
		}

		// Генерация случайного списка чисел
		private void CreateRandomList()
		{
			if (!int.TryParse(NValue, out int n) || n <= 0)
			{
				AppDialog.ShowWarning("Введите положительное целое число N.");
				return;
			}
			if (n > 500)
			{
				AppDialog.ShowWarning("Количество чисел не должно превышать 500.");
				return;
			}
			double[] randomNumbers;

			if (SelectedNumberType == TypeOfNumber.Integer)
			{
				randomNumbers = Enumerable.Range(0, n).Select(_ => (double)rand.Next(1, 101)).ToArray();
			}
			else
			{
				randomNumbers = Enumerable.Range(0, n).Select(_ => Math.Round(rand.NextDouble() * 100, 2)).ToArray();
			}

			string typeName = SelectedNumberType == TypeOfNumber.Integer ? "целых" : "действительных";

			if (IsCustomViewSelected)
			{
				customList.Clear();
				CustomListItems.Clear();
				CustomListResultItems.Clear();
			// Заполнение собственного списка
			foreach (double num in randomNumbers)
			{
				customList.Add(num);
				CustomListItems.Add(num);
			}
				AppDialog.ShowInfo($"Создан случайный список (собственная реализация) из {n} {typeName} чисел.");
			}
			else
			{
				builtInList.Clear();
				BuiltInListItems.Clear();
				BuiltInListResultItems.Clear();
			// Заполнение LinkedList
			foreach (double num in randomNumbers)
			{
				builtInList.AddLast(num);
				BuiltInListItems.Add(num);
			}
				AppDialog.ShowInfo($"Создан случайный список (LinkedList<T>) из {n} {typeName} чисел.");
			}
		}

		// Загрузка списка из текстового ввода
		private void LoadFromString()
		{
			if (string.IsNullOrWhiteSpace(NumbersInput))
			{
				AppDialog.ShowWarning("Введите числа через запятую или пробел.");
				return;
			}
			string[] parts = NumbersInput.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
			List<double> parsedNumbers = new List<double>();
			foreach (string part in parts)
			{
				if (double.TryParse(part, out double val))
					parsedNumbers.Add(val);
				else
				{
					AppDialog.ShowError($"Некорректное число: {part}");
					return;
				}
			}
			if (parsedNumbers.Count == 0) return;
			if (parsedNumbers.Count > 500)
			{
				AppDialog.ShowWarning("Количество чисел не должно превышать 500.");
				return;
			}

			if (IsCustomViewSelected)
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
			else
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

		// Перенос третьего элемента в начало
		private void MoveThirdToFront()
		{
			if (IsCustomViewSelected)
			{
				if (customList.Head?.Next?.Next != null)
				{
					// Синхронизируем "До" с текущим состоянием списка
					CustomListItems.Clear();
				foreach (double item in customList.ToList())
					CustomListItems.Add(item);

				// Копируем "До" в "После"
				CustomListResultItems.Clear();
				foreach (double item in CustomListItems)
					CustomListResultItems.Add(item);

					bool success = customList.MoveThirdToFront();
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
						CustomListResultItems.Clear();
						AppDialog.ShowWarning("Собственная реализация: в списке меньше трёх элементов. Операция невозможна.");
					}
				}
				else
					AppDialog.ShowWarning("Собственная реализация: в списке меньше трёх элементов. Операция невозможна.");
			}
			else
			{
				if (builtInList.Count >= 3)
				{
					// Синхронизируем "До" с текущим состоянием списка
					BuiltInListItems.Clear();
				foreach (double item in builtInList)
					BuiltInListItems.Add(item);

				// Копируем "До" в "После"
				BuiltInListResultItems.Clear();
				foreach (double item in BuiltInListItems)
					BuiltInListResultItems.Add(item);

				LinkedListNode<double> thirdNode = builtInList.First?.Next?.Next;
					if (thirdNode != null)
					{
						double value = thirdNode.Value;
						builtInList.Remove(thirdNode);
						builtInList.AddFirst(value);
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

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}