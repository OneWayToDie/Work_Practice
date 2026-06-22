using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Work_Practice.Converters
{
	public class HexToBrushConverter : IValueConverter
	{
		public static readonly HexToBrushConverter Instance = new HexToBrushConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string hex && !string.IsNullOrEmpty(hex))
			{
				try
				{
					Color color = (Color)ColorConverter.ConvertFromString(hex);
					return new SolidColorBrush(color);
				}
				catch
				{
					return Brushes.White;
				}
			}
			return Brushes.White;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
