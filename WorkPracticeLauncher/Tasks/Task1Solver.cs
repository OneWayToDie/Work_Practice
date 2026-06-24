//========================================================= Класс Task1Solver – решение задания 1 (числа, процедура/функция) ================================================================//
using System;                     // Базовые типы и консольный ввод-вывод
using System.Collections.Generic; // Коллекции (List<T>)
using System.Numerics;			  // Работа с большими числами (BigInteger)

namespace WorkPracticeLauncher.Tasks
{
	public static class Task1Solver
	{
		//========================================================= Поля класса ================================================================//
		private static List<string> numbers = new List<string>();     // Список введённых чисел (в виде строк)
		private static int lastWidth;                                 // Последняя ширина окна (для отслеживания изменения)
		private static int lastHeight;                                // Последняя высота окна (для отслеживания изменения)

		//========================================================= Основной метод ================================================================//
		public static void Run()
		{
			lastWidth = Console.WindowWidth;																			// Запоминаем начальную ширину
			lastHeight = Console.WindowHeight;																			// Запоминаем начальную высоту

			RedrawScreen();																								// Очищаем экран и показываем заголовок

			// ============ Ввод количества чисел  =======================//
			int count = 0;																								// Переменная для количества чисел
			while (true)																								// Бесконечный цикл до корректного ввода
			{
				CheckResizeAndRedraw();																					// Проверяем, не изменился ли размер окна
				Console.Write("Введите количество чисел (N) от 1 до 50 (или 0 для выхода): ");							// Запрос N
				string input = Console.ReadLine();																		// Считываем строку
				if (input == "0")																						// Если введён 0
				{
					Console.WriteLine("Ввод отменён.");																	// Сообщаем об отмене
					Console.ReadKey();																					// Ожидаем нажатия клавиши
					return;																								// Выход из метода
				}
				if (!int.TryParse(input, out count) || count <= 0)														// Если не число или <=0
				{
					Console.ForegroundColor = ConsoleColor.Red;															// Красный цвет для ошибки
					Console.WriteLine("Введите положительное целое число.");											// Сообщение
					Console.ResetColor();																				// Сброс цвета
					continue;																							// Повторяем ввод
				}
				if (count > 50)																							// Если больше 50
				{
					Console.ForegroundColor = ConsoleColor.Red;															// Красный цвет
					Console.WriteLine("Максимальное количество чисел – 50.");											// Сообщение
					Console.ResetColor();																				// Сброс цвета
					continue;																							// Повторяем ввод
				}
				break;																									// Ввод корректен – выходим из цикла
			}

			numbers.Clear();																							// Очищаем список чисел (на случай повторного запуска)
			Console.WriteLine($"\nВведите {count} чисел (каждое до 25 цифр). Для досрочного завершения введите 0:");	// Инструкция
			for (int i = 0; i < count; i++)																				// Цикл ввода каждого числа
			{
				while (true)																							// Бесконечный цикл до корректного ввода
				{
					CheckResizeAndRedraw();																				// Проверяем размер окна
					Console.Write($"Число {i + 1}: ");																	// Запрос числа
					string input = Console.ReadLine();																	// Считываем строку
					if (input == "0")																					// Если введён 0
					{
						Console.WriteLine($"Ввод прерван (введено 0). Обработано {i} чисел.");							// Сообщение
						goto EndInput;																					// Переход к метке EndInput (прерывание ввода)
					}
					if (string.IsNullOrWhiteSpace(input))																// Если пустая строка
					{
						Console.ForegroundColor = ConsoleColor.Red;														// Красный цвет
						Console.WriteLine("Пустой ввод, повторите.");													// Сообщение
						Console.ResetColor();																			// Сброс цвета
						continue;																						// Повторяем
					}

					bool isValid = true;																				// Флаг корректности
					foreach (char c in input)																			// Проверяем каждый символ
						if (!char.IsDigit(c)) { isValid = false; break; }												// Если не цифра – флаг false
					if (!isValid)																						// Если есть не цифры
					{
						Console.ForegroundColor = ConsoleColor.Red;														// Красный цвет
						Console.WriteLine("Введите целое число (только цифры).");										// Сообщение
						Console.ResetColor();																			// Сброс цвета
						continue;																						// Повторяем
					}

					string trimmed = input.TrimStart('0');																// Убираем ведущие нули
					if (string.IsNullOrEmpty(trimmed)) trimmed = "0";													// Если было только 0 – оставляем 0
					if (trimmed.Length > 25)																			// Если больше 25 цифр
					{
						Console.ForegroundColor = ConsoleColor.Red;														// Красный цвет
						Console.WriteLine("Число не должно превышать 25 знаков.");										// Сообщение
						Console.ResetColor();																			// Сброс цвета
						continue;																						// Повторяем
					}

					numbers.Add(trimmed);																				// Добавляем число в список
					break;																								// Выход из внутреннего цикла (число корректно)
				}
			}
			EndInput:																									// Метка для прерывания ввода по 0

			if (numbers.Count == 0)																						// Если не введено ни одного числа
			{
				Console.WriteLine("Нет чисел для обработки.");															// Сообщение
				Console.ReadKey();																						// Ожидание клавиши
				return;																									// Выход из метода
			}

			// Выбор варианта (процедура или функция)
			CheckResizeAndRedraw();																						// Проверяем размер окна
			Console.ForegroundColor = ConsoleColor.Green;																// Зелёный цвет для подсказки
			Console.WriteLine($"{(Program.SupportsEmoji ? "▶ " : " ")} Выберите вариант:");																	// Заголовок выбора
			Console.ResetColor();																						// Сброс цвета
			Console.WriteLine("  1 – Процедура");																		// Пункт 1
			Console.WriteLine("  2 – Функция");																			// Пункт 2
			Console.Write("Ваш выбор: ");																				// Приглашение
			string varChoice = Console.ReadLine();																		// Считываем выбор
			bool useProc = (varChoice == "1");																			// true – процедура, false – функция

			// Вычисление минимального и максимального числа
			BigInteger minVal = BigInteger.Parse(numbers[0]);															// Минимальное значение (BigInteger)
			BigInteger maxVal = BigInteger.Parse(numbers[0]);															// Максимальное значение (BigInteger)
			string minNumber = numbers[0];																				// Строка минимального числа
			string maxNumber = numbers[0];																				// Строка максимального числа
			foreach (string n in numbers)																				// Перебираем все числа
			{
				BigInteger val = BigInteger.Parse(n);																	// Преобразуем в BigInteger
				if (val < minVal) { minVal = val; minNumber = n; }														// Если меньше – обновляем минимум
				if (val > maxVal) { maxVal = val; maxNumber = n; }														// Если больше – обновляем максимум
			}

			// Вычисление ширины столбца для таблицы
			int maxLength = 0;																							// Максимальная длина числа
			foreach (string n in numbers)
				if (n.Length > maxLength) maxLength = n.Length;															// Обновляем максимум
			int colWidth = Math.Max(maxLength, "Число".Length) + 2;														// Ширина столбца "Число" + отступ

			// Вывод таблицы результатов
			Console.Clear();																							// Очищаем экран
			Console.ForegroundColor = ConsoleColor.Yellow;																// Жёлтый цвет для заголовка
			Console.WriteLine("Результаты:");																			// Заголовок
			Console.ResetColor();																						// Сброс цвета
			Console.WriteLine($"{"Число".PadRight(colWidth)} {"Кол-во цифр".PadRight(15)} {"Мин. цифра".PadRight(15)}");// Шапка таблицы
			Console.WriteLine(new string('-', colWidth + 15 + 15));														// Разделитель

			foreach (string n in numbers)																				// Перебираем числа
			{
				if (useProc)																							// Если выбрана процедура
				{
					GetDigitsInfoProc(n, out int countDigits, out int minDigit);										// Вызов процедуры
					Console.WriteLine($"{n.PadRight(colWidth)} {countDigits.ToString().PadRight(15)} {minDigit.ToString().PadRight(15)}"); // Вывод строки
				}
				else																									// Если выбрана функция
				{
					(int countDigits, int minDigit) = GetDigitsInfoFunc(n);												// Вызов функции
					Console.WriteLine($"{n.PadRight(colWidth)} {countDigits.ToString().PadRight(15)} {minDigit.ToString().PadRight(15)}"); // Вывод строки
				}
			}

			Console.ForegroundColor = ConsoleColor.Cyan;																// Голубой цвет для итогов
			Console.WriteLine($"\nМинимальное число: {minNumber}");														// Вывод минимума
			Console.WriteLine($"Максимальное число: {maxNumber}");														// Вывод максимума
			Console.ResetColor();																						// Сброс цвета

			Console.WriteLine("\nНажмите любую клавишу для возврата...");												// Ожидание
			Console.ReadKey();																							// Ждём клавишу
		}

