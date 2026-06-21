using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Work_Practice.Models;
using Work_Practice.Commands;

namespace Work_Practice.ViewModels
{
	public class Task1ViewModel : INotifyPropertyChanged
	{
		private string _currentNumberInput = "";
		private ObservableCollection<int> _numbers = new ObservableCollection<int>();
		private ObservableCollection<NumberInfo> _results = new ObservableCollection<NumberInfo>();
		private bool _isProcSelected = true;
		private string _selectedVariant = "Proc";
		private string _codeExampleText = "// Код процедуры будет здесь";

		public string CurrentNumberInput
		{
			get => _currentNumberInput;
			set { _currentNumberInput = value; OnPropertyChanged(); }
		}

		public ObservableCollection<int> Numbers
		{
			get => _numbers;
			set { _numbers = value; OnPropertyChanged(); }
		}

		public ObservableCollection<NumberInfo> Results
		{
			get => _results;
			set { _results = value; OnPropertyChanged(); }
		}

		public bool IsProcSelected
		{
			get => _isProcSelected;
			set
			{
				if (_isProcSelected == value) return;
				_isProcSelected = value;
				OnPropertyChanged();
				if (value)
				{
					_isFuncSelected = false;
					OnPropertyChanged(nameof(IsFuncSelected));
					SelectedVariant = "Proc";
				}
				UpdateCodeExample();
			}
		}

		private bool _isFuncSelected;
		public bool IsFuncSelected
		{
			get => _isFuncSelected;
			set
			{
				if (_isFuncSelected == value) return;
				_isFuncSelected = value;
				OnPropertyChanged();
				if (value)
				{
					_isProcSelected = false;
					OnPropertyChanged(nameof(IsProcSelected));
					SelectedVariant = "Func";
				}
				UpdateCodeExample();
			}
		}

		public string SelectedVariant
		{
			get => _selectedVariant;
			private set { _selectedVariant = value; OnPropertyChanged(); }
		}

		public ICommand AddNumberCommand { get; }
		public ICommand ProcessCommand { get; }
		public ICommand ClearNumbersCommand { get; }

		public Task1ViewModel()
		{
			AddNumberCommand = new DelegateCommand(AddNumber);
			ProcessCommand = new DelegateCommand(Process);
			ClearNumbersCommand = new DelegateCommand(ClearNumbers);
			UpdateCodeExample();
		}

		private void AddNumber()
		{
			if (int.TryParse(CurrentNumberInput, out int num))
			{
				if (num == 0)
				{
					MessageBox.Show("Последовательность завершена (0 не добавляется). Нажмите 'Обработать'.");
					return;
				}
				if (num < 0)
				{
					MessageBox.Show("Введите положительное число.");
					return;
				}
				Numbers.Add(num);
				CurrentNumberInput = "";
			}
			else
			{
				MessageBox.Show("Введите целое число.");
			}
		}

		private void ClearNumbers()
		{
			Numbers.Clear();
			Results.Clear();
		}

		private void Process()
		{
			if (Numbers.Count == 0)
			{
				MessageBox.Show("Нет чисел для обработки.");
				return;
			}

			Results.Clear();
			foreach (int n in Numbers)
			{
				if (SelectedVariant == "Proc")
				{
					GetDigitsInfoProc(n, out int count, out int? minDigit);
					Results.Add(new NumberInfo { Number = n, DigitCount = count, MinDigit = minDigit });
				}
				else
				{
					var (count, minDigit) = GetDigitsInfoFunc(n);
					Results.Add(new NumberInfo { Number = n, DigitCount = count, MinDigit = minDigit });
				}
			}
		}

		// Процедура
		private void GetDigitsInfoProc(int number, out int count, out int? minDigit)
		{
			if (number <= 0)
			{
				count = 0;
				minDigit = null;
				return;
			}
			int temp = number;
			count = 0;
			minDigit = 9;
			while (temp > 0)
			{
				int digit = temp % 10;
				if (digit < minDigit) minDigit = digit;
				count++;
				temp /= 10;
			}
		}

		// Функция, возвращающая кортеж
		private (int count, int? minDigit) GetDigitsInfoFunc(int number)
		{
			if (number <= 0) return (0, null);
			int temp = number;
			int count = 0;
			int? minDigit = 9;
			while (temp > 0)
			{
				int digit = temp % 10;
				if (digit < minDigit) minDigit = digit;
				count++;
				temp /= 10;
			}
			return (count, minDigit);
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		public string CodeExampleText
		{
			get => _codeExampleText;
			set { _codeExampleText = value; OnPropertyChanged(); }
		}

		private void UpdateCodeExample()
		{
			if (SelectedVariant == "Proc")
			{
				CodeExampleText = "void GetDigitsInfoProc(int number, out int count, out int minDigit)\n" +
					"{\n" +
					"    if (number <= 0)\n" +
					"    {\n" +
					"        count = 0;\n" +
					"        minDigit = -1;\n" +
					"        return;\n" +
					"    }\n" +
					"    int temp = number;\n" +
					"    count = 0;\n" +
					"    minDigit = 9;\n" +
					"    while (temp > 0)\n" +
					"    {\n" +
					"        int digit = temp % 10;\n" +
					"        if (digit < minDigit) minDigit = digit;\n" +
					"        count++;\n" +
					"        temp /= 10;\n" +
					"    }\n" +
					"}";
			}
			else
			{
				CodeExampleText =
				"(int count, int minDigit) GetDigitsInfoFunc(int number)\n" +
				"{\n" +
				"    int temp = number;\n" +
				"    int count = 0;\n" +
				"    int minDigit = 9;\n" +
				"    while (temp > 0)\n" +
				"    {\n" +
				"        int digit = temp % 10;\n" +
				"        if (digit < minDigit) minDigit = digit;\n" +
				"        count++;\n" +
				"        temp /= 10;\n" +
				"    }\n" +
				"    return (count, minDigit);\n" +
				"}";
			}
		}
	}
}