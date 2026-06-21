using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using WorkPracticeLauncher.Models;
using System.Windows.Forms;

namespace WorkPracticeLauncher.Tasks
{
	public static class Task2Solver
	{
		private static List<Product> products = new List<Product>();
		private static string currentFileName = "products.xml";
		private static string currentDataDirectory = AppDomain.CurrentDomain.BaseDirectory;
		private enum FileFormat { Xml, Txt }

		private static int lastWidth;
		private static int lastHeight;

		public static void Run()
		{
			lastWidth = Console.WindowWidth;
			lastHeight = Console.WindowHeight;

			bool exit = false;
			while (!exit)
			{
				CheckResizeAndRedraw();
				Console.Clear();
				ShowHeader();

				Console.WriteLine();
				Console.WriteLine("  1 – Внести товары в БД");
				Console.WriteLine("  2 – Посмотреть товары");
				Console.WriteLine("  3 – Удалить товар из БД (по индексу)");
				Console.WriteLine("  4 – Удалить всю БД");
				Console.WriteLine("  5 – Сохранить БД в файл");
				Console.WriteLine("  6 – Загрузить БД из файла (дополнить)");
				Console.WriteLine("  7 – Изменить каталог для сохранения/загрузки");
				Console.WriteLine("  0 – Назад в меню заданий");
				Console.WriteLine();

				if (Program.IsWindowsTerminal)
				{
					Console.ForegroundColor = ConsoleColor.Cyan;
					string[] tipLines = {
						"💡 Для удаления товаров из конкретного файла:",
						"   1) Загрузите его (пункт 6)",
						"   2) Удалите товары (пункт 3 или 4)",
						"   3) Сохраните обратно (пункт 5, выберите перезапись)"
					};
					int maxWidth = tipLines.Max(line => line.Length) + 2;
					string topLeft = "╭";
					string topRight = "╮";
					string bottomLeft = "╰";
					string bottomRight = "╯";
					string horizontal = "─";
					string vertical = "│";

					Console.WriteLine(topLeft + new string(horizontal[0], maxWidth) + topRight);
					foreach (var line in tipLines)
					{
						Console.WriteLine(vertical + " " + line.PadRight(maxWidth - 2) + " " + vertical);
					}
					Console.WriteLine(bottomLeft + new string(horizontal[0], maxWidth) + bottomRight);
					Console.ResetColor();
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.WriteLine("Для удаления товаров из конкретного файла:");
					Console.WriteLine("  1) Загрузите его (пункт 6)");
					Console.WriteLine("  2) Удалите товары (пункт 3 или 4)");
					Console.WriteLine("  3) Сохраните обратно (пункт 5, выберите перезапись)");
					Console.ResetColor();
				}

				Console.Write("Ваш выбор (или 0 для выхода): ");
				string input = Console.ReadLine();
				if (!int.TryParse(input, out int choice))
				{
					Console.WriteLine("Некорректный ввод. Нажмите любую клавишу...");
					Console.ReadKey();
					continue;
				}

				switch (choice)
				{
					case 1: AddProducts(); break;
					case 2: ViewProducts(); break;
					case 3: DeleteProduct(); break;
					case 4: DeleteAllProducts(); break;
					case 5: SaveWithOptions(); break;
					case 6: LoadFromFileWithSelection(); break;
					case 7: ChangeDataDirectory(); break;
					case 0: exit = true; break;
					default:
						Console.WriteLine("Некорректный выбор. Нажмите любую клавишу...");
						Console.ReadKey();
						break;
				}
			}
		}

		private static void ShowHeader()
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("=== ЗАДАНИЕ 2: УПРАВЛЕНИЕ БАЗОЙ ТОВАРОВ ===");
			Console.ResetColor();
			Console.WriteLine($"Всего товаров: {products.Count}");
			Console.WriteLine($"Текущий файл: {currentFileName}");
			Console.WriteLine($"Каталог данных: {currentDataDirectory}");
		}

		private static void CheckResizeAndRedraw()
		{
			if (Console.WindowWidth != lastWidth || Console.WindowHeight != lastHeight)
			{
				lastWidth = Console.WindowWidth;
				lastHeight = Console.WindowHeight;
				Console.Clear();
				ShowHeader();
			}
		}

