//========================================================= Библиотеки ================================================================//
using System;                     // Базовые типы (Type, object)
using System.Globalization;       // Культура (CultureInfo)
using System.Windows;             // WPF-типы (Visibility)
using System.Windows.Data;        // Интерфейс IValueConverter и базовые классы для конвертеров

namespace Work_Practice.Converters
{
	//========================================================= Конвертер Bool → Visibility ================================================================//
	public class BoolToVisibilityConverter : IValueConverter
	{
		//========================================================= Преобразование из bool в Visibility ================================================================//
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool boolValue && boolValue)                 // Если значение – true
				return Visibility.Visible;                            // Возвращаем Visible
			return Visibility.Collapsed;                              // Иначе Collapsed (скрыто и не занимает место)
		}

		//========================================================= Обратное преобразование из Visibility в bool ================================================================//
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Visibility visibility && visibility == Visibility.Visible) // Если передан Visible
				return true;                                          // Возвращаем true
			return false;                                             // Иначе false
		}
	}
}
