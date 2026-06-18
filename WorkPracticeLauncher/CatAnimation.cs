using System;
using System.Threading;

namespace WorkPracticeLauncher
{
	public class CatAnimation
	{
		private readonly int leftPad = 100; // изменено с 100 на 2
		private readonly int topPad;
		private string[][] sleepFrames;
		private string[] frameQuestion;
		private string[] frameOpen;
		private string[] catSquint;
		private string[] catOpen;

		private int sleepIndex = 0;
		private DateTime nextSleepChange;

		public CatAnimation()
		{
			int windowHeight = Console.WindowHeight;
			int frameHeight = 20;
			int menuHeight = 7;

			int availableHeight = windowHeight - menuHeight;
			topPad = menuHeight + (availableHeight - frameHeight) / 2;
			if (topPad < menuHeight) topPad = menuHeight;

			InitializeFrames();
			nextSleepChange = DateTime.Now.AddMilliseconds(700);
		}

		private void InitializeFrames()
		{
			string[] boxClosed = {
				"",
				"",
				"  ____________________",
				" /         /         /|",
				"/_________/_________/ |",
				"|         |         | |",
				"|         |         | /",
				"|_________|_________|/",
				""
			};

			string[] boxOpen = {
				"",
				"",
				"  ____________________",
				" /         /         /|",
				"/_________/_________/ |",
				"|         |         | |",
				"|         |         | /",
				"|_________|_________|/",
				""
			};

			catSquint = new string[] {
				"         /\\_/\\ ",
				"        ( -.- )",
				"         > ^ < ",
				"  ____________________",
				" /         /         /|",
				"/_________/_________/ |",
				"|         |         | |",
				"|         |         | /",
				"|_________|_________|/"
			};

			catOpen = new string[] {
				"         /\\_/\\ ",
				"        ( o.o )",
				"          >^< ",
				"  ____________________",
				" /         /         /|",
				"/_________/_________/ |",
				"|         |         | |",
				"|         |         | /",
				"|_________|_________|/"
			};

			string[] AddText(string[] baseFrame, string text)
			{
				string[] copy = (string[])baseFrame.Clone();
				int maxWidth = 0;
				foreach (var line in baseFrame)
					if (line.Length > maxWidth) maxWidth = line.Length;
				int textPos = (maxWidth - text.Length + 125) / 2;
				if (textPos < 0) textPos = 0;
				copy[0] = new string(' ', leftPad + textPos) + text;
				return copy;
			}

			sleepFrames = new string[][]
			{
				AddText(boxClosed, "Zzz..."),
				AddText(boxClosed, "zZz..."),
				AddText(boxClosed, "zzZ...")
			};
			frameQuestion = AddText(boxClosed, "?");
			frameOpen = boxOpen;
		}

		private void DrawFrame(string[] frame)
		{
			int height = frame.Length;
			for (int row = 0; row < height; row++)
			{
				Console.SetCursorPosition(0, topPad + row);
				Console.Write(new string(' ', Console.WindowWidth - 1));
			}
			for (int row = 0; row < height; row++)
			{
				if (topPad + row < Console.WindowHeight)
				{
					Console.SetCursorPosition(leftPad, topPad + row);
					Console.Write(frame[row]);
				}
			}
		}

		public void ShowSleepFrame() => DrawFrame(sleepFrames[sleepIndex]);

		public void UpdateSleep()
		{
			if (DateTime.Now >= nextSleepChange)
			{
				sleepIndex = (sleepIndex + 1) % sleepFrames.Length;
				DrawFrame(sleepFrames[sleepIndex]);
				nextSleepChange = DateTime.Now.AddMilliseconds(700);
			}
		}

		public void WakeUp()
		{
			DrawFrame(frameQuestion);
			Thread.Sleep(1500);

			DrawFrame(frameOpen);
			Thread.Sleep(1000);

			for (int i = 0; i < 5; i++)
			{
				DrawFrame(catSquint);
				Thread.Sleep(1000);
				DrawFrame(catOpen);
				Thread.Sleep(1000);
			}

			DrawFrame(frameOpen);
			Thread.Sleep(500);

			sleepIndex = 0;
			nextSleepChange = DateTime.Now.AddMilliseconds(700);
			DrawFrame(sleepFrames[0]);
		}

		public void WakeUpWithCancel(Func<bool> shouldCancel)
		{
			// Функция shouldCancel будет возвращать true, если нужно прервать анимацию
			DrawFrame(frameQuestion);
			if (shouldCancel()) return;
			Thread.Sleep(1500);
			if (shouldCancel()) return;

			DrawFrame(frameOpen);
			if (shouldCancel()) return;
			Thread.Sleep(1000);
			if (shouldCancel()) return;

			for (int i = 0; i < 5; i++)
			{
				DrawFrame(catSquint);
				if (shouldCancel()) return;
				Thread.Sleep(1000);
				if (shouldCancel()) return;
				DrawFrame(catOpen);
				if (shouldCancel()) return;
				Thread.Sleep(1000);
				if (shouldCancel()) return;
			}

			DrawFrame(frameOpen);
			if (shouldCancel()) return;
			Thread.Sleep(500);
			if (shouldCancel()) return;

			sleepIndex = 0;
			nextSleepChange = DateTime.Now.AddMilliseconds(700);
			DrawFrame(sleepFrames[0]);
		}
	}
}
