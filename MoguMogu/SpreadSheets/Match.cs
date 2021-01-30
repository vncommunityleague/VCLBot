using System;

namespace MoguMogu.SpreadSheets
{
    public class Match
    {
        public Match(string id, DateTime dateTime, string team1, string team2, string referee)
        {
            Id = id;
            DateTime = dateTime;
            Team1 = team1;
            Team2 = team2;
            Referee = referee;
        }
        public string Id { get; }
        public DateTime DateTime { get; }
        public string Team1 { get; }
        public string Team2 { get; }
        public string Referee { get; }
    }
}