using System;
using System.Collections.Generic;
using System.Globalization;

public class Scheduler
{
    public static List<string> FindFreeIntervals(List<string> startTimes, List<int> durations, string beginWorkingTime, string endWorkingTime, int consultationTime)
    {
        int startOfDay = TimeToMinutes(beginWorkingTime);
        int endOfDay = TimeToMinutes(endWorkingTime);

        // Создаем список занятых промежутков времени в формате [start, end]
        List<(int Start, int End)> busyIntervals = new List<(int, int)>();
        for (int i = 0; i < startTimes.Count; i++)
        {
            int start = TimeToMinutes(startTimes[i]);
            int end = start + durations[i];
            busyIntervals.Add((start, end));
        }

        // Сортируем занятые интервалы и объединяем пересекающиеся
        busyIntervals.Sort((a, b) => a.Start.CompareTo(b.Start));
        List<(int Start, int End)> mergedBusyIntervals = new List<(int, int)>();

        foreach (var interval in busyIntervals)
        {
            if (mergedBusyIntervals.Count == 0 || mergedBusyIntervals[^1].End < interval.Start)
            {
                mergedBusyIntervals.Add(interval);
            }
            else
            {
                mergedBusyIntervals[^1] = (mergedBusyIntervals[^1].Start, Math.Max(mergedBusyIntervals[^1].End, interval.End));
            }
        }

        // Находим свободные промежутки времени и делим их на интервалы консультации
        List<string> freeIntervals = new List<string>();
        int currentStart = startOfDay;

        foreach (var interval in mergedBusyIntervals)
        {
            if (interval.Start > currentStart)
            {
                AddConsultationIntervals(freeIntervals, currentStart, interval.Start, consultationTime);
            }
            currentStart = Math.Max(currentStart, interval.End);
        }

        // Проверка на оставшийся промежуток после последнего занятого времени до конца дня
        if (endOfDay > currentStart)
        {
            AddConsultationIntervals(freeIntervals, currentStart, endOfDay, consultationTime);
        }

        return freeIntervals;
    }

    // Метод для добавления интервалов консультации в свободное время
    private static void AddConsultationIntervals(List<string> freeIntervals, int start, int end, int consultationTime)
    {
        while (start + consultationTime <= end)
        {
            freeIntervals.Add($"{MinutesToTime(start)}-{MinutesToTime(start + consultationTime)}");
            start += consultationTime;
        }
    }

    // Метод для преобразования времени (строки формата HH:mm) в минуты
    private static int TimeToMinutes(string time)
    {
        var parts = time.Split(':');
        int hours = int.Parse(parts[0]);
        int minutes = int.Parse(parts[1]);
        return hours * 60 + minutes;
    }

    // Метод для преобразования минут обратно в строку формата HH:mm
    private static string MinutesToTime(int minutes)
    {
        int hours = minutes / 60;
        int mins = minutes % 60;
        return $"{hours:D2}:{mins:D2}";
    }
}

class Program
{
    static void Main()
    {
        List<string> startTimes = new List<string>();
        List<int> durations = new List<int>();

        Console.WriteLine("Введите время начала приёма и его длительность (в формате HH:mm mm), введите 'end' для завершения ввода:");

        while (true)
        {
            Console.Write("Ввод: ");
            string input = Console.ReadLine();
            if (input.ToLower() == "end") break;

            var parts = input.Split(' ');
            if (parts.Length == 2 && ValidateTime(parts[0]) && int.TryParse(parts[1], out int duration))
            {
                startTimes.Add(parts[0]);
                durations.Add(duration);
            }
            else
            {
                Console.WriteLine("Неверный формат. Попробуйте снова (формат: HH:mm mm).");
            }
        }

        string beginWorkingTime, endWorkingTime;
        while (true)
        {
            Console.Write("Введите рабочий день в формате HH:mm-HH:mm: ");
            string workingHours = Console.ReadLine();
            var times = workingHours.Split('-');

            if (times.Length == 2 && ValidateTime(times[0]) && ValidateTime(times[1]))
            {
                beginWorkingTime = times[0];
                endWorkingTime = times[1];
                break;
            }
            else
            {
                Console.WriteLine("Неверный формат. Попробуйте снова.");
            }
        }

        int consultationTime;
        while (true)
        {
            Console.Write("Введите длительность консультации (в минутах): ");
            if (int.TryParse(Console.ReadLine(), out consultationTime) && consultationTime > 0)
            {
                break;
            }
            else
            {
                Console.WriteLine("Неверный ввод. Введите положительное число.");
            }
        }

        List<string> freeIntervals = Scheduler.FindFreeIntervals(startTimes, durations, beginWorkingTime, endWorkingTime, consultationTime);

        Console.WriteLine("Свободные интервалы для консультации:");
        foreach (var interval in freeIntervals)
        {
            Console.WriteLine(interval);
        }
    }

    // Метод для валидации формата времени, который учитывает как HH:mm, так и H:mm
    private static bool ValidateTime(string time)
    {
        return DateTime.TryParseExact(time, new[] { "H:mm", "HH:mm" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }
}
