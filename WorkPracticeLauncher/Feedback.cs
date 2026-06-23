//============================================================================= Библиотеки ================================================================================================//
using System;                     // Базовые типы (DateTime, string, Console)
using System.Collections.Generic; // Коллекции (List<T>)
using System.IO;                  // Работа с файлами (File, Path)
using System.Linq;                

namespace WorkPracticeLauncher
{
	//========================================================================= Модель отзыва =============================================================================================//
	public class Review
	{
		public string Author { get; set; }   // Имя автора отзыва (или "Аноним")
		public string Date { get; set; }     // Дата создания отзыва (dd.MM.yyyy)
		public string Type { get; set; }     // Тип: "Отзыв" или "Предложение"
		public string Text { get; set; }     // Текст отзыва
	}

	//=========================================================== Класс для управления отзывами (сохранение, загрузка, UI) ================================================================//
	public static class FeedbackManager
	{
		// Путь к файлу отзывов (JSON) в папке с программой
		private static string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reviews.json");

		//========================================================= Главное меню раздела "Отзывы и предложения" ===========================================================================//
		public static void Run()
		{
			while (true)                                               // Бесконечный цикл до выхода
			{
				Console.Clear();                                       // Очищаем экран
				Console.ForegroundColor = ConsoleColor.Cyan;           // Голубой цвет для заголовка
				Console.WriteLine("=== ОТЗЫВЫ И ПРЕДЛОЖЕНИЯ ===");
				Console.ResetColor();
				Console.WriteLine();
				Console.WriteLine("  1 – Оставить отзыв о работе");    // Пункт 1
				Console.WriteLine("  2 – Оставить предложение по улучшению"); // Пункт 2
				Console.WriteLine("  3 – Просмотреть все отзывы");     // Пункт 3
				Console.WriteLine("  0 – Назад");                      // Пункт 0
				Console.ForegroundColor = ConsoleColor.DarkGray;       // Тёмно-серый для подсказки
				Console.WriteLine("  (или Enter – Назад)");
				Console.ResetColor();
				Console.WriteLine();
				Console.Write("Ваш выбор: ");                          // Приглашение

				string input = Console.ReadLine();                     // Считываем строку
				if (string.IsNullOrEmpty(input)) return;               // Пустой ввод – выход

				switch (input)                                         // Обработка выбора
				{
					case "1":
						WriteReview("Отзыв");                          // Вызов формы для отзыва
						break;
					case "2":
						WriteReview("Предложение");                    // Вызов формы для предложения
						break;
					case "3":
						ShowReviews();                                 // Просмотр всех отзывов
						break;
					case "0":
						return;                                        // Выход
					default:
						Console.ForegroundColor = ConsoleColor.Red;   // Красный для ошибки
						Console.WriteLine("Некорректный выбор.");
						Console.ResetColor();
						Console.WriteLine("Нажмите любую клавишу...");
						Console.ReadKey(true);                         // Ожидание клавиши
						break;
				}
			}
		}

