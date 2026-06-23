using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Work_Practice.Models;

namespace Work_Practice.Services
{
	public class ProductDataService
	{
		private string filePath;

		public string FilePath
		{
			get => filePath;
			set => filePath = value;
		}

		public ProductDataService(string filePath = "products.xml")
		{
			this.filePath = filePath;
		}

		public bool SaveToXml(List<Product> products)
		{
			try
			{
				XmlSerializer serializer = new XmlSerializer(typeof(List<Product>));
				using (StreamWriter writer = new StreamWriter(filePath))
				{
					serializer.Serialize(writer, products);
				}
				return true;
			}
			catch
			{
				return false;
			}
		}

		public List<Product> LoadFromXml()
		{
			if (!File.Exists(filePath))
				return null;

			return LoadFromXml(filePath);
		}

		public List<Product> LoadFromXml(string filePath)
		{
			if (!File.Exists(filePath))
				return null;

			try
			{
				XmlSerializer serializer = new XmlSerializer(typeof(List<Product>));
				using (StreamReader reader = new StreamReader(filePath))
				{
					return (List<Product>)serializer.Deserialize(reader);
				}
			}
			catch
			{
				return null;
			}
		}
	}
}
