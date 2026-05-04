using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

enum TimeFrame
{
    Year,
    TwoYears,
    Long
}

interface INameAndCopy
{
    string Name { get; set; }
    object DeepCopy();
}

class Person
{
    public string FirstName { get; init; } = "";
    public string LastName { get; init; } = "";
    public DateTime BirthDate { get; init; }

    public Person(string firstName, string lastName, DateTime birthDate)
    {
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
    }

    public Person() : this("Невідомо", "Невідомо", new DateTime(2000, 1, 1)) { }

    public override bool Equals(object? obj)
    {
        if (obj is not Person p) return false;
        return FirstName == p.FirstName && LastName == p.LastName && BirthDate == p.BirthDate;
    }

    public static bool operator ==(Person? left, Person? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(Person? left, Person? right) => !(left == right);

    public override int GetHashCode() => HashCode.Combine(FirstName, LastName, BirthDate);

    public virtual object DeepCopy() => new Person(FirstName, LastName, BirthDate);

    public override string ToString() => $"{FirstName} {LastName}, Народився: {BirthDate:dd.MM.yyyy}";

    public virtual string ToShortString() => $"{FirstName} {LastName}";
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

    public Paper() : this("Без назви", new Person(), DateTime.Now) { }

    public virtual object DeepCopy() => new Paper(Title, (Person)Author.DeepCopy(), Date);

    public override string ToString() => $"'{Title}' (Автор: {Author.ToShortString()}), Дата: {Date:dd.MM.yyyy}";
}

class Team : INameAndCopy, IComparable
{
    protected string _organization;
    protected int _registrationNumber;

    public Team(string organization, int registrationNumber)
    {
        _organization = organization;
        RegistrationNumber = registrationNumber;
    }

    public Team() : this("Невідома організація", 1) { }

    public string Organization
    {
        get => _organization;
        init => _organization = value;
    }

    public int RegistrationNumber
    {
        get => _registrationNumber;
        init
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Реєстраційний номер повинен бути більшим за нуль.");
            _registrationNumber = value;
        }
    }

    public string Name
    {
        get => _organization;
        set => _organization = value;
    }

    public virtual object DeepCopy() => new Team(Organization, RegistrationNumber);

    public override bool Equals(object? obj)
    {
        if (obj is not Team t) return false;
        return Organization == t.Organization && RegistrationNumber == t.RegistrationNumber;
    }

    public static bool operator ==(Team? left, Team? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(Team? left, Team? right) => !(left == right);

    public override int GetHashCode() => HashCode.Combine(Organization, RegistrationNumber);

    public override string ToString() => $"Орг: {Organization}, Реєстр.№: {RegistrationNumber}";

    public int CompareTo(object? obj)
    {
        if (obj is Team other)
            return RegistrationNumber.CompareTo(other.RegistrationNumber);
        throw new ArgumentException("Об'єкт не є типом Team");
    }
}

class ResearchTeam : Team, IComparer<ResearchTeam>
{
    private string _topic;
    private TimeFrame _duration;
    private List<Person> _participants;
    private List<Paper> _publications;

    public ResearchTeam(string topic, string organization, int registrationNumber, TimeFrame duration)
        : base(organization, registrationNumber)
    {
        _topic = topic;
        _duration = duration;
        _participants = new List<Person>();
        _publications = new List<Paper>();
    }

    public ResearchTeam() : base()
    {
        _topic = "Невідома тема";
        _duration = TimeFrame.Year;
        _participants = new List<Person>();
        _publications = new List<Paper>();
    }

    public string Topic { get => _topic; init => _topic = value; }
    public TimeFrame Duration { get => _duration; init => _duration = value; }
    public List<Person> Participants { get => _participants; init => _participants = value; }
    public List<Paper> Publications { get => _publications; init => _publications = value; }

    public Team TeamBase
    {
        get => new Team(Organization, RegistrationNumber);
        init
        {
            _organization = value.Organization;
            _registrationNumber = value.RegistrationNumber;
        }
    }

    public Paper? LatestPaper => _publications.OrderByDescending(p => p.Date).FirstOrDefault();

    public void AddPapers(params Paper[] newPapers) => _publications.AddRange(newPapers);
    public void AddPersons(params Person[] newPersons) => _participants.AddRange(newPersons);

    public override string ToString()
    {
        string res = $"Тема: {_topic}, {base.ToString()}, Тривалість: {_duration}\nУчасники:\n";
        foreach (var p in _participants) res += $"  - {p}\n";
        res += "Публікації:\n";
        foreach (var p in _publications) res += $"  - {p}\n";
        return res;
    }

