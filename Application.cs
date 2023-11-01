namespace Application;

public static class Application
{
    public static async Task Main()
    {
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        await GameClient.RunAsync(cts.Token);
    }
}
