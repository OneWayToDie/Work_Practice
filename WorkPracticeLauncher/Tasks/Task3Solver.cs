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
			Console.WriteLine("=== ЗАДАНИЕ 3: ОДНОСВЯЗНЫЙ СПИСОК ===");
			Console.ResetColor();
			Console.WriteLine("Введите действительные числа (через пробел или запятую).");
			Console.Write("Числа: ");
			string inputLine = Console.ReadLine();
			var parts = inputLine.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
			var numbers = new List<double>();
			foreach (var p in parts)
				if (double.TryParse(p, out double val)) numbers.Add(val);

			if (numbers.Count < 3)
			{
				Console.WriteLine("Недостаточно чисел (нужно минимум 3).");
				Console.ReadKey();
				return;
			}

			Console.WriteLine("\nВыберите реализацию:");
			Console.WriteLine("  1 – Собственная реализация");
			Console.WriteLine("  2 – LinkedList<T>");
			Console.Write("Ваш выбор: ");
			string choice = Console.ReadLine();
			bool useCustom = (choice == "1");

			if (useCustom)
			{
				var list = new MyLinkedList<double>();
				foreach (var n in numbers) list.Add(n);
				Console.WriteLine("Исходный список: " + string.Join(" → ", list.ToList()));
				if (list.MoveThirdToFront())
					Console.WriteLine("После переноса:   " + string.Join(" → ", list.ToList()));
				else
					Console.WriteLine("Операция невозможна (меньше 3 элементов).");
			}
			else
			{
				var list = new LinkedList<double>();
				foreach (var n in numbers) list.AddLast(n);
				Console.WriteLine("Исходный список: " + string.Join(" → ", list));
				if (list.Count >= 3)
				{
					var thirdNode = list.First?.Next?.Next;
					if (thirdNode != null)
					{
						var value = thirdNode.Value;
						list.Remove(thirdNode);
						list.AddFirst(value);
						Console.WriteLine("После переноса:   " + string.Join(" → ", list));
					}
				}
				else
					Console.WriteLine("Операция невозможна (меньше 3 элементов).");
			}

			Console.WriteLine("\nНажмите любую клавишу для возврата...");
			Console.ReadKey();
		}
	}
}
