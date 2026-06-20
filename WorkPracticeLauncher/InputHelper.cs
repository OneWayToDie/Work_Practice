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
				// Перерисовываем строку ввода + подчёркивание
				Console.SetCursorPosition(_cursorCol, _cursorRow);
				Console.Write(new string(' ', input.Length + 1)); // затираем
				Console.SetCursorPosition(_cursorCol, _cursorRow);
				if (input.Length > 0)
				{
					Console.Write(input.ToString());
				}
				// Мигающее подчёркивание
				if ((DateTime.Now - _lastToggle).TotalMilliseconds >= _blinkInterval)
				{
					_cursorVisible = !_cursorVisible;
					_lastToggle = DateTime.Now;
				}
				Console.Write(_cursorVisible ? '_' : ' ');

				if (Console.KeyAvailable)
				{
					var keyInfo = Console.ReadKey(true);
					var key = keyInfo.Key;

					if (key == ConsoleKey.Enter)
					{
						Console.SetCursorPosition(_cursorCol, _cursorRow);
						Console.Write(new string(' ', input.Length + 1));
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
							// Перерисовка на следующем цикле
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
						// Перерисовка на следующем цикле
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