		//========================================================= Перерисовка экрана ================================================================//
		private static void RedrawScreen()
		{
			Console.Clear();																							// Очищаем экран
			Console.ForegroundColor = ConsoleColor.Yellow;																// Жёлтый цвет для заголовка
			if (Program.SupportsEmoji)																					// Если поддерживаются эмодзи
				Console.WriteLine("=== ЗАДАНИЕ 1: ПОСЛЕДОВАТЕЛЬНОСТЬ ЧИСЕЛ, ПРОЦЕДУРА/ФУНКЦИЯ ===");					// Полный заголовок
			else
				Console.WriteLine("=== ЗАДАНИЕ 1: ПОСЛЕДОВАТЕЛЬНОСТЬ ЧИСЕЛ ===");										// Упрощённый заголовок
			Console.ResetColor();																						// Сброс цвета
			if (numbers.Count > 0)																						// Если есть введённые числа
			{
				Console.Write("Введено чисел: " + numbers.Count + " (");												// Вывод количества
				Console.WriteLine(string.Join(", ", numbers) + ")");													// Вывод списка чисел через запятую
			}
		}

		//========================================================= Проверка изменения размера окна ================================================================//
		private static void CheckResizeAndRedraw()
		{
			if (Console.WindowWidth != lastWidth || Console.WindowHeight != lastHeight)									// Если размер изменился
			{
				lastWidth = Console.WindowWidth;																		// Обновляем сохранённую ширину
				lastHeight = Console.WindowHeight;																		// Обновляем сохранённую высоту
				RedrawScreen();																							// Перерисовываем экран
			}
		}

