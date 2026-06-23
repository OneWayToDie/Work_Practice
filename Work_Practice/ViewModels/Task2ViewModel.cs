//========================================================= Библиотеки ================================================================//
using System;                        // Базовые типы (decimal, int, string)
using System.Collections.Generic;     // Коллекции (List<T>)
using System.Collections.ObjectModel; // ObservableCollection<T>
using System.ComponentModel;          // INotifyPropertyChanged
using System.Linq;                    // LINQ-запросы (Where, ToList)
using System.Runtime.CompilerServices; // CallerMemberName
using System.Windows;                 // MessageBox (заменён на AppDialog)
using System.Windows.Input;           // ICommand
using Microsoft.Win32;                // OpenFileDialog (диалог выбора файла)
using Work_Practice.Commands;         // DelegateCommand
using Work_Practice.Models;           // Product
using Work_Practice.Services;         // ProductDataService
using Work_Practice.Views;            // AppDialog

namespace Work_Practice.ViewModels
{
	//========================================================= ViewModel для задания 2 (товары, файл, сортировка) ================================================================//
	public class Task2ViewModel : INotifyPropertyChanged
	{
		//========================================================= Поля ================================================================//
		private ObservableCollection<Product> products;                   // Основная коллекция товаров (все)
		private ObservableCollection<Product> filteredProducts;           // Отфильтрованная и отсортированная коллекция
		private readonly ProductDataService dataService;                 // Сервис для работы с XML-файлом
		private string currentFilePath = "products.xml";                // Полный путь к текущему файлу (используется, но не активно в коде)
		private string currentFileName = "products.xml";                // Имя текущего файла (для отображения)

		// Поля для нового товара (привязка к полям ввода)
		private string newName = "";
		private string newManufacturer = "";
		private int newShelfLife = 0;
		private decimal newPrice = 0;
		private int newStockQuantity = 0;

		// Поле для фильтра по количеству на складе
		private int minStockQuantity = 0;

		//========================================================= Публичные свойства ================================================================//
		public ObservableCollection<Product> Products
		{
			get => products;
			set { products = value; OnPropertyChanged(); }
		}

		public ObservableCollection<Product> FilteredProducts
		{
			get => filteredProducts;
			set { filteredProducts = value; OnPropertyChanged(); }
		}

		public string NewName { get => newName; set { newName = value; OnPropertyChanged(); } }
		public string NewManufacturer { get => newManufacturer; set { newManufacturer = value; OnPropertyChanged(); } }
		public int NewShelfLife { get => newShelfLife; set { newShelfLife = value; OnPropertyChanged(); } }
		public decimal NewPrice { get => newPrice; set { newPrice = value; OnPropertyChanged(); } }
		public int NewStockQuantity { get => newStockQuantity; set { newStockQuantity = value; OnPropertyChanged(); } }

		public int MinStockQuantity
		{
			get => minStockQuantity;
			set { minStockQuantity = value; OnPropertyChanged(); }
		}

		public string CurrentFileName
		{
			get => currentFileName;
			set { currentFileName = value; OnPropertyChanged(); }
		}

		//========================================================= Команды ================================================================//
		public ICommand SaveToFileCommand { get; }          // Сохранить в XML
		public ICommand LoadFromFileCommand { get; }        // Загрузить из XML
		public ICommand LoadSampleDataCommand { get; }     // Загрузить демо-данные (15 товаров)
		public ICommand AddProductCommand { get; }          // Добавить товар
		public ICommand ClearDatabaseCommand { get; }      // Очистить БД
		public ICommand SortAndFilterCommand { get; }      // Применить фильтр и сортировку

		//========================================================= Конструктор ================================================================//
		public Task2ViewModel()
		{
			dataService = new ProductDataService();                        // Инициализируем сервис

			List<Product> loaded = dataService.LoadFromXml();             // Пытаемся загрузить из XML
			if (loaded != null && loaded.Count > 0)                       // Если есть данные
			{
				Products = new ObservableCollection<Product>(loaded);
			}
			else
			{
				Products = new ObservableCollection<Product>();          // Иначе пустая коллекция
			}

			FilteredProducts = new ObservableCollection<Product>(Products); // Инициализация отфильтрованной коллекции

			SaveToFileCommand = new DelegateCommand(SaveToFile);         // Привязка команд
			LoadFromFileCommand = new DelegateCommand(LoadFromFile);
			LoadSampleDataCommand = new DelegateCommand(LoadSampleData);
			AddProductCommand = new DelegateCommand(AddProduct);
			ClearDatabaseCommand = new DelegateCommand(ClearDatabase);
			SortAndFilterCommand = new DelegateCommand(SortAndFilter);
		}

