using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWatcher.Messages
{
    public class StartWatching
    {
        public string FileName { get; set; }
        public IActorRef Subscriber { get; set; }
    }
}
