//========================================================= Библиотеки ================================================================//
using System;                // Базовые типы и консольный ввод-вывод
using System.Collections.Generic; // Коллекции (List, LinkedList)
using WorkPracticeLauncher.Models; // Модель односвязного списка (MyLinkedList, Node)

namespace WorkPracticeLauncher.Tasks
{
	//========================================================= Класс Task3Solver – решение задания 3 (односвязный список) ================================================================//
	public static class Task3Solver
	{
		//========================================================= Поля класса ================================================================//
		private static int lastWidth;    // Последняя ширина окна (для отслеживания изменения)
		private static int lastHeight;   // Последняя высота окна

		//========================================================= Основной метод задания 3 ================================================================//
		public static void Run()
		{
			lastWidth = Console.WindowWidth;               // Запоминаем начальную ширину
			lastHeight = Console.WindowHeight;             // Запоминаем начальную высоту

			RedrawScreen();                                 // Очищаем экран и выводим заголовок

			Console.ForegroundColor = ConsoleColor.Cyan;   // Голубой цвет для инструкции
			if (Program.SupportsEmoji)                     // Если поддерживаются эмодзи
				Console.WriteLine("Введите действительные числа (через пробел или запятую):"); // Полная инструкция
			else
				Console.WriteLine("Введите числа (через пробел):"); // Упрощённая инструкция
			Console.ResetColor();                           // Сброс цвета

			List<double> numbers = null;                    // Список для введённых чисел
			while (true)                                    // Цикл ввода чисел с валидацией
			{
				CheckResizeAndRedraw();                     // Проверяем изменение размера окна
				Console.Write("Числа (Enter – Назад): "); // Приглашение ввода
				string inputLine = Console.ReadLine();       // Считываем строку
				if (string.IsNullOrEmpty(inputLine))        // Если пустая строка или Esc (ввод null) – выход
					return;
				string[] parts = inputLine.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries); // Разделяем на части
				numbers = new List<double>();               // Создаём новый список
				foreach (string p in parts)                 // Парсинг каждого числа из строки
				{
					string normalized = p.Replace(',', '.'); // Заменяем запятую на точку
					if (double.TryParse(normalized, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double val)) // Парсим с инвариантной культурой
						numbers.Add(val);                    // Добавляем число в список
				}

				if (numbers.Count >= 3)                     // Если чисел >= 3 – выходим из цикла
					break;
				else                                         // Если меньше 3
				{
					Console.ForegroundColor = ConsoleColor.Red; // Красный цвет для ошибки
					if (Program.SupportsEmoji)               // Если поддерживаются эмодзи
						Console.WriteLine("❌ Недостаточно чисел (нужно минимум 3). Попробуйте снова."); // С эмодзи
					else
						Console.WriteLine("Недостаточно чисел (нужно минимум 3). Попробуйте снова."); // Без эмодзи
					Console.ResetColor();                    // Сброс цвета
				}
			}

			CheckResizeAndRedraw();                          // Проверка размера перед выбором реализации
			Console.ForegroundColor = ConsoleColor.Green;   // Зелёный цвет для заголовка выбора
			Console.WriteLine($"{(Program.SupportsEmoji ? "▶ " : " ")} Выберите реализацию:");    // Заголовок
			Console.ResetColor();                            // Сброс цвета
			Console.WriteLine("  1 – Собственная реализация"); // Пункт 1
			Console.WriteLine("  2 – LinkedList<T>");        // Пункт 2
			Console.ForegroundColor = ConsoleColor.DarkGray; // Тёмно-серый для подсказки
			Console.WriteLine("  (или ESC/Enter – возврат)"); // Подсказка о выходе
			Console.ResetColor();                            // Сброс цвета
			Console.Write("Ваш выбор: ");                    // Приглашение
			string choice = Console.ReadLine();              // Считываем выбор
			if (string.IsNullOrEmpty(choice))                // Если пусто – возврат
				return;
			bool useCustom = (choice == "1");                // true – собственная реализация, false – LinkedList<T>

			Console.WriteLine();                             // Пустая строка
			Console.ForegroundColor = ConsoleColor.Cyan;     // Голубой цвет для разделителя
			Console.WriteLine("─" + new string('─', 40) + "─"); // Разделитель
			Console.ResetColor();                            // Сброс цвета

