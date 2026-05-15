using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

// ENUM 
enum TimeFrame
{
    Year,
    TwoYears,
    Long
}

// INTERFACES 
interface INameAndCopy
{
    string Name { get; set; }
    object DeepCopy();
}

//  PERSON 
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

//  PAPER 
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

//  TEAM 
class Team : INameAndCopy, IComparable, IComparable<Team>
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

    // Реалізація для об'єктів Object (для старих колекцій)
    public int CompareTo(object? obj)
    {
        if (obj is Team other) return CompareTo(other);
        throw new ArgumentException("Об'єкт не є типом Team");
    }

    // Реалізація IComparable<Team> (Обов'язково для SortedDictionary/SortedList)
    public int CompareTo(Team? other)
    {
        if (other == null) return 1;
        return RegistrationNumber.CompareTo(other.RegistrationNumber);
    }
}

//  RESEARCH TEAM 
class ResearchTeam : Team, IComparer<ResearchTeam>
{
    private string _topic;
    private TimeFrame _duration;
    
    // Використовуємо ImmutableList замість звичайного List
    private ImmutableList<Person> _participants;
    private ImmutableList<Paper> _publications;

    public ResearchTeam(string topic, string organization, int registrationNumber, TimeFrame duration)
        : base(organization, registrationNumber)
    {
        _topic = topic;
        _duration = duration;
        _participants = ImmutableList<Person>.Empty;
        _publications = ImmutableList<Paper>.Empty;
    }

    public ResearchTeam() : base()
    {
        _topic = "Невідома тема";
        _duration = TimeFrame.Year;
        _participants = ImmutableList<Person>.Empty;
        _publications = ImmutableList<Paper>.Empty;
    }

    public string Topic { get => _topic; init => _topic = value; }
    public TimeFrame Duration { get => _duration; init => _duration = value; }
    
    public ImmutableList<Person> Participants { get => _participants; init => _participants = value; }
    public ImmutableList<Paper> Publications { get => _publications; init => _publications = value; }

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

    // При додаванні ImmutableList повертає новий список, тому треба переприсвоювати
    public void AddPapers(params Paper[] newPapers) => _publications = _publications.AddRange(newPapers);
    public void AddPersons(params Person[] newPersons) => _participants = _participants.AddRange(newPersons);

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
        foreach (var p in _participants) copy.AddPersons((Person)p.DeepCopy());
        foreach (var p in _publications) copy.AddPapers((Paper)p.DeepCopy());
        return copy;
    }

    public int Compare(ResearchTeam? x, ResearchTeam? y)
    {
        if (x == null || y == null) return 0;
        return string.Compare(x.Topic, y.Topic, StringComparison.Ordinal);
    }
}

// COMPARER 
class ResearchTeamPublicationComparer : IComparer<ResearchTeam>
{
    public int Compare(ResearchTeam? x, ResearchTeam? y)
    {
        if (x == null || y == null) return 0;
        return x.Publications.Count.CompareTo(y.Publications.Count);
    }
}

// RESEARCH TEAM COLLECTION 
class ResearchTeamCollection
{
    // ImmutableList замість звичайного List
    private ImmutableList<ResearchTeam> _teams = ImmutableList<ResearchTeam>.Empty;

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

        _teams = _teams.Add(rt1).Add(rt2).Add(rt3);
    }

    public void AddResearchTeams(params ResearchTeam[] teams) => _teams = _teams.AddRange(teams);

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

    // Сортування Immutable колекцій повертає нову колекцію
    public void SortByRegistrationNumber() => _teams = _teams.Sort(); 
    public void SortByTopic() => _teams = _teams.Sort(new ResearchTeam()); 
    public void SortByPublicationCount() => _teams = _teams.Sort(new ResearchTeamPublicationComparer());

    // LINQ
    public int MinRegistrationNumber => _teams.Count == 0 ? 0 : _teams.Min(t => t.RegistrationNumber);
    public IEnumerable<ResearchTeam> TwoYearsTeams => _teams.Where(t => t.Duration == TimeFrame.TwoYears);
    
    public List<ResearchTeam> NGroup(int value)
    {
        return _teams.GroupBy(t => t.Participants.Count)
                     .FirstOrDefault(g => g.Key == value)?
                     .ToList() ?? new List<ResearchTeam>();
    }
}

// TEST COLLECTIONS 
class TestCollections
{
    // --- 1. Standard Collections ---
    private List<Team> _stdListKeys = new();
    private List<string> _stdStringKeys = new();
    private Dictionary<Team, ResearchTeam> _stdDict = new();
    private Dictionary<string, ResearchTeam> _stdStringDict = new();

    // --- 2. Immutable Collections ---
    private ImmutableList<Team> _immListKeys;
    private ImmutableList<string> _immStringKeys;
    private ImmutableDictionary<Team, ResearchTeam> _immDict;
    private ImmutableDictionary<string, ResearchTeam> _immStringDict;

    // --- 3. Sorted Collections ---
    private SortedList<Team, ResearchTeam> _sortList = new();
    private SortedList<string, ResearchTeam> _sortStringList = new();
    private SortedDictionary<Team, ResearchTeam> _sortDict = new();
    private SortedDictionary<string, ResearchTeam> _sortStringDict = new();

    public static ResearchTeam Generate(int i)
    {
        return new ResearchTeam($"Еко-Тема #{i}", $"Організація #{i}", i + 1, TimeFrame.Year);
    }

