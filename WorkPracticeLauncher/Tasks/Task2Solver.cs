//========================================================= Библиотеки ================================================================//
using System;                // Базовые типы и консольный ввод-вывод
using System.Collections.Generic; // Коллекции (List<T>)
using System.IO;             // Работа с файлами и путями (File, Path, Directory)
using System.Linq;           // LINQ-запросы (OrderBy, Concat, Where, Select)
using System.Xml.Serialization; // Сериализация в XML (XmlSerializer)
using WorkPracticeLauncher.Models; // Модель товара (Product)
using System.Windows.Forms;  // Диалог выбора папки (FolderBrowserDialog)

namespace WorkPracticeLauncher.Tasks
{
	//========================================================= Класс Task2Solver – решение задания 2 (товары, файл, сортировка) ================================================================//
	public static class Task2Solver
	{
		//========================================================= Поля и свойства ================================================================//
		private static List<Product> products = new List<Product>();     // Список товаров в памяти (БД)
		private static string currentFileName = "products.xml";          // Имя текущего файла (по умолчанию XML)
		private static string currentDataDirectory = AppDomain.CurrentDomain.BaseDirectory; // Текущий каталог данных
		private enum FileFormat { Xml, Txt }                             // Формат файла (XML или TXT)

		private static int lastWidth;                                    // Последняя ширина окна (для отслеживания изменения)
		private static int lastHeight;                                   // Последняя высота окна

		//========================================================= Главное меню задания 2 ================================================================//
		public static void Run()
		{
			lastWidth = Console.WindowWidth;                             // Запоминаем начальную ширину
			lastHeight = Console.WindowHeight;                           // Запоминаем начальную высоту

			bool exit = false;                                           // Флаг выхода из меню
			while (!exit)                                                // Главный цикл меню
			{
				CheckResizeAndRedraw();                                  // Проверяем изменение размера окна
				Console.Clear();                                         // Очищаем экран
				ShowHeader();                                            // Выводим заголовок и информацию о БД

				Console.WriteLine();                                     // Пустая строка для отступа
				Console.WriteLine("  1 – Внести товары в БД");           // Пункт 1
				Console.WriteLine("  2 – Посмотреть товары");            // Пункт 2
				Console.WriteLine("  3 – Удалить товар из БД (по индексу)"); // Пункт 3
				Console.WriteLine("  4 – Удалить всю БД");               // Пункт 4
				Console.WriteLine("  5 – Сохранить БД в файл");          // Пункт 5
				Console.WriteLine("  6 – Загрузить БД из файла (дополнить)"); // Пункт 6
				Console.WriteLine("  7 – Изменить каталог для сохранения/загрузки"); // Пункт 7
				Console.WriteLine("  0 – Назад в меню заданий");         // Пункт 0
				Console.ForegroundColor = ConsoleColor.DarkGray;         // Тёмно-серый для подсказки
				Console.WriteLine("  (Enter – Назад)");                  // Подсказка о выходе по Enter
				Console.ResetColor();                                    // Сброс цвета
				Console.WriteLine();                                     // Пустая строка

				if (Program.SupportsEmoji)                               // Если поддерживаются эмодзи (Windows Terminal)
				{
					Console.ForegroundColor = ConsoleColor.Cyan;        // Голубой цвет для подсказки в рамке
					string[] tipLines = {                                // Строки подсказки
                        "💡 Для удаления товаров из конкретного файла:",
						"   1) Загрузите его (пункт 6)",
						"   2) Удалите товары (пункт 3 или 4)",
						"   3) Сохраните обратно (пункт 5, выберите перезапись)"
					};
					int maxWidth = tipLines.Max(line => line.Length) + 2; // Максимальная ширина строки + отступы
					string topLeft = "╭";                                // Левый верхний угол рамки
					string topRight = "╮";                               // Правый верхний угол
					string bottomLeft = "╰";                             // Левый нижний угол
					string bottomRight = "╯";                            // Правый нижний угол
					string horizontal = "─";                             // Горизонтальная линия
					string vertical = "│";                               // Вертикальная линия

					Console.WriteLine(topLeft + new string(horizontal[0], maxWidth) + topRight); // Верхняя граница
					foreach (string line in tipLines)                    // Вывод строк внутри рамки
					{
						Console.WriteLine(vertical + " " + line.PadRight(maxWidth - 2) + " " + vertical);
					}
					Console.WriteLine(bottomLeft + new string(horizontal[0], maxWidth) + bottomRight); // Нижняя граница
					Console.ResetColor();                                // Сброс цвета
				}
				else                                                     // Если эмодзи не поддерживаются
				{
					Console.ForegroundColor = ConsoleColor.Cyan;        // Голубой цвет для текстовой подсказки
					Console.WriteLine("Для удаления товаров из конкретного файла:");
					Console.WriteLine("  1) Загрузите его (пункт 6)");
					Console.WriteLine("  2) Удалите товары (пункт 3 или 4)");
					Console.WriteLine("  3) Сохраните обратно (пункт 5, выберите перезапись)");
					Console.ResetColor();                                // Сброс цвета
				}

				Console.Write("Ваш выбор: ");                            // Приглашение ввода
				string input = Console.ReadLine();                       // Считываем строку
				if (string.IsNullOrEmpty(input))                         // Если пустая строка
				{
					exit = true;                                         // Выход из меню
					continue;                                            // Переход к следующей итерации
				}
				if (!int.TryParse(input, out int choice))               // Если не число
				{
					Console.WriteLine("Некорректный ввод. Нажмите любую клавишу для возврата..."); // Сообщение об ошибке
					Console.ReadKey();                                   // Ожидание
					continue;                                            // Повторяем цикл
				}

				switch (choice)                                          // Обработка выбора
				{
					case 1: AddProducts(); break;                        // Добавление товаров
					case 2: ViewProducts(); break;                       // Просмотр товаров
					case 3: DeleteProduct(); break;                     // Удаление товара
					case 4: DeleteAllProducts(); break;                 // Удаление всей БД
					case 5: SaveWithOptions(); break;                   // Сохранение в файл
					case 6: LoadFromFileWithSelection(); break;         // Загрузка из файла
					case 7: ChangeDataDirectory(); break;               // Смена каталога
					case 0: exit = true; break;                         // Выход
					default:                                             // Некорректный выбор
						Console.WriteLine("Некорректный выбор. Нажмите любую клавишу для возврата...");
						Console.ReadKey();
						break;
				}
			}
		}

