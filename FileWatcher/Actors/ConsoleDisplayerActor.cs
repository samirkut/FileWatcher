using Akka.Actor;
using FileWatcher.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWatcher.Actors
{
    public class ConsoleDisplayerActor : ReceiveActor
    {
        public ConsoleDisplayerActor()
        {
            Receive<FileChanged>(_=>ShowDisplay(_));
        }

        private void ShowDisplay(FileChanged changeEvt)
        {
            foreach(var change in changeEvt.Changes)
            {
                Console.WriteLine($"{change}");
            }
        }
    }
}