		//=================================================================== Форма ввода отзыва или предложения ==========================================================================//
		private static void WriteReview(string type)
		{
			Console.Clear();                                           // Очищаем экран
			Console.ForegroundColor = ConsoleColor.Yellow;             // Жёлтый заголовок
			Console.WriteLine($"=== НОВЫЙ {type.ToUpper()} ===");
			Console.ResetColor();
			Console.WriteLine();

			// Ввод имени автора
			Console.Write("Ваше имя (или пропустите): ");
			string author = Console.ReadLine();
			if (string.IsNullOrWhiteSpace(author))                     // Если пусто – устанавливаем "Аноним"
				author = "Аноним";

			// Ввод текста (многострочный)
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.DarkGray;           // Серый для инструкции
			Console.WriteLine("Введите текст отзыва (Enter – завершить ввод):");
			Console.ResetColor();
			Console.WriteLine();

			string text = "";                                          // Буфер для текста
			while (true)                                               // Бесконечный цикл чтения строк
			{
				string line = Console.ReadLine();                      // Читаем строку
				if (string.IsNullOrEmpty(line)) break;                 // Пустая строка – завершаем ввод
				if (text.Length > 0) text += " ";                      // Добавляем пробел между строками
				text += line;                                          // Добавляем строку к тексту
			}

			if (string.IsNullOrWhiteSpace(text))                       // Если текст пустой
			{
				Console.ForegroundColor = ConsoleColor.Yellow;         // Жёлтое предупреждение
				Console.WriteLine("Текст пуст. Отзыв не сохранён.");
				Console.ResetColor();
				Console.WriteLine("Нажмите любую клавишу...");
				Console.ReadKey(true);
				return;
			}

			// Подтверждение сохранения
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.DarkGray;           // Серый для превью
			Console.WriteLine("Текст отзыва:");
			Console.ResetColor();
			Console.WriteLine($"  \"{text}\"");
			Console.WriteLine();
			Console.Write("Сохранить? (y/n): ");

			if (Console.ReadKey(true).KeyChar != 'y')                  // Если не 'y'
			{
				Console.WriteLine();
				Console.ForegroundColor = ConsoleColor.Yellow;         // Жёлтое сообщение
				Console.WriteLine("Отзыв не сохранён.");
				Console.ResetColor();
				Console.WriteLine("Нажмите любую клавишу...");
				Console.ReadKey(true);
				return;
			}

			// Создаём объект отзыва
			Review review = new Review
			{
				Author = author,
				Date = DateTime.Now.ToString("dd.MM.yyyy"),            // Текущая дата
				Type = type,
				Text = text
			};

			SaveReview(review);                                        // Сохраняем

			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Green;              // Зелёный успех
			Console.WriteLine("Отзыв сохранён! Спасибо за обратную связь.");
			Console.ResetColor();
			Console.WriteLine("Нажмите любую клавишу...");
			Console.ReadKey(true);
		}

		//======================================================================= Просмотр всех отзывов ===================================================================================//
		private static void ShowReviews()
		{
			Console.Clear();                                           // Очищаем экран
			List<Review> reviews = LoadReviews();                      // Загружаем список из файла

			Console.ForegroundColor = ConsoleColor.Cyan;               // Голубой заголовок
			Console.WriteLine($"=== ВСЕ ОТЗЫВЫ ({reviews.Count} шт.) ===");
			Console.ResetColor();
			Console.WriteLine();

			if (reviews.Count == 0)                                    // Если список пуст
			{
				Console.ForegroundColor = ConsoleColor.DarkGray;       // Серое сообщение
				Console.WriteLine("Отзывов пока нет.");
				Console.ResetColor();
				Console.WriteLine();
				Console.WriteLine("Нажмите любую клавишу...");
				Console.ReadKey(true);
				return;
			}

			Console.ForegroundColor = ConsoleColor.DarkGray;           // Разделитель
			Console.WriteLine(new string('─', 50));
			Console.ResetColor();

			for (int i = 0; i < reviews.Count; i++)                    // Вывод каждого отзыва
			{
				Review r = reviews[i];
				Console.ForegroundColor = ConsoleColor.Yellow;         // Жёлтый для автора
				Console.Write($"[{i + 1}] {r.Author}");
				Console.ForegroundColor = ConsoleColor.DarkGray;       // Серый для даты и типа
				Console.Write($" | {r.Date} | ");
				Console.ForegroundColor = r.Type == "Отзыв" ? ConsoleColor.Green : ConsoleColor.Magenta; // Отзыв – зелёный, предложение – маджента
				Console.WriteLine(r.Type);
				Console.ResetColor();
				Console.WriteLine($"    {r.Text}");                    // Текст отзыва
				Console.WriteLine();
			}

			Console.ForegroundColor = ConsoleColor.DarkGray;           // Нижний разделитель
			Console.WriteLine(new string('─', 50));
			Console.ResetColor();

			Console.WriteLine("Нажмите любую клавишу...");
			Console.ReadKey(true);
		}

		//====================================================================== Загрузка отзывов из JSON-файла ===========================================================================//
		private static List<Review> LoadReviews()
		{
			if (!File.Exists(filePath))                                // Если файла нет
				return new List<Review>();                             // Возвращаем пустой список

			try
			{
				string json = File.ReadAllText(filePath);              // Читаем весь файл
				return ParseReviews(json);                             // Парсим JSON в список
			}
			catch                                                      // Если ошибка чтения или парсинга
			{
				return new List<Review>();                             // Возвращаем пустой список
			}
		}

