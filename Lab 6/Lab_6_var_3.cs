using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

// ENUM  
enum TimeFrame { Year, TwoYears, Long }

// INTERFACES  
interface INameAndCopy
{
    string Name { get; set; }
    object DeepCopy();
}

// PERSON  
class Person
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public DateTime BirthDate { get; set; }

    public Person(string firstName, string lastName, DateTime birthDate)
    {
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
    }

    public Person() { }

    public override string ToString() => $"{FirstName} {LastName}, Народився: {BirthDate:dd.MM.yyyy}";
    public virtual string ToShortString() => $"{FirstName} {LastName}";
}

// PAPER  
class Paper
{
    public string Title { get; set; } = "";
    public Person Author { get; set; } = new Person();
    public DateTime Date { get; set; }

    public Paper(string title, Person author, DateTime date)
    {
        Title = title;
        Author = author;
        Date = date;
    }

    public Paper() { }

    public override string ToString() => $"'{Title}' (Автор: {Author.ToShortString()}), Дата: {Date:dd.MM.yyyy}";
}

// TEAM  
class Team : INameAndCopy
{
    public string Organization { get; set; } = "Невідома організація";
    public int RegistrationNumber { get; set; } = 1;

    public Team(string organization, int registrationNumber)
    {
        Organization = organization;
        RegistrationNumber = registrationNumber;
    }

    public Team() { }

    [JsonIgnore]
    public string Name
    {
        get => Organization;
        set => Organization = value;
    }

    public virtual object DeepCopy() => new Team(Organization, RegistrationNumber);

    public override string ToString() => $"Орг: {Organization}, Реєстр.№: {RegistrationNumber}";
}

// RESEARCH TEAM  
class ResearchTeam : Team
{
    public string Topic { get; set; } = "Невідома тема";
    public TimeFrame Duration { get; set; } = TimeFrame.Year;
    public List<Person> Participants { get; set; } = new List<Person>();
    public List<Paper> Publications { get; set; } = new List<Paper>();

    public ResearchTeam(string topic, string organization, int registrationNumber, TimeFrame duration)
        : base(organization, registrationNumber)
    {
        Topic = topic;
        Duration = duration;
    }

    public ResearchTeam() { }

    [JsonIgnore]
    public Paper? LatestPaper => Publications.OrderByDescending(p => p.Date).FirstOrDefault();

    public void AddPapers(params Paper[] newPapers) => Publications.AddRange(newPapers);
    public void AddPersons(params Person[] newPersons) => Participants.AddRange(newPersons);

    public override string ToString()
    {
        string res = $"Тема: {Topic}, {base.ToString()}, Тривалість: {Duration}\nУчасники:\n";
        foreach (var p in Participants) res += $"  - {p}\n";
        res += "Публікації:\n";
        foreach (var p in Publications) res += $"  - {p}\n";
        return res;
    }

    
    // МЕТОДИ СЕРІАЛІЗАЦІЇ ТА РОБОТИ З ФАЙЛАМИ
    
    public new ResearchTeam DeepCopy()
    {
        ResearchTeam? copy = null;
        MemoryStream? ms = null;
        try
        {
            ms = new MemoryStream();
            JsonSerializer.Serialize(ms, this); 
            ms.Position = 0; 
            copy = JsonSerializer.Deserialize<ResearchTeam>(ms); 
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка при DeepCopy: {ex.Message}");
        }
        finally
        {
            ms?.Close();
        }
        return copy ?? new ResearchTeam();
    }


    public bool Save(string filename)
    {
        FileStream? fs = null;
        try
        {
            fs = new FileStream(filename, FileMode.Create);
            JsonSerializer.Serialize(fs, this, new JsonSerializerOptions { WriteIndented = true });
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка збереження: {ex.Message}");
            return false;
        }
        finally
        {
            fs?.Close();
        }
    }

