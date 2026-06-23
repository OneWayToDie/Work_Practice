//========================================================= Библиотеки ================================================================//
using System;                     // Базовые типы и консольный ввод-вывод
using System.Diagnostics;         // Запуск процессов (Process.Start)
using System.IO;                  // Работа с файлами и путями (File, Path)
using System.Net.Http;            // HTTP-запросы (HttpClient)
using System.IO.Compression;      // Работа с ZIP-архивами (ZipArchive)
using System.Text;                // Кодировки (Encoding.UTF8)
using System.Threading;           // Потоки (Thread, Thread.Sleep)
using WorkPracticeLauncher.Tasks; // Пространство имён с решениями заданий

namespace WorkPracticeLauncher
{
	//========================================================= Класс Program ================================================================//
	class Program
	{
		//========================================================= Статические поля ================================================================//
		private static int inputRow;                // Координата строки курсора на строке ввода
		private static int inputCol;                // Координата столбца курсора на строке ввода
		private static string inputBuffer = "";     // Буфер для введённой цифры (0-4)
		private static bool cancelAnimation = false;// Флаг прерывания анимации кота
		private static bool showCursorState = true; // Состояние мигающего курсора (видим/скрыт)
		private static DateTime lastCursorToggle = DateTime.Now; // Время последнего переключения курсора
		private static readonly int cursorBlinkInterval = 500;  // Интервал мигания курсора (мс)
		private static int lastWidth;               // Последняя ширина окна консоли
		private static int lastHeight;              // Последняя высота окна консоли
		private static bool isWindowsTerminal;      // Флаг запуска в Windows Terminal
		private static bool supportsEmoji;          // Флаг поддержки эмодзи в терминале
		public static bool IsWindowsTerminal { get; private set; } // Публичный доступ
		public static bool SupportsEmoji => supportsEmoji;         // Публичный доступ

