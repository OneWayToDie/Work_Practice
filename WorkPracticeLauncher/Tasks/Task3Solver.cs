using System;
using System.Collections.Generic;
using WorkPracticeLauncher.Models;

namespace WorkPracticeLauncher.Tasks
{
	public static class Task3Solver
	{
		public static void Run()
		{
			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("╔═══════════════════════════════╗");
			Console.WriteLine("║ ЗАДАНИЕ 3: ОДНОСВЯЗНЫЙ СПИСОК ║");
			Console.WriteLine("╚═══════════════════════════════╝");
			Console.ResetColor();
			Console.WriteLine();

			// Инструкция выводится один раз
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("Введите действительные числа (через пробел или запятую):");
			Console.ResetColor();

			List<double> numbers = null;

			while (true)
			{
				string inputLine = InputHelper.ReadLine("Числа: ");
				var parts = inputLine.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
				numbers = new List<double>();
				foreach (var p in parts)
				{
					if (double.TryParse(p, out double val))
						numbers.Add(val);
				}

				if (numbers.Count >= 3)
					break;
				else
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("❌ Недостаточно чисел (нужно минимум 3). Попробуйте снова.");
					Console.ResetColor();
				}
			}

			// Далее выбор реализации и вывод результатов
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("\nВыберите реализацию:");
			Console.ResetColor();
			Console.WriteLine("  1 – Собственная реализация");
			Console.WriteLine("  2 – LinkedList<T>");
			string choice = InputHelper.ReadLine("Ваше решение: ");
			bool useCustom = (choice == "1");

			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("─" + new string('─', 40) + "─");
			Console.ResetColor();

			if (useCustom)
			{
				var list = new MyLinkedList<double>();
				foreach (var n in numbers) list.Add(n);
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write("Исходный список: ");
				Console.ResetColor();
				Console.WriteLine(string.Join(" → ", list.ToList()));

				if (list.MoveThirdToFront())
				{
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write("После переноса:   ");
					Console.ResetColor();
					Console.WriteLine(string.Join(" → ", list.ToList()));
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Операция невозможна (меньше 3 элементов).");
					Console.ResetColor();
				}
			}
			else
			{
				var list = new LinkedList<double>();
				foreach (var n in numbers) list.AddLast(n);
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write("Исходный список: ");
				Console.ResetColor();
				Console.WriteLine(string.Join(" → ", list));

				if (list.Count >= 3)
				{
					var thirdNode = list.First?.Next?.Next;
					if (thirdNode != null)
					{
						var value = thirdNode.Value;
						list.Remove(thirdNode);
						list.AddFirst(value);
						Console.ForegroundColor = ConsoleColor.Green;
						Console.Write("После переноса:   ");
						Console.ResetColor();
						Console.WriteLine(string.Join(" → ", list));
					}
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Операция невозможна (меньше 3 элементов).");
					Console.ResetColor();
				}
			}

			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("─" + new string('─', 40) + "─");
			Console.ResetColor();
			Console.WriteLine("\nНажмите любую клавишу для возврата...");
			Console.ReadKey();
		}
	}
}