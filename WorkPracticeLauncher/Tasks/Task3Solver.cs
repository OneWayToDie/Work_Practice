using System;
using System.Collections.Generic;
using WorkPracticeLauncher.Models;

namespace WorkPracticeLauncher.Tasks
{
	public static class Task3Solver
	{
		private static int lastWidth;
		private static int lastHeight;

		public static void Run()
		{
			lastWidth = Console.WindowWidth;
			lastHeight = Console.WindowHeight;

			RedrawScreen();

			Console.ForegroundColor = ConsoleColor.Cyan;
			if (Program.SupportsEmoji)
				Console.WriteLine("Введите действительные числа (через пробел или запятую):");
			else
				Console.WriteLine("Введите числа (через пробел):");
			Console.ResetColor();

			List<double> numbers = null;
			while (true)
			{
				CheckResizeAndRedraw();
				Console.Write("Числа (Enter/ESC – Назад): ");
				string inputLine = Console.ReadLine();
				if (string.IsNullOrEmpty(inputLine))
					return;
				string[] parts = inputLine.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
				numbers = new List<double>();
				foreach (string p in parts)
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
					if (Program.SupportsEmoji)
						Console.WriteLine("❌ Недостаточно чисел (нужно минимум 3). Попробуйте снова.");
					else
						Console.WriteLine("Недостаточно чисел (нужно минимум 3). Попробуйте снова.");
					Console.ResetColor();
				}
			}

			CheckResizeAndRedraw();
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("▶ Выберите реализацию:");
			Console.ResetColor();
			Console.WriteLine("  1 – Собственная реализация");
			Console.WriteLine("  2 – LinkedList<T>");
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.WriteLine("  (или ESC/Enter – возврат)");
			Console.ResetColor();
			Console.Write("Ваш выбор: ");
			string choice = Console.ReadLine();
			if (string.IsNullOrEmpty(choice))
				return;
			bool useCustom = (choice == "1");

			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("─" + new string('─', 40) + "─");
			Console.ResetColor();

			if (useCustom)
			{
				MyLinkedList<double> list = new MyLinkedList<double>();
				foreach (double n in numbers) list.Add(n);
				Console.ForegroundColor = ConsoleColor.Cyan;
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
				LinkedList<double> list = new LinkedList<double>();
				foreach (double n in numbers) list.AddLast(n);
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.Write("Исходный список: ");
				Console.ResetColor();
				Console.WriteLine(string.Join(" → ", list));

				if (list.Count >= 3)
				{
					LinkedListNode<double> thirdNode = list.First?.Next?.Next;
					if (thirdNode != null)
					{
						double value = thirdNode.Value;
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
			Console.WriteLine("\nНажмите любую клавишу для возврата (ESC – Назад)...");
			Console.ReadKey();
			// ESC обрабатывается на уровне вызывающего метода
		}

		private static void RedrawScreen()
		{
			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			if (Program.SupportsEmoji)
			{
				Console.WriteLine("╔═══════════════════════════════╗");
				Console.WriteLine("║ ЗАДАНИЕ 3: ОДНОСВЯЗНЫЙ СПИСОК ║");
				Console.WriteLine("╚═══════════════════════════════╝");
			}
			else
				Console.WriteLine("=== ЗАДАНИЕ 3: ОДНОСВЯЗНЫЙ СПИСОК ===");
			Console.ResetColor();
			Console.WriteLine();
		}

		private static void CheckResizeAndRedraw()
		{
			if (Console.WindowWidth != lastWidth || Console.WindowHeight != lastHeight)
			{
				lastWidth = Console.WindowWidth;
				lastHeight = Console.WindowHeight;
				RedrawScreen();
				Console.WriteLine("Размер окна изменён, экран обновлён.");
			}
		}
	}
}