		//========================================================= Вывод заголовка с информацией о БД ================================================================//
		private static void ShowHeader()
		{
			Console.ForegroundColor = ConsoleColor.Yellow;              // Жёлтый цвет
			Console.WriteLine("=== ЗАДАНИЕ 2: УПРАВЛЕНИЕ БАЗОЙ ТОВАРОВ ==="); // Заголовок
			Console.ResetColor();                                        // Сброс цвета
			Console.WriteLine($"Всего товаров: {products.Count}");       // Количество товаров
			Console.WriteLine($"Текущий файл: {currentFileName}");      // Имя текущего файла
			Console.WriteLine($"Каталог данных: {currentDataDirectory}"); // Каталог
			Console.ForegroundColor = ConsoleColor.DarkGray;            // Тёмно-серый для подсказки
			Console.WriteLine("  (пустая строка — возврат в меню)");   // Подсказка о выходе
			Console.ResetColor();                                        // Сброс цвета
		}

		//========================================================= Проверка изменения размера окна ================================================================//
		private static void CheckResizeAndRedraw()
		{
			if (Console.WindowWidth != lastWidth || Console.WindowHeight != lastHeight) // Если размер изменился
			{
				lastWidth = Console.WindowWidth;                        // Обновляем сохранённую ширину
				lastHeight = Console.WindowHeight;                      // Обновляем сохранённую высоту
				Console.Clear();                                         // Очищаем экран
				ShowHeader();                                            // Выводим заголовок заново
			}
		}

		//========================================================= Смена каталога для хранения данных ================================================================//
		private static void ChangeDataDirectory()
		{
			Console.Clear();                                             // Очищаем экран
			Console.ForegroundColor = ConsoleColor.Yellow;             // Жёлтый цвет
			Console.WriteLine("=== ИЗМЕНЕНИЕ КАТАЛОГА ДАННЫХ ===");     // Заголовок
			Console.ResetColor();                                        // Сброс цвета
			Console.WriteLine($"Текущий каталог: {currentDataDirectory}"); // Вывод текущего пути
			Console.ForegroundColor = ConsoleColor.Green;              // Зелёный цвет для подсказки
			Console.WriteLine("▶ Выберите способ указания пути:");      // Заголовок выбора
			Console.ResetColor();                                        // Сброс цвета
			Console.WriteLine("  1 – Ввести путь вручную");             // Пункт 1
			Console.WriteLine("  2 – Выбрать через проводник");         // Пункт 2
			Console.WriteLine("  3 – Использовать папку с программой"); // Пункт 3
			Console.ForegroundColor = ConsoleColor.DarkGray;            // Тёмно-серый для подсказки
			Console.WriteLine("  (Enter – отмена)");                    // Подсказка отмены
			Console.ResetColor();                                        // Сброс цвета
			Console.Write("Ваш выбор: ");                                // Приглашение
			string choice = Console.ReadLine();                          // Считываем выбор
			if (string.IsNullOrEmpty(choice))                            // Если пусто – отмена
				return;

			string newPath = null;                                       // Переменная для нового пути
			if (choice == "1")                                           // Ручной ввод
			{
				Console.Write("Введите новый путь: ");                   // Запрос пути
				newPath = Console.ReadLine();                            // Считываем путь
				if (string.IsNullOrWhiteSpace(newPath))                  // Если пусто
				{
					Console.WriteLine("Изменение отменено.");           // Сообщение
					Console.ReadKey();                                   // Ожидание
					return;
				}
			}
			else if (choice == "2")                                      // Через проводник
			{
				newPath = SelectFolderViaDialog();                       // Вызываем диалог
				if (string.IsNullOrEmpty(newPath))                       // Если отменено
				{
					Console.WriteLine("Изменение отменено.");
					Console.ReadKey();
					return;
				}
			}
			else if (choice == "3")                                      // Папка с программой
			{
				newPath = AppDomain.CurrentDomain.BaseDirectory;        // Берём базовую директорию
			}
			else                                                         // Некорректный выбор
			{
				Console.WriteLine("Некорректный выбор.");
				Console.ReadKey();
				return;
			}

			if (!Directory.Exists(newPath))                              // Если каталог не существует
			{
				Console.Write("Каталог не существует. Создать? (y/n): "); // Запрос создания
				if (Console.ReadKey().KeyChar == 'y')                    // Если нажата 'y'
				{
					try
					{
						Directory.CreateDirectory(newPath);              // Создаём каталог
						Console.WriteLine("\nКаталог создан.");         // Сообщение
					}
					catch (Exception ex)                                 // Ошибка создания
					{
						Console.ForegroundColor = ConsoleColor.Red;     // Красный цвет
						Console.WriteLine($"\nНе удалось создать каталог: {ex.Message}"); // Сообщение ошибки
						Console.ResetColor();
						Console.ReadKey();
						return;
					}
				}
				else                                                     // Если не 'y'
				{
					Console.WriteLine("\nИзменение отменено.");
					Console.ReadKey();
					return;
				}
			}
			currentDataDirectory = newPath;                              // Обновляем текущий каталог
			Console.ForegroundColor = ConsoleColor.Green;              // Зелёный для успеха
			Console.WriteLine($"\nКаталог изменён на: {currentDataDirectory}"); // Сообщение
			Console.ResetColor();
			Console.ReadKey();                                           // Ожидание
		}

		//========================================================= Выбор папки через проводник Windows ================================================================//
		private static string SelectFolderViaDialog()
		{
			try
			{
				using (FolderBrowserDialog dialog = new FolderBrowserDialog()) // Создаём диалог выбора папки
				{
					dialog.Description = "Выберите каталог для данных";   // Текст описания
					dialog.SelectedPath = currentDataDirectory;          // Начальный путь (текущий каталог)
					dialog.ShowNewFolderButton = true;                   // Разрешаем создание новой папки
					if (dialog.ShowDialog() == DialogResult.OK)         // Если нажата OK
						return dialog.SelectedPath;                      // Возвращаем выбранный путь
					return null;                                         // Иначе null
				}
			}
			catch (Exception ex)                                          // Ошибка при открытии диалога
			{
				Console.ForegroundColor = ConsoleColor.Red;             // Красный цвет
				Console.WriteLine($"Ошибка при открытии диалога: {ex.Message}"); // Сообщение
				Console.ResetColor();
				return null;                                             // Возвращаем null
			}
		}

