//========================================================= Библиотеки ================================================================//
using System;                     // Базовые типы (InvalidOperationException)
using System.Windows;             // Базовые WPF-классы (Window, WindowState)
using System.Windows.Controls;    // Элементы управления (ButtonBase, FrameworkElement)
using System.Windows.Controls.Primitives; // ButtonBase (базовый класс для кнопок)
using System.Windows.Input;       // MouseButtonState, MouseButtonEventArgs

namespace Work_Practice.Helpers
{
	//========================================================= Класс-помощник для управления кастомным заголовком окна ================================================================//
	public static class WindowButtonHelper
	{
		//========================================================= Привязка обработчиков к кнопкам и заголовку окна ================================================================//
		public static void Attach(Window window)
		{
			ControlTemplate template = window.Template;                 // Получаем шаблон окна (если задан)
			if (template == null) return;                               // Если шаблона нет – выходим (нечего привязывать)

			// Поиск кнопок управления окном в шаблоне
			ButtonBase minimize = template.FindName("PART_MinimizeButton", window) as ButtonBase; // Кнопка "Свернуть"
			ButtonBase maximize = template.FindName("PART_MaximizeButton", window) as ButtonBase; // Кнопка "Развернуть/Восстановить"
			ButtonBase close = template.FindName("PART_CloseButton", window) as ButtonBase;       // Кнопка "Закрыть"

			// Подписка на события кликов
			if (minimize != null) minimize.Click += (s, e) => window.WindowState = WindowState.Minimized; // Свернуть окно
			if (maximize != null) maximize.Click += (s, e) =>
				window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized; // Переключение состояния
			if (close != null) close.Click += (s, e) => window.Close(); // Закрыть окно

			// Поиск элемента-заголовка для перетаскивания окна
			FrameworkElement titleBar = template.FindName("PART_TitleBar", window) as FrameworkElement; // Заголовок (обычно Border или Panel)
			if (titleBar != null)                                         // Если заголовок найден
			{
				titleBar.MouseLeftButtonDown += (s, e) =>                 // При нажатии левой кнопки мыши на заголовке
				{
					if (e.ButtonState == MouseButtonState.Pressed)       // Если кнопка действительно нажата
					{
						try { window.DragMove(); }                       // Пытаемся начать перетаскивание окна
						catch (InvalidOperationException) { }           // Игнорируем ошибки (например, если окно уже в режиме изменения размера)
					}
				};
			}
		}
	}
}