    public bool Load(string filename)
    {
        FileStream? fs = null;
        try
        {
            fs = new FileStream(filename, FileMode.Open);
            var loaded = JsonSerializer.Deserialize<ResearchTeam>(fs);
            if (loaded != null)
            {
                // Оновлюємо поточний об'єкт
                this.Topic = loaded.Topic;
                this.Organization = loaded.Organization;
                this.RegistrationNumber = loaded.RegistrationNumber;
                this.Duration = loaded.Duration;
                this.Participants = loaded.Participants;
                this.Publications = loaded.Publications;
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка завантаження: {ex.Message}");
            return false; // Стан об'єкту залишається незмінним
        }
        finally
        {
            fs?.Close();
        }
    }

    public bool AddFromConsole()
    {
        Console.WriteLine("\n[ВВІД] Додавання нової публікації.");
        Console.WriteLine("Введіть дані через крапку з комою (;)");
        Console.WriteLine("Формат: Назва статті ; Ім'я Автора ; Прізвище Автора ; ДН Автора(РРРР-ММ-ДД) ; Дата публікації(РРРР-ММ-ДД)");
        Console.Write("> ");
        
        string? input = Console.ReadLine();
        try
        {
            if (string.IsNullOrWhiteSpace(input)) throw new Exception("Рядок порожній.");

            string[] parts = input.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 5) throw new FormatException("Невірна кількість параметрів. Потрібно рівно 5.");

            string title = parts[0].Trim();
            string fName = parts[1].Trim();
            string lName = parts[2].Trim();
            DateTime bDate = DateTime.Parse(parts[3].Trim());
            DateTime pDate = DateTime.Parse(parts[4].Trim());

            Person author = new Person(fName, lName, bDate);
            Paper paper = new Paper(title, author, pDate);
            
            this.AddPapers(paper);
            Console.WriteLine("Публікацію додано.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка вводу/парсингу: {ex.Message}");
            return false;
        }
    }


    public static bool Save(string filename, ResearchTeam obj)
    {
        return obj.Save(filename);
    }


    public static bool Load(string filename, ResearchTeam obj)
    {
        return obj.Load(filename);
    }
}

// MAIN  
class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("=== 1. Створення об'єкта та DeepCopy (MemoryStream) ===");
        ResearchTeam team = new ResearchTeam("Збереження Карпат", "Інститут Екології", 111, TimeFrame.TwoYears);
        team.AddPersons(new Person("Віктор", "Мельник", new DateTime(1980, 5, 12)));
        team.AddPapers(new Paper("Ліси", team.Participants[0], new DateTime(2023, 1, 15)));

        ResearchTeam copy = team.DeepCopy();
        
        Console.WriteLine("--- Оригінал ---");
        Console.WriteLine(team);
        Console.WriteLine("--- Копія ---");
        Console.WriteLine(copy);

        Console.WriteLine("\n=== 2. Робота з файлом ===");
        Console.Write("Введіть ім'я файлу (натисніть Enter для 'data.json'): ");
        string filename = Console.ReadLine() ?? "";
        if (string.IsNullOrWhiteSpace(filename)) filename = "data.json";

        if (!File.Exists(filename))
        {
            Console.WriteLine($"Файл '{filename}' не існує. Створюємо файл та записуємо поточні дані...");
            team.Save(filename);
        }
        else
        {
            Console.WriteLine($"Файл '{filename}' знайдено. Ініціалізація даними з файлу...");
            team.Load(filename);
        }

        Console.WriteLine("\n=== 3. Поточний стан об'єкта ===");
        Console.WriteLine(team);

        Console.WriteLine("\n=== 4. Метод AddFromConsole() та Save() ===");
        team.AddFromConsole();
        team.Save(filename);
        Console.WriteLine("\n--- Стан після додавання ---");
        Console.WriteLine(team);

        Console.WriteLine("\n=== 5. Тестування статичних методів ===");
        Console.WriteLine("Завантаження даних з файлу...");
        ResearchTeam.Load(filename, team);
        
        team.AddFromConsole();
        
        Console.WriteLine("Збереження даних у файл...");
        ResearchTeam.Save(filename, team);

        Console.WriteLine("\n=== 6. Фінальний стан об'єкта ===");
        Console.WriteLine(team);
    }
}