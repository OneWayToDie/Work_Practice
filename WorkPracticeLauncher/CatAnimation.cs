//========================================================= Класс анимации кота ================================================================//
using System;
using System.Threading;

public class CatAnimation
{
	//========================================================= Поля класса ================================================================//
	private readonly int leftPad = 2;               // Отступ слева для вывода анимации
	private int topPad;                             // Отступ сверху для вывода анимации (рассчитывается динамически)
	private string[][] sleepFrames;                 // Массив кадров для анимации сна (меняющиеся Zzz)
	private string[] frameQuestion;                 // Кадр с вопросительным знаком (пробуждение)
	private string[] frameOpen;                     // Кадр с открытой коробкой
	private string[] catSquint;                     // Кадр кота с прищуренными глазами
	private string[] catOpen;                       // Кадр кота с открытыми глазами

	private int sleepIndex = 0;                     // Индекс текущего кадра сна (0, 1, 2)
	private DateTime nextSleepChange;               // Время следующей смены кадра сна (для мигания Zzz)

	//========================================================= Конструктор ================================================================//
	public CatAnimation()
	{
		RecalculatePosition();                      // Вычисляем позицию для анимации (отступ сверху)
		InitializeFrames();                         // Инициализируем все кадры (строки с рисунками)
		nextSleepChange = DateTime.Now.AddMilliseconds(700); // Смена кадра сна через 700 мс
	}

	//========================================================= Пересчёт позиции анимации ================================================================//
	private void RecalculatePosition()
	{
		int windowHeight = Console.WindowHeight;    // Текущая высота окна консоли
		int frameHeight = 9;                        // Реальная высота одного кадра (кот + коробка)
		int menuHeight = 15;                        // Примерная высота меню (для отступа)

		int availableHeight = windowHeight - menuHeight; // Доступное пространство под меню
		topPad = menuHeight + (availableHeight - frameHeight) / 2; // Центрируем по вертикали

		// Проверка границ: не выходим за пределы окна
		if (topPad < menuHeight) topPad = menuHeight;
		if (topPad + frameHeight > windowHeight)
			topPad = windowHeight - frameHeight - 1;
		if (topPad < 0) topPad = 0;
	}

	//========================================================= Инициализация кадров ================================================================//
	private void InitializeFrames()
	{
		// Кадр коробки (закрытая)
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

		// Кадр коробки (открытая)
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

		// Кадр кота с прищуренными глазами
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

		// Кадр кота с открытыми глазами
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

		// Локальная функция добавления текста в верхнюю строку кадра
		string[] AddText(string[] baseFrame, string text)
		{
			string[] copy = (string[])baseFrame.Clone();       // Копируем базовый кадр
			int maxWidth = 0;                                   // Вычисляем максимальную ширину строки
			foreach (var line in baseFrame)
				if (line.Length > maxWidth) maxWidth = line.Length;
			int textPos = (maxWidth - text.Length) / 2;        // Позиция для центрирования текста
			if (textPos < 0) textPos = 0;
			copy[0] = new string(' ', leftPad + textPos) + text; // Вставляем текст с отступом
			return copy;
		}

		// Три кадра сна с разными вариантами Zzz
		sleepFrames = new string[][]
		{
			AddText(boxClosed, "Zzz..."),
			AddText(boxClosed, "zZz..."),
			AddText(boxClosed, "zzZ...")
		};
		frameQuestion = AddText(boxClosed, "?");               // Кадр с вопросом
		frameOpen = boxOpen;                                    // Открытая коробка (без текста)
	}

	//========================================================= Отрисовка одного кадра ================================================================//
	private void DrawFrame(string[] frame)
	{
		RecalculatePosition();                                  // Актуализируем позицию (при изменении размера)
		int height = frame.Length;                              // Высота кадра
		int windowWidth = Console.WindowWidth;                  // Текущая ширина окна
		int windowHeight = Console.WindowHeight;                // Текущая высота окна

		// Очищаем область кадра (заливаем пробелами)
		for (int row = 0; row < height; row++)
		{
			if (topPad + row < windowHeight && topPad + row >= 0) // Если строка в пределах видимости
			{
				Console.SetCursorPosition(0, topPad + row);     // Курсор в начало строки
				Console.Write(new string(' ', windowWidth));    // Затираем всю строку пробелами
			}
		}

		// Рисуем кадр
		for (int row = 0; row < height; row++)
		{
			if (topPad + row < windowHeight && topPad + row >= 0) // Если строка в пределах видимости
			{
				string line = frame[row];                       // Берём строку кадра
				if (line.Length + leftPad > windowWidth)        // Если строка не помещается по ширине
					line = line.Substring(0, Math.Max(0, windowWidth - leftPad - 1)); // Обрезаем
				Console.SetCursorPosition(leftPad, topPad + row); // Курсор на позицию строки
				Console.Write(line);                            // Выводим строку
			}
		}
	}