		//========================================================= Добавление новых товаров в БД ================================================================//
		private static void AddProducts()
		{
			Console.Clear();                                             // Очищаем экран
			Console.ForegroundColor = ConsoleColor.Yellow;             // Жёлтый цвет
			Console.WriteLine("=== ДОБАВЛЕНИЕ ТОВАРОВ ===");             // Заголовок
			Console.ResetColor();

			Console.ForegroundColor = ConsoleColor.Red;                // Красный цвет для предупреждения
			Console.WriteLine("Для выхода в меню введите пустую строку в поле 'Наименование'."); // Инструкция
			Console.ResetColor();

			int added = 0;                                               // Счётчик добавленных товаров
			while (true)                                                 // Бесконечный цикл ввода товаров
			{
				Console.Write("Наименование: ");                         // Запрос наименования
				string name = Console.ReadLine();                        // Считываем
				if (string.IsNullOrWhiteSpace(name)) break;             // Если пусто – выход из цикла

				Console.Write("Фирма: ");                                // Запрос фирмы
				string manufacturer = Console.ReadLine();                // Считываем
				if (string.IsNullOrWhiteSpace(manufacturer)) break;     // Если пусто – выход

				int shelfLife;                                           // Срок хранения
				while (true)                                             // Цикл валидации срока
				{
					Console.Write("Срок хранения (дни): ");              // Запрос
					string input = Console.ReadLine();                   // Считываем
					if (string.IsNullOrWhiteSpace(input))                // Если пусто – выход
					{
						Console.WriteLine("Выход в меню.");
						return;
					}
					if (int.TryParse(input, out shelfLife) && shelfLife > 0) // Если число >0
						break;                                           // Выход из цикла валидации
					Console.ForegroundColor = ConsoleColor.Red;        // Красный для ошибки
					Console.WriteLine("Некорректный ввод, повторите."); // Сообщение
					Console.ResetColor();
				}

				decimal price;                                           // Цена
				while (true)                                             // Цикл валидации цены
				{
					Console.Write("Цена: ");                             // Запрос
					string input = Console.ReadLine();                   // Считываем
					if (string.IsNullOrWhiteSpace(input))                // Если пусто – выход
					{
						Console.WriteLine("Выход в меню.");
						return;
					}
					string normalized = input.Replace(',', '.');        // Заменяем запятую на точку
					if (decimal.TryParse(normalized, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out price) && price > 0) // Парсим с инвариантной культурой
						break;                                           // Выход из цикла
					Console.ForegroundColor = ConsoleColor.Red;        // Красный для ошибки
					Console.WriteLine("Некорректный ввод, введите положительное число (например, 10.5 или 10,5).");
					Console.ResetColor();
				}

				int stock;                                               // Количество на складе
				while (true)                                             // Цикл валидации количества
				{
					Console.Write("Количество на складе: ");             // Запрос
					string input = Console.ReadLine();                   // Считываем
					if (string.IsNullOrWhiteSpace(input))                // Если пусто – выход
					{
						Console.WriteLine("Выход в меню.");
						return;
					}
					if (int.TryParse(input, out stock) && stock >= 0)   // Если число >=0
						break;                                           // Выход из цикла
					Console.ForegroundColor = ConsoleColor.Red;        // Красный для ошибки
					Console.WriteLine("Некорректный ввод, повторите.");
					Console.ResetColor();
				}

				Product newProduct = new Product                        // Создаём новый товар
				{
					Name = name,
					Manufacturer = manufacturer,
					ShelfLife = shelfLife,
					Price = price,
					StockQuantity = stock
				};
				products.Add(newProduct);                                // Добавляем в список
				added++;                                                 // Увеличиваем счётчик

				Console.ForegroundColor = ConsoleColor.Green;          // Зелёный для успеха
				Console.WriteLine($"Товар добавлен. Всего в БД: {products.Count}"); // Сообщение
				Console.ResetColor();

				Console.WriteLine("Нажмите Enter для продолжения...");   // Ожидание
				Console.ReadKey();
			}

			Console.ForegroundColor = ConsoleColor.Green;              // Зелёный итог
			Console.WriteLine($"\nДобавлено {added} товаров.");         // Сообщение о добавленных
			Console.ResetColor();
			Console.WriteLine("Нажмите любую клавишу для возврата..."); // Ожидание
			Console.ReadKey();
		}

