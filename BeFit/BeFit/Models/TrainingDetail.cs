using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Importuje przestrzenie nazw dla atrybutów schematu.

namespace BeFit.Models
{
    // Definicja modelu reprezentującego szczegół wykonanego ćwiczenia w ramach sesji treningowej.
    public class TrainingDetail
    {
        // Klucz główny encji TrainingDetail.
        public int Id { get; set; }

        // Klucz obcy wskazujący na powiązaną sesję treningową (TrainingSession).
        [Required(ErrorMessage = "Należy powiązać z sesją treningową.")] // Określa, że pole jest wymagane.
        [Display(Name = "Sesja Treningowa")] // Nazwa wyświetlana dla pola w interfejsie użytkownika.
        public int TrainingSessionId { get; set; }

        // Klucz obcy wskazujący na powiązany typ ćwiczenia (Exercise).
        [Required(ErrorMessage = "Należy wybrać typ ćwiczenia.")] // Określa, że pole jest wymagane.
        [Display(Name = "Typ Ćwiczenia")] // Nazwa wyświetlana dla pola w interfejsie użytkownika.
        public int ExerciseId { get; set; }

        // Właściwość przechowująca użyte obciążenie podczas ćwiczenia.
        [Required(ErrorMessage = "Obciążenie jest wymagane.")] // Określa, że pole jest wymagane.
        [Range(0.0, 1000.0, ErrorMessage = "Obciążenie musi być wartością między 0 a 1000.")] // Określa dozwolony zakres wartości.
        [Display(Name = "Użyte Obciążenie (kg)")] // Nazwa wyświetlana dla pola w interfejsie użytkownika.
        [Column(TypeName = "decimal(6, 2)")] // Definiuje typ kolumny w bazie danych jako decimal(6, 2).
        public decimal Load { get; set; }

        // Właściwość przechowująca liczbę wykonanych serii.
        [Required(ErrorMessage = "Liczba serii jest wymagana.")] // Określa, że pole jest wymagane.
        [Range(1, 100, ErrorMessage = "Liczba serii musi być między 1 a 100.")] // Określa dozwolony zakres wartości.
        [Display(Name = "Liczba Serii")] // Nazwa wyświetlana dla pola w interfejsie użytkownika.
        public int Sets { get; set; }

        // Właściwość przechowująca liczbę powtórzeń w serii.
        [Required(ErrorMessage = "Liczba powtórzeń jest wymagana.")] // Określa, że pole jest wymagane.
        [Range(1, 200, ErrorMessage = "Liczba powtórzeń musi być między 1 a 200.")] // Określa dozwolony zakres wartości.
        [Display(Name = "Liczba Powtórzeń w Serii")] // Nazwa wyświetlana dla pola w interfejsie użytkownika.
        public int Repetitions { get; set; }

        // Właściwość nawigacyjna do powiązanej encji TrainingSession.
        [ForeignKey("TrainingSessionId")] // Określa klucz obcy dla tej relacji.
        public virtual TrainingSession? TrainingSession { get; set; }

        // Właściwość nawigacyjna do powiązanej encji Exercise.
        [ForeignKey("ExerciseId")] // Określa klucz obcy dla tej relacji.
        public virtual Exercise? Exercise { get; set; }
    }
}
