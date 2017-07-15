using Akka.Actor;
using FileWatcher.Actors;
using FileWatcher.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileWatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 1)
            {
                Console.WriteLine("No args passed");
                return;
            }

            var cts = new CancellationTokenSource();

            //start a log generator so something is moving in the file
            Task.Run(new Action(() => GenerateLogFile(args[0], cts.Token)), cts.Token);

            var system = ActorSystem.Create("tail");
            var watcher = system.ActorOf(LocalFileWatcherActor.Props());
            var displayer = system.ActorOf<ConsoleDisplayerActor>();
            watcher.Tell(new StartWatching { FileName=args[0], Subscriber = displayer });

            Console.ReadLine();

            watcher.Tell(new StopWatching());
            cts.Cancel();

            Console.WriteLine("Shutdown completed...");
            Console.ReadLine();
        }

        static async Task GenerateLogFile(string fileName, CancellationToken cToken)
        {
            while (!cToken.IsCancellationRequested)
            {
                File.AppendAllText(fileName, $"Ticked at {DateTime.Now.ToLongTimeString()}\r\n");
                await Task.Delay(TimeSpan.FromSeconds(1), cToken);
            }
        }
    }
}