		//========================================================= Просмотр всех товаров с сортировкой ================================================================//
		private static void ViewProducts()
		{
			if (products.Count == 0)                                     // Если товаров нет
			{
				Console.ForegroundColor = ConsoleColor.Red;            // Красный
				Console.WriteLine("База данных пуста.");
				Console.ResetColor();
				Console.ReadKey();
				return;
			}

			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;             // Жёлтый заголовок
			Console.WriteLine("=== ПРОСМОТР ТОВАРОВ ===");
			Console.ResetColor();

			int windowWidth = Console.WindowWidth;                      // Ширина окна для адаптации
			int maxLen = 20;                                            // Максимальная длина строки для обрезки

			// Проверка, есть ли длинные наименования
			bool hasLongName = false;
			foreach (Product p in products)
			{
				if (p.Name.Length > maxLen) { hasLongName = true; break; }
			}
			if (hasLongName)                                            // Если есть длинные
			{
				Console.ForegroundColor = ConsoleColor.Yellow;         // Жёлтое предупреждение
				Console.WriteLine("Внимание: некоторые наименования превышают 20 символов и будут обрезаны.");
				Console.ResetColor();
			}

			// Вычисление ширины столбцов (с учётом максимальной длины)
			int nameWidth = Math.Max("Наименование".Length + 1, Math.Min(maxLen, "Наименование".Length + 1));
			int mfrWidth = Math.Max("Фирма".Length + 1, Math.Min(maxLen, "Фирма".Length + 1));
			int shelfWidth = Math.Max("Срок".Length + 1, Math.Min(maxLen, "Срок".Length + 1));
			int priceWidth = Math.Max("Цена".Length + 1, Math.Min(maxLen, "Цена".Length + 1));
			int stockWidth = Math.Max("Кол-во".Length + 1, Math.Min(maxLen, "Кол-во".Length + 1));

			foreach (Product p in products)                             // Уточняем ширину по данным
			{
				nameWidth = Math.Max(nameWidth, Math.Min(p.Name.Length + 1, maxLen));
				mfrWidth = Math.Max(mfrWidth, Math.Min(p.Manufacturer.Length + 1, maxLen));
				shelfWidth = Math.Max(shelfWidth, Math.Min(p.ShelfLife.ToString().Length + 1, maxLen));
				priceWidth = Math.Max(priceWidth, Math.Min(p.Price.ToString("F2").Length + 1, maxLen));
				stockWidth = Math.Max(stockWidth, Math.Min(p.StockQuantity.ToString().Length + 1, maxLen));
			}

			int total = nameWidth + mfrWidth + shelfWidth + priceWidth + stockWidth + 8; // Общая ширина с пробелами
			if (total > windowWidth)                                     // Если не помещается
			{
				double ratio = (windowWidth - 8) / (double)(total - 8); // Коэффициент сжатия
				nameWidth = Math.Max(3, (int)(nameWidth * ratio));
				mfrWidth = Math.Max(3, (int)(mfrWidth * ratio));
				shelfWidth = Math.Max(3, (int)(shelfWidth * ratio));
				priceWidth = Math.Max(3, (int)(priceWidth * ratio));
				stockWidth = Math.Max(3, (int)(stockWidth * ratio));
			}

			// Вывод шапки таблицы
			Console.WriteLine($"{"Наименование".PadRight(nameWidth)} {"Фирма".PadRight(mfrWidth)} {"Срок".PadRight(shelfWidth)} {"Цена".PadRight(priceWidth)} {"Кол-во".PadRight(stockWidth)}");
			Console.WriteLine(new string('-', nameWidth + mfrWidth + shelfWidth + priceWidth + stockWidth + 4));

			// Выбор сортировки
			Console.WriteLine("Выберите сортировку:");
			Console.WriteLine("  1 – По сроку хранения (по возрастанию)");
			Console.WriteLine("  2 – По цене (по возрастанию)");
			Console.WriteLine("  3 – По наименованию (по алфавиту)");
			Console.WriteLine("  4 – Без сортировки (в порядке добавления)");
			Console.Write("Ваш выбор: ");
			string input = Console.ReadLine();                           // Считываем выбор
			int sortMode = 0;
			if (!int.TryParse(input, out sortMode) || sortMode < 1 || sortMode > 4) // Если некорректный
			{
				Console.ForegroundColor = ConsoleColor.Red;            // Красный
				Console.WriteLine("Некорректный ввод, применяем сортировку по умолчанию (по сроку).");
				Console.ResetColor();
				sortMode = 1;                                            // По умолчанию по сроку
			}

			IEnumerable<Product> sorted;                                // Отсортированная коллекция
			switch (sortMode)
			{
				case 1:
					sorted = products.OrderBy(p => p.ShelfLife);       // По сроку
					break;
				case 2:
					sorted = products.OrderBy(p => p.Price);           // По цене
					break;
				case 3:
					sorted = products.OrderBy(p => p.Name);            // По наименованию
					break;
				default:
					sorted = products;                                  // Без сортировки
					break;
			}

			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("=== СПИСОК ТОВАРОВ ===");
			Console.ResetColor();
			// Повторно выводим шапку
			Console.WriteLine($"{"Наименование".PadRight(nameWidth)} {"Фирма".PadRight(mfrWidth)} {"Срок".PadRight(shelfWidth)} {"Цена".PadRight(priceWidth)} {"Кол-во".PadRight(stockWidth)}");
			Console.WriteLine(new string('-', nameWidth + mfrWidth + shelfWidth + priceWidth + stockWidth + 4));
			foreach (Product p in sorted)                               // Вывод строк
			{
				Console.WriteLine($"{p.Name.PadRight(nameWidth)} {p.Manufacturer.PadRight(mfrWidth)} {p.ShelfLife.ToString().PadRight(shelfWidth)} {p.Price.ToString("F2").PadRight(priceWidth)} {p.StockQuantity.ToString().PadRight(stockWidth)}");
			}
			Console.ForegroundColor = ConsoleColor.Cyan;              // Голубой для итога
			Console.WriteLine($"\nВсего товаров: {products.Count}");   // Количество
			Console.ResetColor();
			Console.WriteLine("\nНажмите любую клавишу для возврата...");
			Console.ReadKey();
		}

		//========================================================= Удаление товара по индексу ================================================================//
		private static void DeleteProduct()
		{
			if (products.Count == 0)                                     // Если БД пуста
			{
				Console.ForegroundColor = ConsoleColor.Red;            // Красный
				Console.WriteLine("База данных пуста, удалять нечего.");
				Console.ResetColor();
				Console.ReadKey();
				return;
			}

			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;             // Жёлтый заголовок
			Console.WriteLine("=== УДАЛЕНИЕ ТОВАРА (ПО ИНДЕКСУ) ===");
			Console.ResetColor();
			Console.WriteLine("Список товаров:");
			Console.WriteLine("  Индекс  |  Наименование");
			Console.WriteLine("  -----------------------");
			for (int i = 0; i < products.Count; i++)                    // Вывод списка с индексами
				Console.WriteLine($"  {i + 1,-7} | {products[i].Name}");
			Console.WriteLine();

			Console.Write($"Введите индекс товара (1..{products.Count}) для удаления (Enter – отмена): "); // Запрос
			string idxStr = Console.ReadLine();                          // Считываем
			if (string.IsNullOrEmpty(idxStr))                            // Если пусто – отмена
				return;
			if (!int.TryParse(idxStr, out int idx) || idx < 1 || idx > products.Count) // Некорректный индекс
			{
				Console.ForegroundColor = ConsoleColor.Red;            // Красный
				Console.WriteLine("Некорректный индекс.");
				Console.ResetColor();
				Console.ReadKey();
				return;
			}

			Product removedProduct = products[idx - 1];                 // Товар для удаления
			products.RemoveAt(idx - 1);                                 // Удаляем из списка
			Console.ForegroundColor = ConsoleColor.Green;              // Зелёный успех
			Console.WriteLine($"Удалён товар: {removedProduct.Name} (индекс {idx})");
			Console.ResetColor();
			Console.WriteLine($"В БД осталось {products.Count} товаров.");
			Console.WriteLine("Нажмите любую клавишу для возврата...");
			Console.ReadKey();
		}

