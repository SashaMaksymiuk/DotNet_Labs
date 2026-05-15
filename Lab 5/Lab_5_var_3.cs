using System;
using System.Collections.Generic;

// ================== ENUM ==================
enum TimeFrame { Year, TwoYears, Long }

// ================== INTERFACES ==================
interface INameAndCopy
{
    string Name { get; set; }
    object DeepCopy();
}

// ================== PERSON ==================
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

    public override int GetHashCode() => HashCode.Combine(FirstName, LastName, BirthDate);
    public virtual object DeepCopy() => new Person(FirstName, LastName, BirthDate);
    public override string ToString() => $"{FirstName} {LastName}";
    public virtual string ToShortString() => $"{FirstName} {LastName}";
}

// ================== PAPER ==================
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
    public override string ToString() => $"'{Title}' ({Author.ToShortString()})";
}

// ================== TEAM ==================
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
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Номер має бути > 0");
            _registrationNumber = value;
        }
    }

    public string Name
    {
        get => _organization;
        set => _organization = value;
    }

    public virtual object DeepCopy() => new Team(Organization, RegistrationNumber);
    public int CompareTo(object? obj)
    {
        if (obj is Team other) return RegistrationNumber.CompareTo(other.RegistrationNumber);
        throw new ArgumentException("Не Team");
    }
    public override string ToString() => $"Орг: {Organization}, Реєстр.№: {RegistrationNumber}";
}

// ================== RESEARCH TEAM ==================
class ResearchTeam : Team
{
    public string Topic { get; init; }
    public TimeFrame Duration { get; init; }
    public List<Person> Participants { get; init; }
    public List<Paper> Publications { get; init; }

    public ResearchTeam(string topic, string organization, int registrationNumber, TimeFrame duration)
        : base(organization, registrationNumber)
    {
        Topic = topic;
        Duration = duration;
        Participants = new List<Person>();
        Publications = new List<Paper>();
    }

    public override string ToString() => $"Тема: '{Topic}' ({Organization})";
}

// ================== ПОДІЇ ТА ДЕЛЕГАТИ (ЛАБА 5) ==================

// 1. Делегат
public delegate void TeamListHandler(object source, TeamListHandlerEventArgs args);

// 2. Клас аргументів події
public class TeamListHandlerEventArgs : EventArgs
{
    public string CollectionName { get; set; }
    public string ChangeInfo { get; set; }
    public int ElementIndex { get; set; }

    public TeamListHandlerEventArgs(string collectionName, string changeInfo, int elementIndex)
    {
        CollectionName = collectionName;
        ChangeInfo = changeInfo;
        ElementIndex = elementIndex;
    }

    public override string ToString()
    {
        return $"Колекція: '{CollectionName}' | Дія: {ChangeInfo} | Індекс елемента: {ElementIndex}";
    }
}

// 3. Клас запису журналу
public class TeamsJournalEntry
{
    public string CollectionName { get; set; }
    public string ChangeInfo { get; set; }
    public int ElementIndex { get; set; }

    public TeamsJournalEntry(string collectionName, string changeInfo, int elementIndex)
    {
        CollectionName = collectionName;
        ChangeInfo = changeInfo;
        ElementIndex = elementIndex;
    }

    public override string ToString()
    {
        return $"[ЖУРНАЛ] {CollectionName} -> {ChangeInfo} (Індекс: {ElementIndex})";
    }
}

// 4. Журнал, який зберігає записи
public class TeamsJournal
{
    private List<TeamsJournalEntry> _entries = new List<TeamsJournalEntry>();

    public void CollectionChangedHandler(object source, TeamListHandlerEventArgs args)
    {
        _entries.Add(new TeamsJournalEntry(args.CollectionName, args.ChangeInfo, args.ElementIndex));
    }

    public override string ToString()
    {
        if (_entries.Count == 0) return "Журнал порожній.";
        string res = "";
        foreach (var entry in _entries) res += entry.ToString() + "\n";
        return res;
    }
}

// ================== RESEARCH TEAM COLLECTION ==================
class ResearchTeamCollection
{
    public string CollectionName { get; set; }
    private List<ResearchTeam> _teams = new List<ResearchTeam>();

    public event TeamListHandler? ResearchTeamAdded;
    public event TeamListHandler? ResearchTeamInserted;

    public ResearchTeamCollection(string name)
    {
        CollectionName = name;
    }

    // Індексатор
    public ResearchTeam this[int index]
    {
        get => _teams[index];
        set
        {
            _teams[index] = value;
            ResearchTeamInserted?.Invoke(this, new TeamListHandlerEventArgs(CollectionName, "Елемент замінено (через індексатор)", index));
        }
    }

