namespace ConsoleThreads;

class Bank
{
    private string _name;
    private int _money = 0;
    private int _percent = 0;
    private readonly string _logFilePath = "bank_logs.txt";
    private readonly object _lock = new object();

    public Bank(string name, int money, int percent)
    {
        _name = name;
        _money = money;
        _percent = percent;
        
        File.WriteAllText(_logFilePath, $"Bank Logs - Created at {DateTime.Now}\n");
    }

    public int Money
    {
        get => _money;
        set
        {
            if (_money != value)
            {
                _money = value;
                _StartThreads();
            }
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                _StartThreads();
            }
        }
    }

    public int Percent
    {
        get => _percent;
        set
        {
            if (_percent != value)
            {
                _percent = value;
                _StartThreads();
            }
        }
    }

    private void _StartThreads()
    {
        Thread thread = new Thread(_LogsWrite);
        thread.Start();
    }

    private void _LogsWrite()
    {
        try
        {
            lock (_lock) // mutex Thread racing
            {
                string logEntry = $"{DateTime.Now}: Money={_money}, Name={_name}, Percent={_percent}%";
                File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка запису у файл: {ex.Message}");
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        var myBank = new Bank("PrivatBank", 333, 5);

        myBank.Name = "S";
        myBank.Money = 15000;
        myBank.Percent = 7;

        Console.WriteLine(myBank);
    }
}