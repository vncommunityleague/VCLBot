using System;
using System.Timers;

namespace MoguMogu.Schedule
{
    public abstract class IntervalTask
    {
        private readonly TimeSpan _delay;
        private Timer _timer;

        protected IntervalTask(TimeSpan delay)
        {
            _delay = delay;
        }

        public virtual void Start()
        {
            _timer = new Timer(_delay.TotalMilliseconds)
            {
                AutoReset = true
            };
            _timer.Elapsed += (sender, args) => { OnTask(); };
            _timer.Start();
        }

        protected virtual void OnTask()
        {
        }
    }
}