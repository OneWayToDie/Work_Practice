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

namespace Work_Practice.ViewModels
{
	public class Task3ViewModel : INotifyPropertyChanged
	{
		private static readonly Random _rand = new Random();
		// --- Режимы отображения ---
		private bool _isCustomViewSelected = true;
		public bool IsCustomViewSelected
		{
			get => _isCustomViewSelected;
			set { _isCustomViewSelected = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsBuiltInViewSelected)); }
		}
		public bool IsBuiltInViewSelected
		{
			get => !_isCustomViewSelected;
			set { _isCustomViewSelected = !value; OnPropertyChanged(); OnPropertyChanged(nameof(IsCustomViewSelected)); }
		}

		// --- Тип генерируемых чисел (теперь просто TypeOfNumber) ---
		private TypeOfNumber _selectedNumberType = TypeOfNumber.Integer;
		public TypeOfNumber SelectedNumberType
		{
			get => _selectedNumberType;
			set { _selectedNumberType = value; OnPropertyChanged(); }
		}

		// --- Собственная реализация списка ---
		private SinglyLinkedList<double> _customList = new SinglyLinkedList<double>();
		private ObservableCollection<double> _customListItems = new ObservableCollection<double>();
		public ObservableCollection<double> CustomListItems
		{
			get => _customListItems;
			set { _customListItems = value; OnPropertyChanged(); }
		}
		private ObservableCollection<double> _customListResultItems = new ObservableCollection<double>();
		public ObservableCollection<double> CustomListResultItems
		{
			get => _customListResultItems;
			set { _customListResultItems = value; OnPropertyChanged(); }
		}

		// --- Реализация через LinkedList<T> ---
		private LinkedList<double> _builtInList = new LinkedList<double>();
		private ObservableCollection<double> _builtInListItems = new ObservableCollection<double>();
		public ObservableCollection<double> BuiltInListItems
		{
			get => _builtInListItems;
			set { _builtInListItems = value; OnPropertyChanged(); }
		}
		private ObservableCollection<double> _builtInListResultItems = new ObservableCollection<double>();
		public ObservableCollection<double> BuiltInListResultItems
		{
			get => _builtInListResultItems;
			set { _builtInListResultItems = value; OnPropertyChanged(); }
		}

		// --- Общие свойства UI ---
		private string _numbersInput = "";
		public string NumbersInput
		{
			get => _numbersInput;
			set { _numbersInput = value; OnPropertyChanged(); }
		}

		private string _nValue = "";
		public string NValue
		{
			get => _nValue;
			set { _nValue = value; OnPropertyChanged(); }
		}

		// --- Команды ---
		public ICommand CreateRandomListCommand { get; }
		public ICommand LoadFromStringCommand { get; }
		public ICommand MoveThirdToFrontCommand { get; }

		public Task3ViewModel()
		{
			CreateRandomListCommand = new DelegateCommand(CreateRandomList);
			LoadFromStringCommand = new DelegateCommand(LoadFromString);
			MoveThirdToFrontCommand = new DelegateCommand(MoveThirdToFront);
		}

		private void CreateRandomList()
		{
			if (!int.TryParse(NValue, out int n) || n <= 0)
			{
				MessageBox.Show("Введите положительное целое число N.");
				return;
			}
			if (n > 500)
			{
				MessageBox.Show("Количество чисел не должно превышать 500.");
				return;
			}
			var rand = _rand;
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
				_customList.Clear();
				CustomListItems.Clear();
				CustomListResultItems.Clear();
				foreach (var num in randomNumbers)
				{
					_customList.Add(num);
					CustomListItems.Add(num);
				}
				MessageBox.Show($"Создан случайный список (собственная реализация) из {n} {typeName} чисел.");
			}
			else
			{
				_builtInList.Clear();
				BuiltInListItems.Clear();
				BuiltInListResultItems.Clear();
				foreach (var num in randomNumbers)
				{
					_builtInList.AddLast(num);
					BuiltInListItems.Add(num);
				}
				MessageBox.Show($"Создан случайный список (LinkedList<T>) из {n} {typeName} чисел.");
			}
		}

		private void LoadFromString()
		{
			if (string.IsNullOrWhiteSpace(NumbersInput))
			{
				MessageBox.Show("Введите числа через запятую или пробел.");
				return;
			}
			string[] parts = NumbersInput.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
			var parsedNumbers = new List<double>();
			foreach (var part in parts)
			{
				if (double.TryParse(part, out double val))
					parsedNumbers.Add(val);
				else
				{
					MessageBox.Show($"Некорректное число: {part}");
					return;
				}
			}
			if (parsedNumbers.Count == 0) return;
			if (parsedNumbers.Count > 500)
			{
				MessageBox.Show("Количество чисел не должно превышать 500.");
				return;
			}

			if (IsCustomViewSelected)
			{
				_customList.Clear();
				CustomListItems.Clear();
				CustomListResultItems.Clear();
				foreach (var num in parsedNumbers)
				{
					_customList.Add(num);
					CustomListItems.Add(num);
				}
				MessageBox.Show($"Загружено {parsedNumbers.Count} чисел (собственная реализация).");
			}
			else
			{
				_builtInList.Clear();
				BuiltInListItems.Clear();
				BuiltInListResultItems.Clear();
				foreach (var num in parsedNumbers)
				{
					_builtInList.AddLast(num);
					BuiltInListItems.Add(num);
				}
				MessageBox.Show($"Загружено {parsedNumbers.Count} чисел (LinkedList<T>).");
			}
		}

		private void MoveThirdToFront()
		{
			if (IsCustomViewSelected)
			{
				if (_customList.Head?.Next?.Next != null)
				{
					CustomListResultItems.Clear();
					foreach (var item in CustomListItems)
						CustomListResultItems.Add(item);

					bool success = _customList.MoveThirdToFront();
					if (success)
					{
						CustomListItems.Clear();
						foreach (var item in _customList.ToList())
							CustomListItems.Add(item);
						MessageBox.Show("Собственная реализация: третий элемент перенесён в начало.");
					}
					else
					{
						CustomListResultItems.Clear();
						MessageBox.Show("Собственная реализация: в списке меньше трёх элементов. Операция невозможна.");
					}
				}
				else
					MessageBox.Show("Собственная реализация: в списке меньше трёх элементов. Операция невозможна.");
			}
			else
			{
				if (_builtInList.Count >= 3)
				{
					var thirdNode = _builtInList.First?.Next?.Next;
					if (thirdNode != null)
					{
						BuiltInListResultItems.Clear();
						foreach (var item in BuiltInListItems)
							BuiltInListResultItems.Add(item);

						var value = thirdNode.Value;
						_builtInList.Remove(thirdNode);
						_builtInList.AddFirst(value);
						BuiltInListItems.Clear();
						foreach (var item in _builtInList)
							BuiltInListItems.Add(item);
						MessageBox.Show("LinkedList<T>: третий элемент перенесён в начало.");
					}
					else
						MessageBox.Show("LinkedList<T>: не удалось найти третий элемент.");
				}
				else
					MessageBox.Show($"LinkedList<T>: в списке {_builtInList.Count} элементов. Операция требует минимум 3 элемента.");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}