		//========================================================= Точка входа ================================================================//
		static void Main()
		{
			Console.OutputEncoding = Encoding.UTF8;                 // Устанавливаем кодировку UTF-8
			Console.Clear();                                        // Очищаем экран

			isWindowsTerminal = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WT_SESSION")); // Проверяем, запущены ли в Windows Terminal
			IsWindowsTerminal = isWindowsTerminal;                  // Сохраняем в публичное свойство
			supportsEmoji = TestEmojiSupport();                     // Проверяем поддержку эмодзи

			lastWidth = Console.WindowWidth;                        // Запоминаем начальную ширину окна
			lastHeight = Console.WindowHeight;                      // Запоминаем начальную высоту окна

			ShowTerminalRecommendation();                           // Выводим рекомендацию по терминалу

			CatAnimation cat = new CatAnimation();                 // Создаём экземпляр анимации кота

			RedrawScreen(cat);                                      // Первичная отрисовка экрана
			ResetCursorState();                                     // Устанавливаем курсор и рисуем подчёркивание

			if (isWindowsTerminal)                                  // Если Windows Terminal
			{
				Thread.Sleep(150);                                  // Небольшая задержка для стабилизации
				lastWidth = Console.WindowWidth;                    // Обновляем размеры
				lastHeight = Console.WindowHeight;
				RedrawScreen(cat);                                  // Повторная перерисовка
				ResetCursorState();
			}

			bool running = true;                                    // Флаг работы главного цикла

			while (running)                                         // Главный цикл приложения
			{
				if (Console.WindowWidth != lastWidth || Console.WindowHeight != lastHeight) // Если размер изменился
				{
					try
					{
						lastWidth = Console.WindowWidth;            // Обновляем размеры
						lastHeight = Console.WindowHeight;
						RedrawScreen(cat);                          // Перерисовываем экран
						ResetCursorState();                         // Сбрасываем курсор
						if (supportsEmoji)                          // Если поддерживаются эмодзи (дополнительная перерисовка)
						{
							Thread.Sleep(30);
							RedrawScreen(cat);
							ResetCursorState();
						}
					}
					catch (System.IO.IOException) { }               // Игнорируем ошибки ввода-вывода
					catch (ArgumentOutOfRangeException) { }         // Игнорируем ошибки выхода за границы
				}

				cat.UpdateSleep();                                  // Обновляем анимацию сна кота
				DrawCursor();                                       // Рисуем мигающий курсор

				if (Console.KeyAvailable)                           // Если есть нажатая клавиша
				{
					ConsoleKeyInfo keyInfo = Console.ReadKey(true); // Считываем клавишу (без отображения)
					ConsoleKey key = keyInfo.Key;
					char keyChar = keyInfo.KeyChar;

				bool isValidKey = (key >= ConsoleKey.D0 && key <= ConsoleKey.D5) ||
								  (key == ConsoleKey.Enter) ||
								  (key == ConsoleKey.Backspace) ||
								  (key == ConsoleKey.Escape);   // Допустимые клавиши: 0-5, Enter, Backspace, Escape

					if (!isValidKey) continue;                      // Если клавиша недопустима – игнорируем

					if (key >= ConsoleKey.D0 && key <= ConsoleKey.D5) // Если цифра 0-5
					{
						if (inputBuffer.Length > 0)                 // Если уже есть введённая цифра, стираем её
						{
							Console.SetCursorPosition(inputCol, inputRow);
							Console.Write(new string(' ', inputBuffer.Length + 1));
							inputBuffer = "";
							SetCursorToInput();
						}

						inputBuffer = keyChar.ToString();           // Сохраняем цифру в буфер
						SetCursorToInput();                         // Устанавливаем курсор после неё
						DrawCursor();                               // Рисуем подчёркивание

						cancelAnimation = false;                    // Сбрасываем флаг отмены анимации
						Thread animThread = new Thread(() =>        // Запускаем анимацию пробуждения в фоновом потоке
						{
							cat.WakeUpWithCancel(() => cancelAnimation);
						});
						animThread.Start();

						bool cancelled = false;                     // Флаг, была ли анимация отменена

						while (animThread.IsAlive)                  // Ожидаем завершения анимации с возможностью прерывания
						{
							if (Console.KeyAvailable)
							{
								ConsoleKey checkKey = Console.ReadKey(true).Key;
								if (checkKey == ConsoleKey.Enter)   // Enter – подтверждение
								{
									cancelAnimation = true;
									break;
								}
								else if (checkKey == ConsoleKey.Backspace || checkKey == ConsoleKey.Escape) // Отмена
								{
									cancelAnimation = true;
									cancelled = true;
									break;
								}
							}
							Thread.Sleep(10);
						}
						animThread.Join(100);                       // Дожидаемся завершения потока

						while (Console.KeyAvailable) Console.ReadKey(true); // Очищаем буфер ввода

						if (cancelled)                              // Если выбор отменён
						{
							Console.SetCursorPosition(inputCol, inputRow);
							Console.Write(new string(' ', inputBuffer.Length + 1));
							inputBuffer = "";
							SetCursorToInput();
							RedrawScreen(cat);
							ResetCursorState();
							continue;
						}

						if (cancelAnimation)                        // Если анимация прервана по Enter – подтверждаем выбор
						{
							int choice = int.Parse(inputBuffer);    // Преобразуем буфер в число
							inputBuffer = "";
							Console.SetCursorPosition(inputCol, inputRow);
							Console.Write(new string(' ', 2));      // Стираем цифру
							SetCursorToInput();
							Console.Clear();                        // Очищаем экран
							ExecuteChoice(choice);                  // Выполняем выбор
							if (choice == 0)                        // Если выход
								running = false;
							else
							{
								RedrawScreen(cat);
								ResetCursorState();
							}
							continue;
						}
						else                                         // Если анимация завершилась сама
						{
							string digit = inputBuffer;
							inputBuffer = "";
							RedrawScreen(cat);                      // Перерисовываем
							SetCursorToInput();
							if (!string.IsNullOrEmpty(digit))
							{
								inputBuffer = digit;                // Восстанавливаем цифру
								DrawCursor();
							}
							ResetCursorState();
						}
					}
					else if (key == ConsoleKey.Enter && !string.IsNullOrEmpty(inputBuffer)) // Если Enter, а в буфере есть цифра
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
					else if (key == ConsoleKey.Backspace && !string.IsNullOrEmpty(inputBuffer)) // Backspace – очистка
					{
						inputBuffer = "";
						Console.SetCursorPosition(inputCol, inputRow);
						Console.Write(new string(' ', 2));
						SetCursorToInput();
						while (Console.KeyAvailable) Console.ReadKey(true);
					}
					else if (key == ConsoleKey.Escape)              // Escape – выход
					{
						inputBuffer = "";
						Console.SetCursorPosition(inputCol, inputRow);
						Console.Write(new string(' ', 2));
						SetCursorToInput();
						while (Console.KeyAvailable) Console.ReadKey(true);
						running = false;
					}
				}

				Thread.Sleep(30);                                   // Задержка для снижения нагрузки
			}

			Console.Clear();                                        // Очищаем экран при выходе
			Console.CursorVisible = true;                           // Делаем курсор видимым
		}

		//========================================================= Ожидание нажатия клавиши ================================================================//
		private static bool WaitForAnyKey()
		{
			ConsoleKey key = Console.ReadKey(true).Key;             // Считываем клавишу (без отображения)
			return key == ConsoleKey.Escape;                        // Возвращаем true, если нажат Escape, иначе false
		}

		//========================================================= Проверка поддержки эмодзи ================================================================//
		private static bool TestEmojiSupport()
		{
			int before = Console.CursorLeft;                        // Запоминаем позицию курсора до вывода
			int top = Console.CursorTop;                            // Запоминаем строку (не используется, но сохраняем)
			Console.Write("⚡");                                     // Выводим тестовый эмодзи (молния)
			int after = Console.CursorLeft;                         // Запоминаем новую позицию курсора после вывода
			Console.SetCursorPosition(before, top);                 // Возвращаем курсор на исходную позицию
			Console.Write("  ");                                    // Затираем выведенный эмодзи двумя пробелами
			Console.SetCursorPosition(before, top);                 // Снова возвращаем курсор
			return (after - before) >= 2;                           // Если курсор сдвинулся на 2 или более позиций — эмодзи поддерживается
		}

