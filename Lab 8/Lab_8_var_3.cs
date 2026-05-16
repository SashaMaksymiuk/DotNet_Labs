using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Diagnostics;

class Program
{
    static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("==================================================");
        Console.WriteLine("       СИСТЕМА БАГАТОПОТОКОВОГО ПОШУКУ             ");
        Console.WriteLine("==================================================\n");
        Console.ResetColor();

        string directoryPath = "TestFiles";
        string searchTerm = "ЕКОЛОГІЯ"; 

        EnsureTestFilesExist(directoryPath, searchTerm);

        string[] files = Directory.GetFiles(directoryPath, "*.txt");
        if (files.Length == 0)
        {
            Console.WriteLine("У каталозі немає текстових файлів для обробки.");
            return;
        }

        Console.WriteLine($"Знайдено файлів для обробки: {files.Length}");
        Console.WriteLine($"Шукаємо рядок: '{searchTerm}'\n");
        Console.WriteLine("Починаємо сканування...\n");

        Stopwatch totalTime = Stopwatch.StartNew();

        var executionOptions = new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded 
        };

        var processFileBlock = new ActionBlock<string>(filePath =>
        {
            ProcessSingleFile(filePath, searchTerm);
        }, executionOptions);

        foreach (var file in files)
        {
            processFileBlock.Post(file);
        }

        processFileBlock.Complete();
        await processFileBlock.Completion;

        totalTime.Stop();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n[ФІНІШ] Усі файли успішно оброблено! Загальний час: {totalTime.ElapsedMilliseconds / 1000.0} с.");
        Console.ResetColor();
    }

    static void ProcessSingleFile(string filePath, string searchTerm)
    {
        string fileName = Path.GetFileName(filePath);
        
        int totalLines = File.ReadLines(filePath).Count();
        if (totalLines == 0) return;

        int currentLine = 0;
        int matches = 0;
        
        Stopwatch updateTimer = Stopwatch.StartNew();

        foreach (string line in File.ReadLines(filePath))
        {
            currentLine++;

            if (line.Contains(searchTerm))
            {
                matches++;
            }

            if (updateTimer.ElapsedMilliseconds >= 500)
            {
                double percent = (double)currentLine / totalLines * 100;
                Console.WriteLine($"{fileName} - оброблено {percent:F1}%");
                updateTimer.Restart();
            }
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[ГОТОВО] {fileName}: знайдено {matches} входжень.");
        Console.ResetColor();
    }

    static void EnsureTestFilesExist(string dirPath, string searchTerm)
    {
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        if (Directory.GetFiles(dirPath, "*.txt").Length == 0)
        {
            Console.WriteLine("Генерую великі тестові файли... Зачекайте пару секунд.\n");
            Random rnd = new Random();

            for (int i = 1; i <= 3; i++)
            {
                string filePath = Path.Combine(dirPath, $"BigDataFile_{i}.txt");
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    int totalLines = 300000;
                    for (int line = 1; line <= totalLines; line++)
                    {
                        if (rnd.Next(1000) == 1) 
                        {
                            writer.WriteLine($"Цей рядок містить важливе слово {searchTerm} для перевірки.");
                        }
                        else
                        {
                            writer.WriteLine($"Це звичайний тестовий рядок номер {line}. Нічого цікавого.");
                        }
                    }
                }
            }
            Console.WriteLine("Файли успішно згенеровано!\n");
        }
    }
}