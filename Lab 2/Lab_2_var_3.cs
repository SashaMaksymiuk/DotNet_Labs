using System;
using System.Collections;

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
    private string _firstName = "";
    private string _lastName = "";
    private DateTime _birthDate;

    public Person(string firstName, string lastName, DateTime birthDate)
    {
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
    }

    public Person() : this("Невідомо", "Невідомо", new DateTime(2000, 1, 1)) { }

    public string FirstName { get => _firstName; init => _firstName = value; }
    public string LastName { get => _lastName; init => _lastName = value; }
    public DateTime BirthDate { get => _birthDate; init => _birthDate = value; }

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType()) return false;
        Person p = (Person)obj;
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

// ================== TEAM ==================
class Team : INameAndCopy
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
        if (obj == null || GetType() != obj.GetType()) return false;
        Team t = (Team)obj;
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

    public override string ToString() => $"Організація: {Organization}, Номер: {RegistrationNumber}";
}

class ResearchTeam : Team, IEnumerable
{
    private string _topic;
    private TimeFrame _duration;
    private ArrayList _participants; 
    private ArrayList _publications;

    public ResearchTeam(string topic, string organization, int registrationNumber, TimeFrame duration) 
        : base(organization, registrationNumber)
    {
        _topic = topic;
        _duration = duration;
        _participants = new ArrayList();
        _publications = new ArrayList();
    }

    public ResearchTeam() : base()
    {
        _topic = "Невідома тема";
        _duration = TimeFrame.Year;
        _participants = new ArrayList();
        _publications = new ArrayList();
    }

    public ArrayList Participants { get => _participants; init => _participants = value; }
    public ArrayList Publications { get => _publications; init => _publications = value; }

    public Team TeamBase
    {
        get => new Team(Organization, RegistrationNumber);
        init
        {
            _organization = value.Organization;
            _registrationNumber = value.RegistrationNumber;
        }
    }

    public Paper? LatestPaper
    {
        get
        {
            if (_publications.Count == 0) return null;
            Paper latest = (Paper)_publications[0]!;
            foreach (Paper paper in _publications)
            {
                if (paper.Date > latest.Date) latest = paper;
            }
            return latest;
        }
    }

    public void AddPapers(params Paper[] newPapers) => _publications.AddRange(newPapers);
    public void AddPersons(params Person[] newPersons) => _participants.AddRange(newPersons); 

    public override string ToString()
    {
        string res = $"Тема: {_topic}, {base.ToString()}, Тривалість: {_duration}\nУчасники:\n";
        foreach (Person p in _participants) res += $"  - {p}\n";
        res += "Публікації:\n";
        foreach (Paper p in _publications) res += $"  - {p}\n";
        return res;
    }

    public string ToShortString() => $"Тема: {_topic}, {base.ToString()}, Тривалість: {_duration}";

    public override object DeepCopy()
    {
        ResearchTeam copy = new ResearchTeam(_topic, Organization, RegistrationNumber, _duration);
        foreach (Person p in _participants) copy._participants.Add(p.DeepCopy());
        foreach (Paper p in _publications) copy._publications.Add(p.DeepCopy());
        return copy;
    }

    private int CountPublications(Person person)
    {
        int count = 0;
        foreach (Paper paper in _publications)
        {
            if (paper.Author.Equals(person)) count++;
        }
        return count;
    }

    public IEnumerable PersonsWithoutPublications()
    {
        foreach (Person p in _participants)
        {
            if (CountPublications(p) == 0) yield return p;
        }
    }

    public IEnumerable RecentPublications(int n)
    {
        DateTime cutoffDate = DateTime.Now.AddYears(-n);
        foreach (Paper p in _publications)
        {
            if (p.Date >= cutoffDate) yield return p;
        }
    }

    public IEnumerable PersonsWithMultiplePublications()
    {
        foreach (Person p in _participants)
        {
            if (CountPublications(p) > 1) yield return p;
        }
    }

    public IEnumerable PublicationsLastYear()
    {
        DateTime cutoffDate = DateTime.Now.AddYears(-1);
        foreach (Paper p in _publications)
        {
            if (p.Date >= cutoffDate) yield return p;
        }
    }

    // Реалізація IEnumerable
    public IEnumerator GetEnumerator() => new ResearchTeamEnumerator(_participants, _publications);
}

class ResearchTeamEnumerator : IEnumerator
{
    private ArrayList _participants;
    private ArrayList _publications;
    private int _position = -1;
    private ArrayList _filteredParticipants;

