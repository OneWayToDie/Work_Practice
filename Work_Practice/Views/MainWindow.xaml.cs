using System.Windows;
using System.Windows.Input;
using Work_Practice.ViewModels;

namespace Work_Practice.Views
{
	public partial class MainWindow : Window
	{
		private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
		private void MaximizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
		private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
		public MainWindow()
		{
			InitializeComponent();
			this.Loaded += (s, e) => Helpers.WindowButtonHelper.Attach(this);
		}
		private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ButtonState == MouseButtonState.Pressed)
				this.DragMove();
		}

		private void TitleBar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			// Ничего не делаем – отключаем разворачивание
			e.Handled = true;
		}
	}
}