		//========================================================= Полная очистка базы данных ================================================================//
		private static void DeleteAllProducts()
		{
			if (products.Count == 0)                                     // Если уже пусто
			{
				Console.ForegroundColor = ConsoleColor.Red;            // Красный
				Console.WriteLine("База данных уже пуста.");
				Console.ResetColor();
				Console.ReadKey();
				return;
			}

			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Red;                // Красный для предупреждения
			Console.WriteLine("=== УДАЛЕНИЕ ВСЕЙ БАЗЫ ДАННЫХ ===");
			Console.ResetColor();
			Console.WriteLine($"Внимание! Вы собираетесь удалить все {products.Count} товаров из памяти."); // Предупреждение
			Console.Write("Подтвердите удаление (введите 'ДА' или Enter для отмены): ");
			string confirm = Console.ReadLine();                         // Считываем подтверждение
			if (string.IsNullOrEmpty(confirm) || confirm != "ДА")       // Если не 'ДА'
			{
				Console.ForegroundColor = ConsoleColor.Yellow;         // Жёлтый
				Console.WriteLine("Удаление отменено.");
				Console.ResetColor();
				Console.WriteLine("Нажмите любую клавишу для возврата...");
				Console.ReadKey();
				return;
			}

			products.Clear();                                            // Очищаем список
			Console.ForegroundColor = ConsoleColor.Green;              // Зелёный успех
			Console.WriteLine("База данных в памяти полностью очищена.");
			Console.ResetColor();

			string fullPath = Path.Combine(currentDataDirectory, currentFileName); // Путь к файлу
			if (File.Exists(fullPath))                                   // Если файл существует
			{
				Console.Write("Удалить файл с диска тоже? (y/n): ");    // Запрос удаления файла
				string deleteFile = Console.ReadLine();
				if (deleteFile?.ToLower() == "y")                       // Если 'y'
				{
					try
					{
						File.Delete(fullPath);                           // Удаляем файл
						Console.ForegroundColor = ConsoleColor.Green;  // Зелёный
						Console.WriteLine($"Файл {currentFileName} удалён.");
						Console.ResetColor();
					}
					catch (Exception ex)                                 // Ошибка удаления
					{
						Console.ForegroundColor = ConsoleColor.Red;    // Красный
						Console.WriteLine($"Не удалось удалить файл: {ex.Message}");
						Console.ResetColor();
					}
				}
				else                                                     // Иначе оставляем
				{
					Console.ForegroundColor = ConsoleColor.Yellow;     // Жёлтый
					Console.WriteLine("Файл остался на диске. Вы можете перезаписать его при следующем сохранении.");
					Console.ResetColor();
				}
			}
			else
				Console.WriteLine("Файл не найден на диске.");

			Console.WriteLine("Нажмите любую клавишу для возврата...");
			Console.ReadKey();
		}

		//========================================================= Сохранение БД в файл с выбором формата ================================================================//
		private static void SaveWithOptions()
		{
			if (products.Count == 0)                                     // Если нет товаров
			{
				Console.ForegroundColor = ConsoleColor.Red;            // Красный
				Console.WriteLine("Нет товаров для сохранения.");
				Console.ResetColor();
				Console.ReadKey();
				return;
			}

			List<string> xmlFiles = GetFilesByExtension(".xml", currentDataDirectory); // Список XML-файлов
			List<string> txtFiles = GetFilesByExtension(".txt", currentDataDirectory); // Список TXT-файлов
			List<string> allFiles = xmlFiles.Concat(txtFiles).ToList(); // Объединённый список

			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;             // Жёлтый заголовок
			Console.WriteLine("=== СОХРАНЕНИЕ БАЗЫ ДАННЫХ ===");
			Console.ResetColor();
			Console.WriteLine($"Текущий каталог: {currentDataDirectory}"); // Путь
			Console.ForegroundColor = ConsoleColor.Green;              // Зелёный подзаголовок
			Console.WriteLine("▶ Выберите действие:");
			Console.ResetColor();
			Console.WriteLine("  1 – Сохранить в новый файл");
			if (allFiles.Count > 0)                                      // Если есть файлы
			{
				Console.WriteLine("  2 – Дополнить существующий файл");
				Console.WriteLine("  3 – Перезаписать существующий файл");
			}
			Console.WriteLine("  0 – Отмена");
			Console.ForegroundColor = ConsoleColor.DarkGray;            // Тёмно-серый
			Console.WriteLine("  (Enter – отмена)");
			Console.ResetColor();
			Console.Write("Ваш выбор: ");
			string choiceStr = Console.ReadLine();                       // Считываем
			if (string.IsNullOrEmpty(choiceStr))                         // Если пусто – отмена
				return;
			if (!int.TryParse(choiceStr, out int choice))               // Если не число
			{
				Console.WriteLine("Некорректный ввод.");
				Console.ReadKey();
				return;
			}

			if (choice == 0) return;                                     // Отмена

			if (choice == 1)                                             // Сохранить в новый файл
			{
				Console.ForegroundColor = ConsoleColor.Green;          // Зелёный
				Console.WriteLine("▶ Выберите формат файла:");
				Console.ResetColor();
				Console.WriteLine("  1 – XML (совместим с WPF-приложением)");
				Console.WriteLine("  2 – TXT (текстовый, для ручного просмотра)");
				Console.Write("Ваш выбор: ");
				string formatChoice = Console.ReadLine();                 // Считываем формат
				if (string.IsNullOrEmpty(formatChoice))                  // Если пусто – отмена
					return;
				FileFormat format;
				if (formatChoice == "1")
					format = FileFormat.Xml;
				else if (formatChoice == "2")
					format = FileFormat.Txt;
				else
				{
					Console.WriteLine("Некорректный выбор.");
					Console.ReadKey();
					return;
				}

				Console.ForegroundColor = ConsoleColor.Green;          // Зелёный
				Console.WriteLine("▶ Выберите способ указания имени и пути:");
				Console.ResetColor();
				Console.WriteLine("  1 – Ввести имя файла (сохранится в текущий каталог)");
				Console.WriteLine("  2 – Выбрать каталог и имя файла через проводник");
				Console.WriteLine("  3 – Использовать папку с программой");
				Console.Write("Ваш выбор: ");
				string pathChoice = Console.ReadLine();                   // Считываем способ
				if (string.IsNullOrEmpty(pathChoice))                    // Если пусто – отмена
					return;
				string fileName = "";
				string targetPath = "";

				if (pathChoice == "1")                                   // Ввести имя в текущий каталог
				{
					Console.Write("Введите имя файла (без расширения): ");
					fileName = Console.ReadLine();
					if (string.IsNullOrWhiteSpace(fileName))
					{
						Console.WriteLine("Имя не может быть пустым.");
						Console.ReadKey();
						return;
					}
					string ext = (format == FileFormat.Xml) ? ".xml" : ".txt";
					targetPath = Path.Combine(currentDataDirectory, fileName + ext);
				}
				else if (pathChoice == "2")                             // Через проводник
				{
					string selectedDir = SelectFolderViaDialog();       // Выбираем папку
					if (string.IsNullOrEmpty(selectedDir))
					{
						Console.WriteLine("Операция отменена.");
						Console.ReadKey();
						return;
					}
					Console.Write("Введите имя файла (без расширения): ");
					fileName = Console.ReadLine();
					if (string.IsNullOrWhiteSpace(fileName))
					{
						Console.WriteLine("Имя не может быть пустым.");
						Console.ReadKey();
						return;
					}
					string ext = (format == FileFormat.Xml) ? ".xml" : ".txt";
					targetPath = Path.Combine(selectedDir, fileName + ext);
					currentDataDirectory = selectedDir;                  // Запоминаем новый каталог
				}
				else if (pathChoice == "3")                             // Папка с программой
				{
					Console.Write("Введите имя файла (без расширения): ");
					fileName = Console.ReadLine();
					if (string.IsNullOrWhiteSpace(fileName))
					{
						Console.WriteLine("Имя не может быть пустым.");
						Console.ReadKey();
						return;
					}
					string ext = (format == FileFormat.Xml) ? ".xml" : ".txt";
					targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName + ext);
					currentDataDirectory = AppDomain.CurrentDomain.BaseDirectory;
				}
				else
				{
					Console.WriteLine("Некорректный выбор.");
					Console.ReadKey();
					return;
				}

				SaveToFile(targetPath, format, append: false);          // Сохраняем без дополнения
				if (File.Exists(targetPath))
					currentFileName = Path.GetFileName(targetPath);     // Обновляем имя текущего файла
				Console.ReadKey();
				return;
			}