		//========================================================= Процедура получения информации о цифрах числа ================================================================//
		private static void GetDigitsInfoProc(string number, out int count, out int minDigit)
		{
			if (string.IsNullOrEmpty(number))																			// Если строка пустая
			{
				count = 0;																								// Количество цифр = 0
				minDigit = -1;																							// Минимальная цифра = -1 (неопределено)
				return;																									// Выход
			}
			count = number.Length;																						// Количество цифр = длина строки
			minDigit = 9;																								// Начальное значение для поиска минимума
			foreach (char c in number)																					// Перебираем все символы
			{
				int digit = c - '0';																					// Преобразуем символ в число
				if (digit < minDigit) minDigit = digit;																	// Если меньше – обновляем минимум
			}
		}

		//========================================================= Функция получения информации о цифрах числа ================================================================//
		private static (int count, int minDigit) GetDigitsInfoFunc(string number)
		{
			if (string.IsNullOrEmpty(number))																			// Если строка пустая
				return (0, -1);																							// Возвращаем (0, -1)
			int count = number.Length;																					// Количество цифр
			int minDigit = 9;																							// Начальное значение для поиска минимума
			foreach (char c in number)																					// Перебираем символы
			{
				int digit = c - '0';																					// Преобразуем в число
				if (digit < minDigit) minDigit = digit;																	// Обновляем минимум
			}
			return (count, minDigit);																					// Возвращаем кортеж
		}
	}
}