		//========================================================= Рекомендация по использованию Windows Terminal ================================================================//
		private static void ShowTerminalRecommendation()
		{
			if (!supportsEmoji)                                      // Если терминал не поддерживает эмодзи (старая консоль)
			{
				Console.ForegroundColor = ConsoleColor.Red;         // Устанавливаем красный цвет текста
				Console.WriteLine("Для лучшего отображения рекомендуется использовать Windows Terminal."); // Предупреждение
				Console.ResetColor();                               // Сбрасываем цвет
				Console.WriteLine("Нажмите любую клавишу для продолжения (ESC – отмена)..."); // Приглашение для продолжения
				WaitForAnyKey();                                    // Ожидаем нажатия клавиши (Escape завершает программу)
				Console.Clear();                                    // Очищаем экран после продолжения
			}
			else                                                     // Если эмодзи поддерживаются (Windows Terminal)
			{
				int w = Console.WindowWidth;                        // Получаем текущую ширину окна консоли
				string msg = "Рекомендуемый размер окна: 120x40. При изменении размера содержимое будет перерисовано автоматически."; // Текст рекомендации
				if (msg.Length > w - 4) msg = msg.Substring(0, w - 4); // Если сообщение длиннее ширины окна, обрезаем его
				Console.ForegroundColor = ConsoleColor.Cyan;        // Устанавливаем голубой цвет текста
				Console.WriteLine(AlignCenter(msg, w));             // Выводим центрированное сообщение
				Console.ResetColor();                               // Сбрасываем цвет
				Console.WriteLine();                                // Пустая строка для отступа
			}
		}

		//========================================================= Вывод подвала (бренд и предупреждение) ================================================================//
		private static void PrintFooter()
		{
			try
			{
				int w = Console.WindowWidth;                        // Получаем текущую ширину окна
				int row = Console.WindowHeight - 1;                 // Определяем последнюю строку окна (индекс 0-based)
				if (row < 0) row = 0;                               // Если высота окна меньше 1, устанавливаем строку 0

				// Название компании в левом нижнем углу (с ёлочкой, если поддерживается эмодзи)
				string brand = supportsEmoji ? "elki-igolki company\U0001F384" : "elki-igolki company";
				Console.SetCursorPosition(0, row);                  // Перемещаем курсор в левый нижний угол
				Console.ForegroundColor = ConsoleColor.Green;       // Устанавливаем зелёный цвет
				Console.Write(brand);                               // Выводим название
				Console.ResetColor();                               // Сбрасываем цвет

				// Предупреждение в правом нижнем углу (только если эмодзи не поддерживаются – старая консоль)
				if (!supportsEmoji)
				{
					string msg = "[!] Для корректной работы рекомендуется использовать Windows Terminal!"; // Текст предупреждения
					if (msg.Length > w - 2) msg = msg.Substring(0, w - 2); // Обрезаем, если не помещается по ширине
					int left = w - msg.Length - 1;                  // Вычисляем позицию для выравнивания вправо
					if (left < 0) left = 0;                         // Защита от отрицательного смещения
					Console.SetCursorPosition(left, row);           // Перемещаем курсор в правую часть нижней строки
					Console.ForegroundColor = ConsoleColor.Yellow;  // Устанавливаем жёлтый цвет
					Console.Write(msg);                             // Выводим предупреждение
					Console.ResetColor();                           // Сбрасываем цвет
				}
			}
			catch (System.IO.IOException) { }                       // Игнорируем ошибки ввода-вывода
			catch (ArgumentOutOfRangeException) { }                 // Игнорируем ошибки выхода за границы
		}

		//========================================================= Центрирование текста по ширине ================================================================//
		private static string AlignCenter(string text, int width)
		{
			if (width <= 0) return text;                            // Если ширина меньше или равна 0, возвращаем текст как есть
			if (text.Length >= width) return text;                  // Если текст длиннее ширины, возвращаем без изменений
			int pad = (width - text.Length) / 2;                    // Вычисляем количество пробелов для отступа слева
			return new string(' ', pad) + text + new string(' ', width - text.Length - pad); // Возвращаем текст с пробелами слева и справа
		}