	//========================================================= Показать текущий кадр сна ================================================================//
	public void ShowSleepFrame() => DrawFrame(sleepFrames[sleepIndex]); // Рисуем текущий кадр сна по индексу

	//========================================================= Обновление анимации сна (Zzz) ================================================================//
	public void UpdateSleep()
	{
		if (DateTime.Now >= nextSleepChange)                    // Если настало время смены кадра
		{
			sleepIndex = (sleepIndex + 1) % sleepFrames.Length; // Переключаем на следующий кадр
			DrawFrame(sleepFrames[sleepIndex]);                 // Рисуем новый кадр
			nextSleepChange = DateTime.Now.AddMilliseconds(700); // Устанавливаем следующее время смены
		}
	}

	//========================================================= Полное пробуждение кота (без возможности отмены) ================================================================//
	public void WakeUp()
	{
		DrawFrame(frameQuestion);                               // Показываем вопрос
		Thread.Sleep(1500);                                     // Пауза 1.5 сек

		DrawFrame(frameOpen);                                   // Открываем коробку
		Thread.Sleep(1000);                                     // Пауза 1 сек

		for (int i = 0; i < 5; i++)                             // Моргаем 5 раз
		{
			DrawFrame(catSquint);                               // Прищур
			Thread.Sleep(1000);                                 // 1 сек
			DrawFrame(catOpen);                                 // Открытые глаза
			Thread.Sleep(1000);                                 // 1 сек
		}

		DrawFrame(frameOpen);                                   // Возвращаем открытую коробку
		Thread.Sleep(500);                                      // Пауза 0.5 сек

		sleepIndex = 0;                                         // Сбрасываем индекс сна
		nextSleepChange = DateTime.Now.AddMilliseconds(700);   // Перезапускаем таймер сна
		DrawFrame(sleepFrames[0]);                              // Показываем начальный сон
	}

	//==================================================================== Пробуждение с возможностью отмены ===================================================================================//
	public void WakeUpWithCancel(Func<bool> shouldCancel)
	{
		DrawFrame(frameQuestion);                               // Показываем вопрос
		if (shouldCancel()) return;                             // Если отмена – выходим
		Thread.Sleep(1500);                                     // Пауза 1.5 сек
		if (shouldCancel()) return;                             // Проверка отмены после паузы

		DrawFrame(frameOpen);                                   // Открываем коробку
		if (shouldCancel()) return;                             // Проверка отмены
		Thread.Sleep(1000);                                     // Пауза 1 сек
		if (shouldCancel()) return;                             // Проверка отмены

		for (int i = 0; i < 5; i++)                             // Моргаем 5 раз
		{
			DrawFrame(catSquint);                               // Прищур
			if (shouldCancel()) return;                         // Проверка отмены
			Thread.Sleep(1000);                                 // 1 сек
			if (shouldCancel()) return;                         // Проверка отмены
			DrawFrame(catOpen);                                 // Открытые глаза
			if (shouldCancel()) return;                         // Проверка отмены
			Thread.Sleep(1000);                                 // 1 сек
			if (shouldCancel()) return;                         // Проверка отмены
		}

		DrawFrame(frameOpen);                                   // Возвращаем открытую коробку
		if (shouldCancel()) return;                             // Проверка отмены
		Thread.Sleep(500);                                      // Пауза 0.5 сек
		if (shouldCancel()) return;                             // Проверка отмены

		sleepIndex = 0;                                         // Сбрасываем индекс сна
		nextSleepChange = DateTime.Now.AddMilliseconds(700);    // Перезапускаем таймер сна
		DrawFrame(sleepFrames[0]);                              // Показываем начальный сон
	}
}