    public string ToShortString() => $"Тема: {_topic}, {base.ToString()}, Тривалість: {_duration}";

    public override object DeepCopy()
    {
        ResearchTeam copy = new ResearchTeam(_topic, Organization, RegistrationNumber, _duration);
        foreach (var p in _participants) copy._participants.Add((Person)p.DeepCopy());
        foreach (var p in _publications) copy._publications.Add((Paper)p.DeepCopy());
        return copy;
    }

    public int Compare(ResearchTeam? x, ResearchTeam? y)
    {
        if (x == null || y == null) return 0;
        return string.Compare(x.Topic, y.Topic, StringComparison.Ordinal);
    }
}

//  COMPARER (Допоміжний клас)
class ResearchTeamPublicationComparer : IComparer<ResearchTeam>
{
    // Сортування за кількістю публікацій
    public int Compare(ResearchTeam? x, ResearchTeam? y)
    {
        if (x == null || y == null) return 0;
        return x.Publications.Count.CompareTo(y.Publications.Count);
    }
}

class ResearchTeamCollection
{
    private List<ResearchTeam> _teams = new List<ResearchTeam>();

    public void AddDefaults()
    {
        ResearchTeam rt1 = new ResearchTeam("Збереження лісів", "Інститут Екології", 101, TimeFrame.TwoYears);
        rt1.AddPersons(new Person("Віктор", "Мельник", new DateTime(1980, 5, 12)));
        rt1.AddPapers(new Paper("Ліси Карпат", rt1.Participants[0], DateTime.Now));

        ResearchTeam rt2 = new ResearchTeam("Аналіз водойм", "НАН України", 50, TimeFrame.Year);
        rt2.AddPersons(new Person("Софія", "Ткаченко", new DateTime(1992, 8, 20)), new Person("Іван", "Коваль", new DateTime(2000, 1, 1)));
        rt2.AddPapers(new Paper("Вода Дніпра", rt2.Participants[0], DateTime.Now), new Paper("Озера", rt2.Participants[1], DateTime.Now));

        ResearchTeam rt3 = new ResearchTeam("Екологія міста", "КНУ", 305, TimeFrame.TwoYears);
        rt3.AddPersons(new Person("Олег", "Петренко", new DateTime(1995, 2, 10)));

        _teams.Add(rt1);
        _teams.Add(rt2);
        _teams.Add(rt3);
    }

    public void AddResearchTeams(params ResearchTeam[] teams) => _teams.AddRange(teams);

    public override string ToString()
    {
        string res = "=== Повний список проектів ===\n";
        foreach (var t in _teams) res += t.ToString() + "\n";
        return res;
    }

    public virtual string ToShortString()
    {
        string res = "=== Короткий список проектів ===\n";
        foreach (var t in _teams)
        {
            res += $"{t.ToShortString()}, К-сть учасників: {t.Participants.Count}, К-сть публікацій: {t.Publications.Count}\n";
        }
        return res;
    }

    // МЕТОДИ СОРТУВАННЯ
    public void SortByRegistrationNumber() => _teams.Sort(); // Використовує IComparable в Team
    public void SortByTopic() => _teams.Sort(new ResearchTeam()); // Використовує IComparer в ResearchTeam
    public void SortByPublicationCount() => _teams.Sort(new ResearchTeamPublicationComparer());

    // LINQ ВЛАСТИВОСТІ ТА МЕТОДИ
    public int MinRegistrationNumber
    {
        get => _teams.Count == 0 ? 0 : _teams.Min(t => t.RegistrationNumber);
    }

    public IEnumerable<ResearchTeam> TwoYearsTeams
    {
        get => _teams.Where(t => t.Duration == TimeFrame.TwoYears);
    }

    public List<ResearchTeam> NGroup(int value)
    {
        return _teams.GroupBy(t => t.Participants.Count)
                     .FirstOrDefault(g => g.Key == value)?
                     .ToList() ?? new List<ResearchTeam>();
    }
}

class TestCollections
{
    private List<Team> _listKeys = new List<Team>();
    private List<string> _listStringKeys = new List<string>();
    private Dictionary<Team, ResearchTeam> _dict = new Dictionary<Team, ResearchTeam>();
    private Dictionary<string, ResearchTeam> _dictString = new Dictionary<string, ResearchTeam>();

    // Метод для автоматичної генерації 
    public static ResearchTeam Generate(int i)
    {
        return new ResearchTeam($"Еко-Тема #{i}", $"Організація #{i}", i + 1, TimeFrame.Year);
    }

