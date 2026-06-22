using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Work_Practice.Commands;
using Work_Practice.Models;
using Work_Practice.Services;
using Work_Practice.Views;

namespace Work_Practice.ViewModels
{
	public class Task2ViewModel : INotifyPropertyChanged
	{
		private ObservableCollection<Product> products;
		private ObservableCollection<Product> filteredProducts;
		private readonly ProductDataService dataService;

		// Поля для нового товара
		private string newName = "";
		private string newManufacturer = "";
		private int newShelfLife = 0;
		private decimal newPrice = 0;
		private int newStockQuantity = 0;

		// Поле для фильтра
		private int minStockQuantity = 0;

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

		public ICommand SaveToFileCommand { get; }
		public ICommand LoadFromFileCommand { get; }
		public ICommand LoadSampleDataCommand { get; }
		public ICommand AddProductCommand { get; }
		public ICommand ClearDatabaseCommand { get; }
		public ICommand SortAndFilterCommand { get; }

		public Task2ViewModel()
		{
			dataService = new ProductDataService();

			List<Product> loaded = dataService.LoadFromXml();
			if (loaded != null && loaded.Count > 0)
			{
				Products = new ObservableCollection<Product>(loaded);
			}
			else
			{
				Products = new ObservableCollection<Product>();
			}

			FilteredProducts = new ObservableCollection<Product>(Products);

			SaveToFileCommand = new DelegateCommand(SaveToFile);
			LoadFromFileCommand = new DelegateCommand(LoadFromFile);
			LoadSampleDataCommand = new DelegateCommand(LoadSampleData);
			AddProductCommand = new DelegateCommand(AddProduct);
			ClearDatabaseCommand = new DelegateCommand(ClearDatabase);
			SortAndFilterCommand = new DelegateCommand(SortAndFilter);
		}

		private void SaveToFile()
		{
			bool success = dataService.SaveToXml(new List<Product>(Products));
			if (success)
				AppDialog.ShowInfo("Данные сохранены в файл products.xml");
			else
				AppDialog.ShowError("Ошибка при сохранении. Проверьте, что файл не заблокирован.");
		}

		private void LoadSampleData()
		{
			Products.Clear();
			List<Product> samples = new List<Product>
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
				Products.Add(p);
			FilterProductsAndSort();
			AppDialog.ShowInfo("Загружены примеры (15 товаров).");
		}

		private void LoadFromFile()
		{
			OpenFileDialog dialog = new OpenFileDialog
			{
				Filter = "XML файлы (*.xml)|*.xml|Все файлы (*.*)|*.*",
				Title = "Выберите файл для загрузки"
			};
			if (dialog.ShowDialog() != true)
				return;

			List<Product> loaded = dataService.LoadFromXml(dialog.FileName);
			if (loaded != null && loaded.Count > 0)
			{
				Products.Clear();
				foreach (Product p in loaded)
					Products.Add(p);
				FilterProductsAndSort();
				AppDialog.ShowInfo($"Данные загружены из файла: {System.IO.Path.GetFileName(dialog.FileName)}");
			}
			else if (loaded != null)
			{
				AppDialog.ShowWarning("Файл пуст. Данные не загружены.");
			}
			else
			{
				AppDialog.ShowError("Не удалось загрузить файл. Проверьте формат.");
			}
		}

		private void AddProduct()
		{
			// Проверка наименования
			if (string.IsNullOrWhiteSpace(NewName))
			{
				AppDialog.ShowError("Наименование товара не может быть пустым.");
				return;
			}
			// Проверка фирмы
			if (string.IsNullOrWhiteSpace(NewManufacturer))
			{
				AppDialog.ShowError("Фирма-изготовитель не может быть пустой.");
				return;
			}
			// Проверка цены
			if (NewPrice <= 0)
			{
				AppDialog.ShowError("Цена должна быть больше 0.");
				return;
			}
			// Проверка срока хранения
			if (NewShelfLife <= 0)
			{
				AppDialog.ShowError("Срок хранения должен быть больше 0 дней.");
				return;
			}
			// Проверка количества на складе
			if (NewStockQuantity < 0)
			{
				AppDialog.ShowError("Количество на складе не может быть отрицательным.");
				return;
			}

			Product newProduct = new Product
			{
				Name = NewName.Trim(),
				Manufacturer = NewManufacturer.Trim(),
				ShelfLife = NewShelfLife,
				Price = NewPrice,
				StockQuantity = NewStockQuantity
			};
			Products.Add(newProduct);
			FilterProductsAndSort();

			// Очистка полей
			NewName = "";
			NewManufacturer = "";
			NewShelfLife = 0;
			NewPrice = 0;
			NewStockQuantity = 0;

			AppDialog.ShowInfo("Товар добавлен.");
		}

		private void ClearDatabase()
		{
			Products.Clear();
			FilteredProducts.Clear();
			bool success = dataService.SaveToXml(new List<Product>());
			if (success)
				AppDialog.ShowInfo("База данных очищена.");
			else
				AppDialog.ShowWarning("БД очищена из памяти, но не удалось сохранить в файл.");
		}

		private void SortAndFilter()
		{
			FilterProductsAndSort();
		}

		private void FilterProductsAndSort()
		{
			// Фильтр по остатку
			List<Product> filtered = Products.Where(p => p.StockQuantity >= MinStockQuantity).ToList();

			// Сортировка пузырьком по сроку хранения (по возрастанию)
			for (int i = 0; i < filtered.Count - 1; i++)
			{
				for (int j = 0; j < filtered.Count - i - 1; j++)
				{
					if (filtered[j].ShelfLife > filtered[j + 1].ShelfLife)
					{
						Product temp = filtered[j];
						filtered[j] = filtered[j + 1];
						filtered[j + 1] = temp;
					}
				}
			}

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

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}