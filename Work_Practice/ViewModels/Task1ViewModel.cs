using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Work_Practice.Models;
using Work_Practice.Commands;
using Work_Practice.Views;

namespace Work_Practice.ViewModels
{
	public class Task1ViewModel : INotifyPropertyChanged
	{
		private string currentNumberInput = "";
		private ObservableCollection<long> numbers = new ObservableCollection<long>();
		private ObservableCollection<NumberInfo> results = new ObservableCollection<NumberInfo>();
		private bool isProcSelected = true;
		private string selectedVariant = "Proc";
		private string codeExampleText = "// Код процедуры будет здесь";

		public string CurrentNumberInput
		{
			get => currentNumberInput;
			set { currentNumberInput = value; OnPropertyChanged(); }
		}

		public ObservableCollection<long> Numbers
		{
			get => numbers;
			set { numbers = value; OnPropertyChanged(); }
		}

		public ObservableCollection<NumberInfo> Results
		{
			get => results;
			set { results = value; OnPropertyChanged(); }
		}

		public bool IsProcSelected
		{
			get => isProcSelected;
			set
			{
				if (isProcSelected == value) return;
				isProcSelected = value;
				OnPropertyChanged();
				if (value)
				{
					isFuncSelected = false;
					OnPropertyChanged(nameof(IsFuncSelected));
					SelectedVariant = "Proc";
				}
				UpdateCodeExample();
			}
		}

		private bool isFuncSelected;
		public bool IsFuncSelected
		{
			get => isFuncSelected;
			set
			{
				if (isFuncSelected == value) return;
				isFuncSelected = value;
				OnPropertyChanged();
				if (value)
				{
					isProcSelected = false;
					OnPropertyChanged(nameof(IsProcSelected));
					SelectedVariant = "Func";
				}
				UpdateCodeExample();
			}
		}

		public string SelectedVariant
		{
			get => selectedVariant;
			private set { selectedVariant = value; OnPropertyChanged(); }
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
			if (long.TryParse(CurrentNumberInput, out long num))
			{
				if (num == 0)
				{
					AppDialog.ShowInfo("Последовательность завершена (0 не добавляется). Нажмите 'Обработать'.");
					return;
				}
				if (num < 0)
				{
					AppDialog.ShowWarning("Введите положительное число.");
					return;
				}
				Numbers.Add(num);
				CurrentNumberInput = "";
			}
			else
			{
				AppDialog.ShowWarning("Введите целое число.");
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
				AppDialog.ShowWarning("Нет чисел для обработки.");
				return;
			}

			Results.Clear();
			foreach (long n in Numbers)
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
		private void GetDigitsInfoProc(long number, out int count, out int? minDigit)
		{
			if (number <= 0)
			{
				count = 0;
				minDigit = null;
				return;
			}
			long temp = number;
			count = 0;
			minDigit = 9;
			while (temp > 0)
			{
				int digit = (int)(temp % 10);
				if (digit < minDigit) minDigit = digit;
				count++;
				temp /= 10;
			}
		}

		// Функция, возвращающая кортеж
		private (int count, int? minDigit) GetDigitsInfoFunc(long number)
		{
			if (number <= 0) return (0, null);
			long temp = number;
			int count = 0;
			int? minDigit = 9;
			while (temp > 0)
			{
				int digit = (int)(temp % 10);
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
			get => codeExampleText;
			set { codeExampleText = value; OnPropertyChanged(); }
		}

		private void UpdateCodeExample()
		{
			if (SelectedVariant == "Proc")
			{
				CodeExampleText = "void GetDigitsInfoProc(long number, out int count, out int minDigit)\n" +
					"{\n" +
					"    if (number <= 0)\n" +
					"    {\n" +
					"        count = 0;\n" +
					"        minDigit = -1;\n" +
					"        return;\n" +
					"    }\n" +
					"    long temp = number;\n" +
					"    count = 0;\n" +
					"    minDigit = 9;\n" +
					"    while (temp > 0)\n" +
					"    {\n" +
					"        int digit = (int)(temp % 10);\n" +
					"        if (digit < minDigit) minDigit = digit;\n" +
					"        count++;\n" +
					"        temp /= 10;\n" +
					"    }\n" +
					"}";
			}
			else
			{
				CodeExampleText =
				"(int count, int minDigit) GetDigitsInfoFunc(long number)\n" +
				"{\n" +
				"    long temp = number;\n" +
				"    int count = 0;\n" +
				"    int minDigit = 9;\n" +
				"    while (temp > 0)\n" +
				"    {\n" +
				"        int digit = (int)(temp % 10);\n" +
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