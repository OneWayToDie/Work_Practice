using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Work_Practice.Models;

namespace Work_Practice.Services
{
	public class ProductDataService
	{
		private readonly string _filePath;

		public ProductDataService(string filePath = "products.xml")
		{
			_filePath = filePath;
		}

		public void SaveToXml(List<Product> products)
		{
			var serializer = new XmlSerializer(typeof(List<Product>));
			using (var writer = new StreamWriter(_filePath))
			{
				serializer.Serialize(writer, products);
			}
		}

		public List<Product> LoadFromXml()
		{
			if (!File.Exists(_filePath))
				return null;

			var serializer = new XmlSerializer(typeof(List<Product>));
			using (var reader = new StreamReader(_filePath))
			{
				return (List<Product>)serializer.Deserialize(reader);
			}
		}
	}
}