    public TestCollections(int count)
    {
        for (int i = 0; i < count; i++)
        {
            ResearchTeam rt = Generate(i);
            Team key = rt.TeamBase; 
            string strKey = key.ToString();

            _listKeys.Add(key);
            _listStringKeys.Add(strKey);
            _dict.Add(key, rt);
            _dictString.Add(strKey, rt);
        }
    }

    public void MeasureSearchTimes()
    {
        int count = _listKeys.Count;
        if (count == 0) return;

        // Визначаємо 4 елементи для пошуку
        Team first = _listKeys[0];
        Team middle = _listKeys[count / 2];
        Team last = _listKeys[count - 1];
        Team notFound = Generate(count + 100).TeamBase; 

        Console.WriteLine("\n=== ТЕСТУВАННЯ ЧАСУ ПОШУКУ (у тіках Stopwatch) ===");
        MeasureElement("ПЕРШИЙ ЕЛЕМЕНТ", first);
        MeasureElement("ЦЕНТРАЛЬНИЙ ЕЛЕМЕНТ", middle);
        MeasureElement("ОСТАННІЙ ЕЛЕМЕНТ", last);
        MeasureElement("ЕЛЕМЕНТ ПОЗА КОЛЕКЦІЄЮ", notFound);
    }

    private void MeasureElement(string label, Team key)
    {
        string strKey = key.ToString();
        ResearchTeam valueToFind = new ResearchTeam("Тест", key.Organization, key.RegistrationNumber, TimeFrame.Year);

        Console.WriteLine($"--- {label} ---");
        
        Stopwatch sw = Stopwatch.StartNew();
        _listKeys.Contains(key);
        sw.Stop();
        Console.WriteLine($"List<Team>.Contains:                  {sw.ElapsedTicks} ticks");

        sw.Restart();
        _listStringKeys.Contains(strKey);
        sw.Stop();
        Console.WriteLine($"List<string>.Contains:                {sw.ElapsedTicks} ticks");

        sw.Restart();
        _dict.ContainsKey(key);
        sw.Stop();
        Console.WriteLine($"Dictionary<Team, ...>.ContainsKey:    {sw.ElapsedTicks} ticks");

        sw.Restart();
        _dictString.ContainsKey(strKey);
        sw.Stop();
        Console.WriteLine($"Dictionary<string, ...>.ContainsKey:  {sw.ElapsedTicks} ticks");

        sw.Restart();
        _dict.ContainsValue(valueToFind);
        sw.Stop();
        Console.WriteLine($"Dictionary<Team, ...>.ContainsValue:  {sw.ElapsedTicks} ticks");
        Console.WriteLine();
    }
}

class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // 1. Створення колекції та додавання елементів
        ResearchTeamCollection collection = new ResearchTeamCollection();
        collection.AddDefaults();
        Console.WriteLine(collection.ToShortString());

        // 2. Сортування
        Console.WriteLine("\n=== Сортування за Номером Реєстрації ===");
        collection.SortByRegistrationNumber();
        Console.WriteLine(collection.ToShortString());

        Console.WriteLine("=== Сортування за Темою Дослідження ===");
        collection.SortByTopic();
        Console.WriteLine(collection.ToShortString());

        Console.WriteLine("=== Сортування за Кількістю Публікацій ===");
        collection.SortByPublicationCount();
        Console.WriteLine(collection.ToShortString());

        // 3. Робота з LINQ
        Console.WriteLine("\n=== LINQ: Мінімальний номер реєстрації ===");
        Console.WriteLine(collection.MinRegistrationNumber);

        Console.WriteLine("\n=== LINQ: Проекти з тривалістю TwoYears ===");
        foreach (var t in collection.TwoYearsTeams) Console.WriteLine(t.ToShortString());

        Console.WriteLine("\n=== LINQ: Проекти з 1 учасником (NGroup) ===");
        foreach (var t in collection.NGroup(1)) Console.WriteLine(t.ToShortString());

        // 4. Безпечне введення для TestCollections
        Console.WriteLine("\n=== ГЕНЕРАЦІЯ КОЛЕКЦІЙ ДЛЯ ТЕСТУВАННЯ ===");
        int count;
        while (true)
        {
            Console.Write("Введіть кількість елементів для генерації (наприклад, 100000): ");
            if (int.TryParse(Console.ReadLine(), out count) && count > 0)
                break;
            Console.WriteLine("Помилка! Введіть ціле додатнє число.");
        }

        // 5. Тестування часу
        TestCollections tests = new TestCollections(count);
        tests.MeasureSearchTimes();
    }
}