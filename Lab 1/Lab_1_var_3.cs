using System;

enum TimeFrame
{
    Year,
    TwoYears,
    Long
}

class Person
{
    private string _firstName;
    private string _lastName;
    private DateTime _birthDate;

    public Person(string firstName, string lastName, DateTime birthDate)
    {
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
    }

    public Person() : this(firstName: "Невідомо", lastName: "Невідомо", birthDate: DateTime.Now) { }

    public string FirstName
    {
        get => _firstName;
        init => _firstName = value;
    }

    public string LastName
    {
        get => _lastName;
        init => _lastName = value;
    }

    public DateTime BirthDate
    {
        get => _birthDate;
        init => _birthDate = value;
    }

    public int BirthYear
    {
        get => _birthDate.Year;
        set => _birthDate = new DateTime(value, _birthDate.Month, _birthDate.Day);
    }

    public override string ToString()
    {
        return $"Ім'я: {FirstName}, Прізвище: {LastName}, Дата народження: {_birthDate:dd.MM.yyyy}";
    }

    public virtual string ToShortString()
    {
        return $"{LastName} {FirstName}";
    }
}

class Paper
{
    public string Title { get; set; }
    public Person Author { get; set; }
    public DateTime Date { get; set; }

    public Paper(string title, Person author, DateTime date)
    {
        Title = title;
        Author = author;
        Date = date;
    }

    public Paper() : this(title: "Невідомо", author: new Person(), date: DateTime.Now) { }

    public override string ToString()
    {
        return $"[{Title}, автор: {Author.ToShortString()}, дата: {Date:dd.MM.yyyy}]";
    }
}

class ResearchTeam
{
    private string _topic;
    private string _organization;
    private int _registrationNumber;
    private TimeFrame _duration;
    private Paper[] _publications;

    public ResearchTeam(string topic, string organization, int registrationNumber, TimeFrame duration)
    {
        Topic = topic;
        Organization = organization;
        RegistrationNumber = registrationNumber;
        Duration = duration;
        _publications = new Paper[0];
    }

    public ResearchTeam() : this(topic: "Невідома тема", organization: "Невідома організація", registrationNumber: 1, duration: TimeFrame.Year) { }

    public string Topic
    {
        get => _topic;
        init => _topic = value;
    }

    public string Organization
    {
        get => _organization;
        init => _organization = value;
    }

    public int RegistrationNumber
    {
        get => _registrationNumber;
        init => _registrationNumber = value;
    }

    public TimeFrame Duration
    {
        get => _duration;
        init => _duration = value;
    }

    public Paper[] Publications
    {
        get => _publications;
        init => _publications = value;
    }

    public Paper? LatestPaper
    {
        get
        {
            if (_publications == null || _publications.Length == 0) return null;
            Paper latest = _publications[0];
            foreach (var p in _publications)
            {
                if (p.Date > latest.Date) latest = p;
            }
            return latest;
        }
    }

    public bool this[TimeFrame tf]
    {
        get => _duration == tf;
    }

    public void AddPapers(params Paper[] newPapers)
    {
        if (newPapers == null) return;
        int oldLen = _publications.Length;
        Paper[] updated = new Paper[oldLen + newPapers.Length];
        
        _publications.CopyTo(updated, 0);
        newPapers.CopyTo(updated, oldLen);
        _publications = updated;
    }

    public override string ToString()
    {
        string pubList = _publications.Length == 0 ? "немає" : string.Join(",\n  ", (object[])_publications);
        return $"Тема: {_topic}\nОрганізація: {_organization}\nНомер: {_registrationNumber}\nТривалість: {_duration}\nПублікації:\n  {pubList}";
    }

    public virtual string ToShortString()
    {
        return $"Тема: {_topic}, Організація: {_organization}, Номер: {_registrationNumber}, Тривалість: {_duration}";
    }
}

