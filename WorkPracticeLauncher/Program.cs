using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using WorkPracticeLauncher.Tasks;

namespace WorkPracticeLauncher
{
	class Program
	{
		private static int inputRow = 10;
		private static int inputCol = "Ваш выбор: ".Length;
		private static string inputBuffer = "";
		private static bool cancelAnimation = false;

		static void Main()
		{
			Console.OutputEncoding = Encoding.UTF8;
			Console.CursorVisible = false;
			Console.Clear();

			var cat = new CatAnimation();

			RedrawScreen(cat);
			SetCursorToInput();

			bool running = true;

			while (running)
			{
				cat.UpdateSleep();
				SetCursorToInput();

				if (Console.KeyAvailable)
				{
					var keyInfo = Console.ReadKey(true);
					var key = keyInfo.Key;
					char keyChar = keyInfo.KeyChar;

					if (key >= ConsoleKey.D0 && key <= ConsoleKey.D4 && inputBuffer.Length == 0)
					{
						inputBuffer = keyChar.ToString();
						SetCursorToInput();
						Console.Write(keyChar);

						cancelAnimation = false;
						Thread animThread = new Thread(() =>
						{
							cat.WakeUpWithCancel(() => cancelAnimation);
						});
						animThread.Start();

						while (animThread.IsAlive)
						{
							if (Console.KeyAvailable)
							{
								var checkKey = Console.ReadKey(true).Key;
								if (checkKey == ConsoleKey.Enter)
								{
									cancelAnimation = true;
									break;
								}
							}
							Thread.Sleep(10);
						}
						animThread.Join(100);

						if (cancelAnimation)
						{
							while (Console.KeyAvailable) Console.ReadKey(true);

							int choice = int.Parse(inputBuffer);
							inputBuffer = "";

							Console.SetCursorPosition(inputCol, inputRow);
							Console.Write(" ");
							SetCursorToInput();

							Console.Clear();
							ExecuteChoice(choice);
							if (choice == 0)
								running = false;
							else
							{
								RedrawScreen(cat);
								SetCursorToInput();
							}
							continue;
						}
						else
						{
							SetCursorToInput();
						}
					}
					else if (key == ConsoleKey.Enter && !string.IsNullOrEmpty(inputBuffer))
					{
						int choice = int.Parse(inputBuffer);
						inputBuffer = "";

						Console.SetCursorPosition(inputCol, inputRow);
						Console.Write(" ");
						SetCursorToInput();

						while (Console.KeyAvailable) Console.ReadKey(true);

						Console.Clear();
						ExecuteChoice(choice);
						if (choice == 0)
							running = false;
						else
						{
							RedrawScreen(cat);
							SetCursorToInput();
						}
					}
					else if (key == ConsoleKey.Escape && !string.IsNullOrEmpty(inputBuffer))
					{
						inputBuffer = "";
						Console.SetCursorPosition(inputCol, inputRow);
						Console.Write(" ");
						SetCursorToInput();
						while (Console.KeyAvailable) Console.ReadKey(true);
					}
					else
					{
						cat.WakeUpWithCancel(() => false);
						SetCursorToInput();
						while (Console.KeyAvailable) Console.ReadKey(true);
					}
				}

				Thread.Sleep(30);
			}

			Console.CursorVisible = true;
			Console.SetCursorPosition(0, Console.WindowHeight - 2);
			Console.WriteLine("До свидания!");
			Console.ReadKey();
		}

		static void RedrawScreen(CatAnimation cat)
		{
			Console.Clear();
			PrintMenu();
			cat.ShowSleepFrame();
		}

		static void PrintMenu()
		{
			Console.SetCursorPosition(0, 0);
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("========================================");
			Console.WriteLine("   Учебная практика – Прикладная информатика");
			Console.WriteLine("   Версия 1.0 (лаунчер)");
			Console.WriteLine("========================================");
			Console.ResetColor();

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("Выберите действие:");
			Console.ResetColor();
			Console.WriteLine("  1 – Запустить WPF приложение");
			Console.WriteLine("  2 – Просмотр решения в консоли (интерактивно)");
			Console.WriteLine("  3 – Скачать актуальную версию (GitHub)");
			Console.WriteLine("  4 – Связь с автором");
			Console.WriteLine("  0 – Выход");
			Console.Write("Ваш выбор: ");
		}

