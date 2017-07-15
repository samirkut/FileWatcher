using Akka.Actor;
using FileWatcher.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileWatcher.Actors
{
    public class LocalFileWatcherActor : ReceiveActor
    {
        private CancellationTokenSource _cts;

        public LocalFileWatcherActor()
        {          
            Become(Free);
        }

        public static Props Props()
        {
            return Akka.Actor.Props.Create(() => new LocalFileWatcherActor());
        }

        private void Free()
        {
            Receive<StartWatching>(x =>
            {
                Become(Watching);                
                Start(x.FileName, x.Subscriber);
            });
        }

        private void Watching()
        {
            Receive<StopWatching>(_ =>
            {
                Stop();               
                Become(Free);               
            });
        }

        private void Start(string fileName, IActorRef subscriber)
        {
            _cts = new CancellationTokenSource();

            Task.Factory.StartNew((_) => RunWatcher(fileName, subscriber, _cts.Token), _cts.Token, TaskCreationOptions.LongRunning);
        }

        private static void RunWatcher(string fileName, IActorRef subscriber, CancellationToken cToken)
        {
            long oldFileSize = 0;
            using (var fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                oldFileSize = fs.Length;

            var fsw = new FileSystemWatcher(Path.GetDirectoryName(fileName));
            while (!cToken.IsCancellationRequested)
            {
                var res = fsw.WaitForChanged(WatcherChangeTypes.Changed | WatcherChangeTypes.Created, 1000);
                if (res.TimedOut || cToken.IsCancellationRequested || res.Name != Path.GetFileName(fileName)) continue;
                
                var changeEvent = new FileChanged
                {
                    FileName = fileName
                };
                
                using(var st = new StreamReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), true))
                {
                    var newFileSize = st.BaseStream.Length;
                    if (newFileSize > oldFileSize)
                        st.BaseStream.Seek(oldFileSize, SeekOrigin.Begin);
                    else
                        changeEvent.Replace = true;

                    var changes = new List<string>();
                    while (!st.EndOfStream)
                        changes.Add(st.ReadLine());

                    changeEvent.Changes = changes.ToArray();
                    subscriber.Tell(changeEvent);

                    oldFileSize = newFileSize;
                }
            }
        }

        private void Stop()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts = null;
            }
        }

        protected override void PostStop()
        {
            base.PostStop();
            Stop();
        }
    }
}
