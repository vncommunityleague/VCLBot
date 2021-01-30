using System;

namespace MoguMogu.SpreadSheets
{
    public class Match
    {
        public Match(int id, DateTime dateTime)
        {
            Id = id;
            DateTime = dateTime;
        }

        public int Id { get; }
        public DateTime DateTime { get; }
    }
}