		static void SetCursorToInput()
		{
			int row = inputRow;
			int col = inputCol;
			if (row >= Console.WindowHeight) row = Console.WindowHeight - 1;
			if (col >= Console.WindowWidth) col = Console.WindowWidth - 1;
			Console.SetCursorPosition(col, row);
		}

		static void ExecuteChoice(int choice)
		{
			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("Вы выбрали пункт " + choice);
			Console.ResetColor();

			switch (choice)
			{
				case 1:
					LaunchWpfApp();
					break;
				case 2:
					RunInteractiveMode();
					break;
				case 3:
					DownloadLatestVersion();
					break;
				case 4:
					ShowContacts();
					break;
				case 0:
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("Выход...");
					Console.ResetColor();
					break;
				default:
					break;
			}

			Console.WriteLine("\nНажмите любую клавишу для возврата...");
			Console.ReadKey(true);
		}

		static void RunInteractiveMode()
		{
			while (true)
			{
				Console.Clear();
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.WriteLine("========================================");
				Console.WriteLine("   ИНТЕРАКТИВНЫЙ ПРОСМОТР РЕШЕНИЙ");
				Console.WriteLine("========================================");
				Console.ResetColor();
				Console.WriteLine("Выберите задание:");
				Console.WriteLine("  1 – Задание 1 (числа, процедура/функция)");
				Console.WriteLine("  2 – Задание 2 (товары, сортировка)");
				Console.WriteLine("  3 – Задание 3 (односвязный список)");
				Console.WriteLine("  0 – Назад в главное меню");
				Console.Write("Ваш выбор: ");

				string input = Console.ReadLine();
				if (!int.TryParse(input, out int choice))
				{
					Console.WriteLine("Некорректный ввод. Нажмите любую клавишу...");
					Console.ReadKey();
					continue;
				}

				switch (choice)
				{
					case 1:
						Task1Solver.Run();
						break;
					case 2:
						Task2Solver.Run();
						break;
					case 3:
						Task3Solver.Run();
						break;
					case 0:
						return;
					default:
						Console.WriteLine("Некорректный выбор. Нажмите любую клавишу...");
						Console.ReadKey();
						break;
				}
			}
		}

		static void LaunchWpfApp()
		{
			string wpfExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SummerPractice.exe");
			if (!File.Exists(wpfExePath))
			{
				wpfExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "SummerPractice.exe");
				if (!File.Exists(wpfExePath))
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Не удалось найти SummerPractice.exe. Проверьте, что он находится в той же папке, что и лаунчер.");
					Console.ResetColor();
					return;
				}
			}

			try
			{
				Process.Start(wpfExePath);
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("WPF приложение запущено.");
				Console.ResetColor();
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Ошибка запуска: {ex.Message}");
				Console.ResetColor();
			}
		}

		static void DownloadLatestVersion()
		{
			string repoUrl = "https://github.com/ваш-логин/SummerPractice";
			try
			{
				Process.Start(new ProcessStartInfo(repoUrl) { UseShellExecute = true });
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine($"Открыт репозиторий: {repoUrl}");
				Console.ResetColor();
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Не удалось открыть ссылку: {ex.Message}");
				Console.ResetColor();
			}
		}

		static void ShowContacts()
		{
			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.WriteLine("=== Контакты ===");
			Console.WriteLine("Telegram: https://t.me/ваш_ник");
			Console.WriteLine("Email: ваша_почта@example.com");
			Console.ResetColor();

			Console.WriteLine("Открыть Telegram? (y/n)");
			if (Console.ReadKey().KeyChar == 'y')
			{
				try
				{
					Process.Start(new ProcessStartInfo("https://t.me/ваш_ник") { UseShellExecute = true });
				}
				catch { }
			}
			Console.WriteLine();
			Console.WriteLine("Открыть почтовый клиент? (y/n)");
			if (Console.ReadKey().KeyChar == 'y')
			{
				try
				{
					Process.Start(new ProcessStartInfo("mailto:ваша_почта@example.com") { UseShellExecute = true });
				}
				catch { }
			}
			Console.WriteLine();
		}
	}
}