using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.IO.Compression;
using System.Text;
using System.Threading;
using WorkPracticeLauncher.Tasks;

namespace WorkPracticeLauncher
{
	class Program
	{
		private static int inputRow;
		private static int inputCol;
		private static string inputBuffer = "";
		private static bool cancelAnimation = false;

		private static bool showCursorState = true;
		private static DateTime lastCursorToggle = DateTime.Now;
		private static readonly int cursorBlinkInterval = 500;

		private static int lastWidth;
		private static int lastHeight;
		private static bool isWindowsTerminal;
		private static bool supportsEmoji;

		public static bool IsWindowsTerminal { get; private set; }
		public static bool SupportsEmoji => supportsEmoji;

		// Точка входа — инициализация и главный цикл
		static void Main()
		{
			Console.OutputEncoding = Encoding.UTF8;
			Console.Clear();

			isWindowsTerminal = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WT_SESSION"));
			IsWindowsTerminal = isWindowsTerminal;

			supportsEmoji = TestEmojiSupport();

			lastWidth = Console.WindowWidth;
			lastHeight = Console.WindowHeight;

			ShowTerminalRecommendation();

			CatAnimation cat = new CatAnimation();

			RedrawScreen(cat);
			ResetCursorState();

			if (isWindowsTerminal)
			{
				Thread.Sleep(150);
				lastWidth = Console.WindowWidth;
				lastHeight = Console.WindowHeight;
				RedrawScreen(cat);
				ResetCursorState();
			}

			bool running = true;

			// Главный цикл приложения
			while (running)
			{
				if (Console.WindowWidth != lastWidth || Console.WindowHeight != lastHeight)
				{
					lastWidth = Console.WindowWidth;
					lastHeight = Console.WindowHeight;
					// Полная перерисовка без изменения буфера
					RedrawScreen(cat);
					ResetCursorState();
					// Для Windows Terminal дополнительная перерисовка для затирания артефактов
			if (supportsEmoji)
					{
						Thread.Sleep(30);
						RedrawScreen(cat);
						ResetCursorState();
					}
				}

				cat.UpdateSleep();
				DrawCursor();

				if (Console.KeyAvailable)
				{
					ConsoleKeyInfo keyInfo = Console.ReadKey(true);
					ConsoleKey key = keyInfo.Key;
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
						DrawCursor();

						cancelAnimation = false;
						Thread animThread = new Thread(() =>
						{
							cat.WakeUpWithCancel(() => cancelAnimation);
						});
						animThread.Start();

						bool cancelled = false;
						// Ожидание завершения анимации
						while (animThread.IsAlive)
						{
							if (Console.KeyAvailable)
							{
								ConsoleKey checkKey = Console.ReadKey(true).Key;
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
								DrawCursor();
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
					else if (key == ConsoleKey.Escape)
					{
						inputBuffer = "";
						Console.SetCursorPosition(inputCol, inputRow);
						Console.Write(new string(' ', 2));
						SetCursorToInput();
						while (Console.KeyAvailable) Console.ReadKey(true);
						running = false;
					}
				}

				Thread.Sleep(30);
			}

			Console.Clear();

			Console.CursorVisible = true;
		}

		// ---- Вспомогательные методы ----

		// Ожидание нажатия любой клавиши
		private static bool WaitForAnyKey()
		{
			ConsoleKey key = Console.ReadKey(true).Key;
			return key == ConsoleKey.Escape;
		}

		// Проверка поддержки эмодзи в терминале
		private static bool TestEmojiSupport()
		{
			int before = Console.CursorLeft;
			int top = Console.CursorTop;
			Console.Write("⚡");
			int after = Console.CursorLeft;
			Console.SetCursorPosition(before, top);
			Console.Write("  ");
			Console.SetCursorPosition(before, top);
			return (after - before) >= 2;
		}

		// Рекомендация по использованию Windows Terminal
		private static void ShowTerminalRecommendation()
		{
			if (!supportsEmoji)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Для лучшего отображения рекомендуется использовать Windows Terminal.");
				Console.ResetColor();
				Console.WriteLine("Нажмите любую клавишу для продолжения (ESC – отмена)...");
				WaitForAnyKey();
				Console.Clear();
			}
			else
			{
				int w = Console.WindowWidth;
				string msg = "Рекомендуемый размер окна: 120x40. При изменении размера содержимое будет перерисовано автоматически.";
				if (msg.Length > w - 4) msg = msg.Substring(0, w - 4);
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.WriteLine(AlignCenter(msg, w));
				Console.ResetColor();
				Console.WriteLine();
			}
		}

		// Вывод предупреждения в подвале окна
		private static void PrintFooter()
		{
			if (!supportsEmoji)
			{
				int w = Console.WindowWidth;
				string msg = "[!] Для корректной работы рекомендуется использовать Windows Terminal!";
				if (msg.Length > w - 2) msg = msg.Substring(0, w - 2);
				int row = Console.WindowHeight - 1;
				if (row < 0) row = 0;
				int left = w - msg.Length - 1;
				if (left < 0) left = 0;
				Console.SetCursorPosition(left, row);
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Write(msg);
				int remaining = w - left - msg.Length;
				if (remaining > 0)
					Console.Write(new string(' ', remaining));
				Console.ResetColor();
			}
		}

		// Центрирование текста по ширине
		private static string AlignCenter(string text, int width)
		{
			if (width <= 0) return text;
			if (text.Length >= width) return text;
			int pad = (width - text.Length) / 2;
			return new string(' ', pad) + text + new string(' ', width - text.Length - pad);
		}

		// ---- Курсор и анимация ----

		// Отрисовка мигающего курсора ввода
		private static void DrawCursor()
		{
			int row = inputRow;
			int col = inputCol;
			int len = inputBuffer.Length;
			if (row >= Console.WindowHeight) row = Console.WindowHeight - 1;
			if (col + len >= Console.WindowWidth) len = Console.WindowWidth - col - 1;
			if (col < 0) col = 0;
			if (row < 0) row = 0;

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

		// Установка позиции курсора на поле ввода
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

		// Сброс состояния курсора
		private static void ResetCursorState()
		{
			Console.CursorVisible = false;
			showCursorState = true;
			lastCursorToggle = DateTime.Now;
			SetCursorToInput();
			DrawCursor();
		}

		// Полная перерисовка главного экрана
		static void RedrawScreen(CatAnimation cat)
		{
			Console.Clear();
			Console.SetCursorPosition(0, 0);
			PrintMenu();
			cat.ShowSleepFrame();
			PrintFooter();
		}

		// Отрисовка главного меню
		static void PrintMenu()
		{
			int w = Console.WindowWidth;
			if (w < 10)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("Окно слишком узкое. Разверните его для корректного отображения.");
				Console.ResetColor();
				return;
			}

			Console.SetCursorPosition(0, 0);

			if (supportsEmoji)
			{
				int lineWidth = Math.Max(w - 2, 2);
				string topLine = "╔" + new string('═', lineWidth) + "╗";
				string bottomLine = "╚" + new string('═', lineWidth) + "╝";

				Console.ForegroundColor = ConsoleColor.Green;
				WriteLinePadded(topLine, w);
				WriteLinePadded("║" + AlignCenter("Учебная практика – Прикладная информатика", lineWidth) + "║", w);
				WriteLinePadded("║" + AlignCenter("Launcher(V1.0)", lineWidth) + "║", w);
				WriteLinePadded(bottomLine, w);
				Console.ResetColor();

				// Одна пустая строка после рамки – оставляем
				Console.WriteLine();

				Console.ForegroundColor = ConsoleColor.Yellow;
				WriteLinePadded("▶ Выберите действие:", w);
				Console.ResetColor();

				string[] lines = {
			"  ⚡ 1 – Запустить WPF приложение",
			"  💻 2 – Просмотр решения в консоли (интерактивно)",
			"  📦 3 – Скачать актуальную версию (GitHub / Google Drive)",
			"  📧 4 – Связь с автором",
			"  🚪 0 – Выход"
		};
				ConsoleColor[] colors = { ConsoleColor.Cyan, ConsoleColor.Yellow, ConsoleColor.Green, ConsoleColor.Magenta, ConsoleColor.Red };
				// Вывод пунктов меню с эмодзи
				for (int i = 0; i < lines.Length; i++)
				{
					Console.ForegroundColor = colors[i % colors.Length];
					WriteLinePadded(lines[i], w);
				}
				Console.ForegroundColor = ConsoleColor.DarkGray;
				WriteLinePadded("  (или ESC Назад)", w);
				Console.ResetColor();
			}
			else
			{
				// упрощённый вариант без рамок
				Console.ForegroundColor = ConsoleColor.Green;
				WriteLinePadded("=============================================", w);
				WriteLinePadded("   Учебная практика – Прикладная информатика", w);
				WriteLinePadded("             Launcher(V1.0)", w);
				WriteLinePadded("=============================================", w);
				Console.ResetColor();

				Console.WriteLine();

				Console.ForegroundColor = ConsoleColor.Yellow;
				WriteLinePadded("Выберите действие:", w);
				Console.ResetColor();

				WriteLinePadded("  1 – Запустить WPF приложение", w);
				WriteLinePadded("  2 – Просмотр решения в консоли (интерактивно)", w);
				WriteLinePadded("  3 – Скачать актуальную версию (GitHub / Google Drive)", w);
				WriteLinePadded("  4 – Связь с автором", w);
				WriteLinePadded("  0 – Выход", w);
				Console.ForegroundColor = ConsoleColor.DarkGray;
				WriteLinePadded("  (или ESC Назад)", w);
				Console.ResetColor();

			
			}

			
			Console.WriteLine();
			Console.Write("Ваш выбор: ");
			inputCol = Console.CursorLeft;
			inputRow = Console.CursorTop;
			if (inputCol >= w) inputCol = w - 1;
			if (inputRow >= Console.WindowHeight) inputRow = Console.WindowHeight - 1;
		}

		// ---- Вспомогательный метод для вывода строки с полным затиранием ----
		// Вывод строки с полным затиранием
		private static void WriteLinePadded(string text, int width)
		{
			// Если строка пустая, просто выводим пустую строку (перевод)
			if (string.IsNullOrEmpty(text))
			{
				Console.WriteLine();
				return;
			}

			// Получаем текущую позицию курсора (предполагаем, что он в начале строки)
			int top = Console.CursorTop;
			// Если мы вышли за пределы видимой области, просто выводим как есть
			if (top >= Console.WindowHeight)
			{
				Console.WriteLine(text);
				return;
			}

			// 1. Затираем всю строку пробелами
			Console.SetCursorPosition(0, top);
			Console.Write(new string(' ', width - 1));
			// 2. Возвращаемся в начало строки
			Console.SetCursorPosition(0, top);
			// 3. Выводим текст
			Console.Write(text);
			// 4. Переходим на новую строку
			Console.WriteLine();
		}
				// Обработка выбора пользователя
		static void ExecuteChoice(int choice)
		{
			Console.Clear();

			if (choice != 0)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("Вы выбрали пункт " + choice);
				Console.ResetColor();
			}

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
					return;
				default:
					break;
			}

