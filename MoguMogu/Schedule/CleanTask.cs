using System;
using System.Threading.Tasks;
using MoguMogu.Database;

namespace MoguMogu.Schedule
{
    public class CleanTask : IntervalTask
    {
        public CleanTask() : base(TimeSpan.FromHours(6))
        {
        }

        public override void Start()
        {
            base.Start();
            Task.Run(OnTask);
        }

        protected override void OnTask()
        {
            using var db = new DBContext();
            //dcm linq https://zoo.hololive.wtf/i/y55whngn.png
            foreach (var v in db.Verification)
                if (v.Timestamp.AddMinutes(5) < DateTime.UtcNow)
                    db.Verification.Remove(v);
            db.SaveChanges();
        }
    }
}