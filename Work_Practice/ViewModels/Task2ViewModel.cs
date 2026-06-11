using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Work_Practice.Commands;
using Work_Practice.Models;
using Work_Practice.Services;

namespace Work_Practice.ViewModels
{
	public class Task2ViewModel : INotifyPropertyChanged
	{
		private ObservableCollection<Product> _products;
		private ObservableCollection<Product> _filteredProducts;
		private readonly ProductDataService _dataService;

		// Поля для нового товара
		private string _newName = "";
		private string _newManufacturer = "";
		private int _newShelfLife = 0;
		private decimal _newPrice = 0;
		private int _newStockQuantity = 0;

		// Поле для фильтра
		private int _minStockQuantity = 0;

		public ObservableCollection<Product> Products
		{
			get => _products;
			set { _products = value; OnPropertyChanged(); }
		}

		public ObservableCollection<Product> FilteredProducts
		{
			get => _filteredProducts;
			set { _filteredProducts = value; OnPropertyChanged(); }
		}

		public string NewName { get => _newName; set { _newName = value; OnPropertyChanged(); } }
		public string NewManufacturer { get => _newManufacturer; set { _newManufacturer = value; OnPropertyChanged(); } }
		public int NewShelfLife { get => _newShelfLife; set { _newShelfLife = value; OnPropertyChanged(); } }
		public decimal NewPrice { get => _newPrice; set { _newPrice = value; OnPropertyChanged(); } }
		public int NewStockQuantity { get => _newStockQuantity; set { _newStockQuantity = value; OnPropertyChanged(); } }

		public int MinStockQuantity
		{
			get => _minStockQuantity;
			set { _minStockQuantity = value; OnPropertyChanged(); }
		}

		public ICommand SaveToFileCommand { get; }
		public ICommand LoadFromFileCommand { get; }
		public ICommand AddProductCommand { get; }
		public ICommand ClearDatabaseCommand { get; }
		public ICommand SortAndFilterCommand { get; }

		public Task2ViewModel()
		{
			_dataService = new ProductDataService();

			var loaded = _dataService.LoadFromXml();
			if (loaded != null && loaded.Count > 0)
			{
				Products = new ObservableCollection<Product>(loaded);
			}
			else
			{
				// Тестовые данные (15 штук)
				Products = new ObservableCollection<Product>
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
			}

			FilteredProducts = new ObservableCollection<Product>(Products);

			SaveToFileCommand = new DelegateCommand(SaveToFile);
			LoadFromFileCommand = new DelegateCommand(LoadFromFile);
			AddProductCommand = new DelegateCommand(AddProduct);
			ClearDatabaseCommand = new DelegateCommand(ClearDatabase);
			SortAndFilterCommand = new DelegateCommand(SortAndFilter);
		}

		private void SaveToFile()
		{
			_dataService.SaveToXml(new List<Product>(Products));
			MessageBox.Show("Данные сохранены в файл products.xml");
		}

		private void LoadFromFile()
		{
			var loaded = _dataService.LoadFromXml();
			if (loaded != null && loaded.Count > 0)
			{
				Products.Clear();
				foreach (var p in loaded)
					Products.Add(p);
				FilterProductsAndSort();
				MessageBox.Show("Данные загружены из файла");
			}
			else
			{
				MessageBox.Show("Файл не найден или пуст");
			}
		}

		private void AddProduct()
		{
			if (string.IsNullOrWhiteSpace(NewName) || string.IsNullOrWhiteSpace(NewManufacturer))
			{
				MessageBox.Show("Заполните наименование и фирму.");
				return;
			}
			if (NewPrice <= 0 || NewShelfLife < 0 || NewStockQuantity < 0)
			{
				MessageBox.Show("Цена должна быть >0, срок хранения и количество >=0.");
				return;
			}
			var newProduct = new Product
			{
				Name = NewName,
				Manufacturer = NewManufacturer,
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

			MessageBox.Show("Товар добавлен.");
		}

		private void ClearDatabase()
		{
			Products.Clear();
			FilteredProducts.Clear();
			_dataService.SaveToXml(new List<Product>());
			MessageBox.Show("База данных очищена.");
		}

		private void SortAndFilter()
		{
			FilterProductsAndSort();
		}

		private void FilterProductsAndSort()
		{
			// Фильтр по остатку
			var filtered = Products.Where(p => p.StockQuantity >= MinStockQuantity).ToList();

			// Сортировка пузырьком по сроку хранения (по возрастанию)
			for (int i = 0; i < filtered.Count - 1; i++)
			{
				for (int j = 0; j < filtered.Count - i - 1; j++)
				{
					if (filtered[j].ShelfLife > filtered[j + 1].ShelfLife)
					{
						var temp = filtered[j];
						filtered[j] = filtered[j + 1];
						filtered[j + 1] = temp;
					}
				}
			}

			FilteredProducts.Clear();
			foreach (var p in filtered)
				FilteredProducts.Add(p);
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}