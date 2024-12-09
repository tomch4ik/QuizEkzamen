using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
namespace QuizEkzamen
{
    class Program
    {
        class User
        {
            public string Login { get; set; }
            public string Password { get; set; }
            public string BirthDate { get; set; }
            public User(string login, string password, string birthDate)
            {
                Login = login;
                Password = password;
                BirthDate = birthDate;
            }
        }
        class QuizResult
        {
            public string UserLogin { get; set; }
            public int Point { get; set; }
            public string QuizName { get; set; }

            public QuizResult(string userLogin, int point, string quizName)
            {
                UserLogin = userLogin;
                Point = point;
                QuizName = quizName;
            }
        }
        class Quiz
        {
            public string Text { get; set; }
            public List<string> CorrectAnswers { get; set; }

            public Quiz(string text, List<string> correctAnswers)
            {
                Text = text;
                CorrectAnswers = correctAnswers;
            }
        }
        static List<User> users = new List<User>();
        static List<QuizResult> results = new List<QuizResult>();
        static User loggedInUser = null;
        static Dictionary<string, List<Quiz>> quizQuestion = new Dictionary<string, List<Quiz>>();
        static void Main(string[] args)
        {
            QuizFile();
            while (true)
            {
                if (loggedInUser == null)
                {
                    Console.WriteLine("Меню: ");
                    Console.WriteLine("1 - Войти ");
                    Console.WriteLine("2 - Регистрация ");
                    Console.WriteLine("3 - Выйти ");
                    string userInput = Console.ReadLine();
                    switch (userInput)
                    {
                        case "1":
                            Login();
                            break;
                        case "2":
                            Registration();
                            break;
                        case "3":
                            Console.WriteLine("Выход");
                            return;
                    }
                }
                else
                {
                    UserMenu();
                }
            }
        }
        static void QuizFile()
        {
            foreach (var file in Directory.EnumerateFiles("quiz", "*.txt"))
            {
                string category = Path.GetFileNameWithoutExtension(file);
                var questions = new List<Quiz>();
                string[] lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i += 2)
                {
                    string text = lines[i];
                    var correctAnswers = lines[i + 1].Split(',').Select(a => a.Trim()).ToList();
                    questions.Add(new Quiz(text, correctAnswers));
                }
                quizQuestion[category] = questions;
            }
        }
        static void UserMenu()
        {
            if (loggedInUser == null)
            {
                Console.WriteLine("Ошибка: вы не авторизованы");
                return;
            }

            Console.WriteLine($"{loggedInUser.Login}, выберите действие:");
            Console.WriteLine("1 - Начать новую викторину");
            Console.WriteLine("2 - Результаты прошлых викторин");
            Console.WriteLine("3 - Топ 20 игроков");
            Console.WriteLine("4 - Изменить настройки");
            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    QuizStart();
                    break;
                case "2":
                    Results();
                    break;
                case "3":
                    Top20();
                    break;
                case "4":
                    Settings();
                    break;
            }
        }
        static void Registration()
        {
            Console.WriteLine("Регистрация нового пользователя");
            Console.Write("Введите логин: ");
            string login = Console.ReadLine();

            if (users.Exists(user => user.Login == login))
            {
                Console.WriteLine("Логин уже занят");
                return;
            }

            string password;
            while (true)
            {
                Console.Write("Введите пароль: ");
                password = Console.ReadLine();
                if (IsValidPassword(password))
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Пароль должен содержать хотя бы одну заглавную букву, одну строчную букву и быть длиной не менее 6 символов");
                }
            }
            string birthDate;
            while (true)
            {
                Console.Write("Введите дату рождения (\"yyyy-MM-dd\"): ");
                birthDate = Console.ReadLine();
                if (IsValidDate(birthDate))
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Дата введена некорректно. Убедитесь, что она соответствует формату (\"yyyy-MM-dd\")");
                }
            }
            users.Add(new User(login, password, birthDate));
            Console.WriteLine("Регистрация окончена");
        }
        static bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 6)
                return false;

            bool hasUpperCase = password.Any(char.IsUpper);
            bool hasLowerCase = password.Any(char.IsLower);

            return hasUpperCase && hasLowerCase;
        }
        static bool IsValidDate(string date)
        {
            return DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out _);
        }
        static void Login()
        {
            Console.WriteLine("Вход");
            Console.Write("Введите логин: ");
            string login = Console.ReadLine();
            Console.Write("Введите пароль: ");
            string password = Console.ReadLine();

            User user = users.Find(u => u.Login == login && u.Password == password);
            if (user != null)
            {
                loggedInUser = user;
                Console.WriteLine($"Добро пожаловать, {user.Login}!");
                UserMenu();
            }
            else
            {
                Console.WriteLine("Неверный логин или пароль");
            }
        }
        static void Results()
        {
            Console.WriteLine("Прошлые результаты:");
            var userResult = results.Where(r => r.UserLogin == loggedInUser.Login);
            foreach (var result in userResult)
            {
                Console.WriteLine($"Викторина: {result.QuizName}, Результат: {result.Point}");
            }
        }
        static void Top20()
        {
            Console.WriteLine("Топ-20 игроков:");
            var topPlayers = results
                .OrderByDescending(r => r.Point)
                .Take(20);

            foreach (var result in topPlayers)
            {
                Console.WriteLine($"Пользователь: {result.UserLogin}, Результат: {result.Point} баллов");
            }
        }
        static void Settings()
        {
            Console.WriteLine("1 - Изменить пароль");
            Console.WriteLine("2 - Изменить дату рождения");

            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    while (true)
                    {
                        Console.Write("Введите новый пароль: ");
                        string newPassword = Console.ReadLine();
                        if (IsValidPassword(newPassword))
                        {
                            loggedInUser.Password = newPassword;
                            Console.WriteLine("Пароль успешно изменен");
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Пароль должен содержать хотя бы одну заглавную букву, одну строчную букву и быть длиной не менее 6 символов");
                        }
                    }
                    break;
                case "2":
                    while (true)
                    {
                        Console.Write("Введите новую дату рождения (\"yyyy-MM-dd\"): ");
                        string newBirthDate = Console.ReadLine();
                        if (IsValidDate(newBirthDate))
                        {
                            loggedInUser.BirthDate = newBirthDate;
                            Console.WriteLine("Дата рождения успешно изменена");
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Дата введена некорректно. Убедитесь, что она соответствует формату (\"yyyy-MM-dd\")");
                        }
                    }
                    break;
                default:
                    Console.WriteLine("Неверный выбор");
                    break;
            }
        }
        static void QuizStart()
        {
            Console.WriteLine("Выберите раздел викторины:");
            int index = 1;
            foreach (var category in quizQuestion.Keys)
            {
                Console.WriteLine($"{index}. {category}");
                index++;
            }
            Console.WriteLine($"{index}. Смешанная викторина");
            Console.Write("Введите номер категории: ");
            if (!int.TryParse(Console.ReadLine(), out int selectedCategoryIndex) ||
                selectedCategoryIndex < 1 || selectedCategoryIndex > quizQuestion.Keys.Count + 1)
            {
                Console.WriteLine("Неверный выбор категории");
                return;
            }

            string selectedCategory = selectedCategoryIndex == quizQuestion.Keys.Count + 1
                ? "Смешанная"
                : quizQuestion.Keys.ElementAt(selectedCategoryIndex - 1);

            List<Quiz> selectedQuestions;
            if (selectedCategory == "Смешанная")
            {
                selectedQuestions = quizQuestion.Values.SelectMany(q => q).OrderBy(_ => Guid.NewGuid()).Take(20).ToList();
            }
            else
            {
                if (!quizQuestion.ContainsKey(selectedCategory))
                {
                    Console.WriteLine("Категория не найдена");
                    return;
                }
                selectedQuestions = quizQuestion[selectedCategory].OrderBy(_ => Guid.NewGuid()).Take(20).ToList();
            }
            int point = 0;
            foreach (var question in selectedQuestions)
            {
                Console.WriteLine(question.Text);
                Console.WriteLine("Введите ответ через запятую если указано в вопросе что их несколько:");
                string[] userAnswers = Console.ReadLine()?.Split(',').Select(a => a.Trim()).ToArray();
                if (userAnswers != null &&
                    userAnswers.OrderBy(a => a).SequenceEqual(question.CorrectAnswers.OrderBy(a => a)))
                {
                    point++;
                }
            }
            Console.WriteLine($"Викторина завершена. Результат {loggedInUser.Login}: {point} из {selectedQuestions.Count}.");
            results.Add(new QuizResult(loggedInUser.Login, point, selectedCategory));
        }
    }
}
