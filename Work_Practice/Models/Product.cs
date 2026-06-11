namespace Work_Practice.Models
{
	public class Product
	{
		public string Name { get; set; }
		public string Manufacturer { get; set; }
		public int ShelfLife { get; set; }      // срок хранения (дни)
		public decimal Price { get; set; }
		public int StockQuantity { get; set; }   // количество на складе
	}
}
