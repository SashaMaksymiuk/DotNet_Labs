using System;
using System.Collections.Generic;
using System.Reflection;


public interface IHasName
{
    string Name { get; } 
}

public class SameGenderException : Exception
{
    public SameGenderException(string message) : base(message) { }
}


[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CoupleAttribute : Attribute
{
    public string Pair { get; set; }
    public double Probability { get; set; }
    public string ChildType { get; set; }

    public CoupleAttribute(string pair, double probability, string childType)
    {
        Pair = pair;
        Probability = probability;
        ChildType = childType;
    }
}

public abstract class Human : IHasName
{
    public string Name { get; protected set; }
    public bool IsMale { get; protected set; }
    public string Patronymic { get; set; } 

    public Human(string name, bool isMale)
    {
        Name = name;
        IsMale = isMale;
        Patronymic = "";
    }
}

public class Book : IHasName
{
    public string Name { get; private set; }
    public Book(string name) { Name = name; }
}

// --- ЧОЛОВІКИ ---
[Couple("Girl", 0.7, "Girl")]
[Couple("PrettyGirl", 1.0, "PrettyGirl")]
[Couple("SmartGirl", 0.5, "Girl")]
public class Student : Human
{
    public Student(string name) : base(name, true) { }
    public string ThinkOfName() => "Оленка";
}

[Couple("Girl", 0.7, "SmartGirl")]
[Couple("PrettyGirl", 1.0, "PrettyGirl")]
[Couple("SmartGirl", 0.8, "Book")]
public class Botan : Human
{
    public Botan(string name) : base(name, true) { }
    public string InvalidMethod(int x) => "Помилка"; 
    public string InventName() => "Книга Життя";
}

[Couple("Student", 0.7, "Girl")]
[Couple("Botan", 0.3, "SmartGirl")]
public class Girl : Human
{
    public Girl(string name) : base(name, false) { }
    public string SuggestName() => "Марія";
}

[Couple("Student", 0.4, "PrettyGirl")]
[Couple("Botan", 0.1, "PrettyGirl")]
public sealed class PrettyGirl : Human 
{
    public PrettyGirl(string name) : base(name, false) { }
    public string WhisperName() => "Анастасія";
}

[Couple("Student", 0.2, "Girl")]
[Couple("Botan", 0.5, "Book")]
public sealed class SmartGirl : Human 
{
    public SmartGirl(string name) : base(name, false) { }
    public string CalculateName() => "Енциклопедія";
}


class Program
{
    static Random _random = new Random();

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Консоль не працює по неділях. Йдіть відпочивати! :)");
            Console.ResetColor();
            return;
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("==================================================");
        Console.WriteLine("                СИМУЛЯТОР ЗУСТРІЧЕЙ               ");
        Console.WriteLine("==================================================");
        Console.WriteLine("Натисніть [ENTER] для генерації нової пари.");
        Console.WriteLine("Натисніть [Q] або [F10] для виходу з програми.\n");
        Console.ResetColor();

        string[] maleNames = { "Іван", "Максим", "Олексій", "Денис", "Андрій" };
        string[] femaleNames = { "Анна", "Софія", "Юлія", "Катерина", "Дар'я" };

        while (true)
        {
            ConsoleKeyInfo key = Console.ReadKey(true);
            
            if (key.Key == ConsoleKey.Q || key.Key == ConsoleKey.F10)
            {
                Console.WriteLine("Вихід з програми. До побачення!");
                break;
            }

            if (key.Key == ConsoleKey.Enter)
            {
                Console.Clear();
                Console.WriteLine("... Зустрілися двоє людей ...\n");

                Human h1 = GenerateRandomHuman(maleNames, femaleNames);
                Human h2 = GenerateRandomHuman(maleNames, femaleNames);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Перший: {h1.Name} ({h1.GetType().Name})");
                Console.WriteLine($"Другий: {h2.Name} ({h2.GetType().Name})\n");
                Console.ResetColor();

                try
                {
                    IHasName child = Couple(h1, h2);
                    
                    if (child != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\n[УСПІХ!] Вони сподобалися одне одному!");
                        Console.Write("З'явився новий об'єкт -> ");
                        PrintType(child);
                        Console.Write("Ім'я об'єкта -> ");
                        PrintName(child);

                        if (child is Human humanChild && !string.IsNullOrEmpty(humanChild.Patronymic))
                        {
                            Console.WriteLine($"По батькові: {humanChild.Patronymic}");
                        }
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine("\n[ФІАСКО] На жаль, взаємної симпатії не виникло.");
                        Console.ResetColor();
                    }
                }
                catch (SameGenderException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[ПОМИЛКА] {ex.Message}");
                    Console.ResetColor();
                }

                Console.WriteLine("\n--------------------------------------------------");
                Console.WriteLine("Натисніть [ENTER] для наступної пари, або [Q] / [F10] для виходу.");
            }
        }
    }

    public static IHasName Couple(Human h1, Human h2)
    {
        if (h1.IsMale == h2.IsMale)
        {
            throw new SameGenderException($"Зустрілися дві людини однієї статі: {h1.Name} та {h2.Name}. Нащадків не буде!");
        }

        CoupleAttribute attr1 = GetCoupleAttribute(h1, h2.GetType().Name);
        CoupleAttribute attr2 = GetCoupleAttribute(h2, h1.GetType().Name);

        if (attr1 == null || attr2 == null) return null;

        bool likes1 = CheckSympathy(attr1.Probability);
        bool likes2 = CheckSympathy(attr2.Probability);

        Console.WriteLine($"{h1.Name} каже: \"{(likes1 ? "Вона мені подобається!" : "Не мій тип.")}\"");
        Console.WriteLine($"{h2.Name} каже: \"{(likes2 ? "Він мені подобається!" : "Не мій тип.")}\"");

        if (likes1 && likes2)
        {
            string newName = GetNameFromMethodViaReflection(h2);

            Type childType = Type.GetType(attr1.ChildType);
            if (childType == null) return null;

            IHasName child = (IHasName)Activator.CreateInstance(childType, new object[] { newName });

            PropertyInfo patronymicProp = childType.GetProperty("Patronymic");
            if (patronymicProp != null && child is Human cHuman)
            {
                string fatherName = h1.IsMale ? h1.Name : h2.Name;
                string suffix = cHuman.IsMale ? "ович" : "овна";
                patronymicProp.SetValue(child, fatherName + suffix);
            }

            return child;
        }

        return null;
    }

    static CoupleAttribute GetCoupleAttribute(Human human, string targetTypeName)
    {
        object[] attrs = human.GetType().GetCustomAttributes(typeof(CoupleAttribute), false);
        IEnumerator<object> enumerator = ((IEnumerable<object>)attrs).GetEnumerator();
        
        while (enumerator.MoveNext())
        {
            if (enumerator.Current is CoupleAttribute ca && ca.Pair == targetTypeName)
            {
                return ca;
            }
        }
        return null;
    }

    static bool CheckSympathy(double probability)
    {
        return _random.NextDouble() <= probability;
    }

    static void PrintType(object obj)
    {
        Console.WriteLine(obj.GetType().Name);
    }

    static void PrintName(IHasName obj)
    {
        Console.WriteLine(obj.Name);
    }

    static string GetNameFromMethodViaReflection(Human h2)
    {
        MethodInfo[] methods = h2.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        
        foreach (MethodInfo method in methods)
        {
            if (method.ReturnType == typeof(string))
            {
                try
                {
                    return (string)method.Invoke(h2, null);
                }
                catch
                {
                    continue; 
                }
            }
        }
        return "Невідомий";
    }

    static Human GenerateRandomHuman(string[] maleNames, string[] femaleNames)
    {
        int typeIndex = _random.Next(5);
        string mName = maleNames[_random.Next(maleNames.Length)];
        string fName = femaleNames[_random.Next(femaleNames.Length)];

        return typeIndex switch
        {
            0 => new Student(mName),
            1 => new Botan(mName),
            2 => new Girl(fName),
            3 => new PrettyGirl(fName),
            _ => new SmartGirl(fName)
        };
    }
}