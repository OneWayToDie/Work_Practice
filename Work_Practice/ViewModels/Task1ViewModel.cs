using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Work_Practice.Models;

namespace Work_Practice.ViewModels
{
	public class Task1ViewModel : INotifyPropertyChanged
	{
		private string _currentNumberInput = "";
		private ObservableCollection<int> _numbers = new ObservableCollection<int>();
		private ObservableCollection<NumberInfo> _results = new ObservableCollection<NumberInfo>();

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

		public ICommand AddNumberCommand { get; }
		public ICommand ProcessCommand { get; }
		public ICommand ClearNumbersCommand { get; }

		public Task1ViewModel()
		{
			AddNumberCommand = new DelegateCommand(AddNumber);
			ProcessCommand = new DelegateCommand(Process);
			ClearNumbersCommand = new DelegateCommand(ClearNumbers);
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
				GetDigitsInfoProc(n, out int count, out int minDigit);
				Results.Add(new NumberInfo { Number = n, DigitCount = count, MinDigit = minDigit });
			}
		}

		// Реальная процедура
		private void GetDigitsInfoProc(int number, out int count, out int minDigit)
		{
			if (number <= 0)
			{
				count = 0;
				minDigit = -1;
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

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}