    public ResearchTeamEnumerator(ArrayList participants, ArrayList publications)
    {
        _participants = participants;
        _publications = publications;
        _filteredParticipants = new ArrayList();

        foreach (Person person in _participants)
        {
            bool hasPublication = false;
            foreach (Paper paper in _publications)
            {
                if (paper.Author.Equals(person))
                {
                    hasPublication = true;
                    break;
                }
            }
            if (hasPublication) _filteredParticipants.Add(person);
        }
    }

    public bool MoveNext()
    {
        _position++;
        return _position < _filteredParticipants.Count;
    }

    public void Reset() => _position = -1;

    public object Current
    {
        get
        {
            if (_position < 0 || _position >= _filteredParticipants.Count)
                throw new InvalidOperationException();
            return _filteredParticipants[_position]!;
        }
    }
}


class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("=== 1. Перевірка об'єктів Team ===");
        Team t1 = new Team("Інститут Екології", 12345);
        Team t2 = new Team("Інститут Екології", 12345);

        Console.WriteLine($"Посилання різні? {ReferenceEquals(t1, t2) == false}"); 
        Console.WriteLine($"Об'єкти рівні (==)? {t1 == t2}"); 
        Console.WriteLine($"Хеш-код t1: {t1.GetHashCode()}");
        Console.WriteLine($"Хеш-код t2: {t2.GetHashCode()}");
        Console.WriteLine();

        Console.WriteLine("=== 2. Перевірка виключень (try-catch) ===");
        try
        {
            Team errorTeam = new Team
            {
                Organization = "Невідома Лабораторія",
                RegistrationNumber = -5 
            };
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Console.WriteLine($"Спіймано помилку: {ex.Message}");
        }
        Console.WriteLine();

        Console.WriteLine("=== 3. Створення ResearchTeam ===");
        ResearchTeam rt = new ResearchTeam("Моніторинг лісових екосистем", "Інститут Екології", 987, TimeFrame.Long);

        Person p1 = new Person("Віктор", "Мельник", new DateTime(1975, 3, 10));
        Person p2 = new Person("Софія", "Ткаченко", new DateTime(1992, 7, 18));
        Person p3 = new Person("Тарас", "Григоренко", new DateTime(2003, 11, 2));

        rt.AddPersons(p1, p2, p3);

        rt.AddPapers(
            new Paper("Вплив клімату на ріст лісів", p1, new DateTime(2022, 5, 10)),
            new Paper("Аналіз якості повітря", p2, new DateTime(2018, 8, 20)),
            new Paper("Сучасна екологія Карпат", p2, DateTime.Now)
        );

        Console.WriteLine(rt.ToString());
        Console.WriteLine();

        Console.WriteLine("=== 4. Властивість TeamBase ===");
        Console.WriteLine(rt.TeamBase.ToString());
        Console.WriteLine();

        Console.WriteLine("=== 5. Глибоке копіювання (DeepCopy) ===");
        ResearchTeam rtCopy = (ResearchTeam)rt.DeepCopy();

        // Змінюємо оригінал через властивість Name
        rt.Name = "ЗМІНЕНИЙ ІНСТИТУТ"; 
        ((Paper)rt.Publications[0]!).Title = "ЗМІНЕНА НАЗВА СТАТТІ";

        Console.WriteLine("Оригінал після змін:");
        Console.WriteLine(rt.ToString());
        Console.WriteLine("Копія (має залишитися без змін):");
        Console.WriteLine(rtCopy.ToString());
        Console.WriteLine();

        // Відновлюємо оригінал
        rt = rtCopy; 

        Console.WriteLine("=== 6. Тестування ітераторів ===");
        Console.WriteLine("Учасники без публікацій:");
        foreach (Person p in rt.PersonsWithoutPublications()) Console.WriteLine($"  - {p.ToShortString()}");

        Console.WriteLine("\nПублікації за останні 2 роки:");
        foreach (Paper p in rt.RecentPublications(2)) Console.WriteLine($"  - {p.Title}");

        Console.WriteLine("\n=== 7. Додаткове завдання ===");
        Console.WriteLine("Учасники, у яких Є публікації (через IEnumerable):");
        foreach (Person p in rt) Console.WriteLine($"  - {p.ToShortString()}");

        Console.WriteLine("\nУчасники, у яких БІЛЬШЕ однієї публікації:");
        foreach (Person p in rt.PersonsWithMultiplePublications()) Console.WriteLine($"  - {p.ToShortString()}");

        Console.WriteLine("\nПублікації за останній 1 рік:");
        foreach (Paper p in rt.PublicationsLastYear()) Console.WriteLine($"  - {p.Title}");
    }
}