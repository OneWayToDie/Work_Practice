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
		private static readonly string DataFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "products.xml");

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
				Console.WriteLine();
				Console.WriteLine("  1 – Внести товары в БД");
				Console.WriteLine("  2 – Посмотреть товары");
				Console.WriteLine("  3 – Удалить товар из БД");
				Console.WriteLine("  4 – Удалить всю БД");
				Console.WriteLine("  5 – Сохранить БД в файл (XML)");
				Console.WriteLine("  6 – Загрузить БД из файла (XML)");
				Console.WriteLine("  0 – Назад в меню заданий");
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
						SaveToXml();
						break;
					case 6:
						LoadFromXml();
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

		private static string ReadLineOrEscape(string prompt = "")
		{
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.Write(prompt);
			Console.ResetColor();
			string input = "";
			while (true)
			{
				var key = Console.ReadKey(true);
				if (key.Key == ConsoleKey.Escape)
				{
					Console.WriteLine();
					return null;
				}
				if (key.Key == ConsoleKey.Enter)
				{
					Console.WriteLine();
					return input;
				}
				if (key.Key == ConsoleKey.Backspace && input.Length > 0)
				{
					input = input.Remove(input.Length - 1);
					Console.Write("\b \b");
				}
				else if (!char.IsControl(key.KeyChar))
				{
					input += key.KeyChar;
					Console.Write(key.KeyChar);
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
				string name = ReadLineOrEscape("Наименование: ");
				if (name == null) break;
				if (string.IsNullOrWhiteSpace(name))
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Наименование не может быть пустым.");
					Console.ResetColor();
					continue;
				}

				string manufacturer = ReadLineOrEscape("Фирма: ");
				if (manufacturer == null) break;

				int shelfLife;
				while (true)
				{
					string input = ReadLineOrEscape("Срок хранения (дни): ");
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
					string input = ReadLineOrEscape("Цена: ");
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
					string input = ReadLineOrEscape("Количество на складе: ");
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
									string newName = ReadLineOrEscape("Новое наименование: ");
									if (newName != null && !string.IsNullOrWhiteSpace(newName))
									{
										lastProduct.Name = newName;
										updated = true;
									}
									break;
								case ConsoleKey.D2:
									string newManufacturer = ReadLineOrEscape("Новая фирма: ");
									if (newManufacturer != null)
									{
										lastProduct.Manufacturer = newManufacturer;
										updated = true;
									}
									break;
								case ConsoleKey.D3:
									string newShelf = ReadLineOrEscape("Новый срок хранения (дни): ");
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
									string newPrice = ReadLineOrEscape("Новая цена: ");
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
									string newStock = ReadLineOrEscape("Новое количество на складе: ");
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
			Console.WriteLine("=== УДАЛЕНИЕ ТОВАРА ===");
			Console.ResetColor();
			Console.WriteLine("Выберите способ удаления:");
			Console.WriteLine("  1 – По наименованию (удаляются все совпадения)");
			Console.WriteLine("  2 – По индексу (в порядке добавления, начиная с 1)");
			Console.Write("Ваш выбор: ");

			string input = Console.ReadLine();
			if (input == "1")
			{
				Console.Write("Введите наименование товара для удаления: ");
				string name = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(name))
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Некорректное наименование.");
					Console.ResetColor();
					Console.ReadKey();
					return;
				}
				int removed = products.RemoveAll(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
				if (removed > 0)
				{
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine($"Удалено {removed} товаров с наименованием '{name}'.");
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"Товары с наименованием '{name}' не найдены.");
				}
				Console.ResetColor();
			}
			else if (input == "2")
			{
				Console.Write($"Введите индекс товара (1..{products.Count}): ");
				if (!int.TryParse(Console.ReadLine(), out int idx) || idx < 1 || idx > products.Count)
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
				Console.WriteLine($"Удалён товар: {removedProduct.Name} ({removedProduct.Manufacturer})");
				Console.ResetColor();
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Некорректный выбор.");
				Console.ResetColor();
			}
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine($"\nВ БД осталось {products.Count} товаров.");
			Console.ResetColor();
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
			Console.WriteLine($"Внимание! Вы собираетесь удалить все {products.Count} товаров.");
			Console.Write("Подтвердите удаление (введите 'ДА'): ");
			string confirm = Console.ReadLine();
			if (confirm == "ДА")
			{
				products.Clear();
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("База данных полностью очищена.");
				Console.ResetColor();
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("Удаление отменено.");
				Console.ResetColor();
			}
			Console.WriteLine("Нажмите любую клавишу...");
			Console.ReadKey();
		}

		private static void SaveToXml()
		{
			try
			{
				var serializer = new XmlSerializer(typeof(List<Product>));
				using (var writer = new StreamWriter(DataFile))
				{
					serializer.Serialize(writer, products);
				}
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine($"База данных успешно сохранена в файл:\n{DataFile}");
				Console.ResetColor();
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Ошибка при сохранении: {ex.Message}");
				Console.ResetColor();
			}
			Console.WriteLine("Нажмите любую клавишу...");
			Console.ReadKey();
		}

		private static void LoadFromXml()
		{
			if (!File.Exists(DataFile))
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Файл не найден: {DataFile}");
				Console.ResetColor();
				Console.WriteLine("Нажмите любую клавишу...");
				Console.ReadKey();
				return;
			}

			try
			{
				var serializer = new XmlSerializer(typeof(List<Product>));
				using (var reader = new StreamReader(DataFile))
				{
					var loaded = (List<Product>)serializer.Deserialize(reader);
					if (loaded != null)
					{
						products = loaded;
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine($"Загружено {products.Count} товаров из файла:\n{DataFile}");
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("Файл пуст или повреждён.");
					}
					Console.ResetColor();
				}
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Ошибка при загрузке: {ex.Message}");
				Console.ResetColor();
			}
			Console.WriteLine("Нажмите любую клавишу...");
			Console.ReadKey();
		}
	}
}