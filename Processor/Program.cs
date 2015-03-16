using System;
using System.Collections.Generic;
using Messages;
using NServiceBus;
using NServiceBus.Saga;

namespace Processor
{
    class Program
    {
        static void Main(string[] args)
        {
            var busConfig = new BusConfiguration();
            busConfig.UsePersistence<NHibernatePersistence>();
            busConfig.UseTransport<SqlServerTransport>();
            busConfig.EnableInstallers();

            FiveSecondThroughputCalculator = new AverageThroughputCalculator(
                TimeSpan.FromSeconds(5), 5, avg => Console.WriteLine(string.Format("Average throughput in last 5 seconds: {0,10:0.000}", avg)));

            FiveSecondThroughputCalculator.Start();

            using (Bus.Create(busConfig).Start())
            {
                Console.WriteLine("Press <enter> to exit.");
                Console.ReadLine();
            }

            FiveSecondThroughputCalculator.Stop();
        }

        public static AverageThroughputCalculator FiveSecondThroughputCalculator;
    }

    //public class TestMessageHandler : IHandleMessages<TestMessage>
    //{
    //    public void Handle(TestMessage message)
    //    {
    //        Program.FiveSecondThroughputCalculator.Done();
    //    }
    //}

    public class TestSaga : Saga<TestSagaData>, IAmStartedByMessages<TestMessage>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<TestSagaData> mapper)
        {
            mapper.ConfigureMapping<TestMessage>(m => m.TaskId).ToSaga(s => s.TaskId);
        }

        public void Handle(TestMessage message)
        {
            Data.TaskId = message.TaskId;
            Data.WorkItems.Add(new WorkItem()
            {
                Data = message.Data
            });
            if (Data.WorkItems.Count > 20)
            {
                Data.WorkItems.RemoveAt(0);
            }
            Program.FiveSecondThroughputCalculator.Done();
        }
    }

    public class TestSagaData : ContainSagaData
    {
        public virtual IList<WorkItem> WorkItems { get; set; }
        [Unique]
        public virtual Guid TaskId { get; set; }

        public TestSagaData()
        {
            WorkItems = new List<WorkItem>();
        }
    }

    public class WorkItem
    {
        public virtual string Data { get; set; }
    }
}