		//========================================================= Отрисовка мигающего курсора ввода ================================================================//
		private static void DrawCursor()
		{
			try
			{
				int row = inputRow;                                 // Текущая строка ввода
				int col = inputCol;                                 // Текущий столбец ввода
				int len = inputBuffer.Length;                       // Длина буфера
				if (row >= Console.WindowHeight) row = Console.WindowHeight - 1; // Не выходим за нижнюю границу
				if (col + len >= Console.WindowWidth) len = Console.WindowWidth - col - 1; // Не выходим за правую границу
				if (col < 0) col = 0;                               // Защита от отрицательного столбца
				if (row < 0) row = 0;                               // Защита от отрицательной строки

				Console.SetCursorPosition(col, row);                // Перемещаем курсор на позицию ввода
				Console.Write(new string(' ', len + 1));            // Затираем старую строку (буфер + подчёркивание)
				Console.SetCursorPosition(col, row);                // Возвращаемся в начало строки
				if (!string.IsNullOrEmpty(inputBuffer))             // Если есть введённый текст
				{
					string display = inputBuffer;                   // Берём буфер
					if (display.Length > len) display = display.Substring(0, len); // Обрезаем, если не помещается
					Console.Write(display);                         // Выводим буфер
				}
				if ((DateTime.Now - lastCursorToggle).TotalMilliseconds >= cursorBlinkInterval) // Проверяем, пора ли мигать
				{
					showCursorState = !showCursorState;             // Переключаем состояние видимости
					lastCursorToggle = DateTime.Now;                // Сбрасываем таймер
				}
				Console.Write(showCursorState ? '_' : ' ');         // Рисуем подчёркивание или пробел
			}
			catch (System.IO.IOException) { }                       // Игнорируем ошибки ввода-вывода
			catch (ArgumentOutOfRangeException) { }                 // Игнорируем ошибки выхода за границы
		}

		//========================================================= Установка позиции курсора на поле ввода ================================================================//
		private static void SetCursorToInput()
		{
			try
			{
				int offset = inputBuffer.Length;                    // Количество символов, введённых в буфере
				int col = inputCol + offset;                        // Вычисляем столбец: начальная позиция + длина буфера
				int row = inputRow;                                 // Строка остаётся неизменной (строка ввода)
				if (col >= Console.WindowWidth) col = Console.WindowWidth - 1; // Не выходим за правую границу
				if (row >= Console.WindowHeight) row = Console.WindowHeight - 1; // Не выходим за нижнюю границу
				if (col < 0) col = 0;                               // Защита от отрицательного столбца
				if (row < 0) row = 0;                               // Защита от отрицательной строки
				Console.SetCursorPosition(col, row);                // Перемещаем курсор на рассчитанную позицию
			}
			catch (System.IO.IOException) { }                       // Игнорируем ошибки ввода-вывода
			catch (ArgumentOutOfRangeException) { }                 // Игнорируем ошибки выхода за границы
		}

		//========================================================= Сброс состояния курсора ================================================================//
		private static void ResetCursorState()
		{
			Console.CursorVisible = false;                          // Скрываем стандартный курсор (будем рисовать свой)
			showCursorState = true;                                 // Устанавливаем состояние видимости мигающего курсора (показываем подчёркивание)
			lastCursorToggle = DateTime.Now;                        // Сбрасываем таймер мигания, чтобы курсор сразу отобразился
			SetCursorToInput();                                     // Перемещаем курсор на строку ввода
			DrawCursor();                                           // Рисуем мигающее подчёркивание (или пробел, если буфер пуст)
		}

		//========================================================= Полная перерисовка главного экрана ================================================================//
		static void RedrawScreen(CatAnimation cat)
		{
			try
			{
				Console.Clear();                                    // Очищаем весь экран от предыдущего вывода
				Console.SetCursorPosition(0, 0);                    // Перемещаем курсор в верхний левый угол (начало экрана)
			}
			catch (System.IO.IOException) { }                       // Игнорируем ошибки ввода-вывода
			catch (ArgumentOutOfRangeException) { }                 // Игнорируем ошибки выхода за границы
			PrintMenu();                                            // Выводим главное меню (заголовки, пункты, подсказки)
			cat.ShowSleepFrame();                                   // Отображаем начальный кадр анимации кота (спящий)
			PrintFooter();                                          // Выводим информацию в нижней строке (компания, предупреждение)
		}

