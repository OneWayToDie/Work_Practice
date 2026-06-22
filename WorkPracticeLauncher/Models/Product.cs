namespace WorkPracticeLauncher.Models
{
	// Модель товара для консольного задания 2
	public class Product
	{
		public string Name { get; set; }
		public string Manufacturer { get; set; }
		public int ShelfLife { get; set; }
		public decimal Price { get; set; }
		public int StockQuantity { get; set; }
	}
}
