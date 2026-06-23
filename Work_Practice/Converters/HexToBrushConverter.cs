//========================================================= Библиотеки ================================================================//
using System;                      // Базовые типы (object, string)
using System.Globalization;        // CultureInfo
using System.Windows.Data;         // IValueConverter, Binding
using System.Windows.Media;        // Color, SolidColorBrush, Brushes, ColorConverter

namespace Work_Practice.Converters
{
	//========================================================= Конвертер для преобразования HEX-строки в кисть (SolidColorBrush) ================================================================//
	public class HexToBrushConverter : IValueConverter
	{
		//========================================================= Статический экземпляр (для удобства использования в XAML) ================================================================//
		public static readonly HexToBrushConverter Instance = new HexToBrushConverter();

		//========================================================= Преобразование HEX → кисть ================================================================//
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string hex && !string.IsNullOrEmpty(hex))          // Если значение – непустая строка
			{
				try
				{
					Color color = (Color)ColorConverter.ConvertFromString(hex); // Парсим HEX (#RRGGBB или #AARRGGBB)
					return new SolidColorBrush(color);                      // Создаём и возвращаем кисть
				}
				catch
				{
					return Brushes.White;                                   // При ошибке – белая кисть
				}
			}
			return Brushes.White;                                           // Если не строка – белая кисть (значение по умолчанию)
		}

		//========================================================= Обратное преобразование (не реализовано) ================================================================//
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();                           // Обратное преобразование не требуется
		}
	}
}
