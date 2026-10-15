using System;
using System.Threading;
using System.Threading.Tasks;
using SysPulse.NET;

namespace SysPulseDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var engine = new SysPulseEngine();
            using var cts = new CancellationTokenSource();

            // Background task to listen for ESC key to exit safely
            var inputTask = Task.Run(() =>
            {
                while (true)
                {
                    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                    {
                        cts.Cancel();
                        break;
                    }
                    Thread.Sleep(100);
                }
            });

            // Run monitor loop
            await engine.RunMonitorAsync(cts.Token);

            Console.WriteLine("SysPulse session terminated. System resources normal.");
        }
    }
}