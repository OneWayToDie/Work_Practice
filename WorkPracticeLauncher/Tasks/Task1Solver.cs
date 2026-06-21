using System;
using System.Collections.Generic;
using System.Numerics;

namespace WorkPracticeLauncher.Tasks
{
	public static class Task1Solver
	{
		private static List<string> numbers = new List<string>();
		private static int lastWidth;
		private static int lastHeight;

		public static void Run()
		{
			lastWidth = Console.WindowWidth;
			lastHeight = Console.WindowHeight;

			RedrawScreen();

			// Ввод количества чисел
			int count = 0;
			while (true)
			{
				CheckResizeAndRedraw();
				Console.Write("Введите количество чисел (N) от 1 до 50 (или 0 для выхода): ");
				string input = Console.ReadLine();
				if (input == "0")
				{
					Console.WriteLine("Ввод отменён.");
					Console.ReadKey();
					return;
				}
				if (!int.TryParse(input, out count) || count <= 0)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Введите положительное целое число.");
					Console.ResetColor();
					continue;
				}
				if (count > 50)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Максимальное количество чисел – 50.");
					Console.ResetColor();
					continue;
				}
				break;
			}

			numbers.Clear();
			Console.WriteLine($"\nВведите {count} чисел (каждое до 25 цифр). Для досрочного завершения введите 0:");
			for (int i = 0; i < count; i++)
			{
				while (true)
				{
					CheckResizeAndRedraw();
					Console.Write($"Число {i + 1}: ");
					string input = Console.ReadLine();
					if (input == "0")
					{
						Console.WriteLine($"Ввод прерван (введено 0). Обработано {i} чисел.");
						goto EndInput;
					}
					if (string.IsNullOrWhiteSpace(input))
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("Пустой ввод, повторите.");
						Console.ResetColor();
						continue;
					}

					bool isValid = true;
					foreach (char c in input)
						if (!char.IsDigit(c)) { isValid = false; break; }
					if (!isValid)
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("Введите целое число (только цифры).");
						Console.ResetColor();
						continue;
					}

					string trimmed = input.TrimStart('0');
					if (string.IsNullOrEmpty(trimmed)) trimmed = "0";
					if (trimmed.Length > 25)
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("Число не должно превышать 25 знаков.");
						Console.ResetColor();
						continue;
					}

					numbers.Add(trimmed);
					break;
				}
			}
		EndInput:

			if (numbers.Count == 0)
			{
				Console.WriteLine("Нет чисел для обработки.");
				Console.ReadKey();
				return;
			}

			// Выбор варианта
			CheckResizeAndRedraw();
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("▶ Выберите вариант:");
			Console.ResetColor();
			Console.WriteLine("  1 – Процедура");
			Console.WriteLine("  2 – Функция");
			Console.Write("Ваш выбор: ");
			string varChoice = Console.ReadLine();
			bool useProc = (varChoice == "1");

			// Вычисление результатов
			BigInteger minVal = BigInteger.Parse(numbers[0]);
			BigInteger maxVal = BigInteger.Parse(numbers[0]);
			string minNumber = numbers[0];
			string maxNumber = numbers[0];
			foreach (var n in numbers)
			{
				BigInteger val = BigInteger.Parse(n);
				if (val < minVal) { minVal = val; minNumber = n; }
				if (val > maxVal) { maxVal = val; maxNumber = n; }
			}

			int maxLength = 0;
			foreach (var n in numbers)
				if (n.Length > maxLength) maxLength = n.Length;
			int colWidth = Math.Max(maxLength, "Число".Length) + 2;

			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("Результаты:");
			Console.ResetColor();
			Console.WriteLine($"{"Число".PadRight(colWidth)} {"Кол-во цифр".PadRight(15)} {"Мин. цифра".PadRight(15)}");
			Console.WriteLine(new string('-', colWidth + 15 + 15));

			foreach (string n in numbers)
			{
				if (useProc)
				{
					GetDigitsInfoProc(n, out int countDigits, out int minDigit);
					Console.WriteLine($"{n.PadRight(colWidth)} {countDigits.ToString().PadRight(15)} {minDigit.ToString().PadRight(15)}");
				}
				else
				{
					var (countDigits, minDigit) = GetDigitsInfoFunc(n);
					Console.WriteLine($"{n.PadRight(colWidth)} {countDigits.ToString().PadRight(15)} {minDigit.ToString().PadRight(15)}");
				}
			}

			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine($"\nМинимальное число: {minNumber}");
			Console.WriteLine($"Максимальное число: {maxNumber}");
			Console.ResetColor();

			Console.WriteLine("\nНажмите любую клавишу для возврата...");
			Console.ReadKey();
		}

		private static void RedrawScreen()
		{
			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			if (Program.IsWindowsTerminal)
				Console.WriteLine("=== ЗАДАНИЕ 1: ПОСЛЕДОВАТЕЛЬНОСТЬ ЧИСЕЛ, ПРОЦЕДУРА/ФУНКЦИЯ ===");
			else
				Console.WriteLine("=== ЗАДАНИЕ 1: ПОСЛЕДОВАТЕЛЬНОСТЬ ЧИСЕЛ ===");
			Console.ResetColor();
			if (numbers.Count > 0)
			{
				Console.Write("Введено чисел: " + numbers.Count + " (");
				Console.WriteLine(string.Join(", ", numbers) + ")");
			}
		}

		private static void CheckResizeAndRedraw()
		{
			if (Console.WindowWidth != lastWidth || Console.WindowHeight != lastHeight)
			{
				lastWidth = Console.WindowWidth;
				lastHeight = Console.WindowHeight;
				RedrawScreen();
			}
		}

		private static void GetDigitsInfoProc(string number, out int count, out int minDigit)
		{
			if (string.IsNullOrEmpty(number))
			{
				count = 0;
				minDigit = -1;
				return;
			}
			count = number.Length;
			minDigit = 9;
			foreach (char c in number)
			{
				int digit = c - '0';
				if (digit < minDigit) minDigit = digit;
			}
		}

		private static (int count, int minDigit) GetDigitsInfoFunc(string number)
		{
			if (string.IsNullOrEmpty(number))
				return (0, -1);
			int count = number.Length;
			int minDigit = 9;
			foreach (char c in number)
			{
				int digit = c - '0';
				if (digit < minDigit) minDigit = digit;
			}
			return (count, minDigit);
		}
	}
}