			if (useCustom)                                   // Если выбрана собственная реализация
			{
				MyLinkedList<double> list = new MyLinkedList<double>(); // Создаём экземпляр списка
				foreach (double n in numbers) list.Add(n);   // Заполнение списка числами
				Console.ForegroundColor = ConsoleColor.Cyan; // Голубой для подписи
				Console.Write("Исходный список: ");          // Подпись исходного списка
				Console.ResetColor();                        // Сброс цвета
				Console.WriteLine(string.Join(" → ", list.ToList())); // Вывод списка через стрелки

				if (list.MoveThirdToFront())                 // Если перенос удался
				{
					Console.ForegroundColor = ConsoleColor.Green; // Зелёный для результата
					Console.Write("После переноса:   ");      // Подпись результата
					Console.ResetColor();                    // Сброс цвета
					Console.WriteLine(string.Join(" → ", list.ToList())); // Вывод изменённого списка
				}
				else                                         // Если перенос невозможен
				{
					Console.ForegroundColor = ConsoleColor.Red; // Красный цвет для ошибки
					Console.WriteLine("Операция невозможна (меньше 3 элементов)."); // Сообщение
					Console.ResetColor();                    // Сброс цвета
				}
			}
			else                                             // Если выбрана реализация LinkedList<T>
			{
				LinkedList<double> list = new LinkedList<double>(); // Создаём встроенный LinkedList
				foreach (double n in numbers) list.AddLast(n); // Заполнение
				Console.ForegroundColor = ConsoleColor.Cyan; // Голубой для подписи
				Console.Write("Исходный список: ");          // Подпись
				Console.ResetColor();                        // Сброс цвета
				Console.WriteLine(string.Join(" → ", list)); // Вывод

				if (list.Count >= 3)                         // Если элементов >=3
				{
					LinkedListNode<double> thirdNode = list.First?.Next?.Next; // Получаем третий узел
					if (thirdNode != null)                   // Если он существует
					{
						double value = thirdNode.Value;      // Сохраняем значение
						list.Remove(thirdNode);              // Удаляем третий узел
						list.AddFirst(value);                // Добавляем его в начало
						Console.ForegroundColor = ConsoleColor.Green; // Зелёный для результата
						Console.Write("После переноса:   "); // Подпись
						Console.ResetColor();                // Сброс цвета
						Console.WriteLine(string.Join(" → ", list)); // Вывод
					}
				}
				else                                         // Если меньше 3
				{
					Console.ForegroundColor = ConsoleColor.Red; // Красный
					Console.WriteLine("Операция невозможна (меньше 3 элементов)."); // Сообщение
					Console.ResetColor();                    // Сброс цвета
				}
			}

			Console.ForegroundColor = ConsoleColor.Cyan;     // Голубой для разделителя
			Console.WriteLine("─" + new string('─', 40) + "─"); // Нижний разделитель
			Console.ResetColor();                            // Сброс цвета
			Console.WriteLine("\nНажмите любую клавишу для возврата (ESC – Назад)..."); // Ожидание
			Console.ReadKey();                               // Ждём клавишу (Escape обрабатывается на уровне вызывающего)
		}

		//========================================================= Перерисовка заголовка экрана ================================================================//
		private static void RedrawScreen()
		{
			Console.Clear();                                 // Очищаем экран
			Console.ForegroundColor = ConsoleColor.Yellow;   // Жёлтый цвет
			if (Program.SupportsEmoji)                       // Если поддерживаются эмодзи
			{
				Console.WriteLine("╔═══════════════════════════════╗"); // Верхняя граница
				Console.WriteLine("║ ЗАДАНИЕ 3: ОДНОСВЯЗНЫЙ СПИСОК ║"); // Заголовок в рамке
				Console.WriteLine("╚═══════════════════════════════╝"); // Нижняя граница
			}
			else
				Console.WriteLine("=== ЗАДАНИЕ 3: ОДНОСВЯЗНЫЙ СПИСОК ==="); // Упрощённый заголовок
			Console.ResetColor();                            // Сброс цвета
			Console.WriteLine();                             // Пустая строка
		}

		//========================================================= Проверка и обработка изменения размера окна ================================================================//
		private static void CheckResizeAndRedraw()
		{
			if (Console.WindowWidth != lastWidth || Console.WindowHeight != lastHeight) // Если размер изменился
			{
				lastWidth = Console.WindowWidth;             // Обновляем ширину
				lastHeight = Console.WindowHeight;           // Обновляем высоту
				RedrawScreen();                              // Перерисовываем заголовок
				Console.WriteLine("Размер окна изменён, экран обновлён."); // Сообщение
			}
		}
	}
}