using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.IO.Compression;
using WorkPracticeLauncher.Tasks;

namespace WorkPracticeLauncher
{
	class Program
	{
		private static int inputRow = 10;
		private static int inputCol = "Ваш выбор: ".Length;
		private static string inputBuffer = "";
		private static bool cancelAnimation = false;

		private static bool showCursorState = true;
		private static DateTime lastCursorToggle = DateTime.Now;
		private static readonly int cursorBlinkInterval = 500;

		private static int lastWidth;
		private static int lastHeight;

		static void Main()
		{
			Console.OutputEncoding = Encoding.UTF8;
			Console.Clear();

			SetConsoleSize();

			lastWidth = Console.WindowWidth;
			lastHeight = Console.WindowHeight;

			ShowTerminalRecommendation();

			var cat = new CatAnimation();

			RedrawScreen(cat);
			ResetCursorState();

			bool running = true;

			while (running)
			{
				if (Console.WindowWidth != lastWidth || Console.WindowHeight != lastHeight)
				{
					lastWidth = Console.WindowWidth;
					lastHeight = Console.WindowHeight;
					RedrawScreen(cat);
					ResetCursorState();
				}

				cat.UpdateSleep();
				DrawCursor();

				if (Console.KeyAvailable)
				{
					var keyInfo = Console.ReadKey(true);
					var key = keyInfo.Key;
					char keyChar = keyInfo.KeyChar;

					bool isValidKey = (key >= ConsoleKey.D0 && key <= ConsoleKey.D4) ||
									  (key == ConsoleKey.Enter) ||
									  (key == ConsoleKey.Backspace) ||
									  (key == ConsoleKey.Escape);

					if (!isValidKey) continue;

					if (key >= ConsoleKey.D0 && key <= ConsoleKey.D4)
					{
						if (inputBuffer.Length > 0)
						{
							Console.SetCursorPosition(inputCol, inputRow);
							Console.Write(new string(' ', inputBuffer.Length + 1));
							inputBuffer = "";
							SetCursorToInput();
						}

						inputBuffer = keyChar.ToString();
						SetCursorToInput();
						Console.Write(keyChar);

						cancelAnimation = false;
						Thread animThread = new Thread(() =>
						{
							cat.WakeUpWithCancel(() => cancelAnimation);
						});
						animThread.Start();

						bool cancelled = false;
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
								else if (checkKey == ConsoleKey.Backspace || checkKey == ConsoleKey.Escape)
								{
									cancelAnimation = true;
									cancelled = true;
									break;
								}
							}
							Thread.Sleep(10);
						}
						animThread.Join(100);

						while (Console.KeyAvailable) Console.ReadKey(true);

						if (cancelled)
						{
							Console.SetCursorPosition(inputCol, inputRow);
							Console.Write(new string(' ', inputBuffer.Length + 1));
							inputBuffer = "";
							SetCursorToInput();
							RedrawScreen(cat);
							ResetCursorState();
							continue;
						}

						if (cancelAnimation)
						{
							int choice = int.Parse(inputBuffer);
							inputBuffer = "";
							Console.SetCursorPosition(inputCol, inputRow);
							Console.Write(new string(' ', 2));
							SetCursorToInput();
							Console.Clear();
							ExecuteChoice(choice);
							if (choice == 0)
								running = false;
							else
							{
								RedrawScreen(cat);
								ResetCursorState();
							}
							continue;
						}
						else
						{
							string digit = inputBuffer;
							inputBuffer = "";
							RedrawScreen(cat);
							SetCursorToInput();
							if (!string.IsNullOrEmpty(digit))
							{
								inputBuffer = digit;
								Console.Write(digit);
							}
							ResetCursorState();
						}
					}
					else if (key == ConsoleKey.Enter && !string.IsNullOrEmpty(inputBuffer))
					{
						int choice = int.Parse(inputBuffer);
						inputBuffer = "";
						Console.SetCursorPosition(inputCol, inputRow);
						Console.Write(new string(' ', 2));
						SetCursorToInput();
						while (Console.KeyAvailable) Console.ReadKey(true);
						Console.Clear();
						ExecuteChoice(choice);
						if (choice == 0)
							running = false;
						else
						{
							RedrawScreen(cat);
							ResetCursorState();
						}
					}
					else if (key == ConsoleKey.Backspace && !string.IsNullOrEmpty(inputBuffer))
					{
						inputBuffer = "";
						Console.SetCursorPosition(inputCol, inputRow);
						Console.Write(new string(' ', 2));
						SetCursorToInput();
						while (Console.KeyAvailable) Console.ReadKey(true);
					}
					else if (key == ConsoleKey.Escape && !string.IsNullOrEmpty(inputBuffer))
					{
						inputBuffer = "";
						Console.SetCursorPosition(inputCol, inputRow);
						Console.Write(new string(' ', 2));
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

		// ---- Вспомогательные методы для адаптации ----

		private static void SetConsoleSize()
		{
			try
			{
				int width = 120;
				int height = 40;
				if (Console.LargestWindowWidth >= width && Console.LargestWindowHeight >= height)
				{
					Console.SetBufferSize(width, height);
					Console.SetWindowSize(width, height);
				}
				else
				{
					int maxW = Math.Min(Console.LargestWindowWidth, 120);
					int maxH = Math.Min(Console.LargestWindowHeight, 40);
					if (maxW >= 80 && maxH >= 25)
					{
						Console.SetBufferSize(maxW, maxH);
						Console.SetWindowSize(maxW, maxH);
					}
				}
			}
			catch { /* Игнорируем ошибки */ }
		}

		private static void ShowTerminalRecommendation()
		{
			int w = Console.WindowWidth;
			string msg = "Рекомендуемый размер окна: 120x40. При изменении размера содержимое будет перерисовано автоматически.";
			if (msg.Length > w - 4) msg = msg.Substring(0, w - 4);
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine(AlignCenter(msg, w));
			Console.ResetColor();
			Console.WriteLine();
		}

		private static void PrintFooter()
		{
			int w = Console.WindowWidth;
			string msg = "⚠️ Для корректной работы рекомендуется использовать Windows Terminal!";
			if (msg.Length > w - 2) msg = msg.Substring(0, w - 2);
			int row = Console.WindowHeight - 1;
			if (row < 0) row = 0;
			// Выравниваем вправо
			int left = w - msg.Length - 1;
			if (left < 0) left = 0;
			Console.SetCursorPosition(left, row);
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write(msg);
			Console.ResetColor();
		}

		private static string AlignCenter(string text, int width)
		{
			if (text.Length >= width) return text;
			int pad = (width - text.Length) / 2;
			return new string(' ', pad) + text + new string(' ', width - text.Length - pad);
		}

		// ---- Методы работы с курсором и анимацией ----

		private static void DrawCursor()
		{
			int row = inputRow;
			int col = inputCol;
			int len = inputBuffer.Length;
			if (row >= Console.WindowHeight) row = Console.WindowHeight - 1;
			if (col + len >= Console.WindowWidth) len = Console.WindowWidth - col - 1;

			Console.SetCursorPosition(col, row);
			Console.Write(new string(' ', len + 1));
			Console.SetCursorPosition(col, row);
			if (!string.IsNullOrEmpty(inputBuffer))
			{
				string display = inputBuffer;
				if (display.Length > len) display = display.Substring(0, len);
				Console.Write(display);
			}
			if ((DateTime.Now - lastCursorToggle).TotalMilliseconds >= cursorBlinkInterval)
			{
				showCursorState = !showCursorState;
				lastCursorToggle = DateTime.Now;
			}
			Console.Write(showCursorState ? '_' : ' ');
		}

		private static void SetCursorToInput()
		{
			int offset = inputBuffer.Length;
			int col = inputCol + offset;
			int row = inputRow;
			if (col >= Console.WindowWidth) col = Console.WindowWidth - 1;
			if (row >= Console.WindowHeight) row = Console.WindowHeight - 1;
			if (col < 0) col = 0;
			if (row < 0) row = 0;
			Console.SetCursorPosition(col, row);
		}

		private static void ResetCursorState()
		{
			Console.CursorVisible = false;
			showCursorState = true;
			lastCursorToggle = DateTime.Now;
			SetCursorToInput();
			DrawCursor();
		}

		static void RedrawScreen(CatAnimation cat)
		{
			Console.Clear();
			PrintMenu();
			cat.ShowSleepFrame();
			PrintFooter();
		}

		static void PrintMenu()
		{
			int w = Console.WindowWidth;
			string line = new string('=', w);
			Console.SetCursorPosition(0, 0);
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine(line);
			Console.WriteLine(AlignCenter("Учебная практика – Прикладная информатика", w));
			Console.WriteLine(AlignCenter("Версия 1.0 (лаунчер)", w));
			Console.WriteLine(line);
			Console.ResetColor();

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("Выберите действие:");
			Console.ResetColor();
			Console.WriteLine("1 – Запустить WPF приложение");
			Console.WriteLine("2 – Просмотр решения в консоли (интерактивно)");
			Console.WriteLine("3 – Скачать актуальную версию (GitHub)");
			Console.WriteLine("4 – Связь с автором");
			Console.WriteLine("0 – Выход");
			Console.Write("Ваш выбор: ");
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
					DownloadLatestVersion(); // вместо DownloadLatestVersion()
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

				Console.ForegroundColor = ConsoleColor.Yellow;
				string title = " ИНТЕРАКТИВНЫЙ ПРОСМОТР РЕШЕНИЙ ";
				int width = title.Length;
				string top = "╔" + new string('═', width) + "╗";
				string middle = "║" + title + "║";
				string bottom = "╚" + new string('═', width) + "╝";
				Console.WriteLine(top);
				Console.WriteLine(middle);
				Console.WriteLine(bottom);
				Console.ResetColor();
				Console.WriteLine();

				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine(" 1 – 🔢 Задание 1 (последовательность чисел, процедура/функция)");
				Console.ForegroundColor = ConsoleColor.Blue;
				Console.WriteLine(" 2 – 📦 Задание 2 (товары, сортировка)");
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(" 3 – 🔗 Задание 3 (односвязный список)");
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine(" 0 – ← Назад в главное меню");
				Console.ResetColor();
				Console.WriteLine();

				string input = InputHelper.ReadLine("Ваш выбор: ");
				if (!int.TryParse(input, out int choice))
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Некорректный ввод. Нажмите любую клавишу...");
					Console.ResetColor();
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
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("Некорректный выбор. Нажмите любую клавишу...");
						Console.ResetColor();
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
			while (true)
			{
				Console.Clear();
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("=== СКАЧИВАНИЕ АКТУАЛЬНОЙ ВЕРСИИ ===");
				Console.ResetColor();
				Console.WriteLine("  1 – Перейти на GitHub (релизы)");
				Console.WriteLine("  2 – Скачать с Google Drive (автоматически)");
				Console.WriteLine("  0 – Назад");
				Console.Write("Ваш выбор (или Esc для отмены): ");

				string choice = InputHelper.ReadLine("", allowEscape: true);
				if (choice == null) return;

				if (choice == "1")
				{
					// Открываем репозиторий в браузере
					string repoUrl = "https://github.com/OneWayToDie/Work_Practice";
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
					Console.WriteLine("Нажмите любую клавишу...");
					Console.ReadKey();
					continue;
				}
				else if (choice == "2")
				{
					DownloadFromDrive();
					continue;
				}
				else if (choice == "0")
				{
					return;
				}
				else
				{
					Console.WriteLine("Некорректный выбор.");
					Console.ReadKey();
				}
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


		static void DownloadFromDrive()
		{
			string fileId = "18mXjs8BIOmtAF1VyQer-e5B2r8fy6TGJ";
			string downloadUrl = $"https://drive.google.com/uc?export=download&id={fileId}";
			string zipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SummerPractice_latest.zip");

			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("Начинается скачивание обновления...");
			Console.ResetColor();

			using (var client = new HttpClient())
			{
				try
				{
					using (var response = client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead).Result)
					{
						response.EnsureSuccessStatusCode();
						long totalBytes = response.Content.Headers.ContentLength ?? -1;
						using (var stream = response.Content.ReadAsStreamAsync().Result)
						using (var fileStream = File.Create(zipPath))
						{
							byte[] buffer = new byte[8192];
							long bytesRead = 0;
							int bytesInBuffer;
							int lastPercent = -1;
							int cursorTop = Console.CursorTop;
							int cursorLeft = 0;

							while ((bytesInBuffer = stream.Read(buffer, 0, buffer.Length)) > 0)
							{
								fileStream.Write(buffer, 0, bytesInBuffer);
								bytesRead += bytesInBuffer;
								if (totalBytes > 0)
								{
									int percent = (int)((double)bytesRead / totalBytes * 100);
									if (percent != lastPercent)
									{
										lastPercent = percent;
										Console.SetCursorPosition(cursorLeft, cursorTop);
										int barWidth = 40;
										int filled = (int)((double)percent / 100 * barWidth);
										string bar = new string('█', filled) + new string('░', barWidth - filled);
										Console.ForegroundColor = ConsoleColor.Green;
										Console.Write($"Скачивание: [{bar}] {percent}%");
										Console.ResetColor();
									}
								}
								else
								{
									Console.SetCursorPosition(cursorLeft, cursorTop);
									Console.Write($"Скачивание: {bytesRead / 1024} KB");
								}
							}
							Console.WriteLine();
						}
					}

					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("Распаковка архива...");
					Console.ResetColor();

					// Ручная распаковка с перезаписью
					try
					{
						using (ZipArchive archive = ZipFile.OpenRead(zipPath))
						{
							foreach (ZipArchiveEntry entry in archive.Entries)
							{
								string destinationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, entry.FullName);
								// Если это папка
								if (string.IsNullOrEmpty(entry.Name))
								{
									Directory.CreateDirectory(destinationPath);
									continue;
								}
								// Создаём папку для файла, если её нет
								string directory = Path.GetDirectoryName(destinationPath);
								if (!string.IsNullOrEmpty(directory))
									Directory.CreateDirectory(directory);
								// Извлекаем файл с перезаписью
								entry.ExtractToFile(destinationPath, true);
							}
						}

						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine("Архив успешно распакован. Все файлы обновлены.");
						Console.ResetColor();

						// Удаляем архив
						File.Delete(zipPath);
						Console.WriteLine("Временный архив удалён.");
					}
					catch (Exception ex)
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine($"Ошибка при распаковке: {ex.Message}");
						Console.ResetColor();
						Console.WriteLine("Вы можете распаковать архив вручную из папки с программой.");
					}
				}
				catch (Exception ex)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"Ошибка при скачивании: {ex.Message}");
					Console.ResetColor();
				}
			}

			Console.WriteLine("\nНажмите любую клавишу для возврата...");
			Console.ReadKey();
		}
	}
}