		//========================================================= Отрисовка главного меню ================================================================//
		static void PrintMenu()
		{
			int w = Console.WindowWidth;                            // Получаем текущую ширину окна
			if (w < 10)                                              // Если окно слишком узкое
			{
				Console.ForegroundColor = ConsoleColor.Yellow;      // Жёлтый цвет для предупреждения
				Console.WriteLine("Окно слишком узкое. Разверните его для корректного отображения."); // Сообщение
				Console.ResetColor();                               // Сброс цвета
				return;                                             // Выход из метода
			}

			Console.SetCursorPosition(0, 0);                        // Курсор в верхний левый угол

			if (supportsEmoji)                                       // Если терминал поддерживает эмодзи (Windows Terminal)
			{
				int lineWidth = Math.Max(w - 2, 2);                 // Ширина внутренней части рамки
				string topLine = "╔" + new string('═', lineWidth) + "╗"; // Верхняя граница рамки
				string bottomLine = "╚" + new string('═', lineWidth) + "╝"; // Нижняя граница рамки

				Console.ForegroundColor = ConsoleColor.Green;       // Зелёный цвет для рамки
				WriteLinePadded(topLine, w);                        // Выводим верхнюю границу
				WriteLinePadded("║" + AlignCenter("Учебная практика – Прикладная информатика", lineWidth) + "║", w); // Заголовок в рамке
				WriteLinePadded("║" + AlignCenter("Launcher(V1.0)", lineWidth) + "║", w); // Версия в рамке
				WriteLinePadded(bottomLine, w);                     // Нижняя граница рамки
				Console.ResetColor();                               // Сброс цвета

				Console.WriteLine();                                // Пустая строка после рамки

				Console.ForegroundColor = ConsoleColor.Yellow;      // Жёлтый цвет для заголовка действия
				WriteLinePadded("▶ Выберите действие:", w);         // Заголовок меню
				Console.ResetColor();                               // Сброс цвета

				string[] lines = {                                  // Массив пунктов меню с эмодзи
                    "  ⚡ 1 – Запустить WPF приложение",
					"  💻 2 – Просмотр решения в консоли (интерактивно)",
					"  📦 3 – Скачать актуальную версию (GitHub / Google Drive)",
					"  📧 4 – Связь с автором",
					"  📝 5 – Отзывы и предложения",
					"  🚪 0 – Выход"
				};
				ConsoleColor[] colors = { ConsoleColor.Cyan, ConsoleColor.Yellow, ConsoleColor.Green, ConsoleColor.Magenta, ConsoleColor.Red }; // Цвета для пунктов
				for (int i = 0; i < lines.Length; i++)              // Перебираем все пункты меню
				{
					Console.ForegroundColor = colors[i % colors.Length]; // Устанавливаем цвет для текущего пункта
					WriteLinePadded(lines[i], w);                   // Выводим пункт с затиранием до конца строки
				}
				Console.ForegroundColor = ConsoleColor.DarkGray;    // Тёмно-серый цвет для подсказки
				WriteLinePadded("  (или ESC Назад)", w);            // Подсказка о выходе по Escape
				Console.ResetColor();                               // Сброс цвета
			}
			else                                                     // Если эмодзи не поддерживаются (старая консоль)
			{
				Console.ForegroundColor = ConsoleColor.Green;       // Зелёный цвет для упрощённой рамки
				WriteLinePadded("=============================================", w); // Верхняя граница (текстовая)
				WriteLinePadded("   Учебная практика – Прикладная информатика", w); // Заголовок
				WriteLinePadded("             Launcher(V1.0)", w);   // Версия
				WriteLinePadded("=============================================", w); // Нижняя граница
				Console.ResetColor();                               // Сброс цвета

				Console.WriteLine();                                // Пустая строка после рамки

				Console.ForegroundColor = ConsoleColor.Yellow;      // Жёлтый цвет для заголовка
				WriteLinePadded("Выберите действие:", w);           // Заголовок меню
				Console.ResetColor();                               // Сброс цвета

				WriteLinePadded("  1 – Запустить WPF приложение", w); // Пункт 1
				WriteLinePadded("  2 – Просмотр решения в консоли (интерактивно)", w); // Пункт 2
				WriteLinePadded("  3 – Скачать актуальную версию (GitHub / Google Drive)", w); // Пункт 3
				WriteLinePadded("  4 – Связь с автором", w);        // Пункт 4
				WriteLinePadded("  5 – Отзывы и предложения", w);    // Пункт 5
				WriteLinePadded("  0 – Выход", w);                  // Пункт 0
				Console.ForegroundColor = ConsoleColor.DarkGray;    // Тёмно-серый цвет для подсказки
				WriteLinePadded("  (или ESC Назад)", w);            // Подсказка о выходе
				Console.ResetColor();                               // Сброс цвета
			}

			Console.WriteLine();                                    // Пустая строка перед приглашением
			Console.Write("Ваш выбор: ");                           // Вывод приглашения
			inputCol = Console.CursorLeft;                          // Сохраняем позицию курсора (столбец)
			inputRow = Console.CursorTop;                           // Сохраняем позицию курсора (строка)
			if (inputCol >= w) inputCol = w - 1;                    // Если столбец за пределами окна, корректируем
			if (inputRow >= Console.WindowHeight) inputRow = Console.WindowHeight - 1; // Если строка за пределами, корректируем
		}

		//========================================================= Вывод строки с полным затиранием ================================================================//
		private static void WriteLinePadded(string text, int width)
		{
			// Если строка пустая, просто выводим пустую строку (перевод)
			if (string.IsNullOrEmpty(text))
			{
				Console.WriteLine();                                // Переход на новую строку
				return;
			}

			// Получаем текущую позицию курсора (предполагаем, что он в начале строки)
			int top = Console.CursorTop;                            // Текущая строка курсора
																	// Если мы вышли за пределы видимой области, просто выводим как есть
			if (top >= Console.WindowHeight)
			{
				Console.WriteLine(text);                            // Вывод текста без затирания
				return;
			}

			// 1. Затираем всю строку пробелами (ширина-1, чтобы не выходить за правый край)
			Console.SetCursorPosition(0, top);                      // Курсор в начало строки
			Console.Write(new string(' ', width - 1));              // Печатаем пробелы почти на всю ширину
																	// 2. Возвращаемся в начало строки
			Console.SetCursorPosition(0, top);                      // Снова в начало
																	// 3. Выводим текст
			Console.Write(text);                                    // Печатаем сам текст
																	// 4. Переходим на новую строку
			Console.WriteLine();                                    // Перевод строки
		}

