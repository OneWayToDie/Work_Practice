using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Work_Practice.Commands;
using Work_Practice.Services;

namespace Work_Practice.ViewModels
{
	public class Task3ViewModel : INotifyPropertyChanged
	{
		private bool _isCustomViewSelected = true;
		public bool IsCustomViewSelected
		{
			get => _isCustomViewSelected;
			set { _isCustomViewSelected = value; OnPropertyChanged(); }
		}

		public bool IsBuiltInViewSelected
		{
			get => !_isCustomViewSelected;
			set { _isCustomViewSelected = !value; OnPropertyChanged(); }
		}

		private SinglyLinkedList<double> _customList = new SinglyLinkedList<double>();
		private ObservableCollection<double> _customListItems = new ObservableCollection<double>();
		public ObservableCollection<double> CustomListItems
		{
			get => _customListItems;
			set { _customListItems = value; OnPropertyChanged(); }
		}

		private LinkedList<double> _builtInList = new LinkedList<double>();
		private ObservableCollection<double> _builtInListItems = new ObservableCollection<double>();
		public ObservableCollection<double> BuiltInListItems
		{
			get => _builtInListItems;
			set { _builtInListItems = value; OnPropertyChanged(); }
		}

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
			var rand = new Random();
			double[] randomNumbers = Enumerable.Range(0, n).Select(_ => Math.Round(rand.NextDouble() * 100, 2)).ToArray();

			if (IsCustomViewSelected)
			{
				// Для собственной реализации
				_customList.Clear();
				CustomListItems.Clear();
				foreach (var num in randomNumbers)
				{
					_customList.Add(num);
					CustomListItems.Add(num);
				}
				OnPropertyChanged(nameof(CustomListItems));
				MessageBox.Show($"Создан случайный список (собственная реализация) из {n} элементов.");
			}
			else // IsBuiltInViewSelected
			{
				// Для встроенной реализации
				_builtInList.Clear();
				BuiltInListItems.Clear();
				foreach (var num in randomNumbers)
				{
					_builtInList.AddLast(num);
					BuiltInListItems.Add(num);
				}
				OnPropertyChanged(nameof(BuiltInListItems));
				MessageBox.Show($"Создан случайный список (LinkedList<T>) из {n} элементов.");
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
					bool success = _customList.MoveThirdToFront();
					if (success)
					{
						CustomListItems.Clear();
						foreach (var item in _customList.ToList())
						{
							CustomListItems.Add(item);
						}
						MessageBox.Show("Собственная реализация: третий элемент перенесён в начало.");
					}
					else
					{
						MessageBox.Show("Собственная реализация: в списке меньше трёх элементов. Операция невозможна.");
					}
				}
				else
				{
					MessageBox.Show("Собственная реализация: в списке меньше трёх элементов. Операция невозможна.");
				}
			}
			else // IsBuiltInViewSelected
			{
				if (_builtInList.Count >= 3)
				{
					var thirdNode = _builtInList.First?.Next?.Next;
					if (thirdNode != null)
					{
						var value = thirdNode.Value;
						_builtInList.Remove(thirdNode);
						_builtInList.AddFirst(value);

						BuiltInListItems.Clear();
						foreach (var item in _builtInList)
						{
							BuiltInListItems.Add(item);
						}
						MessageBox.Show("LinkedList<T>: третий элемент перенесён в начало.");
					}
					else
					{
						MessageBox.Show("LinkedList<T>: не удалось найти третий элемент.");
					}
				}
				else
				{
					MessageBox.Show($"LinkedList<T>: в списке {_builtInList.Count} элементов. Операция требует минимум 3 элемента.");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
