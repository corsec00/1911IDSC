using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CompetitionApp.Models
{
    public class PenaltyConfiguration
    {
        [Display(Name = "Bravo")]
        [Range(0, 100, ErrorMessage = "O valor deve estar entre 0 e 100 segundos")]
        public int BravoValue { get; set; } = 3; // Valor padrão

        [Display(Name = "Charlie")]
        [Range(0, 100, ErrorMessage = "O valor deve estar entre 0 e 100 segundos")]
        public int CharlieValue { get; set; } = 5; // Valor padrão

        [Display(Name = "Miss")]
        [Range(0, 100, ErrorMessage = "O valor deve estar entre 0 e 100 segundos")]
        public int MissValue { get; set; } = 10; // Valor padrão

        [Display(Name = "Vítima")]
        [Range(0, 100, ErrorMessage = "O valor deve estar entre 0 e 100 segundos")]
        public int VitimaValue { get; set; } = 15; // Valor padrão

        [Display(Name = "Plate")]
        [Range(0, 100, ErrorMessage = "O valor deve estar entre 0 e 100 segundos")]
        public int PlateValue { get; set; } = 7; // Valor padrão

        [Display(Name = "Fault")]
        [Range(0, 100, ErrorMessage = "O valor deve estar entre 0 e 100 segundos")]
        public int FaultValue { get; set; } = 10; // Valor padrão

        [Display(Name = "Desclassificado")]
        [Range(0, 9999, ErrorMessage = "O valor deve estar entre 0 e 9999 segundos")]
        public int DisqualifiedValue { get; set; } = 999; // Valor padrão
    }
}
