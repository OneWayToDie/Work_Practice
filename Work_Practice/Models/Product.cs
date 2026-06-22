using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Work_Practice.Models
{
	public class Product : INotifyPropertyChanged
	{
		private string name;
		private string manufacturer;
		private int shelfLife;
		private decimal price;
		private int stockQuantity;

		public string Name
		{
			get => name;
			set { name = value; OnPropertyChanged(); }
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

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}
