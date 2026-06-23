//========================================================= Библиотеки ================================================================//
using System.Collections.Generic;    // Коллекции (List<T>)
using System.IO;                     // Работа с файлами и потоками (File, StreamWriter, StreamReader)
using System.Xml.Serialization;      // Сериализация в XML (XmlSerializer)
using Work_Practice.Models;          // Модель товара (Product)

namespace Work_Practice.Services
{
	//========================================================= Класс сервиса для работы с XML-файлом товаров ================================================================//
	public class ProductDataService
	{
		//========================================================= Поля ================================================================//
		private string filePath;                // Путь к XML-файлу (по умолчанию products.xml)

		//========================================================= Свойства ================================================================//
		public string FilePath
		{
			get => filePath;                    // Возвращаем текущий путь
			set => filePath = value;            // Позволяет изменить путь (например, при загрузке другого файла)
		}

		//========================================================= Конструктор ================================================================//
		public ProductDataService(string filePath = "products.xml")
		{
			this.filePath = filePath;           // Устанавливаем путь к файлу (по умолчанию products.xml)
		}

		//========================================================= Сохранение списка товаров в XML ================================================================//
		public bool SaveToXml(List<Product> products)
		{
			try
			{
				XmlSerializer serializer = new XmlSerializer(typeof(List<Product>)); // Создаём сериализатор для списка товаров
				using (StreamWriter writer = new StreamWriter(filePath))             // Открываем поток для записи в файл
				{
					serializer.Serialize(writer, products);                          // Сериализуем список в XML и записываем в файл
				}
				return true;                                                         // Успешно
			}
			catch
			{
				return false;                                                        // В случае ошибки возвращаем false
			}
		}

		//========================================================= Загрузка списка товаров из XML (с путём по умолчанию) ================================================================//
		public List<Product> LoadFromXml()
		{
			if (!File.Exists(filePath))                                              // Если файл не существует
				return null;                                                         // Возвращаем null

			return LoadFromXml(filePath);                                            // Вызываем перегрузку с указанием пути
		}

		//========================================================= Загрузка списка товаров из указанного XML-файла ================================================================//
		public List<Product> LoadFromXml(string filePath)
		{
			if (!File.Exists(filePath))                                              // Если файл не найден
				return null;                                                         // Возвращаем null

			try
			{
				XmlSerializer serializer = new XmlSerializer(typeof(List<Product>)); // Создаём сериализатор
				using (StreamReader reader = new StreamReader(filePath))             // Открываем поток для чтения файла
				{
					return (List<Product>)serializer.Deserialize(reader);            // Десериализуем XML в список товаров и возвращаем
				}
			}
			catch
			{
				return null;                                                         // При ошибке возвращаем null
			}
		}
	}
}