    public void AddDefaults()
    {
        ResearchTeam rt1 = new ResearchTeam("Екологія лісу", "НАН", 10, TimeFrame.Year);
        ResearchTeam rt2 = new ResearchTeam("Очищення річок", "КНУ", 20, TimeFrame.TwoYears);
        
        _teams.Add(rt1);
        ResearchTeamAdded?.Invoke(this, new TeamListHandlerEventArgs(CollectionName, "Додано за замовчуванням", _teams.Count - 1));
        
        _teams.Add(rt2);
        ResearchTeamAdded?.Invoke(this, new TeamListHandlerEventArgs(CollectionName, "Додано за замовчуванням", _teams.Count - 1));
    }

    public void AddResearchTeams(params ResearchTeam[] teams)
    {
        foreach (var t in teams)
        {
            _teams.Add(t);
            ResearchTeamAdded?.Invoke(this, new TeamListHandlerEventArgs(CollectionName, "Додано в кінець", _teams.Count - 1));
        }
    }

    public void InsertAt(int j, ResearchTeam researchTeam)
    {
        if (j >= 0 && j < _teams.Count)
        {
            _teams.Insert(j, researchTeam);
            ResearchTeamInserted?.Invoke(this, new TeamListHandlerEventArgs(CollectionName, "Вставлено перед існуючим", j));
        }
        else
        {
            _teams.Add(researchTeam);
            ResearchTeamAdded?.Invoke(this, new TeamListHandlerEventArgs(CollectionName, "Додано в кінець (індекс поза межами)", _teams.Count - 1));
        }
    }

    public void RemoveAt(int j)
    {
        if (j >= 0 && j < _teams.Count)
        {
            _teams.RemoveAt(j);
            // Використовуємо подію Inserted для фіксації структурних змін (як вимагає логіка лаби)
            ResearchTeamInserted?.Invoke(this, new TeamListHandlerEventArgs(CollectionName, "Елемент ВИЛУЧЕНО", j));
        }
    }
}

// ================== MAIN ==================
class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // 1. Створюємо дві колекції
        ResearchTeamCollection col1 = new ResearchTeamCollection("ЕКО-Колекція №1");
        ResearchTeamCollection col2 = new ResearchTeamCollection("БІО-Колекція №2");

        // 2. Створюємо два журнали
        TeamsJournal journal1 = new TeamsJournal();
        TeamsJournal journal2 = new TeamsJournal();

        // 3. Підписуємо журнали на події
        // Журнал 1 слухає ТІЛЬКИ Колекцію 1
        col1.ResearchTeamAdded += journal1.CollectionChangedHandler;
        col1.ResearchTeamInserted += journal1.CollectionChangedHandler;

        // Журнал 2 слухає ОБИДВІ колекції
        col1.ResearchTeamAdded += journal2.CollectionChangedHandler;
        col1.ResearchTeamInserted += journal2.CollectionChangedHandler;
        col2.ResearchTeamAdded += journal2.CollectionChangedHandler;
        col2.ResearchTeamInserted += journal2.CollectionChangedHandler;

        // 4. Вносимо зміни в колекції
        Console.WriteLine("=== Вносимо зміни в колекції... ===\n");

        // Робота з Колекцією 1
        col1.AddDefaults();
        
        ResearchTeam newRt1 = new ResearchTeam("Тест 1", "Організація 1", 111, TimeFrame.Long);
        ResearchTeam newRt2 = new ResearchTeam("Тест 2", "Організація 2", 222, TimeFrame.Year);
        ResearchTeam newRt3 = new ResearchTeam("Тест 3", "Організація 3", 333, TimeFrame.TwoYears);

        col1.AddResearchTeams(newRt1);
        
        col1.InsertAt(1, newRt2); 
        
        col1.InsertAt(999, newRt3); 

        col1.RemoveAt(0);

        col1[0] = new ResearchTeam("ЗАМІНА", "Новий Орг", 999, TimeFrame.Long);

        // Робота з Колекцією 2 (має потрапити ТІЛЬКИ в Журнал 2)
        col2.AddDefaults();

        Console.WriteLine("========== ЖУРНАЛ 1 (Слухає тільки Колекцію 1) ==========");
        Console.WriteLine(journal1.ToString());

        Console.WriteLine("========== ЖУРНАЛ 2 (Слухає обидві колекції) ==========");
        Console.WriteLine(journal2.ToString());
    }
}