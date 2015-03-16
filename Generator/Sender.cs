using System;
using System.Threading;
using Messages;
using NServiceBus;

namespace Generator
{
    public class Sender
    {
        private readonly MessageGenerator messageGenerator;
        private readonly Thread thread;
        private readonly CancellationTokenSource tokenSource;
        private readonly int sleepTime;

        public Sender(ISendOnlyBus bus, MessageGenerator messageGenerator, Action done, int sleepTime)
        {
            this.messageGenerator = messageGenerator;
            tokenSource = new CancellationTokenSource();
            thread = new Thread(() => SendMessages(bus, done));
            thread.Start();
            this.sleepTime = sleepTime;
        }

        public void Stop()
        {
            tokenSource.Cancel();
            thread.Join();
        }

        private void SendMessages(ISendOnlyBus bus, Action done)
        {
            while (!tokenSource.IsCancellationRequested)
            {
                bus.Send(messageGenerator.Generate());
                Thread.Sleep(sleepTime);
                done();
            }
        }
    }
}