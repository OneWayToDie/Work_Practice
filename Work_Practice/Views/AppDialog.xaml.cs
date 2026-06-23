//========================================================= Библиотеки ================================================================//
using System.Windows;          // Базовые WPF-классы (Window, Application)
using System.Windows.Input;    // Клавиши (Key, KeyEventArgs)
using Work_Practice.ViewModels; // ViewModel для диалога (AppDialogViewModel)

namespace Work_Practice.Views   // Пространство имён для окон и представлений
{
	//========================================================= Класс кастомного диалогового окна ================================================================//
	public partial class AppDialog : Window
	{
		//========================================================= Конструктор окна ================================================================//
		public AppDialog()
		{
			InitializeComponent();                                // Инициализируем XAML-компоненты
			this.Loaded += (s, e) => Helpers.WindowButtonHelper.Attach(this); // Подключаем кастомные кнопки управления окном (при загрузке)
			this.KeyDown += (s, e) =>                              // Обработчик нажатий клавиш на окне
			{
				if (e.Key == Key.Enter || e.Key == Key.Escape)    // Если Enter или Escape
					Close();                                       // Закрываем диалог (как нажатие OK)
			};
		}

		//========================================================= Обработчик нажатия кнопки OK ================================================================//
		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			Close();                                              // Закрываем окно (диалог завершается)
		}

		//========================================================= Статические методы для показа информационного диалога ================================================================//
		public static void ShowInfo(string message, string title = "Информация")
		{
			ShowDialog(message, title, "✓", "#F1C40F");           // Вызов с зелёной галочкой (жёлтый акцент)
		}

		//========================================================= Статический метод показа предупреждения ================================================================//
		public static void ShowWarning(string message, string title = "Внимание")
		{
			ShowDialog(message, title, "⚠", "#F39C12");           // Вызов с жёлтым восклицательным знаком (оранжевый акцент)
		}

		//========================================================= Статический метод показа ошибки ================================================================//
		public static void ShowError(string message, string title = "Ошибка")
		{
			ShowDialog(message, title, "✕", "#E74C3C");           // Вызов с красным крестиком (красный акцент)
		}

		//========================================================= Общий метод создания и показа диалога ================================================================//
		private static void ShowDialog(string message, string title, string icon, string colorHex)
		{
			AppDialog dialog = new AppDialog                     // Создаём экземпляр диалога
			{
				DataContext = new AppDialogViewModel             // Устанавливаем DataContext (ViewModel)
				{
					Title = title,                               // Заголовок окна
					Message = message,                           // Текст сообщения
					IconSymbol = icon,                           // Символ иконки (✓, ⚠, ✕)
					AccentColor = colorHex                       // Цвет акцента (кнопка, рамка)
				},
				Owner = Application.Current.MainWindow           // Устанавливаем владельца (главное окно)
			};
			dialog.ShowDialog();                                 // Открываем диалог модально
		}
	}
}