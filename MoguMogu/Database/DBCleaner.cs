using System;
using System.Timers;
using Microsoft.EntityFrameworkCore;

namespace MoguMogu.Database
{
    public abstract class DbCleaner<SET> where SET : class
    {
        private readonly TimeSpan _delay;
        private Timer _timer;

        protected DbCleaner(TimeSpan delay)
        {
            _delay = delay;
        }

        public virtual void Start()
        {
            _timer = new Timer(_delay.TotalMilliseconds)
            {
                AutoReset = true
            };
            _timer.Elapsed += (sender, args) => { Scan(); };
            _timer.Start();
        }

        protected virtual void Scan()
        {
            using var context = new DBContext();
            var set = context.Set<SET>();
            OnScan(set, context);
        }

        protected virtual void OnScan(DbSet<SET> set, DbContext c)
        {
        }
    }
}