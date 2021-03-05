using System;

namespace MoguMogu.SpreadSheets
{
    public class Match
    {
        public Match(string id, DateTime dateTime, string teamA, string teamB, string referee)
        {
            Id = id;
            DateTime = dateTime;
            TeamA = teamA;
            TeamB = teamB;
            Referee = referee;
        }

        public string Id { get; }
        public DateTime DateTime { get; }
        public string TeamA { get; }
        public string TeamB { get; }
        public string Referee { get; }
    }
}