		//========================================================= Обработка выбора пользователя ================================================================//
		static void ExecuteChoice(int choice)
		{
			Console.Clear();                                        // Очищаем экран

			if (choice != 0)                                        // Если не пункт "Выход"
			{
				Console.ForegroundColor = ConsoleColor.Yellow;      // Жёлтый цвет для подтверждения
				Console.WriteLine("Вы выбрали пункт " + choice);    // Сообщение о выборе
				Console.ResetColor();                               // Сброс цвета
			}

			switch (choice)                                          // Обработка выбора
			{
				case 1:
					LaunchWpfApp();                                 // Запуск WPF-приложения
					break;
				case 2:
					RunInteractiveMode();                           // Интерактивный режим просмотра заданий
					break;
				case 3:
					DownloadLatestVersion();                        // Скачивание обновлений
					break;
				case 4:
					ShowContacts();                                  // Вывод контактной информации
					break;
				case 5:
					FeedbackManager.Run();                          // Раздел отзывов и предложений
					break;
				case 0:
					Console.ForegroundColor = ConsoleColor.Green;   // Зелёный цвет для выхода
					Console.WriteLine("Выход...");                  // Сообщение о выходе
					Console.ResetColor();
					return;                                          // Выход из метода без ожидания
				default:
					break;
			}

			Console.WriteLine("\nНажмите любую клавишу для возврата (ESC/Enter – Назад)..."); // Приглашение
			WaitForAnyKey();                                         // Ожидание клавиши (Escape – возврат)
		}

		//========================================================= Интерактивный режим выбора задания ================================================================//
		static void RunInteractiveMode()
		{
			while (true)                                              // Бесконечный цикл до выхода
			{
				Console.Clear();                                     // Очищаем экран

				if (supportsEmoji)                                   // Если поддерживаются эмодзи – красивый интерфейс
				{
					Console.ForegroundColor = ConsoleColor.Yellow;  // Жёлтый цвет для рамки
					string title = " ИНТЕРАКТИВНЫЙ ПРОСМОТР РЕШЕНИЙ ";
					int width = title.Length;
					string top = "╔" + new string('═', width) + "╗";
					string middle = "║" + title + "║";
					string bottom = "╚" + new string('═', width) + "╝";
					Console.WriteLine(top);                          // Верхняя граница
					Console.WriteLine(middle);                       // Заголовок
					Console.WriteLine(bottom);                       // Нижняя граница
					Console.ResetColor();
					Console.WriteLine();

					Console.ForegroundColor = ConsoleColor.White;   // Белый для пункта 1
					Console.WriteLine("  🔢 1 – Задание 1 (последовательность чисел, процедура/функция)");
					Console.ForegroundColor = ConsoleColor.Blue;    // Синий для пункта 2
					Console.WriteLine("  📦 2 – Задание 2 (товары, сортировка)");
					Console.ForegroundColor = ConsoleColor.Red;     // Красный для пункта 3
					Console.WriteLine("  🔗 3 – Задание 3 (односвязный список)");
					Console.ForegroundColor = ConsoleColor.Yellow;  // Жёлтый для возврата
					Console.WriteLine("  ◀️ 0 – Назад в главное меню");
					Console.ForegroundColor = ConsoleColor.DarkGray;// Тёмно-серый для подсказки
					Console.WriteLine("  (или Enter – Назад)");
					Console.ResetColor();
				}
				else                                                 // Упрощённый вариант без рамок
				{
					Console.ForegroundColor = ConsoleColor.Yellow;  // Жёлтый заголовок
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

				Console.WriteLine();                                 // Пустая строка перед приглашением
				Console.Write("Ваш выбор (ESC/Enter – Назад): ");   // Приглашение
				string input = Console.ReadLine();                   // Считываем строку
				if (string.IsNullOrEmpty(input))                     // Если пустой ввод – возврат
					return;
				if (!int.TryParse(input, out int choice))            // Если не число
				{
					Console.ForegroundColor = ConsoleColor.Red;     // Красный цвет
					Console.WriteLine("Некорректный ввод. Нажмите любую клавишу (ESC – Назад)...");
					Console.ResetColor();
					WaitForAnyKey();                                 // Ожидание клавиши
					continue;
				}

				switch (choice)                                      // Обработка выбора задания
				{
					case 1:
						Task1Solver.Run();                          // Запуск задания 1
						break;
					case 2:
						Task2Solver.Run();                          // Запуск задания 2
						break;
					case 3:
						Task3Solver.Run();                          // Запуск задания 3
						break;
					case 0:
						return;                                      // Возврат в главное меню
					default:
						Console.ForegroundColor = ConsoleColor.Red; // Красный цвет
						Console.WriteLine("Некорректный выбор. Нажмите любую клавишу (ESC – Назад)...");
						Console.ResetColor();
						WaitForAnyKey();                             // Ожидание клавиши
						break;
				}
			}
		}

		//========================================================= Запуск WPF приложения ================================================================//
		static void LaunchWpfApp()
		{
			// Формируем путь к исполняемому файлу WPF-приложения в текущей папке лаунчера
			string wpfExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Work_Practice.exe");
			// Если файл не найден в текущей папке
			if (!File.Exists(wpfExePath))
			{
				// Пробуем перейти на уровень выше (если лаунчер в подпапке)
				wpfExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Work_Practice.exe");
				if (!File.Exists(wpfExePath))
				{
					// Если файл не найден и там – выводим сообщение об ошибке
					Console.ForegroundColor = ConsoleColor.Red;     // Красный цвет для ошибки
					Console.WriteLine("Не удалось найти Work_practice.exe. Проверьте, что он находится в той же папке, что и лаунчер.");
					Console.ResetColor();                           // Сброс цвета
					return;                                          // Выход из метода
				}
			}

			try
			{
				Process.Start(wpfExePath);                          // Запускаем WPF-приложение
				Console.ForegroundColor = ConsoleColor.Green;       // Зелёный цвет для успеха
				Console.WriteLine("WPF приложение запущено.");      // Сообщение
				Console.ResetColor();                               // Сброс цвета
			}
			catch (Exception ex)                                     // Если возникла ошибка при запуске
			{
				Console.ForegroundColor = ConsoleColor.Red;         // Красный цвет для ошибки
				Console.WriteLine($"Ошибка запуска: {ex.Message}"); // Вывод сообщения об ошибке
				Console.ResetColor();                               // Сброс цвета
			}
		}

		//========================================================= Меню скачивания обновлений ================================================================//
		static void DownloadLatestVersion()
		{
			while (true)                                              // Бесконечный цикл меню
			{
				Console.Clear();                                     // Очищаем экран
				Console.ForegroundColor = ConsoleColor.Yellow;      // Жёлтый заголовок
				Console.WriteLine("=== СКАЧИВАНИЕ АКТУАЛЬНОЙ ВЕРСИИ ===");
				Console.ResetColor();
				Console.WriteLine("  1 – Перейти на GitHub (релизы)"); // Пункт 1
				Console.WriteLine("  2 – Скачать с Google Drive (автоматически)"); // Пункт 2
				Console.WriteLine("  0 – Назад");                    // Пункт 0
				Console.ForegroundColor = ConsoleColor.DarkGray;    // Тёмно-серый для подсказки
				Console.WriteLine("  (или Enter – Назад)");
				Console.ResetColor();
				Console.Write("Ваш выбор: ");                       // Приглашение
				string choice = Console.ReadLine();                  // Считываем выбор
				if (string.IsNullOrEmpty(choice)) return;            // Пустой ввод – возврат
				if (choice == "0") return;                           // Выход по 0

				if (choice == "1")                                   // Пункт 1 – переход на GitHub
				{
					string repoUrl = "https://github.com/OneWayToDie/Work_Practice";
					try
					{
						Process.Start(new ProcessStartInfo(repoUrl) { UseShellExecute = true }); // Открываем ссылку в браузере
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine($"Открыт репозиторий: {repoUrl}");
						Console.ResetColor();
					}
					catch (Exception ex)                             // Ошибка открытия
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine($"Не удалось открыть ссылку: {ex.Message}");
						Console.ResetColor();
					}
					Console.WriteLine("Нажмите любую клавишу (ESC – Назад)...");
					WaitForAnyKey();                                 // Ожидание клавиши
					continue;
				}
				else if (choice == "2")                              // Пункт 2 – скачивание с Google Drive
				{
					DownloadFromDrive();                             // Вызов метода скачивания
					continue;
				}
				else
				{
					Console.WriteLine("Некорректный выбор. Нажмите любую клавишу (ESC – Назад)...");
					WaitForAnyKey();                                 // Ожидание клавиши
				}
			}
		}