		// -------------------- Остальные методы (без InputHelper) --------------------

		private static void ChangeDataDirectory()
		{
			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("=== ИЗМЕНЕНИЕ КАТАЛОГА ДАННЫХ ===");
			Console.ResetColor();
			Console.WriteLine($"Текущий каталог: {currentDataDirectory}");
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("▶ Выберите способ указания пути:");
			Console.ResetColor();
			Console.WriteLine("  1 – Ввести путь вручную");
			Console.WriteLine("  2 – Выбрать через проводник");
			Console.WriteLine("  3 – Использовать папку с программой");
			Console.Write("Ваш выбор: ");
			string choice = Console.ReadLine();

			string newPath = null;
			if (choice == "1")
			{
				Console.Write("Введите новый путь: ");
				newPath = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(newPath))
				{
					Console.WriteLine("Изменение отменено.");
					Console.ReadKey();
					return;
				}
			}
			else if (choice == "2")
			{
				newPath = SelectFolderViaDialog();
				if (string.IsNullOrEmpty(newPath))
				{
					Console.WriteLine("Изменение отменено.");
					Console.ReadKey();
					return;
				}
			}
			else if (choice == "3")
			{
				newPath = AppDomain.CurrentDomain.BaseDirectory;
			}
			else
			{
				Console.WriteLine("Некорректный выбор.");
				Console.ReadKey();
				return;
			}

