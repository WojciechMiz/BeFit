using System.ComponentModel.DataAnnotations;
using System.Collections.Generic; // Importuje przestrzeń nazw dla ICollection.

namespace BeFit.Models
{
    // Definicja modelu reprezentującego typ ćwiczenia.
    public class Exercise
    {
        // Klucz główny encji Exercise.
        public int Id { get; set; }

        // Właściwość przechowująca nazwę ćwiczenia.
        [Required(ErrorMessage = "Nazwa ćwiczenia jest wymagana.")] // Określa, że pole Name jest wymagane.
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Nazwa musi mieć od 3 do 100 znaków.")] // Określa minimalną i maksymalną długość nazwy.
        [Display(Name = "Nazwa Ćwiczenia")] // Określa nazwę wyświetlaną dla pola Name w interfejsie użytkownika.
        public string Name { get; set; }

        // Właściwość przechowująca opcjonalny opis ćwiczenia.
        [StringLength(500, ErrorMessage = "Opis może mieć maksymalnie 500 znaków.")] // Określa maksymalną długość opisu.
        [Display(Name = "Opis (opcjonalnie)")] // Określa nazwę wyświetlaną dla pola Description.
        [DataType(DataType.MultilineText)] // Sugeruje, że pole powinno być renderowane jako wieloliniowe pole tekstowe.
        public string? Description { get; set; }

        // Właściwość nawigacyjna reprezentująca kolekcję powiązanych szczegółów treningu (TrainingDetail).
        public virtual ICollection<TrainingDetail>? TrainingDetails { get; set; }
    }
}
