using System;
using System.Collections.Generic;
using System.Linq;
using NServiceBus;
using Messages;

namespace Generator
{
    class Program
    {

        private const int NumberOfSagas = 100;

        private const int Threshold = 40;
        private const int TargetThtoughput = 400;
        private const int SleepTime = 50;

        static void Main(string[] args)
        {
            var busConfig = new BusConfiguration();
            busConfig.UsePersistence<InMemoryPersistence>();
            busConfig.UseTransport<MsmqTransport>();
            busConfig.UseTransport<SqlServerTransport>();
            busConfig.EnableInstallers();

            var messageGenerator = new MessageGenerator(NumberOfSagas);
            var senders = new List<Sender>();
            IBus bus = null;
            IMonitor monitor = null;
            var fiveSecondThroughputCalculator = new AverageThroughputCalculator(
                TimeSpan.FromSeconds(5), 5, avg => Console.WriteLine(string.Format("Average throughput in last 5 seconds: {0,10:0.000}", avg)));

            var tenSecondThroughputCalculator = new AverageThroughputCalculator(
                TimeSpan.FromSeconds(10), 5, avg =>
                {
                    Console.WriteLine("Average throughput in last 10 seconds: {0,10:0.000}", avg);
                    if (avg > TargetThtoughput + Threshold && senders.Count > 1)
                    {
                        Console.WriteLine("Stopping one sender");
                        var first = senders.First();
                        senders.Remove(first);
                        first.Stop();
                    }
                    else if (avg < TargetThtoughput - Threshold)
                    {
                        Console.WriteLine("Adding another sender");
                        senders.Add(new Sender(bus, messageGenerator, monitor.Done, SleepTime));
                    }
                });

            monitor = new CompositeMonitor(fiveSecondThroughputCalculator, tenSecondThroughputCalculator);

            using (bus = Bus.Create(busConfig).Start())
            {
                fiveSecondThroughputCalculator.Start();
                tenSecondThroughputCalculator.Start();

                senders.Add(new Sender(bus, messageGenerator, monitor.Done, SleepTime));

                Console.WriteLine("Type x to exit or type <number> to change the sleep time of a thread.");
                Console.ReadLine();

                fiveSecondThroughputCalculator.Stop();
                tenSecondThroughputCalculator.Stop();
                foreach (var sender in senders)
                {
                    sender.Stop();
                }
            }
        }
    }
}