// MAIN 
class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        ResearchTeam team = new ResearchTeam("Моніторинг лісових екосистем", "Інститут Екології", 5544, TimeFrame.TwoYears);

        Console.WriteLine("=== ToShortString (до додавання публікацій) ===");
        Console.WriteLine(team.ToShortString());
        Console.WriteLine();

        Console.WriteLine("=== Індексатор ===");
        Console.WriteLine($"TimeFrame.Year:          {team[TimeFrame.Year]}");
        Console.WriteLine($"TimeFrame.TwoYears:      {team[TimeFrame.TwoYears]}");
        Console.WriteLine($"TimeFrame.Long:          {team[TimeFrame.Long]}");
        Console.WriteLine();

        team = new ResearchTeam
        {
            Topic = "Аналіз клімату Карпат",
            Organization = "НАН України",
            RegistrationNumber = 101,
            Duration = TimeFrame.Long,
            Publications = new Paper[0]
        };

        Console.WriteLine("=== ToString (після присвоєння через init-властивості) ===");
        Console.WriteLine(team.ToString());
        Console.WriteLine();

        Person p1 = new Person("Віктор", "Мельник", new DateTime(1975, 3, 10));
        Person p2 = new Person("Софія", "Ткаченко", new DateTime(1992, 7, 18));

        team.AddPapers(
            new Paper("Вплив клімату на ріст лісів", p1, new DateTime(2022, 4, 12)),
            new Paper("Аналіз якості повітря", p2, new DateTime(2020, 9, 25)),
            new Paper("Сучасна екологія", p2, new DateTime(2024, 1, 10))
        );

        Console.WriteLine("=== ToString (після додавання публікацій) ===");
        Console.WriteLine(team.ToString());
        Console.WriteLine();
        
        Console.WriteLine("=== Остання публікація (LatestPaper) ===");
        Console.WriteLine(team.LatestPaper?.ToString() ?? "Публікацій немає");
        Console.WriteLine();

        Console.WriteLine("=== Порівняння часу для масивів Paper ===");

        int nRows, nColumns, totalElements;

        Console.Write("Введіть кількість рядків та стовпців через кому (наприклад 100,50): ");
        string input = Console.ReadLine() ?? "100,50";
        string[] parts = input.Split(',');
        nRows = int.Parse(parts[0].Trim());
        nColumns = int.Parse(parts[1].Trim());
        totalElements = nRows * nColumns;

        // Одновимірний
        Paper[] arr1D = new Paper[totalElements];
        for (int i = 0; i < totalElements; i++)
            arr1D[i] = new Paper();

        // Прямокутний
        Paper[,] arr2D = new Paper[nRows, nColumns];
        for (int i = 0; i < nRows; i++)
            for (int j = 0; j < nColumns; j++)
                arr2D[i, j] = new Paper();

        // Зубчастий 
        int r = 0, t = 0;
        do { t += ++r; } while (t < totalElements);
        Paper[][] arrJagged = new Paper[r][];
        
        for (int i = 0; i < r - 1; i++)
        {
            arrJagged[i] = new Paper[i + 1];
        }
        arrJagged[r - 1] = new Paper[r - (t - totalElements)];
        
        for (int i = 0; i < r; i++) 
        {
            if (arrJagged[i] == null)
                arrJagged[i] = new Paper[0];
                
            for (int j = 0; j < arrJagged[i].Length; j++)
            {
                arrJagged[i][j] = new Paper();
            }
        }

        string testSubject = "Тест швидкості";

        long t1 = Environment.TickCount;
        for (int i = 0; i < totalElements; i++)
            arr1D[i].Title = testSubject;
        long time1D = Environment.TickCount - t1;

        long t2 = Environment.TickCount;
        for (int i = 0; i < nRows; i++)
            for (int j = 0; j < nColumns; j++)
                arr2D[i, j].Title = testSubject;
        long time2D = Environment.TickCount - t2;

        long t3 = Environment.TickCount;
        for (int i = 0; i < r; i++)
            for (int j = 0; j < arrJagged[i].Length; j++)
                arrJagged[i][j].Title = testSubject;
        long timeJagged = Environment.TickCount - t3;

        Console.WriteLine($"Розмір: {nRows} x {nColumns} = {totalElements} елементів");
        Console.WriteLine($"Одновимірний масив:          {time1D} мс");
        Console.WriteLine($"Двовимірний прямокутний:     {time2D} мс");
        Console.WriteLine($"Двовимірний зубчастий:       {timeJagged} мс");
    }
}