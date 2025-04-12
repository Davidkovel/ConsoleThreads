namespace ConsoleThreads;

public interface IGameUI
{
    void ShowWelcomeMessage();
    void ShowGetReadyMessage();
    void ShowActionSignal();
    void ShowReactionTime(double milliseconds);
    void ShowTooEarlyMessage();
    void ShowTooSlowMessage();
    void ShowExitMessage();
    ConsoleKey WaitForUserInput();
}

public class ReactionTester
{
    private DateTime _signalTime;
    private bool _isRunning;
    private readonly Random _random = new Random();
    private readonly IGameUI _gameUI;

    public ReactionTester(IGameUI gameUI)
    {
        _gameUI = gameUI;
    }

    public void StartGame()
    {
        _isRunning = true;
        _gameUI.ShowWelcomeMessage();

        var signalThread = new Thread(ShowSignal) { IsBackground = true };
        signalThread.Start();

        while (_isRunning)
        {
            var key = _gameUI.WaitForUserInput();

            if (key == ConsoleKey.Spacebar)
            {
                ProcessUserReaction();
            }
            else if (key == ConsoleKey.Escape)
            {
                _isRunning = false;
                _gameUI.ShowExitMessage();
            }
        }
    }

    private void ProcessUserReaction()
    {
        TimeSpan reactionTime = DateTime.Now - _signalTime;

        if (reactionTime.TotalMilliseconds > 0)
        {
            _gameUI.ShowReactionTime(reactionTime.TotalMilliseconds);
        }
        else
        {
            _gameUI.ShowTooEarlyMessage();
        }
    }

    private void ShowSignal()
    {
        while (_isRunning)
        {
            int delay = _random.Next(1000, 7000);
            Thread.Sleep(delay);

            if (!_isRunning) break;

            _gameUI.ShowGetReadyMessage();
            Thread.Sleep(800);

            _gameUI.ShowActionSignal();
            _signalTime = DateTime.Now;

            Thread.Sleep(3000);

            if (!_isRunning) break;

            _gameUI.ShowTooSlowMessage();
        }
    }
}

public class ConsoleGameUIProvider : IGameUI
{
    public void ShowWelcomeMessage()
    {
        Console.WriteLine("Game started");
        Console.ReadKey(true);
    }

    public void ShowGetReadyMessage()
    {
        Console.Clear();
        Console.WriteLine("Prepare");
    }

    public void ShowActionSignal()
    {
        Console.Clear();
        Console.WriteLine("Click spacebar!");
    }

    public void ShowReactionTime(double milliseconds)
    {
        Console.WriteLine($"Your time reaction: {milliseconds} ms");
    }

    public void ShowTooEarlyMessage()
    {
        Console.WriteLine("Your time is too early");
    }

    public void ShowTooSlowMessage()
    {
        Console.Clear();
        Console.WriteLine("Time is too slow");
    }

    public void ShowExitMessage()
    {
        Console.WriteLine("Game ended");
    }

    public ConsoleKey WaitForUserInput()
    {
        return Console.ReadKey(true).Key;
    }
}

class Program
{
    static void Main(string[] args)
    {
        IGameUI gameUI = new ConsoleGameUIProvider();
        var game = new ReactionTester(gameUI);

        game.StartGame();
    }
}