		//========================================================= Сохранение БД в XML ================================================================//
		private void SaveToFile()
		{
			bool success = dataService.SaveToXml(new List<Product>(Products)); // Сохраняем текущий список
			if (success)
				AppDialog.ShowInfo($"Данные сохранены в файл {currentFileName}");
			else
				AppDialog.ShowError("Ошибка при сохранении. Проверьте, что файл не заблокирован.");
		}

		//========================================================= Загрузка демо-данных ================================================================//
		private void LoadSampleData()
		{
			Products.Clear();                                             // Очищаем основную коллекцию
			List<Product> samples = new List<Product>                     // Создаём список 15 примеров
            {
				new Product { Name = "Молоко", Manufacturer = "Простоквашино", ShelfLife = 7, Price = 80m, StockQuantity = 50 },
				new Product { Name = "Хлеб", Manufacturer = "Дарницкий", ShelfLife = 3, Price = 45m, StockQuantity = 120 },
				new Product { Name = "Сыр", Manufacturer = "Hochland", ShelfLife = 30, Price = 350m, StockQuantity = 30 },
				new Product { Name = "Йогурт", Manufacturer = "Activia", ShelfLife = 14, Price = 60m, StockQuantity = 80 },
				new Product { Name = "Мясо", Manufacturer = "Мираторг", ShelfLife = 5, Price = 400m, StockQuantity = 20 },
				new Product { Name = "Рыба", Manufacturer = "Меридиан", ShelfLife = 4, Price = 300m, StockQuantity = 15 },
				new Product { Name = "Печенье", Manufacturer = "Юбилейное", ShelfLife = 180, Price = 90m, StockQuantity = 200 },
				new Product { Name = "Сок", Manufacturer = "Добрый", ShelfLife = 365, Price = 120m, StockQuantity = 100 },
				new Product { Name = "Вода", Manufacturer = "Святой источник", ShelfLife = 365, Price = 40m, StockQuantity = 500 },
				new Product { Name = "Макароны", Manufacturer = "Макфа", ShelfLife = 720, Price = 70m, StockQuantity = 150 },
				new Product { Name = "Крупа гречневая", Manufacturer = "Увелка", ShelfLife = 540, Price = 95m, StockQuantity = 80 },
				new Product { Name = "Масло подсолнечное", Manufacturer = "Золотая семечка", ShelfLife = 365, Price = 110m, StockQuantity = 60 },
				new Product { Name = "Кофе", Manufacturer = "Jacobs", ShelfLife = 720, Price = 450m, StockQuantity = 40 },
				new Product { Name = "Чай", Manufacturer = "Lipton", ShelfLife = 720, Price = 250m, StockQuantity = 70 },
				new Product { Name = "Конфеты", Manufacturer = "Красный Октябрь", ShelfLife = 180, Price = 200m, StockQuantity = 90 }
			};
			foreach (Product p in samples)
				Products.Add(p);                                          // Добавляем каждый товар
			FilterProductsAndSort();                                      // Применяем сортировку/фильтр (по умолчанию)
			AppDialog.ShowInfo("Загружены примеры (15 товаров).");
		}

		//========================================================= Загрузка из выбранного XML-файла ================================================================//
		private void LoadFromFile()
		{
			OpenFileDialog dialog = new OpenFileDialog                  // Диалог выбора файла
			{
				Filter = "XML файлы (*.xml)|*.xml|Все файлы (*.*)|*.*",
				Title = "Выберите файл для загрузки"
			};
			if (dialog.ShowDialog() != true)                            // Если пользователь отменил
				return;

			List<Product> loaded = dataService.LoadFromXml(dialog.FileName); // Загружаем из указанного файла
			if (loaded != null && loaded.Count > 0)                     // Если загрузка успешна
			{
				currentFilePath = dialog.FileName;                      // Сохраняем полный путь
				currentFileName = System.IO.Path.GetFileName(dialog.FileName); // Извлекаем имя
				dataService.FilePath = dialog.FileName;                 // Обновляем путь в сервисе
				OnPropertyChanged(nameof(CurrentFileName));            // Уведомляем UI
				Products.Clear();                                       // Очищаем основную коллекцию
				foreach (Product p in loaded)
					Products.Add(p);                                    // Добавляем загруженные товары
				FilterProductsAndSort();                                // Применяем фильтр/сортировку
				AppDialog.ShowInfo($"Данные загружены из файла: {currentFileName}");
			}
			else if (loaded != null)                                    // Если файл пустой
			{
				AppDialog.ShowWarning("Файл пуст. Данные не загружены.");
			}
			else                                                       // Ошибка загрузки
			{
				AppDialog.ShowError("Не удалось загрузить файл. Проверьте формат.");
			}
		}