			if (!Directory.Exists(newPath))
			{
				Console.Write("Каталог не существует. Создать? (y/n): ");
				if (Console.ReadKey().KeyChar == 'y')
				{
					try
					{
						Directory.CreateDirectory(newPath);
						Console.WriteLine("\nКаталог создан.");
					}
					catch (Exception ex)
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine($"\nНе удалось создать каталог: {ex.Message}");
						Console.ResetColor();
						Console.ReadKey();
						return;
					}
				}
				else
				{
					Console.WriteLine("\nИзменение отменено.");
					Console.ReadKey();
					return;
				}
			}
			currentDataDirectory = newPath;
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"\nКаталог изменён на: {currentDataDirectory}");
			Console.ResetColor();
			Console.ReadKey();
		}

		private static string SelectFolderViaDialog()
		{
			try
			{
				using (var dialog = new FolderBrowserDialog())
				{
					dialog.Description = "Выберите каталог для данных";
					dialog.SelectedPath = currentDataDirectory;
					dialog.ShowNewFolderButton = true;
					if (dialog.ShowDialog() == DialogResult.OK)
						return dialog.SelectedPath;
					return null;
				}
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Ошибка при открытии диалога: {ex.Message}");
				Console.ResetColor();
				return null;
			}
		}

		private static void ShowProduct(Product p)
		{
			Console.WriteLine($"  Наименование: {p.Name}");
			Console.WriteLine($"  Фирма: {p.Manufacturer}");
			Console.WriteLine($"  Срок хранения: {p.ShelfLife} дней");
			Console.WriteLine($"  Цена: {p.Price:F2} руб.");
			Console.WriteLine($"  Количество на складе: {p.StockQuantity}");
		}

		private static void AddProducts()
		{
			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("=== ДОБАВЛЕНИЕ ТОВАРОВ ===");
			Console.ResetColor();

			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Для выхода в меню введите пустую строку в поле 'Наименование'.");
			Console.ResetColor();

			int added = 0;
			while (true)
			{
				Console.Write("Наименование: ");
				string name = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(name)) break;

				Console.Write("Фирма: ");
				string manufacturer = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(manufacturer)) break;

				int shelfLife;
				while (true)
				{
					Console.Write("Срок хранения (дни): ");
					string input = Console.ReadLine();
					if (string.IsNullOrWhiteSpace(input))
					{
						Console.WriteLine("Выход в меню.");
						return;
					}
					if (int.TryParse(input, out shelfLife) && shelfLife > 0)
						break;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Некорректный ввод, повторите.");
					Console.ResetColor();
				}

				decimal price;
				while (true)
				{
					Console.Write("Цена: ");
					string input = Console.ReadLine();
					if (string.IsNullOrWhiteSpace(input))
					{
						Console.WriteLine("Выход в меню.");
						return;
					}
					string normalized = input.Replace(',', '.');
					if (decimal.TryParse(normalized, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out price) && price > 0)
						break;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Некорректный ввод, введите положительное число (например, 10.5 или 10,5).");
					Console.ResetColor();
				}

				int stock;
				while (true)
				{
					Console.Write("Количество на складе: ");
					string input = Console.ReadLine();
					if (string.IsNullOrWhiteSpace(input))
					{
						Console.WriteLine("Выход в меню.");
						return;
					}
					if (int.TryParse(input, out stock) && stock >= 0)
						break;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Некорректный ввод, повторите.");
					Console.ResetColor();
				}

				var newProduct = new Product
				{
					Name = name,
					Manufacturer = manufacturer,
					ShelfLife = shelfLife,
					Price = price,
					StockQuantity = stock
				};
				products.Add(newProduct);
				added++;

				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine($"Товар добавлен. Всего в БД: {products.Count}");
				Console.ResetColor();

				Console.WriteLine("Нажмите Enter для продолжения или ESC для выхода.");
				var key = Console.ReadKey(true).Key;
				if (key == ConsoleKey.Escape)
				{
					Console.WriteLine("Выход в меню...");
					return;
				}
			}

			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"\nДобавлено {added} товаров.");
			Console.ResetColor();
			Console.WriteLine("Нажмите любую клавишу...");
			Console.ReadKey();
		}

		private static void EditLastProduct()
		{
			if (products.Count == 0) return;
			var lastProduct = products[products.Count - 1];

			bool editing = true;
			while (editing)
			{
				Console.Clear();
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("=== РЕДАКТИРОВАНИЕ ПОСЛЕДНЕЙ ЗАПИСИ ===");
				Console.ResetColor();
				Console.WriteLine("Текущие значения:");
				ShowProduct(lastProduct);
				Console.WriteLine();

				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("▶ Выберите поле для редактирования (1-5) или ESC для отмены:");
				Console.ResetColor();
				Console.WriteLine("  1 – Наименование");
				Console.WriteLine("  2 – Фирма");
				Console.WriteLine("  3 – Срок хранения");
				Console.WriteLine("  4 – Цена");
				Console.WriteLine("  5 – Количество на складе");
				Console.Write("Ваш выбор (или Esc для отмены): ");

				var fieldKey = Console.ReadKey(true).Key;
				if (fieldKey == ConsoleKey.Escape)
				{
					editing = false;
					break;
				}

				bool updated = false;
				switch (fieldKey)
				{
					case ConsoleKey.D1:
						Console.Write("Новое наименование: ");
						string newName = Console.ReadLine();
						if (!string.IsNullOrWhiteSpace(newName))
						{
							lastProduct.Name = newName;
							updated = true;
						}
						break;
					case ConsoleKey.D2:
						Console.Write("Новая фирма: ");
						string newManufacturer = Console.ReadLine();
						if (!string.IsNullOrWhiteSpace(newManufacturer))
						{
							lastProduct.Manufacturer = newManufacturer;
							updated = true;
						}
						break;
					case ConsoleKey.D3:
						Console.Write("Новый срок хранения (дни): ");
						string newShelf = Console.ReadLine();
						if (int.TryParse(newShelf, out int newShelfLife) && newShelfLife > 0)
						{
							lastProduct.ShelfLife = newShelfLife;
							updated = true;
						}
						else
						{
							Console.ForegroundColor = ConsoleColor.Red;
							Console.WriteLine("Некорректный ввод.");
							Console.ResetColor();
						}
						break;
					case ConsoleKey.D4:
						Console.Write("Новая цена: ");
						string newPrice = Console.ReadLine();
						if (!string.IsNullOrWhiteSpace(newPrice))
						{
							string normalized = newPrice.Replace(',', '.');
							if (decimal.TryParse(normalized, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal newPriceValue) && newPriceValue > 0)
							{
								lastProduct.Price = newPriceValue;
								updated = true;
							}
							else
							{
								Console.ForegroundColor = ConsoleColor.Red;
								Console.WriteLine("Некорректный ввод, цена должна быть положительным числом.");
								Console.ResetColor();
							}
						}
						break;
					case ConsoleKey.D5:
						Console.Write("Новое количество на складе: ");
						string newStock = Console.ReadLine();
						if (int.TryParse(newStock, out int newStockValue) && newStockValue >= 0)
						{
							lastProduct.StockQuantity = newStockValue;
							updated = true;
						}
						else
						{
							Console.ForegroundColor = ConsoleColor.Red;
							Console.WriteLine("Некорректный ввод.");
							Console.ResetColor();
						}
						break;
					default:
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("Некорректный выбор.");
						Console.ResetColor();
						continue;
				}

				if (updated)
				{
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("Запись обновлена.");
					Console.ResetColor();
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("Изменение не выполнено.");
					Console.ResetColor();
				}
				Console.ReadKey();
			}
		}

		private static void ViewProducts()
		{
			if (products.Count == 0)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("База данных пуста.");
				Console.ResetColor();
				Console.ReadKey();
				return;
			}

			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("=== ПРОСМОТР ТОВАРОВ ===");
			Console.ResetColor();

			int windowWidth = Console.WindowWidth;
			int nameWidth = Math.Min(20, windowWidth / 5);
			int mfrWidth = Math.Min(15, windowWidth / 5);
			int shelfWidth = Math.Min(8, windowWidth / 5);
			int priceWidth = Math.Min(10, windowWidth / 5);
			int stockWidth = Math.Min(8, windowWidth / 5);

			nameWidth = Math.Max(nameWidth, "Наименование".Length + 1);
			mfrWidth = Math.Max(mfrWidth, "Фирма".Length + 1);
			shelfWidth = Math.Max(shelfWidth, "Срок".Length + 1);
			priceWidth = Math.Max(priceWidth, "Цена".Length + 1);
			stockWidth = Math.Max(stockWidth, "Кол-во".Length + 1);

			int total = nameWidth + mfrWidth + shelfWidth + priceWidth + stockWidth + 8;
			if (total > windowWidth)
			{
				double ratio = (windowWidth - 8) / (double)(total - 8);
				nameWidth = (int)(nameWidth * ratio);
				mfrWidth = (int)(mfrWidth * ratio);
				shelfWidth = (int)(shelfWidth * ratio);
				priceWidth = (int)(priceWidth * ratio);
				stockWidth = (int)(stockWidth * ratio);
				nameWidth = Math.Max(nameWidth, 3);
				mfrWidth = Math.Max(mfrWidth, 3);
				shelfWidth = Math.Max(shelfWidth, 3);
				priceWidth = Math.Max(priceWidth, 3);
				stockWidth = Math.Max(stockWidth, 3);
			}

			Console.WriteLine($"{"Наименование".PadRight(nameWidth)} {"Фирма".PadRight(mfrWidth)} {"Срок".PadRight(shelfWidth)} {"Цена".PadRight(priceWidth)} {"Кол-во".PadRight(stockWidth)}");
			Console.WriteLine(new string('-', nameWidth + mfrWidth + shelfWidth + priceWidth + stockWidth + 4));

			Console.WriteLine("Выберите сортировку:");
			Console.WriteLine("  1 – По сроку хранения (по возрастанию)");
			Console.WriteLine("  2 – По цене (по возрастанию)");
			Console.WriteLine("  3 – По наименованию (по алфавиту)");
			Console.WriteLine("  4 – Без сортировки (в порядке добавления)");
			Console.Write("Ваш выбор: ");
			string input = Console.ReadLine();
			int sortMode = 0;
			if (!int.TryParse(input, out sortMode) || sortMode < 1 || sortMode > 4)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Некорректный ввод, применяем сортировку по умолчанию (по сроку).");
				Console.ResetColor();
				sortMode = 1;
			}

			IEnumerable<Product> sorted;
			switch (sortMode)
			{
				case 1:
					sorted = products.OrderBy(p => p.ShelfLife);
					break;
				case 2:
					sorted = products.OrderBy(p => p.Price);
					break;
				case 3:
					sorted = products.OrderBy(p => p.Name);
					break;
				default:
					sorted = products;
					break;
			}

			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("=== СПИСОК ТОВАРОВ ===");
			Console.ResetColor();
			Console.WriteLine($"{"Наименование".PadRight(nameWidth)} {"Фирма".PadRight(mfrWidth)} {"Срок".PadRight(shelfWidth)} {"Цена".PadRight(priceWidth)} {"Кол-во".PadRight(stockWidth)}");
			Console.WriteLine(new string('-', nameWidth + mfrWidth + shelfWidth + priceWidth + stockWidth + 4));
			foreach (var p in sorted)
			{
				Console.WriteLine($"{p.Name.PadRight(nameWidth)} {p.Manufacturer.PadRight(mfrWidth)} {p.ShelfLife.ToString().PadRight(shelfWidth)} {p.Price.ToString("F2").PadRight(priceWidth)} {p.StockQuantity.ToString().PadRight(stockWidth)}");
			}
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine($"\nВсего товаров: {products.Count}");
			Console.ResetColor();
			Console.WriteLine("\nНажмите любую клавишу для возврата...");
			Console.ReadKey();
		}

		private static void DeleteProduct()
		{
			if (products.Count == 0)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("База данных пуста, удалять нечего.");
				Console.ResetColor();
				Console.ReadKey();
				return;
			}

			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("=== УДАЛЕНИЕ ТОВАРА (ПО ИНДЕКСУ) ===");
			Console.ResetColor();
			Console.WriteLine("Список товаров:");
			Console.WriteLine("  Индекс  |  Наименование");
			Console.WriteLine("  -----------------------");
			for (int i = 0; i < products.Count; i++)
				Console.WriteLine($"  {i + 1,-7} | {products[i].Name}");
			Console.WriteLine();

			Console.Write($"Введите индекс товара (1..{products.Count}) для удаления: ");
			string idxStr = Console.ReadLine();
			if (!int.TryParse(idxStr, out int idx) || idx < 1 || idx > products.Count)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Некорректный индекс.");
				Console.ResetColor();
				Console.ReadKey();
				return;
			}

			var removedProduct = products[idx - 1];
			products.RemoveAt(idx - 1);
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"Удалён товар: {removedProduct.Name} (индекс {idx})");
			Console.ResetColor();
			Console.WriteLine($"В БД осталось {products.Count} товаров.");
			Console.WriteLine("Нажмите любую клавишу...");
			Console.ReadKey();
		}

		private static void DeleteAllProducts()
		{
			if (products.Count == 0)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("База данных уже пуста.");
				Console.ResetColor();
				Console.ReadKey();
				return;
			}

			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("=== УДАЛЕНИЕ ВСЕЙ БАЗЫ ДАННЫХ ===");
			Console.ResetColor();
			Console.WriteLine($"Внимание! Вы собираетесь удалить все {products.Count} товаров из памяти.");
			Console.Write("Подтвердите удаление (введите 'ДА'): ");
			string confirm = Console.ReadLine();
			if (confirm != "ДА")
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("Удаление отменено.");
				Console.ResetColor();
				Console.WriteLine("Нажмите любую клавишу...");
				Console.ReadKey();
				return;
			}

			products.Clear();
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("База данных в памяти полностью очищена.");
			Console.ResetColor();

			string fullPath = Path.Combine(currentDataDirectory, currentFileName);
			if (File.Exists(fullPath))
			{
				Console.Write("Удалить файл с диска тоже? (y/n): ");
				string deleteFile = Console.ReadLine();
				if (deleteFile?.ToLower() == "y")
				{
					try
					{
						File.Delete(fullPath);
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine($"Файл {currentFileName} удалён.");
						Console.ResetColor();
					}
					catch (Exception ex)
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine($"Не удалось удалить файл: {ex.Message}");
						Console.ResetColor();
					}
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("Файл остался на диске. Вы можете перезаписать его при следующем сохранении.");
					Console.ResetColor();
				}
			}
			else
				Console.WriteLine("Файл не найден на диске.");

			Console.WriteLine("Нажмите любую клавишу...");
			Console.ReadKey();
		}

		private static void SaveWithOptions()
		{
			if (products.Count == 0)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Нет товаров для сохранения.");
				Console.ResetColor();
				Console.ReadKey();
				return;
			}

			var xmlFiles = GetFilesByExtension(".xml", currentDataDirectory);
			var txtFiles = GetFilesByExtension(".txt", currentDataDirectory);
			var allFiles = xmlFiles.Concat(txtFiles).ToList();

			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("=== СОХРАНЕНИЕ БАЗЫ ДАННЫХ ===");
			Console.ResetColor();
			Console.WriteLine($"Текущий каталог: {currentDataDirectory}");
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("▶ Выберите действие:");
			Console.ResetColor();
			Console.WriteLine("  1 – Сохранить в новый файл");
			if (allFiles.Count > 0)
			{
				Console.WriteLine("  2 – Дополнить существующий файл");
				Console.WriteLine("  3 – Перезаписать существующий файл");
			}
			Console.WriteLine("  0 – Отмена");
			Console.Write("Ваш выбор: ");
			string choiceStr = Console.ReadLine();
			if (!int.TryParse(choiceStr, out int choice))
			{
				Console.WriteLine("Некорректный ввод.");
				Console.ReadKey();
				return;
			}

			if (choice == 0) return;

			if (choice == 1)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("▶ Выберите формат файла:");
				Console.ResetColor();
				Console.WriteLine("  1 – XML (совместим с WPF-приложением)");
				Console.WriteLine("  2 – TXT (текстовый, для ручного просмотра)");
				Console.Write("Ваш выбор: ");
				string formatChoice = Console.ReadLine();
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

				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("▶ Выберите способ указания имени и пути:");
				Console.ResetColor();
				Console.WriteLine("  1 – Ввести имя файла (сохранится в текущий каталог)");
				Console.WriteLine("  2 – Выбрать каталог и имя файла через проводник");
				Console.WriteLine("  3 – Использовать папку с программой");
				Console.Write("Ваш выбор: ");
				string pathChoice = Console.ReadLine();
				string fileName = "";
				string targetPath = "";

				if (pathChoice == "1")
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
				else if (pathChoice == "2")
				{
					string selectedDir = SelectFolderViaDialog();
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
					currentDataDirectory = selectedDir;
				}
				else if (pathChoice == "3")
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

				SaveToFile(targetPath, format, append: false);
				if (File.Exists(targetPath))
					currentFileName = Path.GetFileName(targetPath);
				Console.ReadKey();
				return;
			}

			if (allFiles.Count == 0)
			{
				Console.WriteLine("Нет существующих файлов. Сначала создайте новый файл.");
				Console.ReadKey();
				return;
			}

			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("▶ Доступные файлы:");
			Console.ResetColor();
			for (int i = 0; i < allFiles.Count; i++)
				Console.WriteLine($"  {i + 1} – {allFiles[i]}");
			Console.Write("Введите номер файла: ");
			string fileIdxStr = Console.ReadLine();
			if (!int.TryParse(fileIdxStr, out int fileIdx) || fileIdx < 1 || fileIdx > allFiles.Count)
			{
				Console.WriteLine("Некорректный выбор.");
				Console.ReadKey();
				return;
			}
			string selectedFile = allFiles[fileIdx - 1];
			string fullPath = Path.Combine(currentDataDirectory, selectedFile);
			FileFormat existingFormat = GetFormatFromExtension(selectedFile);

			if (choice == 2)
				SaveToFile(fullPath, existingFormat, append: true);
			else if (choice == 3)
				SaveToFile(fullPath, existingFormat, append: false);
			else
				Console.WriteLine("Некорректный выбор.");
			Console.ReadKey();
		}

		private static FileFormat GetFormatFromExtension(string fileName)
		{
			if (fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
				return FileFormat.Xml;
			else if (fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
				return FileFormat.Txt;
			else
				return FileFormat.Xml;
		}

		private static void SaveToFile(string path, FileFormat format, bool append)
		{
			try
			{
				List<Product> existing = new List<Product>();
				if (append && File.Exists(path))
				{
					try
					{
						if (format == FileFormat.Xml)
						{
							var serializer = new XmlSerializer(typeof(List<Product>));
							using (var reader = new StreamReader(path))
								existing = (List<Product>)serializer.Deserialize(reader);
						}
						else
							existing = LoadFromTxtInternal(path);
					}
					catch
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("Не удалось прочитать существующий файл. Будет выполнена перезапись.");
						Console.ResetColor();
						append = false;
					}
				}

				List<Product> toSave = append ? existing.Concat(products).ToList() : products;

				if (format == FileFormat.Xml)
				{
					var serializer = new XmlSerializer(typeof(List<Product>));
					using (var writer = new StreamWriter(path))
						serializer.Serialize(writer, toSave);
				}
				else
					SaveToTxtInternal(path, toSave);

				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine($"База данных сохранена в файл: {path}");
				Console.WriteLine($"Всего записей: {toSave.Count}");
				if (format == FileFormat.Txt)
					Console.WriteLine("Внимание: TXT-файл не совместим с WPF-приложением (используйте XML для кросс-платформенности).");
				Console.ResetColor();
			}
			catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Нет прав доступа на создание файла в этой папке.");
				Console.ResetColor();
				Console.Write("Хотите выбрать другой каталог для сохранения? (y/n): ");
				if (Console.ReadKey().KeyChar == 'y')
				{
					Console.WriteLine();
					string newPath = RequestSavePath(format);
					if (!string.IsNullOrEmpty(newPath))
						SaveToFile(newPath, format, append);
					return;
				}
				Console.WriteLine();
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Ошибка при сохранении: {ex.Message}");
				Console.ResetColor();
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Ошибка при сохранении: {ex.Message}");
				Console.ResetColor();
			}
		}

		private static string RequestSavePath(FileFormat format)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("▶ Выберите способ указания пути:");
			Console.ResetColor();
			Console.WriteLine("  1 – Ввести путь вручную");
			Console.WriteLine("  2 – Выбрать через проводник");
			Console.WriteLine("  3 – Использовать папку с программой");
			Console.Write("Ваш выбор: ");
			string choice = Console.ReadLine();

			string dir = null;
			if (choice == "1")
			{
				Console.Write("Введите путь к каталогу: ");
				dir = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(dir)) return null;
			}
			else if (choice == "2")
			{
				dir = SelectFolderViaDialog();
				if (string.IsNullOrEmpty(dir))
				{
					Console.WriteLine("Операция отменена.");
					return null;
				}
			}
			else if (choice == "3")
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
				if (!Directory.Exists(dir))
				{
					Console.Write("Каталог не существует. Создать? (y/n): ");
					if (Console.ReadKey().KeyChar == 'y')
					{
						Directory.CreateDirectory(dir);
						Console.WriteLine("\nКаталог создан.");
					}
					else
					{
						Console.WriteLine("\nОперация отменена.");
						return null;
					}
				}
				currentDataDirectory = dir;
				Console.Write("Введите имя файла (без расширения): ");
				string fileName = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(fileName))
				{
					Console.WriteLine("Имя не может быть пустым. Используем имя по умолчанию 'products'.");
					fileName = "products";
				}
				string ext = (format == FileFormat.Xml) ? ".xml" : ".txt";
				return Path.Combine(dir, fileName + ext);
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"\nНе удалось создать/использовать каталог: {ex.Message}");
				Console.ResetColor();
				return null;
			}
		}

		private static void SaveToTxtInternal(string path, List<Product> data)
		{
			using (var writer = new StreamWriter(path))
				foreach (var p in data)
					writer.WriteLine($"{p.Name}|{p.Manufacturer}|{p.ShelfLife}|{p.Price}|{p.StockQuantity}");
		}

		private static List<Product> LoadFromTxtInternal(string path)
		{
			var result = new List<Product>();
			using (var reader = new StreamReader(path))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					var parts = line.Split('|');
					if (parts.Length != 5) continue;
					if (int.TryParse(parts[2], out int shelf) &&
						decimal.TryParse(parts[3].Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal price) &&
						int.TryParse(parts[4], out int stock))
					{
						result.Add(new Product
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

		private static void LoadFromFileWithSelection()
		{
			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("=== ЗАГРУЗКА ИЗ ФАЙЛА (ДОПОЛНЕНИЕ) ===");
			Console.ResetColor();
			Console.WriteLine($"Текущий каталог: {currentDataDirectory}");
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("▶ Выберите способ указания каталога:");
			Console.ResetColor();
			Console.WriteLine("  1 – Использовать текущий каталог");
			Console.WriteLine("  2 – Выбрать другой каталог");
			Console.WriteLine("  3 – Использовать папку с программой (по умолчанию)");
			Console.Write("Ваш выбор: ");
			string choice = Console.ReadLine();

			string selectedDirectory = currentDataDirectory;

			if (choice == "3")
			{
				selectedDirectory = AppDomain.CurrentDomain.BaseDirectory;
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.WriteLine($"Выбрана папка с программой: {selectedDirectory}");
				Console.ResetColor();
			}
			else if (choice == "2")
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("▶ Выберите способ указания пути:");
				Console.ResetColor();
				Console.WriteLine("  1 – Ввести путь вручную");
				Console.WriteLine("  2 – Выбрать через проводник");
				Console.Write("Ваш выбор: ");
				string pathChoice = Console.ReadLine();
				if (pathChoice == "1")
				{
					Console.Write("Введите путь к каталогу: ");
					string dir = Console.ReadLine();
					if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
						selectedDirectory = dir;
					else
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("Каталог не существует или путь не указан. Используем текущий каталог.");
						Console.ResetColor();
						selectedDirectory = currentDataDirectory;
					}
				}
				else if (pathChoice == "2")
				{
					string dir = SelectFolderViaDialog();
					if (!string.IsNullOrEmpty(dir))
						selectedDirectory = dir;
					else
					{
						Console.ForegroundColor = ConsoleColor.Red;
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
			else if (choice != "1")
			{
				Console.WriteLine("Некорректный выбор. Используем текущий каталог.");
				Console.ReadKey();
				return;
			}

			var files = GetFilesByExtension(".xml", selectedDirectory)
						.Concat(GetFilesByExtension(".txt", selectedDirectory))
						.ToList();
			if (files.Count == 0)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"В каталоге {selectedDirectory} нет XML или TXT файлов.");
				Console.ResetColor();
				Console.ReadKey();
				return;
			}

			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("=== ЗАГРУЗКА ИЗ ФАЙЛА (ДОПОЛНЕНИЕ) ===");
			Console.ResetColor();
			Console.WriteLine($"Текущий каталог: {selectedDirectory}");
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("▶ Список доступных файлов:");
			Console.ResetColor();
			for (int i = 0; i < files.Count; i++)
				Console.WriteLine($"  {i + 1} – {files[i]}");
			Console.Write("Выберите номер файла для загрузки (0 – отмена): ");
			string idxStr = Console.ReadLine();
			if (!int.TryParse(idxStr, out int idx) || idx < 0 || idx > files.Count)
			{
				Console.WriteLine("Некорректный выбор.");
				Console.ReadKey();
				return;
			}
			if (idx == 0) return;

			string selectedFile = files[idx - 1];
			string fullPath = Path.Combine(selectedDirectory, selectedFile);
			FileFormat format = GetFormatFromExtension(selectedFile);

			try
			{
				List<Product> loaded;
				if (format == FileFormat.Xml)
				{
					var serializer = new XmlSerializer(typeof(List<Product>));
					using (var reader = new StreamReader(fullPath))
						loaded = (List<Product>)serializer.Deserialize(reader);
				}
				else
					loaded = LoadFromTxtInternal(fullPath);

				if (loaded == null || loaded.Count == 0)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Файл пуст или не соответствует ожидаемому формату (не содержит данных для базы).");
					Console.ResetColor();
					Console.ReadKey();
					return;
				}

				int before = products.Count;
				products.AddRange(loaded);
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine($"Загружено {loaded.Count} товаров из файла.");
				Console.WriteLine($"Всего товаров в БД: {products.Count} (было {before})");
				Console.ResetColor();
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Ошибка при загрузке: {ex.Message}");
				Console.WriteLine("Файл не подходит или повреждён.");
				Console.ResetColor();
			}
			Console.ReadKey();
		}

		private static List<string> GetFilesByExtension(string extension, string directory)
		{
			if (!Directory.Exists(directory))
				return new List<string>();
			return Directory.GetFiles(directory, "*" + extension)
							.Select(Path.GetFileName)
							.ToList();
		}
	}
}