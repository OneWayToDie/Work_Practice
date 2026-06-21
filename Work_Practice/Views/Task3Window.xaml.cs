using System.Windows;

namespace Work_Practice.Views
{
	public partial class Task3Window : Window
	{
		public Task3Window()
		{
			InitializeComponent();
			this.Loaded += (s, e) => Helpers.WindowButtonHelper.Attach(this);
		}
	}
}
