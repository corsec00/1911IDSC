using System;

namespace CompetitionApp.Models
{
    public class FinalResult
    {
        public string Name { get; set; } = string.Empty;
        public decimal Round1Time { get; set; }
        public decimal Round2Time { get; set; }
        public decimal BestTime { get; set; }
        public string BestRound { get; set; } = string.Empty;
    }
}