			if (allFiles.Count == 0)                                     // Если нет существующих файлов
			{
				Console.WriteLine("Нет существующих файлов. Сначала создайте новый файл.");
				Console.ReadKey();
				return;
			}

			Console.ForegroundColor = ConsoleColor.Green;              // Зелёный
			Console.WriteLine("▶ Доступные файлы:");
			Console.ResetColor();
			for (int i = 0; i < allFiles.Count; i++)                    // Вывод списка файлов
				Console.WriteLine($"  {i + 1} – {allFiles[i]}");
			Console.Write("Введите номер файла: ");                     // Запрос номера
			string fileIdxStr = Console.ReadLine();
			if (!int.TryParse(fileIdxStr, out int fileIdx) || fileIdx < 1 || fileIdx > allFiles.Count) // Некорректный
			{
				Console.WriteLine("Некорректный выбор.");
				Console.ReadKey();
				return;
			}
			string selectedFile = allFiles[fileIdx - 1];                // Выбранный файл
			string fullPath = Path.Combine(currentDataDirectory, selectedFile);
			FileFormat existingFormat = GetFormatFromExtension(selectedFile); // Определяем формат

			if (choice == 2)
				SaveToFile(fullPath, existingFormat, append: true);    // Дополнить
			else if (choice == 3)
				SaveToFile(fullPath, existingFormat, append: false);   // Перезаписать
			else
				Console.WriteLine("Некорректный выбор.");
			Console.ReadKey();
		}

