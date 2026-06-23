//========================================================= Библиотеки ================================================================//
using System;                      // Базовые типы (Type, object)
using System.Globalization;        // CultureInfo
using System.Windows.Data;         // IValueConverter, Binding

namespace Work_Practice.Converters
{
	//========================================================= Конвертер для привязки Enum к RadioButton (и обратно) ================================================================//
	public class EnumToBooleanConverter : IValueConverter
	{
		//========================================================= Преобразование из Enum в bool ================================================================//
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null || parameter == null) return false;       // Если что-то отсутствует – возвращаем false
			return value.Equals(parameter);                             // Сравниваем значение enum с параметром (равны → true)
		}

		//========================================================= Обратное преобразование из bool в Enum ================================================================//
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null || parameter == null) return Binding.DoNothing; // Если что-то отсутствует – ничего не делаем
			return (bool)value ? parameter : Binding.DoNothing;                // Если true – возвращаем параметр (значение enum), иначе – без изменений
		}
	}
}