			Console.WriteLine("\nНажмите любую клавишу для возврата (ESC/Enter – Назад)...");
			WaitForAnyKey();
		}

		// Интерактивный режим выбора задания
		static void RunInteractiveMode()
		{
			while (true)
			{
				Console.Clear();

				if (supportsEmoji)
				{
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
					Console.WriteLine("  🔢 1 – Задание 1 (последовательность чисел, процедура/функция)");
					Console.ForegroundColor = ConsoleColor.Blue;
					Console.WriteLine("  📦 2 – Задание 2 (товары, сортировка)");
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("  🔗 3 – Задание 3 (односвязный список)");
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("  ◀️ 0 – Назад в главное меню");
					Console.ForegroundColor = ConsoleColor.DarkGray;
					Console.WriteLine("  (или Enter – Назад)");
					Console.ResetColor();
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("=== ИНТЕРАКТИВНЫЙ ПРОСМОТР РЕШЕНИЙ ===");
					Console.ResetColor();
					Console.WriteLine();
					Console.WriteLine("  1 – Задание 1 (последовательность чисел, процедура/функция)");
					Console.WriteLine("  2 – Задание 2 (товары, сортировка)");
					Console.WriteLine("  3 – Задание 3 (односвязный список)");
					Console.WriteLine("  0 – Назад в главное меню");
					Console.ForegroundColor = ConsoleColor.DarkGray;
					Console.WriteLine("  (или Enter – Назад)");
					Console.ResetColor();
				}

				Console.WriteLine();
				Console.Write("Ваш выбор (ESC/Enter – Назад): ");
				string input = Console.ReadLine();
				if (string.IsNullOrEmpty(input))
					return;
				if (!int.TryParse(input, out int choice))
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Некорректный ввод. Нажмите любую клавишу (ESC – Назад)...");
					Console.ResetColor();
					WaitForAnyKey();
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
					Console.WriteLine("Некорректный выбор. Нажмите любую клавишу (ESC – Назад)...");
					Console.ResetColor();
					WaitForAnyKey();
						break;
				}
			}
		}

		// Запуск WPF приложения
		static void LaunchWpfApp()
		{
			string wpfExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Work_Practice.exe");
			if (!File.Exists(wpfExePath))
			{
				wpfExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Work_Practice.exe");
				if (!File.Exists(wpfExePath))
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Не удалось найти Work_practice.exe. Проверьте, что он находится в той же папке, что и лаунчер.");
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

		// --------------------------------------------
		// Пункт 3 – Скачать актуальную версию
		// --------------------------------------------

		// Меню скачивания обновлений
		static void DownloadLatestVersion()
		{
			// Цикл меню скачивания
			while (true)
			{
				Console.Clear();
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("=== СКАЧИВАНИЕ АКТУАЛЬНОЙ ВЕРСИИ ===");
				Console.ResetColor();
				Console.WriteLine("  1 – Перейти на GitHub (релизы)");
				Console.WriteLine("  2 – Скачать с Google Drive (автоматически)");
				Console.WriteLine("  0 – Назад");
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.WriteLine("  (или ESC/Enter – Назад)");
				Console.ResetColor();
				Console.Write("Ваш выбор: ");
				string choice = Console.ReadLine();
				if (string.IsNullOrEmpty(choice)) return;
				if (choice == "0") return;

				if (choice == "1")
				{
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
					Console.WriteLine("Нажмите любую клавишу (ESC – Назад)...");
					WaitForAnyKey();
					continue;
				}
				else if (choice == "2")
				{
					DownloadFromDrive();
					continue;
				}
				else
				{
					Console.WriteLine("Некорректный выбор. Нажмите любую клавишу (ESC – Назад)...");
					WaitForAnyKey();
				}
			}
		}

		// Скачивание и распаковка с Google Drive
		static void DownloadFromDrive()
		{
			string fileId = "18mXjs8BIOmtAF1VyQer-e5B2r8fy6TGJ"; // замените на свой ID
			string downloadUrl = $"https://drive.google.com/uc?export=download&id={fileId}";
			string zipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WorkPractice_latest.zip");

			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("Начинается скачивание обновления...");
			Console.ResetColor();

			using (HttpClient client = new HttpClient())
			{
				try
				{
					using (HttpResponseMessage response = client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead).Result)
					{
						response.EnsureSuccessStatusCode();
						long totalBytes = response.Content.Headers.ContentLength ?? -1;
					using (Stream stream = response.Content.ReadAsStreamAsync().Result)
					using (FileStream fileStream = File.Create(zipPath))
						{
							byte[] buffer = new byte[8192];
							long bytesRead = 0;
							int bytesInBuffer;
							int lastPercent = -1;
							int cursorTop = Console.CursorTop;
							int cursorLeft = 0;

							// Чтение данных из потока
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

				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Выберите папку для распаковки:");
				Console.ResetColor();

				string extractPath = null;
				try
				{
					var staThread = new Thread(() =>
						{
							using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
					{
						dialog.Description = "Выберите папку для распаковки обновления";
						dialog.ShowNewFolderButton = true;
						if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
							extractPath = dialog.SelectedPath;
					}
						});
						staThread.SetApartmentState(ApartmentState.STA);
						staThread.Start();
						staThread.Join();
				}
				catch { }

				if (string.IsNullOrEmpty(extractPath))
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("Raspakovka otmenena.");
					Console.ResetColor();
					File.Delete(zipPath);
					return;
				}


					try
					{
						using (ZipArchive archive = ZipFile.OpenRead(zipPath))
						{
							// Распаковка каждого файла
							foreach (ZipArchiveEntry entry in archive.Entries)
							{
								string destinationPath = Path.Combine(extractPath, entry.FullName);
								if (string.IsNullOrEmpty(entry.Name))
								{
									Directory.CreateDirectory(destinationPath);
									continue;
								}
								string directory = Path.GetDirectoryName(destinationPath);
								if (!string.IsNullOrEmpty(directory))
									Directory.CreateDirectory(directory);
								entry.ExtractToFile(destinationPath, true);
							}
						}

						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine("Архив успешно распакован. Все файлы обновлены.");
						Console.ResetColor();

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

			Console.WriteLine("\nНажмите любую клавишу для возврата (ESC – Назад)...");
			WaitForAnyKey();
		}

		// Вывод контактных данных автора
		static void ShowContacts()
		{
			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.WriteLine("=== Контакты ===");
			Console.WriteLine("Telegram: https://t.me/TulskiyGhoul");
			Console.WriteLine("Email: danila.nikiforov.2000@bk.ru");
			Console.ResetColor();

			Console.WriteLine("(нужен VPN) Открыть Telegram? (y/n)");
			if (Console.ReadKey().KeyChar == 'y')
			{
				try
				{
					Process.Start(new ProcessStartInfo("https://t.me/TulskiyGhoul") { UseShellExecute = true });
				}
				catch { }
			}
			Console.WriteLine();
			Console.WriteLine("Открыть почтовый клиент? (y/n)");
			if (Console.ReadKey().KeyChar == 'y')
			{
				try
				{
					Process.Start(new ProcessStartInfo("mailto:danila.nikiforov.2000@bk.ru") { UseShellExecute = true });
				}
				catch { }
			}
			Console.WriteLine();
		}
	}
}