		//========================================================= Определение формата по расширению файла ================================================================//
		private static FileFormat GetFormatFromExtension(string fileName)
		{
			if (fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)) // Если .xml
				return FileFormat.Xml;
			else if (fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)) // Если .txt
				return FileFormat.Txt;
			else
				return FileFormat.Xml;                                    // По умолчанию XML
		}

		//========================================================= Запись данных в файл (XML или TXT) ================================================================//
		private static void SaveToFile(string path, FileFormat format, bool append)
		{
			try
			{
				List<Product> existing = new List<Product>();            // Список существующих товаров (при дополнении)
				if (append && File.Exists(path))                        // Если дополнение и файл существует
				{
					try
					{
						if (format == FileFormat.Xml)                    // Если XML
						{
							XmlSerializer serializer = new XmlSerializer(typeof(List<Product>)); // Создаём сериализатор
							using (StreamReader reader = new StreamReader(path)) // Открываем для чтения
								existing = (List<Product>)serializer.Deserialize(reader); // Десериализуем
						}
						else                                              // Если TXT
							existing = LoadFromTxtInternal(path);       // Загружаем из TXT
					}
					catch                                                // Если ошибка чтения
					{
						Console.ForegroundColor = ConsoleColor.Red;     // Красный
						Console.WriteLine("Не удалось прочитать существующий файл. Будет выполнена перезапись.");
						Console.ResetColor();
						append = false;                                  // Переключаем на перезапись
					}
				}

				List<Product> toSave;
				if (append)                                               // Если дополнение
				{
					List<Product> newOnly = products.Where(p => !existing.Any(e =>
						e.Name == p.Name &&
						e.Manufacturer == p.Manufacturer &&
						e.ShelfLife == p.ShelfLife &&
						e.Price == p.Price &&
						e.StockQuantity == p.StockQuantity)).ToList();   // Находим только новые товары (без дубликатов)
					toSave = existing.Concat(newOnly).ToList();          // Объединяем существующие и новые
					if (newOnly.Count < products.Count)                  // Если были дубликаты
					{
						Console.ForegroundColor = ConsoleColor.Yellow;  // Жёлтый
						Console.WriteLine($"Пропущено дубликатов: {products.Count - newOnly.Count}"); // Сообщение
						Console.ResetColor();
					}
				}
				else
					toSave = products;                                   // Иначе сохраняем все

				if (format == FileFormat.Xml)                            // Если XML
				{
					XmlSerializer serializer = new XmlSerializer(typeof(List<Product>));
					using (StreamWriter writer = new StreamWriter(path)) // Открываем для записи
						serializer.Serialize(writer, toSave);           // Сериализуем
				}
				else                                                     // Если TXT
					SaveToTxtInternal(path, toSave);                    // Сохраняем в TXT

				Console.ForegroundColor = ConsoleColor.Green;          // Зелёный успех
				Console.WriteLine($"База данных сохранена в файл: {path}"); // Сообщение
				Console.WriteLine($"Всего записей: {toSave.Count}");
				if (format == FileFormat.Txt)
					Console.WriteLine("Внимание: TXT-файл не совместим с WPF-приложением (используйте XML для кросс-платформенности).");
				Console.ResetColor();
			}
			catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException) // Ошибка доступа
			{
				Console.ForegroundColor = ConsoleColor.Red;            // Красный
				Console.WriteLine("Нет прав доступа на создание файла в этой папке.");
				Console.ResetColor();
				Console.Write("Хотите выбрать другой каталог для сохранения? (y/n): "); // Предложение
				if (Console.ReadKey().KeyChar == 'y')                    // Если согласен
				{
					Console.WriteLine();
					string newPath = RequestSavePath(format);           // Запрашиваем новый путь
					if (!string.IsNullOrEmpty(newPath))
						SaveToFile(newPath, format, append);            // Повторяем сохранение
					return;
				}
				Console.WriteLine();
				Console.ForegroundColor = ConsoleColor.Red;            // Выводим исходную ошибку
				Console.WriteLine($"Ошибка при сохранении: {ex.Message}");
				Console.ResetColor();
			}
			catch (Exception ex)                                          // Любая другая ошибка
			{
				Console.ForegroundColor = ConsoleColor.Red;            // Красный
				Console.WriteLine($"Ошибка при сохранении: {ex.Message}");
				Console.ResetColor();
			}
		}

		//========================================================= Запрос пути для сохранения файла ================================================================//
		private static string RequestSavePath(FileFormat format)
		{
			Console.ForegroundColor = ConsoleColor.Green;              // Зелёный
			Console.WriteLine("▶ Выберите способ указания пути:");
			Console.ResetColor();
			Console.WriteLine("  1 – Ввести путь вручную");
			Console.WriteLine("  2 – Выбрать через проводник");
			Console.WriteLine("  3 – Использовать папку с программой");
			Console.Write("Ваш выбор: ");
			string choice = Console.ReadLine();

			string dir = null;                                            // Переменная для пути
			if (choice == "1")                                           // Ручной ввод
			{
				Console.Write("Введите путь к каталогу: ");
				dir = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(dir)) return null;
			}
			else if (choice == "2")                                      // Через проводник
			{
				dir = SelectFolderViaDialog();
				if (string.IsNullOrEmpty(dir))
				{
					Console.WriteLine("Операция отменена.");
					return null;
				}
			}
			else if (choice == "3")                                      // Папка с программой
			{
				dir = AppDomain.CurrentDomain.BaseDirectory;
			}
			else
			{
				Console.WriteLine("Некорректный выбор.");
				return null;
			}

			try
			{
				if (!Directory.Exists(dir))                              // Если каталог не существует
				{
					Console.Write("Каталог не существует. Создать? (y/n): ");
					if (Console.ReadKey().KeyChar == 'y')                // Если 'y'
					{
						Directory.CreateDirectory(dir);                  // Создаём
						Console.WriteLine("\nКаталог создан.");
					}
					else
					{
						Console.WriteLine("\nОперация отменена.");
						return null;
					}
				}
				currentDataDirectory = dir;                              // Запоминаем каталог
				Console.Write("Введите имя файла (без расширения): ");
				string fileName = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(fileName))
				{
					Console.WriteLine("Имя не может быть пустым. Используем имя по умолчанию 'products'.");
					fileName = "products";
				}
				string ext = (format == FileFormat.Xml) ? ".xml" : ".txt";
				return Path.Combine(dir, fileName + ext);                // Возвращаем полный путь
			}
			catch (Exception ex)                                          // Ошибка создания
			{
				Console.ForegroundColor = ConsoleColor.Red;            // Красный
				Console.WriteLine($"\nНе удалось создать/использовать каталог: {ex.Message}");
				Console.ResetColor();
				return null;
			}
		}

		//========================================================= Сохранение в текстовый формат ================================================================//
		private static void SaveToTxtInternal(string path, List<Product> data)
		{
			using (StreamWriter writer = new StreamWriter(path))        // Открываем для записи
				foreach (Product p in data)                              // Перебираем товары
					writer.WriteLine($"{p.Name}|{p.Manufacturer}|{p.ShelfLife}|{p.Price}|{p.StockQuantity}"); // Запись строки с разделителями
		}

		//========================================================= Загрузка из текстового формата ================================================================//
		private static List<Product> LoadFromTxtInternal(string path)
		{
			List<Product> result = new List<Product>();
			using (StreamReader reader = new StreamReader(path))        // Открываем для чтения
			{
				string line;
				while ((line = reader.ReadLine()) != null)               // Читаем построчно
				{
					string[] parts = line.Split('|');                    // Разделяем по |
					if (parts.Length != 5) continue;                     // Если не 5 полей – пропускаем
					if (int.TryParse(parts[2], out int shelf) &&        // Парсим срок
						decimal.TryParse(parts[3].Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal price) && // Парсим цену
						int.TryParse(parts[4], out int stock))          // Парсим количество
					{
						result.Add(new Product                          // Добавляем товар
						{
							Name = parts[0],
							Manufacturer = parts[1],
							ShelfLife = shelf,
							Price = price,
							StockQuantity = stock
						});
					}
				}
			}
			return result;
		}

		//========================================================= Загрузка БД из файла с выбором ================================================================//
		private static void LoadFromFileWithSelection()
		{
			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;             // Жёлтый заголовок
			Console.WriteLine("=== ЗАГРУЗКА ИЗ ФАЙЛА (ДОПОЛНЕНИЕ) ===");
			Console.ResetColor();
			Console.WriteLine($"Текущий каталог: {currentDataDirectory}"); // Вывод текущего пути
			Console.ForegroundColor = ConsoleColor.Green;              // Зелёный подзаголовок
			Console.WriteLine("▶ Выберите способ указания каталога:");
			Console.ResetColor();
			Console.WriteLine("  1 – Использовать текущий каталог");
			Console.WriteLine("  2 – Выбрать другой каталог");
			Console.WriteLine("  3 – Использовать папку с программой (по умолчанию)");
			Console.ForegroundColor = ConsoleColor.DarkGray;            // Тёмно-серый
			Console.WriteLine("  (Enter – отмена)");
			Console.ResetColor();
			Console.Write("Ваш выбор: ");
			string choice = Console.ReadLine();                          // Считываем
			if (string.IsNullOrEmpty(choice))                            // Если пусто – отмена
				return;

			string selectedDirectory = currentDataDirectory;            // По умолчанию текущий

			if (choice == "3")                                           // Папка с программой
			{
				selectedDirectory = AppDomain.CurrentDomain.BaseDirectory;
				Console.ForegroundColor = ConsoleColor.Cyan;           // Голубой
				Console.WriteLine($"Выбрана папка с программой: {selectedDirectory}");
				Console.ResetColor();
			}
			else if (choice == "2")                                      // Выбрать другой каталог
			{
				Console.ForegroundColor = ConsoleColor.Green;          // Зелёный
				Console.WriteLine("▶ Выберите способ указания пути:");
				Console.ResetColor();
				Console.WriteLine("  1 – Ввести путь вручную");
				Console.WriteLine("  2 – Выбрать через проводник");
				Console.Write("Ваш выбор: ");
				string pathChoice = Console.ReadLine();                  // Считываем
				if (string.IsNullOrEmpty(pathChoice))                    // Если пусто – отмена
					return;
				if (pathChoice == "1")                                   // Ручной ввод
				{
					Console.Write("Введите путь к каталогу: ");
					string dir = Console.ReadLine();
					if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
						selectedDirectory = dir;
					else
					{
						Console.ForegroundColor = ConsoleColor.Red;    // Красный
						Console.WriteLine("Каталог не существует или путь не указан. Используем текущий каталог.");
						Console.ResetColor();
						selectedDirectory = currentDataDirectory;
					}
				}
				else if (pathChoice == "2")                             // Через проводник
				{
					string dir = SelectFolderViaDialog();
					if (!string.IsNullOrEmpty(dir))
						selectedDirectory = dir;
					else
					{
						Console.ForegroundColor = ConsoleColor.Red;    // Красный
						Console.WriteLine("Выбор отменён. Используем текущий каталог.");
						Console.ResetColor();
						selectedDirectory = currentDataDirectory;
					}
				}
				else
				{
					Console.WriteLine("Некорректный выбор. Используем текущий каталог.");
					selectedDirectory = currentDataDirectory;
				}
			}
			else if (choice != "1")                                      // Если не 1 и не 2, и не 3
			{
				Console.WriteLine("Некорректный выбор. Используем текущий каталог.");
				Console.ReadKey();
				return;
			}

			List<string> files = GetFilesByExtension(".xml", selectedDirectory)
						.Concat(GetFilesByExtension(".txt", selectedDirectory))
						.ToList();                                      // Список файлов в выбранном каталоге
			if (files.Count == 0)                                        // Если файлов нет
			{
				Console.ForegroundColor = ConsoleColor.Red;            // Красный
				Console.WriteLine($"В каталоге {selectedDirectory} нет XML или TXT файлов.");
				Console.ResetColor();
				Console.ReadKey();
				return;
			}

			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;             // Жёлтый заголовок
			Console.WriteLine("=== ЗАГРУЗКА ИЗ ФАЙЛА (ДОПОЛНЕНИЕ) ===");
			Console.ResetColor();
			Console.WriteLine($"Текущий каталог: {selectedDirectory}");
			Console.ForegroundColor = ConsoleColor.Green;              // Зелёный
			Console.WriteLine("▶ Список доступных файлов:");
			Console.ResetColor();
			for (int i = 0; i < files.Count; i++)                        // Вывод списка
				Console.WriteLine($"  {i + 1} – {files[i]}");
			Console.Write("Выберите номер файла для загрузки (0/Enter – отмена): "); // Запрос
			string idxStr = Console.ReadLine();
			if (string.IsNullOrEmpty(idxStr))                            // Если пусто – отмена
				return;
			if (!int.TryParse(idxStr, out int idx) || idx < 0 || idx > files.Count) // Некорректный
			{
				Console.WriteLine("Некорректный выбор.");
				Console.ReadKey();
				return;
			}
			if (idx == 0) return;                                        // Отмена

			string selectedFile = files[idx - 1];                        // Выбранный файл
			string fullPath = Path.Combine(selectedDirectory, selectedFile);
			FileFormat format = GetFormatFromExtension(selectedFile);   // Определяем формат

			try
			{
				List<Product> loaded;
				if (format == FileFormat.Xml)                            // Если XML
				{
					XmlSerializer serializer = new XmlSerializer(typeof(List<Product>));
					using (StreamReader reader = new StreamReader(fullPath))
						loaded = (List<Product>)serializer.Deserialize(reader);
				}
				else                                                     // Если TXT
					loaded = LoadFromTxtInternal(fullPath);

				if (loaded == null || loaded.Count == 0)                // Если пустой файл
				{
					Console.ForegroundColor = ConsoleColor.Red;        // Красный
					Console.WriteLine("Файл пуст или не соответствует ожидаемому формату (не содержит данных для базы).");
					Console.ResetColor();
					Console.ReadKey();
					return;
				}

				int before = products.Count;                             // Количество до загрузки
				products.AddRange(loaded);                               // Добавляем загруженные товары
				Console.ForegroundColor = ConsoleColor.Green;          // Зелёный успех
				Console.WriteLine($"Загружено {loaded.Count} товаров из файла.");
				Console.WriteLine($"Всего товаров в БД: {products.Count} (было {before})");
				Console.ResetColor();
			}
			catch (Exception ex)                                          // Ошибка загрузки
			{
				Console.ForegroundColor = ConsoleColor.Red;            // Красный
				Console.WriteLine($"Ошибка при загрузке: {ex.Message}");
				Console.WriteLine("Файл не подходит или повреждён.");
				Console.ResetColor();
			}
			Console.ReadKey();
		}

		//========================================================= Получение списка файлов по расширению ================================================================//
		private static List<string> GetFilesByExtension(string extension, string directory)
		{
			if (!Directory.Exists(directory))                            // Если каталог не существует
				return new List<string>();                               // Возвращаем пустой список
			return Directory.GetFiles(directory, "*" + extension)        // Получаем файлы
							.Select(Path.GetFileName)                    // Извлекаем имена
							.ToList();
		}
	}
}