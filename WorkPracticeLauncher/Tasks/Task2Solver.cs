using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using WorkPracticeLauncher.Models;

namespace WorkPracticeLauncher.Tasks
{
	public static class Task2Solver
	{
		private static List<Product> products = new List<Product>();
		private static string currentFileName = "products.xml";
		private enum FileFormat { Xml, Txt }

		public static void Run()
		{
			bool exit = false;
			while (!exit)
			{
				Console.Clear();
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("=== ЗАДАНИЕ 2: УПРАВЛЕНИЕ БАЗОЙ ТОВАРОВ ===");
				Console.ResetColor();
				Console.WriteLine($"Всего товаров: {products.Count}");
				Console.WriteLine($"Текущий файл: {currentFileName}");
				Console.WriteLine();
				Console.WriteLine("  1 – Внести товары в БД");
				Console.WriteLine("  2 – Посмотреть товары");
				Console.WriteLine("  3 – Удалить товар из БД (по индексу)");
				Console.WriteLine("  4 – Удалить всю БД");
				Console.WriteLine("  5 – Сохранить БД в файл");
				Console.WriteLine("  6 – Загрузить БД из файла (дополнить)");
				Console.WriteLine("  0 – Назад в меню заданий");
				Console.WriteLine();

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

				string input = InputHelper.ReadLine("Ваш выбор: ");
				if (!int.TryParse(input, out int choice))
				{
					Console.WriteLine("Некорректный ввод. Нажмите любую клавишу...");
					Console.ReadKey();
					continue;
				}

				switch (choice)
				{
					case 1:
						AddProducts();
						break;
					case 2:
						ViewProducts();
						break;
					case 3:
						DeleteProduct();
						break;
					case 4:
						DeleteAllProducts();
						break;
					case 5:
						SaveWithOptions();
						break;
					case 6:
						LoadFromFileWithSelection();
						break;
					case 0:
						exit = true;
						break;
					default:
						Console.WriteLine("Некорректный выбор. Нажмите любую клавишу...");
						Console.ReadKey();
						break;
				}
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

		// -------------------- Добавление товаров --------------------
		private static void AddProducts()
		{
			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("=== ДОБАВЛЕНИЕ ТОВАРОВ ===");
			Console.ResetColor();

			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Для выхода в меню нажмите ESC на любом поле ввода.");
			Console.ResetColor();

			int added = 0;
			while (true)
			{
				string name = InputHelper.ReadLine("Наименование: ", allowEscape: true);
				if (name == null) break;
				if (string.IsNullOrWhiteSpace(name))
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Наименование не может быть пустым.");
					Console.ResetColor();
					continue;
				}

				string manufacturer = InputHelper.ReadLine("Фирма: ", allowEscape: true);
				if (manufacturer == null) break;

				int shelfLife;
				while (true)
				{
					string input = InputHelper.ReadLine("Срок хранения (дни): ", allowEscape: true);
					if (input == null) return;
					if (int.TryParse(input, out shelfLife) && shelfLife > 0)
						break;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Некорректный ввод, повторите.");
					Console.ResetColor();
				}

				decimal price;
				while (true)
				{
					string input = InputHelper.ReadLine("Цена: ", allowEscape: true);
					if (input == null) return;
					if (decimal.TryParse(input, out price) && price > 0)
						break;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Некорректный ввод, повторите.");
					Console.ResetColor();
				}

				int stock;
				while (true)
				{
					string input = InputHelper.ReadLine("Количество на складе: ", allowEscape: true);
					if (input == null) return;
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

				bool innerLoop = true;
				while (innerLoop)
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("\n[1] Продолжить ввод  |  [2] Редактировать последнюю запись  |  [ESC] Выйти");
					Console.ResetColor();
					var actionKey = Console.ReadKey(true).Key;

					if (actionKey == ConsoleKey.D1)
					{
						innerLoop = false;
					}
					else if (actionKey == ConsoleKey.D2)
					{
						var lastProduct = products[products.Count - 1];
						Console.Clear();
						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.WriteLine("=== РЕДАКТИРОВАНИЕ ПОСЛЕДНЕЙ ЗАПИСИ ===");
						Console.ResetColor();
						Console.WriteLine("Текущие значения:");
						ShowProduct(lastProduct);
						Console.WriteLine();

						bool editing = true;
						while (editing)
						{
							Console.ForegroundColor = ConsoleColor.Yellow;
							Console.WriteLine("Выберите поле для редактирования (1-5) или ESC для отмены:");
							Console.ResetColor();
							Console.WriteLine("  1 – Наименование");
							Console.WriteLine("  2 – Фирма");
							Console.WriteLine("  3 – Срок хранения");
							Console.WriteLine("  4 – Цена");
							Console.WriteLine("  5 – Количество на складе");
							Console.Write("Ваш выбор: ");

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
									string newName = InputHelper.ReadLine("Новое наименование: ", allowEscape: true);
									if (newName != null && !string.IsNullOrWhiteSpace(newName))
									{
										lastProduct.Name = newName;
										updated = true;
									}
									break;
								case ConsoleKey.D2:
									string newManufacturer = InputHelper.ReadLine("Новая фирма: ", allowEscape: true);
									if (newManufacturer != null)
									{
										lastProduct.Manufacturer = newManufacturer;
										updated = true;
									}
									break;
								case ConsoleKey.D3:
									string newShelf = InputHelper.ReadLine("Новый срок хранения (дни): ", allowEscape: true);
									if (newShelf != null && int.TryParse(newShelf, out int newShelfLife) && newShelfLife > 0)
									{
										lastProduct.ShelfLife = newShelfLife;
										updated = true;
									}
									else if (newShelf != null)
									{
										Console.ForegroundColor = ConsoleColor.Red;
										Console.WriteLine("Некорректный ввод.");
										Console.ResetColor();
									}
									break;
								case ConsoleKey.D4:
									string newPrice = InputHelper.ReadLine("Новая цена: ", allowEscape: true);
									if (newPrice != null && decimal.TryParse(newPrice, out decimal newPriceValue) && newPriceValue > 0)
									{
										lastProduct.Price = newPriceValue;
										updated = true;
									}
									else if (newPrice != null)
									{
										Console.ForegroundColor = ConsoleColor.Red;
										Console.WriteLine("Некорректный ввод.");
										Console.ResetColor();
									}
									break;
								case ConsoleKey.D5:
									string newStock = InputHelper.ReadLine("Новое количество на складе: ", allowEscape: true);
									if (newStock != null && int.TryParse(newStock, out int newStockValue) && newStockValue >= 0)
									{
										lastProduct.StockQuantity = newStockValue;
										updated = true;
									}
									else if (newStock != null)
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
								Console.WriteLine("Текущие значения:");
								ShowProduct(lastProduct);
								Console.WriteLine();
							}
						}
					}
					else if (actionKey == ConsoleKey.Escape)
					{
						Console.WriteLine("Выход в меню...");
						return;
					}
				}
			}

			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"\nДобавлено {added} товаров.");
			Console.ResetColor();
			Console.WriteLine("Нажмите любую клавишу...");
			Console.ReadKey();
		}

