using System.Windows;
using System.Windows.Controls;

namespace Work_Practice.UserControls
{
	public partial class Task1FuncControl : UserControl
	{
		public Task1FuncControl()
		{
			InitializeComponent();
		}

		private void AddButton_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Добавление числа будет позже");
		}

		private void ProcessButton_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Обработка будет позже");
		}
	}
}
