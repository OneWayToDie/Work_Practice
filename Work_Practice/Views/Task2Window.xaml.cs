using System.Windows;
using System.Windows.Input;

namespace Work_Practice.Views
{
	public partial class Task2Window : Window
	{
		private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
		private void MaximizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
		private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
		public Task2Window()
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
			e.Handled = true;
		}
	}
}
