using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Messages
{
    public interface IMonitor
    {
        void Done();
    }

    public class CompositeMonitor : IMonitor
    {
        private readonly IEnumerable<IMonitor> monitors;

        public CompositeMonitor(params IMonitor[] monitors)
        {
            this.monitors = monitors;
        }

        public void Done()
        {
            foreach (var monitor in monitors)
            {
                monitor.Done();
            }
        }
    }

    public class AverageThroughputCalculator : IMonitor
    {
        private readonly Action<double> onProbe;
        private readonly int[] buffer;
        private readonly Stopwatch[] watches;
        private readonly int period;
        private readonly Timer timer;
        private int currentSlot;

        public AverageThroughputCalculator(TimeSpan windowLenght, int probeFrequency, Action<double> onProbe)
        {
            this.onProbe = onProbe;
            buffer = new int[probeFrequency];
            watches = new Stopwatch[probeFrequency];
            period = (int)(windowLenght.TotalMilliseconds / probeFrequency);
            timer = new Timer(Tick, null, Timeout.Infinite, period);
        }

        private void Tick(object state)
        {
            var newSlot = (currentSlot + 1) % buffer.Length;
            var oldestValue = buffer[newSlot];
            buffer[newSlot] = 0;
            currentSlot = newSlot;
            watches[newSlot].Stop();
            var elapsed = watches[newSlot].Elapsed.TotalSeconds;
            watches[newSlot].Reset();
            watches[newSlot].Start();
            var sum = oldestValue + buffer.Where((t, i) => i != currentSlot).Sum();
            var average = sum / elapsed;
            onProbe(average);
        }

        public void Done()
        {
            Interlocked.Increment(ref buffer[currentSlot]);
        }

        public void Start()
        {
            for (var i = 0; i < buffer.Length; i++)
            {
                buffer[i] = 0;
                watches[i] = new Stopwatch();
                watches[i].Start();
            }
            currentSlot = 0;
            timer.Change(0, period);
        }

        public void Stop()
        {
            timer.Change(Timeout.Infinite, period);
        }
    }
}