		// -------------------- Просмотр --------------------
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
			Console.WriteLine("Выберите сортировку:");
			Console.WriteLine("  1 – По сроку хранения (по возрастанию)");
			Console.WriteLine("  2 – По цене (по возрастанию)");
			Console.WriteLine("  3 – По наименованию (по алфавиту)");
			Console.WriteLine("  4 – Без сортировки (в порядке добавления)");
			string input = InputHelper.ReadLine("Ваш выбор: ");
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
			Console.WriteLine($"{"Наименование",-20} {"Фирма",-15} {"Срок",-8} {"Цена",-10} {"Кол-во",-8}");
			Console.WriteLine(new string('-', 20 + 15 + 8 + 10 + 8));
			foreach (var p in sorted)
			{
				Console.WriteLine($"{p.Name,-20} {p.Manufacturer,-15} {p.ShelfLife,-8} {p.Price,-10:F2} {p.StockQuantity,-8}");
			}
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine($"\nВсего товаров: {products.Count}");
			Console.ResetColor();
			Console.WriteLine("\nНажмите любую клавишу для возврата...");
			Console.ReadKey();
		}

		// -------------------- Удаление (по индексу) --------------------
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
			{
				Console.WriteLine($"  {i + 1,-7} | {products[i].Name}");
			}
			Console.WriteLine();

			string idxStr = InputHelper.ReadLine($"Введите индекс товара (1..{products.Count}) для удаления: ");
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

		// -------------------- Удаление всей БД --------------------
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
			string confirm = InputHelper.ReadLine("Подтвердите удаление (введите 'ДА'): ");
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

			string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, currentFileName);
			if (File.Exists(fullPath))
			{
				string deleteFile = InputHelper.ReadLine("Удалить файл с диска тоже? (y/n): ");
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
			{
				Console.WriteLine("Файл не найден на диске (возможно, он уже был удалён).");
			}

			Console.WriteLine("Нажмите любую клавишу...");
			Console.ReadKey();
		}

		// -------------------- Сохранение с выбором --------------------
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

			var xmlFiles = GetFilesByExtension(".xml");
			var txtFiles = GetFilesByExtension(".txt");
			var allFiles = xmlFiles.Concat(txtFiles).ToList();

			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("=== СОХРАНЕНИЕ БАЗЫ ДАННЫХ ===");
			Console.ResetColor();
			Console.WriteLine("Выберите действие:");
			Console.WriteLine("  1 – Сохранить в новый файл");
			if (allFiles.Count > 0)
			{
				Console.WriteLine("  2 – Дополнить существующий файл");
				Console.WriteLine("  3 – Перезаписать существующий файл");
			}
			Console.WriteLine("  0 – Отмена");
			string choiceStr = InputHelper.ReadLine("Ваш выбор: ");
			if (!int.TryParse(choiceStr, out int choice))
			{
				Console.WriteLine("Некорректный ввод.");
				Console.ReadKey();
				return;
			}

			if (choice == 0) return;

			if (choice == 1)
			{
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.WriteLine("Выберите формат файла:");
				Console.WriteLine("  1 – XML (совместим с WPF-приложением)");
				Console.WriteLine("  2 – TXT (текстовый, для ручного просмотра)");
				Console.ResetColor();
				string formatChoice = InputHelper.ReadLine("Ваш выбор: ");
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

				string fileName = InputHelper.ReadLine("Введите имя файла (без расширения): ");
				if (string.IsNullOrWhiteSpace(fileName))
				{
					Console.WriteLine("Имя не может быть пустым.");
					Console.ReadKey();
					return;
				}
				string ext = (format == FileFormat.Xml) ? ".xml" : ".txt";
				string fullName = fileName + ext;
				string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fullName);

				SaveToFile(path, format, append: false);
				currentFileName = fullName;
				Console.ReadKey();
				return;
			}

			if (allFiles.Count == 0)
			{
				Console.WriteLine("Нет существующих файлов. Сначала создайте новый файл.");
				Console.ReadKey();
				return;
			}

			Console.WriteLine("Доступные файлы:");
			for (int i = 0; i < allFiles.Count; i++)
			{
				Console.WriteLine($"  {i + 1} – {allFiles[i]}");
			}
			string fileIdxStr = InputHelper.ReadLine("Введите номер файла: ");
			if (!int.TryParse(fileIdxStr, out int fileIdx) || fileIdx < 1 || fileIdx > allFiles.Count)
			{
				Console.WriteLine("Некорректный выбор.");
				Console.ReadKey();
				return;
			}
			string selectedFile = allFiles[fileIdx - 1];
			string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, selectedFile);
			FileFormat existingFormat = GetFormatFromExtension(selectedFile);

			if (choice == 2)
			{
				SaveToFile(fullPath, existingFormat, append: true);
			}
			else if (choice == 3)
			{
				SaveToFile(fullPath, existingFormat, append: false);
			}
			else
			{
				Console.WriteLine("Некорректный выбор.");
			}
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
							{
								existing = (List<Product>)serializer.Deserialize(reader);
							}
						}
						else
						{
							existing = LoadFromTxtInternal(path);
						}
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
					{
						serializer.Serialize(writer, toSave);
					}
				}
				else
				{
					SaveToTxtInternal(path, toSave);
				}

				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine($"База данных сохранена в файл: {path}");
				Console.WriteLine($"Всего записей: {toSave.Count}");
				if (format == FileFormat.Txt)
				{
					Console.WriteLine("Внимание: TXT-файл не совместим с WPF-приложением (используйте XML для кросс-платформенности).");
				}
				Console.ResetColor();
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Ошибка при сохранении: {ex.Message}");
				Console.ResetColor();
			}
		}

		// -------------------- Работа с TXT --------------------
		private static void SaveToTxtInternal(string path, List<Product> data)
		{
			using (var writer = new StreamWriter(path))
			{
				foreach (var p in data)
				{
					writer.WriteLine($"{p.Name}|{p.Manufacturer}|{p.ShelfLife}|{p.Price}|{p.StockQuantity}");
				}
			}
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
						decimal.TryParse(parts[3], out decimal price) &&
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

		// -------------------- Загрузка --------------------
		private static void LoadFromFileWithSelection()
		{
			var files = GetFilesByExtension(".xml").Concat(GetFilesByExtension(".txt")).ToList();
			if (files.Count == 0)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("В папке программы нет XML или TXT файлов.");
				Console.ResetColor();
				Console.ReadKey();
				return;
			}

			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("=== ЗАГРУЗКА ИЗ ФАЙЛА (ДОПОЛНЕНИЕ) ===");
			Console.ResetColor();
			Console.WriteLine("Список доступных файлов:");
			for (int i = 0; i < files.Count; i++)
			{
				Console.WriteLine($"  {i + 1} – {files[i]}");
			}
			string idxStr = InputHelper.ReadLine("Выберите номер файла для загрузки (0 – отмена): ");
			if (!int.TryParse(idxStr, out int idx) || idx < 0 || idx > files.Count)
			{
				Console.WriteLine("Некорректный выбор.");
				Console.ReadKey();
				return;
			}
			if (idx == 0) return;

			string selectedFile = files[idx - 1];
			string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, selectedFile);
			FileFormat format = GetFormatFromExtension(selectedFile);

			try
			{
				List<Product> loaded;
				if (format == FileFormat.Xml)
				{
					var serializer = new XmlSerializer(typeof(List<Product>));
					using (var reader = new StreamReader(fullPath))
					{
						loaded = (List<Product>)serializer.Deserialize(reader);
					}
				}
				else
				{
					loaded = LoadFromTxtInternal(fullPath);
				}

				if (loaded == null || loaded.Count == 0)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Файл пуст или не содержит данных.");
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

		// -------------------- Вспомогательные методы для файлов --------------------
		private static List<string> GetFilesByExtension(string extension)
		{
			var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*" + extension)
								 .Select(Path.GetFileName)
								 .ToList();
			return files;
		}
	}
}