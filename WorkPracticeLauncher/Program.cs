using System;					  // Базовые типы и консольный ввод-вывод
using System.Diagnostics;		  // Запуск процессов (Process.Start)
using System.IO;				  // Работа с файлами и путями (File, Path)
using System.Net.Http;			  // HTTP-запросы (HttpClient, скачивание)
using System.IO.Compression;	  // Работа с ZIP-архивами (ZipArchive)
using System.Text;				  // Работа с кодировками (Encoding.UTF8)
using System.Threading;			  // Потоки (Thread, Thread.Sleep)
using WorkPracticeLauncher.Tasks; // Пространство имён с решением заданий (Task1Solver и др.)

namespace WorkPracticeLauncher
{
	class Program
	{
		//========================================================= Статические поля класса Program ================================================================//
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
		public static bool IsWindowsTerminal { get; private set; } // Публичный доступ к флагу Windows Terminal
		public static bool SupportsEmoji => supportsEmoji;         // Публичный доступ к флагу поддержки эмодзи

		//================================================================================Метод Main — точка входа и главный цикл===============================================================//
		static void Main()
		{
			// Устанавливаем кодировку для поддержки Unicode (эмодзи, псевдографика)
			Console.OutputEncoding = Encoding.UTF8;

			// Очищаем консоль от предыдущего вывода
			Console.Clear();

			// Проверяем, запущено ли приложение в Windows Terminal (переменная окружения WT_SESSION)
			isWindowsTerminal = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WT_SESSION"));
			// Сохраняем флаг в публичное статическое свойство для доступа из других классов
			IsWindowsTerminal = isWindowsTerminal;

			// Проверяем поддержку эмодзи в текущем терминале (вызываем тестовый метод)
			supportsEmoji = TestEmojiSupport();

			// Запоминаем начальные размеры окна консоли
			lastWidth = Console.WindowWidth;
			lastHeight = Console.WindowHeight;

			// Выводим рекомендацию по терминалу (если не Windows Terminal)
			ShowTerminalRecommendation();

			// Создаём экземпляр класса анимации кота
			CatAnimation cat = new CatAnimation();

			// Первичная отрисовка экрана (меню, кот, подвал)
			RedrawScreen(cat);

			// Устанавливаем курсор и рисуем мигающее подчёркивание
			ResetCursorState();

			// Дополнительная задержка и повторная отрисовка для Windows Terminal (устраняет артефакты)
			if (isWindowsTerminal)
			{
				Thread.Sleep(150);
				lastWidth = Console.WindowWidth;
				lastHeight = Console.WindowHeight;
				RedrawScreen(cat);
				ResetCursorState();
			}

			// Флаг работы главного цикла
			bool running = true;

			// Главный цикл приложения – обработка ввода и обновление состояния
			while (running)
			{
				// Проверяем изменение размера окна
				if (Console.WindowWidth != lastWidth || Console.WindowHeight != lastHeight)
				{
					// Обновляем запомненные размеры
					lastWidth = Console.WindowWidth;
					lastHeight = Console.WindowHeight;
					// Перерисовываем экран
					RedrawScreen(cat);
					// Возвращаем курсор на место
					ResetCursorState();
					// Дополнительная перерисовка для Windows Terminal, если поддерживаются эмодзи
					if (supportsEmoji)
					{
						Thread.Sleep(30);
						RedrawScreen(cat);
						ResetCursorState();
					}
				}

				// Обновляем анимацию кота (смена Zzz)
				cat.UpdateSleep();

				// Рисуем мигающий курсор (подчёркивание)
				DrawCursor();

				// Проверяем, была ли нажата клавиша (неблокирующий ввод)
				if (Console.KeyAvailable)
				{
					// Считываем нажатую клавишу (без отображения)
					ConsoleKeyInfo keyInfo = Console.ReadKey(true);
					ConsoleKey key = keyInfo.Key;
					char keyChar = keyInfo.KeyChar;

					// Проверяем, является ли клавиша допустимой (цифры 0-4, Enter, Backspace, Escape)
					bool isValidKey = (key >= ConsoleKey.D0 && key <= ConsoleKey.D4) ||
									  (key == ConsoleKey.Enter) ||
									  (key == ConsoleKey.Backspace) ||
									  (key == ConsoleKey.Escape);

					// Если клавиша недопустима – игнорируем
					if (!isValidKey) continue;

					// Обработка цифр 0-4 (выбор пункта меню)
					if (key >= ConsoleKey.D0 && key <= ConsoleKey.D4)
					{
						// Если уже есть введённая цифра, стираем её
						if (inputBuffer.Length > 0)
						{
							Console.SetCursorPosition(inputCol, inputRow);
							Console.Write(new string(' ', inputBuffer.Length + 1));
							inputBuffer = "";
							SetCursorToInput();
						}

						// Запоминаем введённую цифру в буфер
						inputBuffer = keyChar.ToString();
						// Устанавливаем курсор после неё
						SetCursorToInput();
						// Рисуем подчёркивание (курсор)
						DrawCursor();

						// Запускаем анимацию пробуждения кота в фоновом потоке
						cancelAnimation = false;
						Thread animThread = new Thread(() =>
						{
							cat.WakeUpWithCancel(() => cancelAnimation);
						});
						animThread.Start();

						// Переменная для отслеживания, была ли анимация отменена
						bool cancelled = false;

						// Ожидание завершения анимации с возможностью прерывания
						while (animThread.IsAlive)
						{
							if (Console.KeyAvailable)
							{
								ConsoleKey checkKey = Console.ReadKey(true).Key;
								// Если нажат Enter – подтверждаем выбор (прерываем анимацию)
								if (checkKey == ConsoleKey.Enter)
								{
									cancelAnimation = true;
									break;
								}
								// Если Backspace или Escape – отменяем выбор
								else if (checkKey == ConsoleKey.Backspace || checkKey == ConsoleKey.Escape)
								{
									cancelAnimation = true;
									cancelled = true;
									break;
								}
							}
							Thread.Sleep(10);
						}
						// Дожидаемся завершения потока анимации
						animThread.Join(100);

						// Очищаем буфер ввода (удаляем лишние нажатия)
						while (Console.KeyAvailable) Console.ReadKey(true);

						// Если выбор был отменён (Backspace/Escape)
						if (cancelled)
						{
							// Стираем введённую цифру
							Console.SetCursorPosition(inputCol, inputRow);
							Console.Write(new string(' ', inputBuffer.Length + 1));
							inputBuffer = "";
							SetCursorToInput();
							// Перерисовываем экран и сбрасываем состояние курсора
							RedrawScreen(cat);
							ResetCursorState();
							continue;
						}

						// Если анимация была прервана по Enter – подтверждаем выбор
						if (cancelAnimation)
						{
							// Преобразуем буфер в число (выбор пункта)
							int choice = int.Parse(inputBuffer);
							inputBuffer = "";
							// Стираем введённую цифру
							Console.SetCursorPosition(inputCol, inputRow);
							Console.Write(new string(' ', 2));
							SetCursorToInput();
							// Очищаем экран и выполняем действие
							Console.Clear();
							ExecuteChoice(choice);
							// Если выбран пункт 0 – завершаем программу
							if (choice == 0)
								running = false;
							else
							{
								// Иначе перерисовываем экран
								RedrawScreen(cat);
								ResetCursorState();
							}
							continue;
						}
						else
						{
							// Если анимация завершилась сама (без прерывания)
							string digit = inputBuffer;
							inputBuffer = "";
							// Перерисовываем экран и восстанавливаем введённую цифру
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
						// Если нажат Enter, а в буфере уже есть цифра (после завершения анимации)
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
						// Обработка Backspace – очистка введённой цифры
						inputBuffer = "";
						Console.SetCursorPosition(inputCol, inputRow);
						Console.Write(new string(' ', 2));
						SetCursorToInput();
						while (Console.KeyAvailable) Console.ReadKey(true);
					}
					else if (key == ConsoleKey.Escape)
					{
						// Обработка Escape – выход из программы
						inputBuffer = "";
						Console.SetCursorPosition(inputCol, inputRow);
						Console.Write(new string(' ', 2));
						SetCursorToInput();
						while (Console.KeyAvailable) Console.ReadKey(true);
						running = false;
					}
				}

				// Небольшая задержка для снижения нагрузки на процессор
				Thread.Sleep(30);
			}

			// При выходе очищаем экран
			Console.Clear();

			// Делаем курсор видимым (на случай, если он был скрыт)
			Console.CursorVisible = true;
		}

		//========================================================= Ожидание нажатия клавиши с проверкой на Escape =============================================================================//
		private static bool WaitForAnyKey()
		{
			ConsoleKey key = Console.ReadKey(true).Key; // Считываем нажатую клавишу (без отображения на экране)
			return key == ConsoleKey.Escape;            // Возвращаем true, если нажат Escape, иначе false
		}
		//========================================================= Проверка поддержки эмодзи в терминале ======================================================================================//
		private static bool TestEmojiSupport()
		{
			int before = Console.CursorLeft;                    // Запоминаем позицию курсора до вывода эмодзи
			int top = Console.CursorTop;                        // Запоминаем строку (не используется, но сохраняем для восстановления)
			Console.Write("⚡");                                // Выводим тестовый эмодзи (молния)
			int after = Console.CursorLeft;                     // Запоминаем новую позицию курсора после вывода эмодзи
			Console.SetCursorPosition(before, top);             // Возвращаем курсор на исходную позицию
			Console.Write("  ");                                // Затираем выведенный эмодзи двумя пробелами
			Console.SetCursorPosition(before, top);             // Снова возвращаем курсор на исходную позицию
			return (after - before) >= 2;                       // Если курсор сдвинулся на 2 или более позиций — эмодзи поддерживается
		}

		//========================================================= Рекомендация по использованию Windows Terminal =============================================================================//
		private static void ShowTerminalRecommendation()
		{
			if (!supportsEmoji)                                 // Если терминал не поддерживает эмодзи (старая консоль)
			{
				Console.ForegroundColor = ConsoleColor.Red;     // Устанавливаем красный цвет текста
				Console.WriteLine("Для лучшего отображения рекомендуется использовать Windows Terminal."); // Предупреждение
				Console.ResetColor();                           // Сбрасываем цвет
				Console.WriteLine("Нажмите любую клавишу для продолжения (ESC – отмена)..."); // Приглашение для продолжения
				WaitForAnyKey();                                // Ожидаем нажатия клавиши (Escape завершает программу)
				Console.Clear();                                // Очищаем экран после продолжения
			}
			else                                                // Если эмодзи поддерживаются (Windows Terminal)
			{
				int w = Console.WindowWidth;                    // Получаем текущую ширину окна консоли
				string msg = "Рекомендуемый размер окна: 120x40. При изменении размера содержимое будет перерисовано автоматически."; // Текст рекомендации
				if (msg.Length > w - 4) msg = msg.Substring(0, w - 4); // Если сообщение длиннее ширины окна, обрезаем его
				Console.ForegroundColor = ConsoleColor.Cyan;    // Устанавливаем голубой цвет текста
				Console.WriteLine(AlignCenter(msg, w));         // Выводим центрированное сообщение
				Console.ResetColor();                           // Сбрасываем цвет
				Console.WriteLine();                            // Пустая строка для отступа
			}
		}

		//===================================================================== Вывод информации в нижней строке окна ==========================================================================//
		private static void PrintFooter()
		{
			int w = Console.WindowWidth;                                // Получаем текущую ширину окна консоли
			int row = Console.WindowHeight - 1;                         // Определяем последнюю строку окна (индекс 0-based)
			if (row < 0) row = 0;                                       // Если высота окна меньше 1, устанавливаем строку 0

			// Название компании в левом нижнем углу (с ёлочкой, если поддерживается эмодзи)
			string brand = supportsEmoji ? "elki-igolki company\U0001F384" : "elki-igolki company";
			Console.SetCursorPosition(0, row);                          // Перемещаем курсор в левый нижний угол
			Console.ForegroundColor = ConsoleColor.Green;               // Устанавливаем зелёный цвет
			Console.Write(brand);                                       // Выводим название
			Console.ResetColor();                                       // Сбрасываем цвет

			// Предупреждение в правом нижнем углу (только если эмодзи не поддерживаются – старая консоль)
			if (!supportsEmoji)
			{
				string msg = "[!] Для корректной работы рекомендуется использовать Windows Terminal!"; // Текст предупреждения
				if (msg.Length > w - 2) msg = msg.Substring(0, w - 2);  // Обрезаем, если не помещается по ширине
				int left = w - msg.Length - 1;                          // Вычисляем позицию для выравнивания вправо
				if (left < 0) left = 0;                                 // Защита от отрицательного смещения
				Console.SetCursorPosition(left, row);                   // Перемещаем курсор в правую часть нижней строки
				Console.ForegroundColor = ConsoleColor.Yellow;          // Устанавливаем жёлтый цвет
				Console.Write(msg);                                     // Выводим предупреждение
				Console.ResetColor();                                   // Сбрасываем цвет
			}
		}

		//========================================================= Центрирование текста по ширине окна ========================================================================================//
		private static string AlignCenter(string text, int width)
		{
			if (width <= 0) return text;													 // Если ширина меньше или равна 0, возвращаем текст как есть
			if (text.Length >= width) return text;											 // Если текст длиннее ширины, возвращаем без изменений
			int pad = (width - text.Length) / 2;											 // Вычисляем количество пробелов для отступа слева
			return new string(' ', pad) + text + new string(' ', width - text.Length - pad); // Возвращаем текст с пробелами слева и справа
		}
		//=================================================================== Отрисовка мигающего курсора ввода ================================================================================//
		private static void DrawCursor()
		{
			int row = inputRow;																// Получаем строку для курсора
			int col = inputCol;																// Получаем столбец для курсора
			int len = inputBuffer.Length;													// Длина введённого текста (0 или 1)
			if (row >= Console.WindowHeight) row = Console.WindowHeight - 1;				// Корректируем строку, если вышла за границы
			if (col + len >= Console.WindowWidth) len = Console.WindowWidth - col - 1;		// Уменьшаем длину, если текст выходит за границы
			if (col < 0) col = 0;															// Защита от отрицательного столбца
			if (row < 0) row = 0;															// Защита от отрицательной строки
																							
			Console.SetCursorPosition(col, row);											// Устанавливаем курсор в начало строки ввода
			Console.Write(new string(' ', len + 1));										// Затираем старую строку пробелами (буфер + подчёркивание)
			Console.SetCursorPosition(col, row);											// Возвращаем курсор на место
			if (!string.IsNullOrEmpty(inputBuffer))											// Если есть введённый текст
			{																				
				string display = inputBuffer;												// Берём текст из буфера
				if (display.Length > len) display = display.Substring(0, len);				// Обрезаем, если длиннее допустимого
				Console.Write(display);														// Выводим текст
			}
			if ((DateTime.Now - lastCursorToggle).TotalMilliseconds >= cursorBlinkInterval) // Проверяем, прошло ли полпериода мигания
			{
				showCursorState = !showCursorState;											// Переключаем состояние видимости курсора
				lastCursorToggle = DateTime.Now;											// Обновляем время последнего переключения
			}
			Console.Write(showCursorState ? '_' : ' ');										// Выводим подчёркивание или пробел в зависимости от состояния
		}
		//================================================================= Установка позиции курсора на поле ввода ============================================================================//
		private static void SetCursorToInput()
		{
			int offset = inputBuffer.Length;								 // Количество символов, введённых в буфере
			int col = inputCol + offset;									 // Вычисляем столбец: начальная позиция + длина буфера
			int row = inputRow;												 // Строка остаётся неизменной (строка ввода)
			if (col >= Console.WindowWidth) col = Console.WindowWidth - 1;   // Не выходим за правую границу окна
			if (row >= Console.WindowHeight) row = Console.WindowHeight - 1; // Не выходим за нижнюю границу
			if (col < 0) col = 0;											 // Защита от отрицательного столбца
			if (row < 0) row = 0;											 // Защита от отрицательной строки
			Console.SetCursorPosition(col, row);							 // Перемещаем курсор на рассчитанную позицию
		}

		//======================================================================= Сброс состояния курсора ======================================================================================//
		private static void ResetCursorState()
		{
			Console.CursorVisible = false;          // Скрываем стандартный курсор (будем рисовать свой)
			showCursorState = true;                 // Устанавливаем состояние видимости мигающего курсора (показываем подчёркивание)
			lastCursorToggle = DateTime.Now;        // Сбрасываем таймер мигания, чтобы курсор сразу отобразился
			SetCursorToInput();                     // Перемещаем курсор на строку ввода
			DrawCursor();                           // Рисуем мигающее подчёркивание (или пробел, если буфер пуст)
		}

		//========================================================================== Полная перерисовка главного экрана ========================================================================//
		static void RedrawScreen(CatAnimation cat)
		{
			Console.Clear();                            // Очищаем весь экран от предыдущего вывода
			Console.SetCursorPosition(0, 0);            // Перемещаем курсор в верхний левый угол (начало экрана)
			PrintMenu();                                // Выводим главное меню (заголовки, пункты, подсказки)
			cat.ShowSleepFrame();                       // Отображаем начальный кадр анимации кота (спящий)
			PrintFooter();                              // Выводим информацию в нижней строке (компания, предупреждение)
		}
		//===================================================================== Отрисовка главного меню ========================================================================================//
		static void PrintMenu()
		{
			int w = Console.WindowWidth;																										// Получаем текущую ширину окна
			if (w < 10)																															// Если окно слишком узкое
			{
				Console.ForegroundColor = ConsoleColor.Yellow;																					// Жёлтый цвет для предупреждения
				Console.WriteLine("Окно слишком узкое. Разверните его для корректного отображения.");											// Сообщение
				Console.ResetColor();																											// Сброс цвета
				return;																															// Выход из метода
			}

			Console.SetCursorPosition(0, 0);																									// Курсор в верхний левый угол

			if (supportsEmoji)																													// Если терминал поддерживает эмодзи (Windows Terminal)
			{
				int lineWidth = Math.Max(w - 2, 2);																								// Ширина внутренней части рамки
				string topLine = "╔" + new string('═', lineWidth) + "╗";																		// Верхняя граница рамки
				string bottomLine = "╚" + new string('═', lineWidth) + "╝";																		// Нижняя граница рамки

				Console.ForegroundColor = ConsoleColor.Green;																					// Зелёный цвет для рамки
				WriteLinePadded(topLine, w);																									// Выводим верхнюю границу
				WriteLinePadded("║" + AlignCenter("Учебная практика – Прикладная информатика", lineWidth) + "║", w);							// Заголовок в рамке
				WriteLinePadded("║" + AlignCenter("Launcher(V1.0)", lineWidth) + "║", w);														// Версия в рамке
				WriteLinePadded(bottomLine, w);																									// Нижняя граница рамки
				Console.ResetColor();																											// Сброс цвета

				Console.WriteLine();																											// Пустая строка после рамки

				Console.ForegroundColor = ConsoleColor.Yellow;																					// Жёлтый цвет для заголовка действия
				WriteLinePadded("▶ Выберите действие:", w);																						// Заголовок меню
				Console.ResetColor();																											// Сброс цвета

				string[] lines = {																												// Массив пунктов меню с эмодзи
            "  ⚡ 1 – Запустить WPF приложение",
			"  💻 2 – Просмотр решения в консоли (интерактивно)",
			"  📦 3 – Скачать актуальную версию (GitHub / Google Drive)",
			"  📧 4 – Связь с автором",
			"  🚪 0 – Выход"
		};
				ConsoleColor[] colors = { ConsoleColor.Cyan, ConsoleColor.Yellow, ConsoleColor.Green, ConsoleColor.Magenta, ConsoleColor.Red }; // Цвета для пунктов
				for (int i = 0; i < lines.Length; i++)																							// Перебираем все пункты меню
				{
					Console.ForegroundColor = colors[i % colors.Length];																		// Устанавливаем цвет для текущего пункта
					WriteLinePadded(lines[i], w);																								// Выводим пункт с затиранием до конца строки
				}
				Console.ForegroundColor = ConsoleColor.DarkGray;																				// Тёмно-серый цвет для подсказки
				WriteLinePadded("  (или ESC Назад)", w);																						// Подсказка о выходе по Escape
				Console.ResetColor();																											// Сброс цвета
			}
			else																																// Если эмодзи не поддерживаются (старая консоль)
			{
				Console.ForegroundColor = ConsoleColor.Green;																					// Зелёный цвет для упрощённой рамки
				WriteLinePadded("=============================================", w);															// Верхняя граница (текстовая)
				WriteLinePadded("   Учебная практика – Прикладная информатика", w);																// Заголовок
				WriteLinePadded("             Launcher(V1.0)", w);																				// Версия
				WriteLinePadded("=============================================", w);															// Нижняя граница
				Console.ResetColor();																											// Сброс цвета

				Console.WriteLine();																											// Пустая строка после рамки

				Console.ForegroundColor = ConsoleColor.Yellow;																					// Жёлтый цвет для заголовка
				WriteLinePadded("Выберите действие:", w);																						// Заголовок меню
				Console.ResetColor();																											// Сброс цвета

				WriteLinePadded("  1 – Запустить WPF приложение", w);																			// Пункт 1
				WriteLinePadded("  2 – Просмотр решения в консоли (интерактивно)", w);															// Пункт 2
				WriteLinePadded("  3 – Скачать актуальную версию (GitHub / Google Drive)", w);													// Пункт 3
				WriteLinePadded("  4 – Связь с автором", w);																					// Пункт 4
				WriteLinePadded("  0 – Выход", w);																								// Пункт 0
				Console.ForegroundColor = ConsoleColor.DarkGray;																				// Тёмно-серый цвет для подсказки
				WriteLinePadded("  (или ESC Назад)", w);																						// Подсказка о выходе
				Console.ResetColor();																											// Сброс цвета
			}

			Console.WriteLine();																												// Пустая строка перед приглашением
			Console.Write("Ваш выбор: ");																										// Вывод приглашения
			inputCol = Console.CursorLeft;																										// Сохраняем позицию курсора (столбец)
			inputRow = Console.CursorTop;																										// Сохраняем позицию курсора (строка)
			if (inputCol >= w) inputCol = w - 1;																								// Если столбец за пределами окна, корректируем
			if (inputRow >= Console.WindowHeight) inputRow = Console.WindowHeight - 1;															// Если строка за пределами, корректируем
		}
		//=========================================================================== Вывод строки с полным затиранием =========================================================================//
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
		//========================================================================== Запуск WPF-приложения =====================================================================================//
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
					Console.ForegroundColor = ConsoleColor.Red;         // Красный цвет для ошибки
					Console.WriteLine("Не удалось найти Work_practice.exe. Проверьте, что он находится в той же папке, что и лаунчер.");
					Console.ResetColor();                               // Сброс цвета
					return;                                             // Выход из метода
				}
			}

