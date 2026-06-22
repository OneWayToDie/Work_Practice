using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Work_Practice.ViewModels
{
	public class AppDialogViewModel : INotifyPropertyChanged
	{
		private string title;
		private string message;
		private string iconSymbol;
		private string accentColor;

		public string Title
		{
			get => title;
			set { title = value; OnPropertyChanged(); }
		}

		public string Message
		{
			get => message;
			set { message = value; OnPropertyChanged(); }
		}

		public string IconSymbol
		{
			get => iconSymbol;
			set { iconSymbol = value; OnPropertyChanged(); }
		}

		public string AccentColor
		{
			get => accentColor;
			set { accentColor = value; OnPropertyChanged(); }
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}