		//========================================================= Скачивание и распаковка с Google Drive ================================================================//
		static void DownloadFromDrive()
		{
			string fileId = "18mXjs8BIOmtAF1VyQer-e5B2r8fy6TGJ"; // Идентификатор файла на Google Drive
			string downloadUrl = $"https://drive.google.com/uc?export=download&id={fileId}"; // Прямая ссылка для скачивания
			string zipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WorkPractice_latest.zip"); // Путь для сохранения ZIP

			Console.Clear();                                         // Очищаем экран
			Console.ForegroundColor = ConsoleColor.Cyan;            // Голубой цвет для заголовка
			Console.WriteLine("Начинается скачивание обновления...");
			Console.ResetColor();

			using (HttpClient client = new HttpClient())            // Создаём HTTP-клиент
			{
				try
				{
					using (HttpResponseMessage response = client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead).Result) // Отправляем запрос
					{
						response.EnsureSuccessStatusCode();          // Проверяем статус (200 OK)
						long totalBytes = response.Content.Headers.ContentLength ?? -1; // Получаем общий размер (если доступен)
						using (Stream stream = response.Content.ReadAsStreamAsync().Result) // Поток для чтения содержимого
						using (FileStream fileStream = File.Create(zipPath)) // Создаём файл для записи
						{
							byte[] buffer = new byte[8192];          // Буфер для чтения (8 КБ)
							long bytesRead = 0;                      // Счётчик прочитанных байт
							int bytesInBuffer;                       // Количество байт в текущем чтении
							int lastPercent = -1;                    // Последний отображённый процент (для избегания частого обновления)
							int cursorTop = Console.CursorTop;      // Запоминаем строку для вывода прогресса
							int cursorLeft = 0;                      // Столбец для вывода прогресса

							while ((bytesInBuffer = stream.Read(buffer, 0, buffer.Length)) > 0) // Пока есть данные
							{
								fileStream.Write(buffer, 0, bytesInBuffer); // Записываем в файл
								bytesRead += bytesInBuffer;          // Увеличиваем счётчик
								if (totalBytes > 0)                  // Если известен общий размер
								{
									int percent = (int)((double)bytesRead / totalBytes * 100); // Вычисляем процент
									if (percent != lastPercent)      // Если процент изменился
									{
										lastPercent = percent;       // Запоминаем новый процент
										Console.SetCursorPosition(cursorLeft, cursorTop); // Возвращаем курсор на строку прогресса
										int barWidth = 40;           // Ширина прогресс-бара
										int filled = (int)((double)percent / 100 * barWidth); // Сколько символов заполнено
										string bar = new string('█', filled) + new string('░', barWidth - filled); // Строка прогресса
										Console.ForegroundColor = ConsoleColor.Green; // Зелёный для прогресса
										Console.Write($"Скачивание: [{bar}] {percent}%"); // Вывод прогресса
										Console.ResetColor();
									}
								}
								else                                 // Если размер неизвестен
								{
									Console.SetCursorPosition(cursorLeft, cursorTop); // Возврат на строку прогресса
									Console.Write($"Скачивание: {bytesRead / 1024} KB"); // Вывод количества КБ
								}
							}
							Console.WriteLine();                     // Переход на новую строку после завершения
						}
					}

					Console.ForegroundColor = ConsoleColor.Yellow;  // Жёлтый для сообщения о распаковке
					Console.WriteLine("Распаковка архива...");
					Console.ResetColor();

					Console.ForegroundColor = ConsoleColor.Green;   // Зелёный для запроса папки
					Console.WriteLine("Выберите папку для распаковки:");
					Console.ResetColor();

					string extractPath = null;                       // Переменная для пути распаковки
					try
					{
						// Создаём STA-поток для FolderBrowserDialog (требуется для COM)
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
					catch { }                                         // Игнорируем ошибки диалога

					if (string.IsNullOrEmpty(extractPath))           // Если папка не выбрана
					{
						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.WriteLine("Распаковка отменена.");
						Console.ResetColor();
						File.Delete(zipPath);                        // Удаляем скачанный архив
						return;
					}

					try
					{
						using (ZipArchive archive = ZipFile.OpenRead(zipPath)) // Открываем ZIP-архив
						{
							foreach (ZipArchiveEntry entry in archive.Entries) // Перебираем все записи
							{
								string destinationPath = Path.Combine(extractPath, entry.FullName); // Полный путь для распаковки
								if (string.IsNullOrEmpty(entry.Name)) // Если это папка (нет имени файла)
								{
									Directory.CreateDirectory(destinationPath); // Создаём папку
									continue;
								}
								string directory = Path.GetDirectoryName(destinationPath); // Путь к папке файла
								if (!string.IsNullOrEmpty(directory))
									Directory.CreateDirectory(directory); // Создаём папку (если ещё нет)
								entry.ExtractToFile(destinationPath, true); // Извлекаем файл с перезаписью
							}
						}

						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine("Архив успешно распакован. Все файлы обновлены.");
						Console.ResetColor();

						File.Delete(zipPath);                        // Удаляем временный архив
						Console.WriteLine("Временный архив удалён.");
					}
					catch (Exception ex)                             // Ошибка распаковки
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine($"Ошибка при распаковке: {ex.Message}");
						Console.ResetColor();
						Console.WriteLine("Вы можете распаковать архив вручную из папки с программой.");
					}
				}
				catch (Exception ex)                                 // Ошибка скачивания
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"Ошибка при скачивании: {ex.Message}");
					Console.ResetColor();
				}
			}

			Console.WriteLine("\nНажмите любую клавишу для возврата (ESC – Назад)...");
			WaitForAnyKey();                                         // Ожидание клавиши
		}

		//========================================================= Вывод контактных данных автора ================================================================//
		static void ShowContacts()
		{
			Console.ForegroundColor = ConsoleColor.Magenta;          // Маджентовый цвет для контактов
			Console.WriteLine("=== Контакты ===");                   // Заголовок
			Console.WriteLine("Telegram: https://t.me/TulskiyGhoul"); // Telegram
			Console.WriteLine("Email: danila.nikiforov.2000@bk.ru"); // Email
			Console.ResetColor();

			Console.WriteLine("(нужен VPN) Открыть Telegram? (y/n)"); // Запрос открыть Telegram
			if (Console.ReadKey().KeyChar == 'y')                    // Если нажата 'y'
			{
				try
				{
					Process.Start(new ProcessStartInfo("https://t.me/TulskiyGhoul") { UseShellExecute = true }); // Открываем ссылку в браузере
				}
				catch { }                                            // Игнорируем ошибки открытия
			}
			Console.WriteLine();
		}
	}
}