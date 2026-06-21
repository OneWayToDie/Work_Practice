using System;
using System.Text;
using System.Threading;

namespace WorkPracticeLauncher
{
	public static class InputHelper
	{
		private static int _cursorCol;
		private static int _cursorRow;
		private static bool _cursorVisible = true;
		private static DateTime _lastToggle = DateTime.Now;
		private static int _blinkInterval = 500;

		public static string ReadLine(string prompt = "", bool allowEscape = false)
		{
			if (!string.IsNullOrEmpty(prompt))
			{
				Console.Write(prompt);
			}

			_cursorCol = Console.CursorLeft;
			_cursorRow = Console.CursorTop;

			Console.CursorVisible = false;

			var input = new StringBuilder();
			bool escapePressed = false;

			while (true)
			{
				// Обновляем мигание подчёркивания
				if ((DateTime.Now - _lastToggle).TotalMilliseconds >= _blinkInterval)
				{
					_cursorVisible = !_cursorVisible;
					_lastToggle = DateTime.Now;
				}

				// Перерисовываем строку ввода: сначала затираем старую строку (буфер + подчёркивание)
				Console.SetCursorPosition(_cursorCol, _cursorRow);
				// Затираем всю область: длина буфера + 1 (подчёркивание) + запас (на случай, если что-то осталось)
				int currentLen = input.Length;
				Console.Write(new string(' ', currentLen + 2));
				Console.SetCursorPosition(_cursorCol, _cursorRow);

				// Выводим текущий ввод
				if (currentLen > 0)
				{
					Console.Write(input.ToString());
				}

				// Выводим мигающее подчёркивание
				Console.Write(_cursorVisible ? '_' : ' ');

				// Проверяем клавиши
				if (Console.KeyAvailable)
				{
					var keyInfo = Console.ReadKey(true);
					var key = keyInfo.Key;

					if (key == ConsoleKey.Enter)
					{
						// Затираем подчёркивание перед завершением
						Console.SetCursorPosition(_cursorCol + input.Length, _cursorRow);
						Console.Write(' ');
						Console.SetCursorPosition(_cursorCol + input.Length, _cursorRow);
						Console.CursorVisible = true;
						Console.WriteLine();
						return input.ToString();
					}
					else if (key == ConsoleKey.Backspace)
					{
						if (input.Length > 0)
						{
							input.Remove(input.Length - 1, 1);
							// Перерисовка произойдёт на следующем цикле
						}
					}
					else if (key == ConsoleKey.Escape && allowEscape)
					{
						escapePressed = true;
						break;
					}
					else if (!char.IsControl(keyInfo.KeyChar))
					{
						input.Append(keyInfo.KeyChar);
						// Перерисовка произойдёт на следующем цикле
					}
				}

				Thread.Sleep(30);
			}

			Console.CursorVisible = true;
			if (escapePressed)
				return null;
			return input.ToString();
		}
	}
}