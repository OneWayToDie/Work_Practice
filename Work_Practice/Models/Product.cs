using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Work_Practice.Models
{
	public class Product : INotifyPropertyChanged
	{
		private string _name;
		private string _manufacturer;
		private int _shelfLife;
		private decimal _price;
		private int _stockQuantity;

		public string Name
		{
			get => _name;
			set { _name = value; OnPropertyChanged(); }
		}
		public string Manufacturer
		{
			get => _manufacturer;
			set { _manufacturer = value; OnPropertyChanged(); }
		}
		public int ShelfLife
		{
			get => _shelfLife;
			set { _shelfLife = value; OnPropertyChanged(); }
		}
		public decimal Price
		{
			get => _price;
			set { _price = value; OnPropertyChanged(); }
		}
		public int StockQuantity
		{
			get => _stockQuantity;
			set { _stockQuantity = value; OnPropertyChanged(); }
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}
