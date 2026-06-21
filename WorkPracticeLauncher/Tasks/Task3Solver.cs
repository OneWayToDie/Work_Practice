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
					string normalized = p.Replace(',', '.');
					if (double.TryParse(normalized, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double val))
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

			// Микро-заголовок – зелёный с префиксом ▶
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("\n▶ Выберите реализацию:");
			Console.ResetColor();
			Console.WriteLine("  1 – Собственная реализация");
			Console.WriteLine("  2 – LinkedList<T>");
			string choice = InputHelper.ReadLine("Ваш выбор: ");
			bool useCustom = (choice == "1");

			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("─" + new string('─', 40) + "─");
			Console.ResetColor();

			if (useCustom)
			{
				var list = new MyLinkedList<double>();
				foreach (var n in numbers) list.Add(n);
				// Микро-заголовок для исходного списка – зелёный
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write("▶ Исходный список: ");
				Console.ResetColor();
				Console.WriteLine(string.Join(" → ", list.ToList()));

				if (list.MoveThirdToFront())
				{
					// Микро-заголовок для результата – зелёный (уже есть, добавим ▶)
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write("▶ После переноса:   ");
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
				// Микро-заголовок для исходного списка – зелёный
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write("▶ Исходный список: ");
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
						// Микро-заголовок для результата – зелёный
						Console.ForegroundColor = ConsoleColor.Green;
						Console.Write("▶ После переноса:   ");
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