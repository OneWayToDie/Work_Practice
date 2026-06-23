//========================================================= Библиотеки ================================================================//
using System.Collections.ObjectModel; // ObservableCollection<T>
using System.ComponentModel;          // INotifyPropertyChanged
using System.Runtime.CompilerServices; // CallerMemberName
using System.Windows.Input;           // ICommand
using Work_Practice.Models;           // NumberInfo
using Work_Practice.Commands;         // DelegateCommand
using Work_Practice.Views;            // AppDialog

namespace Work_Practice.ViewModels
{
	//========================================================= ViewModel для задания 1 (числа, процедура/функция) ================================================================//
	public class Task1ViewModel : INotifyPropertyChanged
	{
		//========================================================= Приватные поля ================================================================//
		private string currentNumberInput = "";                    // Текст в поле ввода
		private ObservableCollection<long> numbers = new ObservableCollection<long>(); // Список введённых чисел
		private ObservableCollection<NumberInfo> results = new ObservableCollection<NumberInfo>(); // Результаты обработки
		private bool isProcSelected = true;                        // Выбран ли вариант "Процедура"
		private string selectedVariant = "Proc";                   // Текущий вариант ("Proc" или "Func")
		private string codeExampleText = "// Код процедуры будет здесь"; // Пример кода для отображения
		private bool isFuncSelected;                              // Выбран ли вариант "Функция" (инверсный флаг)

		//========================================================= Публичные свойства ================================================================//
		public string CurrentNumberInput
		{
			get => currentNumberInput;
			set { currentNumberInput = value; OnPropertyChanged(); } // Обновляем текст и уведомляем UI
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
				if (isProcSelected == value) return;               // Если значение не изменилось – выходим
				isProcSelected = value;
				OnPropertyChanged();
				if (value)                                         // Если выбрана процедура
				{
					isFuncSelected = false;                        // Сбрасываем флаг функции
					OnPropertyChanged(nameof(IsFuncSelected));
					SelectedVariant = "Proc";                      // Устанавливаем вариант
				}
				UpdateCodeExample();                               // Обновляем пример кода
			}
		}

		public bool IsFuncSelected
		{
			get => isFuncSelected;
			set
			{
				if (isFuncSelected == value) return;
				isFuncSelected = value;
				OnPropertyChanged();
				if (value)                                         // Если выбрана функция
				{
					isProcSelected = false;                        // Сбрасываем флаг процедуры
					OnPropertyChanged(nameof(IsProcSelected));
					SelectedVariant = "Func";                      // Устанавливаем вариант
				}
				UpdateCodeExample();
			}
		}

		public string SelectedVariant
		{
			get => selectedVariant;
			private set { selectedVariant = value; OnPropertyChanged(); }
		}

		public string CodeExampleText
		{
			get => codeExampleText;
			set { codeExampleText = value; OnPropertyChanged(); }
		}

		//========================================================= Команды ================================================================//
		public ICommand AddNumberCommand { get; }     // Команда добавления числа
		public ICommand ProcessCommand { get; }       // Команда обработки
		public ICommand ClearNumbersCommand { get; }  // Команда очистки

		//========================================================= Конструктор ================================================================//
		public Task1ViewModel()
		{
			AddNumberCommand = new DelegateCommand(AddNumber);      // Привязываем методы
			ProcessCommand = new DelegateCommand(Process);
			ClearNumbersCommand = new DelegateCommand(ClearNumbers);
			UpdateCodeExample();                                   // Инициализируем пример кода
		}

		//========================================================= Команда добавления числа ================================================================//
		private void AddNumber()
		{
			if (long.TryParse(CurrentNumberInput, out long num))   // Парсим ввод
			{
				if (num == 0)                                       // Если 0 – завершение последовательности
				{
					AppDialog.ShowInfo("Последовательность завершена (0 не добавляется). Нажмите 'Обработать'.");
					return;
				}
				if (num < 0)                                        // Отрицательные не допускаются
				{
					AppDialog.ShowWarning("Введите положительное число.");
					return;
				}
				Numbers.Add(num);                                   // Добавляем число в коллекцию
				CurrentNumberInput = "";                            // Очищаем поле ввода
			}
			else
			{
				AppDialog.ShowWarning("Введите целое число.");     // Сообщение об ошибке
			}
		}

		//========================================================= Команда очистки ================================================================//
		private void ClearNumbers()
		{
			Numbers.Clear();                                        // Очищаем список чисел
			Results.Clear();                                        // Очищаем результаты
		}

		//========================================================= Команда обработки ================================================================//
		private void Process()
		{
			if (Numbers.Count == 0)                                 // Если нет чисел
			{
				AppDialog.ShowWarning("Нет чисел для обработки.");
				return;
			}

			Results.Clear();                                        // Очищаем старые результаты
			foreach (long n in Numbers)                             // Проходим по всем числам
			{
				if (SelectedVariant == "Proc")                     // Если выбран вариант "Процедура"
				{
					GetDigitsInfoProc(n, out int count, out int? minDigit); // Вызов процедуры
					Results.Add(new NumberInfo { Number = n, DigitCount = count, MinDigit = minDigit });
				}
				else                                                // Иначе "Функция"
				{
					(int count, int? minDigit) = GetDigitsInfoFunc(n); // Вызов функции
					Results.Add(new NumberInfo { Number = n, DigitCount = count, MinDigit = minDigit });
				}
			}
		}

		//========================================================= Процедура получения информации о цифрах ================================================================//
		private void GetDigitsInfoProc(long number, out int count, out int? minDigit)
		{
			if (number <= 0)                                        // Обработка нуля или отрицательных
			{
				count = 0;
				minDigit = null;                                    // Минимальная цифра не определена
				return;
			}
			long temp = number;                                     // Копия для изменений
			count = 0;
			minDigit = 9;                                           // Начинаем с максимальной цифры
			while (temp > 0)                                        // Пока есть цифры
			{
				int digit = (int)(temp % 10);                       // Последняя цифра
				if (digit < minDigit) minDigit = digit;            // Обновляем минимум
				count++;                                            // Увеличиваем счётчик
				temp /= 10;                                         // Убираем последнюю цифру
			}
		}

		//========================================================= Функция получения информации о цифрах (возвращает кортеж) ================================================================//
		private (int count, int? minDigit) GetDigitsInfoFunc(long number)
		{
			if (number <= 0) return (0, null);                      // Для неположительных чисел
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
			return (count, minDigit);                               // Возвращаем кортеж
		}

		//========================================================= Обновление примера кода в интерфейсе ================================================================//
		private void UpdateCodeExample()
		{
			if (SelectedVariant == "Proc")
			{
				CodeExampleText =
					"void GetDigitsInfoProc(long number, out int count, out int minDigit)\n" +
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

		//========================================================= Реализация INotifyPropertyChanged ================================================================//
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}