		//====================================================================== Сохранение отзыва в JSON-файл ============================================================================//
		private static void SaveReview(Review review)
		{
			List<Review> reviews = LoadReviews();                      // Загружаем существующие
			reviews.Add(review);                                       // Добавляем новый

			try
			{
				string json = SerializeReviews(reviews);               // Сериализуем в JSON
				File.WriteAllText(filePath, json);                    // Записываем в файл
			}
			catch { }                                                  // Игнорируем ошибки записи
		}

		//============================================================ Ручной парсинг JSON (без внешних библиотек) ========================================================================//
		private static List<Review> ParseReviews(string json)
		{
			List<Review> reviews = new List<Review>();                 // Результирующий список
			json = json.Trim();                                        // Убираем лишние пробелы

			if (!json.StartsWith("[") || !json.EndsWith("]"))          // Если не массив
				return reviews;

			json = json.Substring(1, json.Length - 2).Trim();          // Убираем внешние скобки
			if (string.IsNullOrEmpty(json))                            // Если пусто
				return reviews;

			// Разделяем объекты по },{ (конец одного объекта, начало другого)
			string[] objects = json.Split(new[] { "},{" }, StringSplitOptions.None);

			foreach (string obj in objects)                            // Перебираем объекты
			{
				string clean = obj.Trim().TrimStart('{').TrimEnd('}'); // Убираем фигурные скобки
				Review r = new Review();                               // Создаём новый объект
				r.Author = ExtractJsonValue(clean, "Author");          // Извлекаем имя
				r.Date = ExtractJsonValue(clean, "Date");              // Извлекаем дату
				r.Type = ExtractJsonValue(clean, "Type");              // Извлекаем тип
				r.Text = ExtractJsonValue(clean, "Text");              // Извлекаем текст
				reviews.Add(r);                                        // Добавляем в список
			}

			return reviews;
		}

		//========================================================== Извлечение значения поля из JSON-строки ==============================================================================//
		private static string ExtractJsonValue(string json, string key)
		{
			string search = $"\"{key}\":\"";                           // Ищем подстроку "key":"
			int start = json.IndexOf(search);                          // Находим позицию
			if (start < 0) return "";                                  // Если не найдено – пустая строка
			start += search.Length;                                    // Сдвигаем на начало значения
			int end = json.IndexOf("\"", start);                       // Ищем закрывающую кавычку
			if (end < 0) return "";                                    // Если не найдена – пустая строка
			return json.Substring(start, end - start);                 // Возвращаем значение
		}

		//========================================================= Ручная сериализация списка отзывов в JSON =============================================================================//
		private static string SerializeReviews(List<Review> reviews)
		{
			string result = "[\n";                                     // Открывающая скобка и перевод строки
			for (int i = 0; i < reviews.Count; i++)                    // Перебираем все отзывы
			{
				Review r = reviews[i];
				result += "  {";                                       // Открываем объект
				result += $"\"Author\":\"{EscapeJson(r.Author)}\"";    // Поле Author
				result += $",\"Date\":\"{EscapeJson(r.Date)}\"";       // Поле Date
				result += $",\"Type\":\"{EscapeJson(r.Type)}\"";       // Поле Type
				result += $",\"Text\":\"{EscapeJson(r.Text)}\"";       // Поле Text
				result += "}";                                         // Закрываем объект
				if (i < reviews.Count - 1) result += ",";              // Добавляем запятую, если не последний
				result += "\n";                                        // Перевод строки
			}
			result += "]";                                             // Закрывающая скобка
			return result;
		}

		//============================================================= Экранирование спецсимволов для JSON ===============================================================================//
		private static string EscapeJson(string s)
		{
			if (string.IsNullOrEmpty(s)) return "";                    // Если строка пустая – возвращаем пустую
			return s.Replace("\\", "\\\\")                             // Экранируем обратную косую черту
					.Replace("\"", "\\\"")                             // Экранируем кавычки
					.Replace("\n", "\\n")                              // Заменяем перенос строки
					.Replace("\r", "");                                // Удаляем возврат каретки
		}
	}
}