		//========================================================= Добавление нового товара с валидацией ================================================================//
		private void AddProduct()
		{
			// Валидация полей
			if (string.IsNullOrWhiteSpace(NewName))
			{
				AppDialog.ShowError("Наименование товара не может быть пустым.");
				return;
			}
			if (string.IsNullOrWhiteSpace(NewManufacturer))
			{
				AppDialog.ShowError("Фирма-изготовитель не может быть пустой.");
				return;
			}
			if (NewPrice <= 0)
			{
				AppDialog.ShowError("Цена должна быть больше 0.");
				return;
			}
			if (NewShelfLife <= 0)
			{
				AppDialog.ShowError("Срок хранения должен быть больше 0 дней.");
				return;
			}
			if (NewStockQuantity < 0)
			{
				AppDialog.ShowError("Количество на складе не может быть отрицательным.");
				return;
			}

			Product newProduct = new Product                           // Создаём новый товар
			{
				Name = NewName.Trim(),
				Manufacturer = NewManufacturer.Trim(),
				ShelfLife = NewShelfLife,
				Price = NewPrice,
				StockQuantity = NewStockQuantity
			};
			Products.Add(newProduct);                                  // Добавляем в коллекцию
			FilterProductsAndSort();                                   // Обновляем отфильтрованный список

			// Очистка полей ввода
			NewName = "";
			NewManufacturer = "";
			NewShelfLife = 0;
			NewPrice = 0;
			NewStockQuantity = 0;

			AppDialog.ShowInfo("Товар добавлен.");
		}

		//========================================================= Очистка всей базы данных ================================================================//
		private void ClearDatabase()
		{
			Products.Clear();                                           // Очищаем основную коллекцию
			FilteredProducts.Clear();                                   // Очищаем отфильтрованную
			bool success = dataService.SaveToXml(new List<Product>());  // Сохраняем пустой список (файл становится пустым)
			if (success)
				AppDialog.ShowInfo("База данных очищена.");
			else
				AppDialog.ShowWarning("БД очищена из памяти, но не удалось сохранить в файл.");
		}

		//========================================================= Применение сортировки и фильтра ================================================================//
		private void SortAndFilter()
		{
			FilterProductsAndSort();                                   // Повторно применяем фильтр и сортировку
		}

		//========================================================= Фильтрация и сортировка (пузырьком) ================================================================//
		private void FilterProductsAndSort()
		{
			// 1. Фильтрация по количеству на складе
			List<Product> filtered = Products.Where(p => p.StockQuantity >= MinStockQuantity).ToList();

			// 2. Сортировка пузырьком по сроку хранения (по возрастанию)
			for (int i = 0; i < filtered.Count - 1; i++)
			{
				for (int j = 0; j < filtered.Count - i - 1; j++)
				{
					if (filtered[j].ShelfLife > filtered[j + 1].ShelfLife)
					{
						// Обмен элементов
						Product temp = filtered[j];
						filtered[j] = filtered[j + 1];
						filtered[j + 1] = temp;
					}
				}
			}

			// 3. Обновление отфильтрованной коллекции (создаём новые объекты, чтобы избежать проблем с привязкой)
			FilteredProducts.Clear();
			foreach (Product p in filtered)
			{
				FilteredProducts.Add(new Product
				{
					Name = p.Name,
					Manufacturer = p.Manufacturer,
					ShelfLife = p.ShelfLife,
					Price = p.Price,
					StockQuantity = p.StockQuantity
				});
			}
		}

		//========================================================= Реализация INotifyPropertyChanged ================================================================//
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}