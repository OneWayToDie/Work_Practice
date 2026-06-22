using System.Windows;
using System.Windows.Input;
using Work_Practice.ViewModels;

namespace Work_Practice.Views
{
	public partial class AppDialog : Window
	{
		public AppDialog()
		{
			InitializeComponent();
			this.Loaded += (s, e) => Helpers.WindowButtonHelper.Attach(this);
			this.KeyDown += (s, e) =>
			{
				if (e.Key == Key.Enter || e.Key == Key.Escape)
					Close();
			};
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		public static void ShowInfo(string message, string title = "Информация")
		{
			ShowDialog(message, title, "✓", "#F1C40F");
		}

		public static void ShowWarning(string message, string title = "Внимание")
		{
			ShowDialog(message, title, "⚠", "#F39C12");
		}

		public static void ShowError(string message, string title = "Ошибка")
		{
			ShowDialog(message, title, "✕", "#E74C3C");
		}

		private static void ShowDialog(string message, string title, string icon, string colorHex)
		{
			AppDialog dialog = new AppDialog
			{
				DataContext = new AppDialogViewModel
				{
					Title = title,
					Message = message,
					IconSymbol = icon,
					AccentColor = colorHex
				},
				Owner = Application.Current.MainWindow
			};
			dialog.ShowDialog();
		}
	}
}
