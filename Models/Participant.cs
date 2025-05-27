using System.ComponentModel.DataAnnotations;

namespace CompetitionApp.Models
{
    public class Participant
    {
        public string Name { get; set; } = string.Empty;
        public int TimeInSeconds { get; set; }
        public int BravoCount { get; set; }
        public int CharlieCount { get; set; }
        public int MissCount { get; set; }
        public int FaltaCount { get; set; }
        
        public int CalculateTotalTime()
        {
            // Cálculo do tempo total com as penalidades
            int penalties = (BravoCount * 3) + (CharlieCount * 5) + (MissCount * 10) + (FaltaCount * 10);
            return TimeInSeconds + penalties;
        }
        
        // Indica se o participante foi eliminado
        public bool IsEliminated => TimeInSeconds == 999;
    }
}
