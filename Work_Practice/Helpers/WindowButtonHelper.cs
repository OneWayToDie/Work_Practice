using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Work_Practice.Helpers
{
	public static class WindowButtonHelper
	{
		public static void Attach(Window window)
		{
			var template = window.Template;
			if (template == null) return;

			var minimize = template.FindName("PART_MinimizeButton", window) as ButtonBase;
			var maximize = template.FindName("PART_MaximizeButton", window) as ButtonBase;
			var close = template.FindName("PART_CloseButton", window) as ButtonBase;

			if (minimize != null) minimize.Click += (s, e) => window.WindowState = WindowState.Minimized;
			if (maximize != null) maximize.Click += (s, e) =>
				window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
			if (close != null) close.Click += (s, e) => window.Close();

			var titleBar = template.FindName("PART_TitleBar", window) as FrameworkElement;
			if (titleBar != null)
			{
				titleBar.MouseLeftButtonDown += (s, e) =>
				{
					if (e.ButtonState == MouseButtonState.Pressed)
					{
						try { window.DragMove(); }
						catch (InvalidOperationException) { }
					}
				};
			}
		}
	}
}