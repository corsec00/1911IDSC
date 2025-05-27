using System.ComponentModel.DataAnnotations;
using CompetitionApp.Pages.Configuration;

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
        public int VitimaCount { get; set; }
        public int PlateCount { get; set; }
        
        public int CalculateTotalTime()
        {
            // Obter a configuração atual de penalidades
            var config = PenaltyConfigModel.GetCurrentConfiguration();
            
            // Cálculo do tempo total com as penalidades configuráveis
            int penalties = (BravoCount * config.BravoValue) + 
                           (CharlieCount * config.CharlieValue) + 
                           (MissCount * config.MissValue) + 
                           (FaltaCount * config.FaultValue) +
                           (VitimaCount * config.VitimaValue) +
                           (PlateCount * config.PlateValue);
                           
            return TimeInSeconds + penalties;
        }
        
        // Indica se o participante foi eliminado usando o valor configurável
        public bool IsEliminated => TimeInSeconds == PenaltyConfigModel.GetCurrentConfiguration().DisqualifiedValue;
    }
}