			try
			{
				// Запускаем WPF-приложение
				Process.Start(wpfExePath);
				// Если запуск успешен, выводим сообщение
				Console.ForegroundColor = ConsoleColor.Green;           // Зелёный цвет для успеха
				Console.WriteLine("WPF приложение запущено.");
				Console.ResetColor();                                   // Сброс цвета
			}
			catch (Exception ex)
			{
				// Если возникла ошибка при запуске, выводим её
				Console.ForegroundColor = ConsoleColor.Red;             // Красный цвет для ошибки
				Console.WriteLine($"Ошибка запуска: {ex.Message}");     // Вывод сообщения об ошибке
				Console.ResetColor();                                   // Сброс цвета
			}
		}

		//============================================================================ Меню скачивания обновлений ==============================================================================//
		static void DownloadLatestVersion()
		{
			// Цикл меню скачивания (бесконечный, выход по 0 или пустому вводу)
			while (true)
			{
				Console.Clear();																		// Очищаем экран для вывода меню
				Console.ForegroundColor = ConsoleColor.Yellow;											// Жёлтый цвет для заголовка
				Console.WriteLine("=== СКАЧИВАНИЕ АКТУАЛЬНОЙ ВЕРСИИ ===");								// Заголовок меню
				Console.ResetColor();																	// Сброс цвета
				Console.WriteLine("  1 – Перейти на GitHub (релизы)");									// Пункт 1 – открыть репозиторий
				Console.WriteLine("  2 – Скачать с Google Drive (автоматически)");						// Пункт 2 – скачать с диска
				Console.WriteLine("  0 – Назад");														// Пункт 0 – возврат
				Console.ForegroundColor = ConsoleColor.DarkGray;										// Тёмно-серый для подсказки
				Console.WriteLine("  (или ESC/Enter – Назад)");											// Подсказка о способах выхода
				Console.ResetColor();																	// Сброс цвета
				Console.Write("Ваш выбор: ");															// Приглашение ввода
				string choice = Console.ReadLine();														// Считываем строку выбора
				if (string.IsNullOrEmpty(choice)) return;												// Пустой ввод – возврат в главное меню
				if (choice == "0") return;																// Ввод 0 – возврат

				if (choice == "1")																		// Пункт 1 – переход на GitHub
				{
					string repoUrl = "https://github.com/OneWayToDie/Work_Practice";					// Адрес репозитория
					try
					{
						Process.Start(new ProcessStartInfo(repoUrl) { UseShellExecute = true });		// Открываем ссылку в браузере
						Console.ForegroundColor = ConsoleColor.Green;									// Зелёный для успеха
						Console.WriteLine($"Открыт репозиторий: {repoUrl}");							// Подтверждение открытия
						Console.ResetColor();															// Сброс цвета
					}
					catch (Exception ex)																// Если ошибка открытия
					{
						Console.ForegroundColor = ConsoleColor.Red;										// Красный для ошибки
						Console.WriteLine($"Не удалось открыть ссылку: {ex.Message}");					// Сообщение ошибки
						Console.ResetColor();															// Сброс цвета
					}
					Console.WriteLine("Нажмите любую клавишу (ESC – Назад)...");						// Ожидание нажатия
					WaitForAnyKey();																	// Ждём клавишу (Escape возвращает)
					continue;																			// Возврат в начало цикла меню
				}
				else if (choice == "2")																	// Пункт 2 – скачивание с Google Drive
				{
					DownloadFromDrive();																// Вызов метода скачивания
					continue;																			// Возврат в начало цикла меню
				}
				else																					// Некорректный ввод
				{
					Console.WriteLine("Некорректный выбор. Нажмите любую клавишу (ESC – Назад)...");	// Сообщение об ошибке
					WaitForAnyKey();																	// Ждём клавишу
				}
			}
		}

		//========================================================= Скачивание и распаковка обновления с Google Drive ==========================================================================//
		static void DownloadFromDrive()
		{
			string fileId = "18mXjs8BIOmtAF1VyQer-e5B2r8fy6TGJ";																		 // Идентификатор файла на Google Drive
			string downloadUrl = $"https://drive.google.com/uc?export=download&id={fileId}";											 // Прямая ссылка для скачивания
			string zipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WorkPractice_latest.zip");							 // Путь для сохранения ZIP

			Console.Clear();																											 // Очищаем экран
			Console.ForegroundColor = ConsoleColor.Cyan;																				 // Голубой цвет для заголовка
			Console.WriteLine("Начинается скачивание обновления...");																	 // Сообщение о начале
			Console.ResetColor();																										 // Сброс цвета
																																		 
			using (HttpClient client = new HttpClient())																				 // Создаём HTTP-клиент
			{
				try
				{
					using (HttpResponseMessage response = client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead).Result) // Отправляем запрос и получаем ответ
					{
						response.EnsureSuccessStatusCode();																				 // Проверяем статус (200 OK)
						long totalBytes = response.Content.Headers.ContentLength ?? -1;													 // Получаем общий размер файла (если доступен)
						using (Stream stream = response.Content.ReadAsStreamAsync().Result)												 // Открываем поток для чтения содержимого
						using (FileStream fileStream = File.Create(zipPath))															 // Создаём файл для записи
						{																												 
							byte[] buffer = new byte[8192];																				 // Буфер для чтения (8 КБ)
							long bytesRead = 0;																							 // Счётчик прочитанных байт
							int bytesInBuffer;																							 // Количество байт в текущем чтении
							int lastPercent = -1;																						 // Последний отображённый процент (для избегания частого обновления)
							int cursorTop = Console.CursorTop;																			 // Запоминаем строку для вывода прогресса
							int cursorLeft = 0;																							 // Столбец для вывода прогресса
																																		 
							// Чтение данных из потока и запись в файл																	 
							while ((bytesInBuffer = stream.Read(buffer, 0, buffer.Length)) > 0)											 // Пока есть данные
							{
								fileStream.Write(buffer, 0, bytesInBuffer);																 // Записываем в файл
								bytesRead += bytesInBuffer;																				 // Увеличиваем счётчик прочитанных байт
								if (totalBytes > 0)																						 // Если известен общий размер
								{
									int percent = (int)((double)bytesRead / totalBytes * 100);                                           // Вычисляем процент
									if (percent != lastPercent)																		     // Если процент изменился
									{																								     
										lastPercent = percent;																		     // Запоминаем новый процент
										Console.SetCursorPosition(cursorLeft, cursorTop);												 // Возвращаем курсор на строку прогресса
										int barWidth = 40;																				 // Ширина прогресс-бара
										int filled = (int)((double)percent / 100 * barWidth);											 // Сколько символов заполнено
										string bar = new string('█', filled) + new string('░', barWidth - filled);						 // Строка прогресса
										Console.ForegroundColor = ConsoleColor.Green;													 // Зелёный для прогресса
										Console.Write($"Скачивание: [{bar}] {percent}%");												 // Вывод прогресса
										Console.ResetColor();																			 // Сброс цвета
									}
								}
								else																									 // Если размер неизвестен
								{
									Console.SetCursorPosition(cursorLeft, cursorTop);													 // Возврат на строку прогресса
									Console.Write($"Скачивание: {bytesRead / 1024} KB");												 // Вывод количества КБ
								}																										 
							}																											 
							Console.WriteLine();																						 // Переход на новую строку после завершения
						}																												 
					}																													 
																																		 
					Console.ForegroundColor = ConsoleColor.Yellow;																		 // Жёлтый для сообщения о распаковке
					Console.WriteLine("Распаковка архива...");																			 // Сообщение о начале распаковки
					Console.ResetColor();																								 // Сброс цвета
																																		 
					Console.ForegroundColor = ConsoleColor.Green;																		 // Зелёный для запроса папки
					Console.WriteLine("Выберите папку для распаковки:");																 // Приглашение выбрать папку
					Console.ResetColor();																								 // Сброс цвета
																																		 
					string extractPath = null;																							 // Переменная для пути распаковки
					try																													 
					{																													 
						// Создаём STA-поток для FolderBrowserDialog (требуется для COM)												 
						var staThread = new Thread(() =>																				 
						{																												 
							using (var dialog = new System.Windows.Forms.FolderBrowserDialog())											 // Создаём диалог выбора папки
							{																											 
								dialog.Description = "Выберите папку для распаковки обновления";										 // Текст описания
								dialog.ShowNewFolderButton = true;																		 // Разрешаем создание новой папки
								if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)										 // Если нажата OK
									extractPath = dialog.SelectedPath;																	 // Сохраняем выбранный путь
							}																											 
						});																												 
						staThread.SetApartmentState(ApartmentState.STA);																 // Устанавливаем STA для диалога
						staThread.Start();																								 // Запускаем поток
						staThread.Join();																								 // Ждём завершения
					}																													 
					catch { }																											 // Игнорируем ошибки диалога
																																		 
					if (string.IsNullOrEmpty(extractPath))																				 // Если папка не выбрана
					{
						Console.ForegroundColor = ConsoleColor.Yellow;																	 // Жёлтый для сообщения об отмене
						Console.WriteLine("Raspakovka otmenena.");																		 // Сообщение об отмене
						Console.ResetColor();																							 // Сброс цвета
						File.Delete(zipPath);																							 // Удаляем скачанный архив
						return;																											 // Выход из метода
					}

					try
					{
						using (ZipArchive archive = ZipFile.OpenRead(zipPath))															 // Открываем ZIP-архив для чтения
						{																												 
							// Распаковка каждого файла																					 
							foreach (ZipArchiveEntry entry in archive.Entries)															 // Перебираем все записи в архиве
							{
								string destinationPath = Path.Combine(extractPath, entry.FullName);										 // Полный путь для распаковки
								if (string.IsNullOrEmpty(entry.Name))																	 // Если это папка (нет имени файла)
								{																										 
									Directory.CreateDirectory(destinationPath);															 // Создаём папку
									continue;																							 // Переходим к следующей записи
								}																										 
								string directory = Path.GetDirectoryName(destinationPath);												 // Получаем путь к папке файла
								if (!string.IsNullOrEmpty(directory))																	 // Если папка не пустая
									Directory.CreateDirectory(directory);																 // Создаём папку (если ещё нет)
								entry.ExtractToFile(destinationPath, true);																 // Извлекаем файл с перезаписью
							}
						}

						Console.ForegroundColor = ConsoleColor.Green;																	 // Зелёный для успеха
						Console.WriteLine("Архив успешно распакован. Все файлы обновлены.");											 // Сообщение об успехе
						Console.ResetColor();																							 // Сброс цвета
																																		 
						File.Delete(zipPath);																							 // Удаляем временный архив
						Console.WriteLine("Временный архив удалён.");																	 // Сообщение об удалении
					}																													 
					catch (Exception ex)																								 // Ошибка при распаковке
					{																													 
						Console.ForegroundColor = ConsoleColor.Red;																		 // Красный для ошибки
						Console.WriteLine($"Ошибка при распаковке: {ex.Message}");														 // Сообщение ошибки
						Console.ResetColor();																							 // Сброс цвета
						Console.WriteLine("Вы можете распаковать архив вручную из папки с программой.");								 // Подсказка
					}
				}
				catch (Exception ex)																									// Ошибка при скачивании
				{
					Console.ForegroundColor = ConsoleColor.Red;																			// Красный для ошибки
					Console.WriteLine($"Ошибка при скачивании: {ex.Message}");															// Сообщение ошибки
					Console.ResetColor();																								// Сброс цвета
				}
			}

			Console.WriteLine("\nНажмите любую клавишу для возврата (ESC – Назад)...");													// Приглашение вернуться
			WaitForAnyKey();																											// Ожидание нажатия (Escape = выход)
		}

		// Вывод контактных данных автора
		//============================================================================== Вывод контактных данных автора ========================================================================//
		static void ShowContacts()
		{
			Console.ForegroundColor = ConsoleColor.Magenta;														 // Устанавливаем маджентовый цвет для контактов
			Console.WriteLine("=== Контакты ===");																 // Заголовок раздела
			Console.WriteLine("Telegram: https://t.me/TulskiyGhoul");											 // Ссылка на Telegram
			Console.WriteLine("Email: danila.nikiforov.2000@bk.ru");											 // Адрес электронной почты
			Console.ResetColor();																				 // Сбрасываем цвет
																												 
			Console.WriteLine("(нужен VPN) Открыть Telegram? (y/n)");											 // Запрос на открытие Telegram
			if (Console.ReadKey().KeyChar == 'y')																 // Если нажата клавиша 'y'
			{																									 
				try																								 
				{																								 
					Process.Start(new ProcessStartInfo("https://t.me/TulskiyGhoul") { UseShellExecute = true }); // Открываем ссылку в браузере
				}
				catch { }																						 // Игнорируем ошибки открытия
			}																									 
			Console.WriteLine();																				 // Пустая строка для отступа
		}
	}
}