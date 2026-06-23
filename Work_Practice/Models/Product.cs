//========================================================= Библиотеки ================================================================//
using System.ComponentModel;          // INotifyPropertyChanged (уведомления об изменении свойств)
using System.Runtime.CompilerServices; // Атрибут CallerMemberName

namespace Work_Practice.Models
{
	//========================================================= Модель товара для базы данных (с поддержкой уведомлений) ================================================================//
	public class Product : INotifyPropertyChanged
	{
		//========================================================= Приватные поля ================================================================//
		private string name;              // Наименование товара
		private string manufacturer;      // Фирма-изготовитель
		private int shelfLife;            // Срок хранения (дни)
		private decimal price;            // Цена товара
		private int stockQuantity;        // Количество на складе

		//========================================================= Публичные свойства ================================================================//
		public string Name
		{
			get => name;                                             // Возвращаем значение поля
			set { name = value; OnPropertyChanged(); }              // При изменении уведомляем UI
		}

		public string Manufacturer
		{
			get => manufacturer;
			set { manufacturer = value; OnPropertyChanged(); }
		}

		public int ShelfLife
		{
			get => shelfLife;
			set { shelfLife = value; OnPropertyChanged(); }
		}

		public decimal Price
		{
			get => price;
			set { price = value; OnPropertyChanged(); }
		}

		public int StockQuantity
		{
			get => stockQuantity;
			set { stockQuantity = value; OnPropertyChanged(); }
		}

		//========================================================= Реализация INotifyPropertyChanged ================================================================//
		public event PropertyChangedEventHandler PropertyChanged; // Событие для уведомления об изменении свойств

		protected void OnPropertyChanged([CallerMemberName] string name = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); // Вызываем событие, если есть подписчики
	}
}
