using System.Windows;

namespace Work_Practice.Views
{
	public partial class Task2Window : Window
	{
		public Task2Window()
		{
			InitializeComponent();
			this.Loaded += (s, e) => Helpers.WindowButtonHelper.Attach(this);
		}
	}
}
