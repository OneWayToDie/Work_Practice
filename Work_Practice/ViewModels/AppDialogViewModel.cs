//========================================================= Библиотеки ================================================================//
using System.ComponentModel;   // Реализация INotifyPropertyChanged (PropertyChangedEventHandler)
using System.Runtime.CompilerServices; // Атрибут CallerMemberName для автоматического подставления имени свойства

namespace Work_Practice.ViewModels  // Пространство имён для ViewModel
{
	//========================================================= Класс ViewModel для кастомного диалогового окна ================================================================//
	public class AppDialogViewModel : INotifyPropertyChanged
	{
		//========================================================= Приватные поля ================================================================//
		private string title;           // Заголовок окна
		private string message;         // Текст сообщения
		private string iconSymbol;      // Символ иконки (✓, ⚠, ✕)
		private string accentColor;     // Цвет акцента (в формате #RRGGBB)

		//========================================================= Публичные свойства ================================================================//
		public string Title
		{
			get => title;                                     // Возвращаем значение поля
			set { title = value; OnPropertyChanged(); }      // При изменении уведомляем UI
		}

		public string Message
		{
			get => message;                                   // Возвращаем текст сообщения
			set { message = value; OnPropertyChanged(); }    // При изменении уведомляем UI
		}

		public string IconSymbol
		{
			get => iconSymbol;                                // Возвращаем символ иконки
			set { iconSymbol = value; OnPropertyChanged(); } // При изменении уведомляем UI
		}

		public string AccentColor
		{
			get => accentColor;                               // Возвращаем цвет акцента
			set { accentColor = value; OnPropertyChanged(); } // При изменении уведомляем UI
		}

		//========================================================= Реализация INotifyPropertyChanged ================================================================//
		public event PropertyChangedEventHandler PropertyChanged; // Событие для уведомления об изменении свойств

		//========================================================= Метод для уведомления об изменении свойства ================================================================//
		protected void OnPropertyChanged([CallerMemberName] string name = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); // Вызываем событие, если есть подписчики, передаём имя свойства
	}
}