    public TestCollections(int count)
    {
        // Використовуємо Builder для швидкого заповнення Immutable колекцій
        var bList = ImmutableList.CreateBuilder<Team>();
        var bStrList = ImmutableList.CreateBuilder<string>();
        var bDict = ImmutableDictionary.CreateBuilder<Team, ResearchTeam>();
        var bStrDict = ImmutableDictionary.CreateBuilder<string, ResearchTeam>();

        for (int i = 0; i < count; i++)
        {
            ResearchTeam rt = Generate(i);
            Team key = rt.TeamBase;
            string strKey = key.ToString();

            // Standard
            _stdListKeys.Add(key);
            _stdStringKeys.Add(strKey);
            _stdDict.Add(key, rt);
            _stdStringDict.Add(strKey, rt);

            // Immutable
            bList.Add(key);
            bStrList.Add(strKey);
            bDict.Add(key, rt);
            bStrDict.Add(strKey, rt);

            // Sorted
            _sortList.Add(key, rt);
            _sortStringList.Add(strKey, rt);
            _sortDict.Add(key, rt);
            _sortStringDict.Add(strKey, rt);
        }

        _immListKeys = bList.ToImmutable();
        _immStringKeys = bStrList.ToImmutable();
        _immDict = bDict.ToImmutable();
        _immStringDict = bStrDict.ToImmutable();
    }

    public void MeasureSearchTimes()
    {
        int count = _stdListKeys.Count;
        if (count == 0) return;

        Team first = _stdListKeys[0];
        Team middle = _stdListKeys[count / 2];
        Team last = _stdListKeys[count - 1];
        Team notFound = Generate(count + 100).TeamBase;

        Console.WriteLine("\n=== ПОРІВНЯННЯ ЧАСУ ПОШУКУ (Standard vs Immutable vs Sorted) ===");
        MeasureElement("ПЕРШИЙ ЕЛЕМЕНТ", first);
        MeasureElement("ЦЕНТРАЛЬНИЙ ЕЛЕМЕНТ", middle);
        MeasureElement("ОСТАННІЙ ЕЛЕМЕНТ", last);
        MeasureElement("ЕЛЕМЕНТ ПОЗА КОЛЕКЦІЄЮ", notFound);
    }

    private void MeasureElement(string label, Team key)
    {
        string strKey = key.ToString();
        ResearchTeam valueToFind = new ResearchTeam("Тест", key.Organization, key.RegistrationNumber, TimeFrame.Year);

        Console.WriteLine($"\n--- {label} ---");

        // LISTS
        Measure("Standard List<Team>", () => _stdListKeys.Contains(key));
        Measure("Immutable List<Team>", () => _immListKeys.Contains(key));
        
        Measure("Standard List<string>", () => _stdStringKeys.Contains(strKey));
        Measure("Immutable List<string>", () => _immStringKeys.Contains(strKey));

        // DICTIONARIES (Key)
        Measure("Standard Dict<Team>", () => _stdDict.ContainsKey(key));
        Measure("Immutable Dict<Team>", () => _immDict.ContainsKey(key));
        Measure("Sorted List<Team>", () => _sortList.ContainsKey(key));
        Measure("Sorted Dict<Team>", () => _sortDict.ContainsKey(key));

        Measure("Standard Dict<string>", () => _stdStringDict.ContainsKey(strKey));
        Measure("Immutable Dict<string>", () => _immStringDict.ContainsKey(strKey));
        Measure("Sorted List<string>", () => _sortStringList.ContainsKey(strKey));
        Measure("Sorted Dict<string>", () => _sortStringDict.ContainsKey(strKey));

        // DICTIONARIES (Value)
        Measure("Standard Dict<Team> (Value)", () => _stdDict.ContainsValue(valueToFind));
        Measure("Immutable Dict<Team> (Value)", () => _immDict.ContainsValue(valueToFind));
        Measure("Sorted List<Team> (Value)", () => _sortList.ContainsValue(valueToFind));
        Measure("Sorted Dict<Team> (Value)", () => _sortDict.ContainsValue(valueToFind));
    }

    private void Measure(string name, Action action)
    {
        Stopwatch sw = Stopwatch.StartNew();
        action();
        sw.Stop();
        Console.WriteLine($"{name,-35} {sw.ElapsedTicks} ticks");
    }
}

// MAIN 
class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        ResearchTeamCollection collection = new ResearchTeamCollection();
        collection.AddDefaults();
        Console.WriteLine(collection.ToShortString());

        Console.WriteLine("\n=== Сортування (Immutable List) за Номером Реєстрації ===");
        collection.SortByRegistrationNumber();
        Console.WriteLine(collection.ToShortString());

        Console.WriteLine("=== Сортування (Immutable List) за Темою Дослідження ===");
        collection.SortByTopic();
        Console.WriteLine(collection.ToShortString());

        Console.WriteLine("=== Сортування (Immutable List) за Кількістю Публікацій ===");
        collection.SortByPublicationCount();
        Console.WriteLine(collection.ToShortString());

        Console.WriteLine("\n=== ГЕНЕРАЦІЯ КОЛЕКЦІЙ ДЛЯ ТЕСТУВАННЯ ===");
        int count;
        while (true)
        {
            Console.Write("Введіть кількість елементів для генерації: ");
            if (int.TryParse(Console.ReadLine(), out count) && count > 0)
                break;
            Console.WriteLine("Помилка! Введіть ціле додатнє число.");
        }

        Console.WriteLine("\nГенерую колекції... Це може зайняти кілька секунд.");
        TestCollections tests = new TestCollections(count);
        tests.MeasureSearchTimes();
    }
}