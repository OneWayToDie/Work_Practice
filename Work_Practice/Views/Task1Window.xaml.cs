using System.Windows;

namespace Work_Practice.Views
{
	public partial class Task1Window : Window
	{
		public Task1Window()
		{
			InitializeComponent();
			this.Loaded += (s, e) => Helpers.WindowButtonHelper.Attach(this);
		}
	}
}
