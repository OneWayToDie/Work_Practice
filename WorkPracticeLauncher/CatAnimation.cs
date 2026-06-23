using System;
using System.Threading;

namespace WorkPracticeLauncher
{
	public class CatAnimation
	{
		private readonly int leftPad = 2;
		private int topPad;
		private string[][] sleepFrames;
		private string[] frameQuestion;
		private string[] frameOpen;
		private string[] catSquint;
		private string[] catOpen;

		private int sleepIndex = 0;
		private DateTime nextSleepChange;

		public CatAnimation()
		{
			RecalculatePosition();
			InitializeFrames();
			nextSleepChange = DateTime.Now.AddMilliseconds(700);
		}

		private void RecalculatePosition()
		{
			int windowHeight = Console.WindowHeight;
			int frameHeight = 9; // реальная высота кадра (кот + коробка)

			// Высота меню – примерно 13-14 строк, но для надёжности возьмём 15
			int menuHeight = 15;

			// Вычисляем доступное пространство под меню
			int availableHeight = windowHeight - menuHeight;
			// Центрируем анимацию в доступном пространстве
			topPad = menuHeight + (availableHeight - frameHeight) / 2;

			// Проверка границ
			if (topPad < menuHeight) topPad = menuHeight;
			if (topPad + frameHeight > windowHeight)
				topPad = windowHeight - frameHeight - 1;
			if (topPad < 0) topPad = 0;
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
				foreach (string line in baseFrame)
					if (line.Length > maxWidth) maxWidth = line.Length;
				int textPos = (maxWidth - text.Length) / 2;
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
			RecalculatePosition();
			int height = frame.Length;
			int windowWidth = Console.WindowWidth;
			int windowHeight = Console.WindowHeight;

			for (int row = 0; row < height; row++)
			{
				if (topPad + row < windowHeight && topPad + row >= 0)
				{
					Console.SetCursorPosition(0, topPad + row);
					Console.Write(new string(' ', windowWidth));
				}
			}

			for (int row = 0; row < height; row++)
			{
				if (topPad + row < windowHeight && topPad + row >= 0)
				{
					string line = frame[row];
					if (line.Length + leftPad > windowWidth)
						line = line.Substring(0, Math.Max(0, windowWidth - leftPad - 1));
					Console.SetCursorPosition(leftPad, topPad + row);
					Console.Write(line);
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