using System;
using System.Collections.Generic;

namespace WorkPracticeLauncher.Tasks
{
	public static class Task1Solver
	{
		public static void Run()
		{
			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("=== ЗАДАНИЕ 1: ЧИСЛА, ПРОЦЕДУРА/ФУНКЦИЯ ===");
			Console.ResetColor();
			Console.WriteLine("Вводите целые числа (до 25 цифр), 0 – конец последовательности.");

			List<string> numbers = new List<string>();
			while (true)
			{
				Console.Write("Число: ");
				string input = Console.ReadLine();
				if (input == "0") break;
				if (string.IsNullOrWhiteSpace(input))
				{
					Console.WriteLine("Пустой ввод, повторите.");
					continue;
				}
				bool isValid = true;
				foreach (char c in input)
				{
					if (!char.IsDigit(c))
					{
						isValid = false;
						break;
					}
				}
				if (!isValid)
				{
					Console.WriteLine("Некорректный ввод, введите целое число (только цифры).");
					continue;
				}
				if (input.Length > 25)
				{
					Console.WriteLine("Число не должно превышать 25 знаков.");
					continue;
				}
				numbers.Add(input);
			}

			if (numbers.Count == 0)
			{
				Console.WriteLine("Последовательность пуста.");
				Console.ReadKey();
				return;
			}

			// Находим минимальное число
			string minNumber = numbers[0];
			foreach (var n in numbers)
			{
				if (n.Length < minNumber.Length)
					minNumber = n;
				else if (n.Length == minNumber.Length && string.Compare(n, minNumber, StringComparison.Ordinal) < 0)
					minNumber = n;
			}

			Console.WriteLine("\nВыберите вариант:");
			Console.WriteLine("  1 – Процедура");
			Console.WriteLine("  2 – Функция");
			Console.Write("Ваш выбор: ");
			string varChoice = Console.ReadLine();
			bool useProc = (varChoice == "1");

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
					GetDigitsInfoProc(n, out int count, out int min);
					Console.WriteLine($"{n.PadRight(colWidth)} {count.ToString().PadRight(15)} {min.ToString().PadRight(15)}");
				}
				else
				{
					var (count, min) = GetDigitsInfoFunc(n);
					Console.WriteLine($"{n.PadRight(colWidth)} {count.ToString().PadRight(15)} {min.ToString().PadRight(15)}");
				}
			}

			// Вывод минимального числа отдельной строкой
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine($"\nМинимальное число из всех введённых: {minNumber}");
			Console.ResetColor();

			Console.WriteLine("\nНажмите любую клавишу для возврата...");
			Console.ReadKey();
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