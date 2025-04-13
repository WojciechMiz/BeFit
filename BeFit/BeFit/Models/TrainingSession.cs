using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Importuje przestrzeń nazw dla [ForeignKey].
using Microsoft.AspNetCore.Identity; // Importuje przestrzeń nazw dla IdentityUser.
using System.Collections.Generic; // Importuje przestrzeń nazw dla ICollection.

namespace BeFit.Models
{
    // Definicja modelu reprezentującego sesję treningową.
    public class TrainingSession
    {
        // Klucz główny encji TrainingSession.
        public int Id { get; set; }

        // Właściwość przechowująca datę i czas rozpoczęcia sesji.
        [Required(ErrorMessage = "Data i czas rozpoczęcia są wymagane.")] // Określa, że pole jest wymagane.
        [DataType(DataType.DateTime)] // Określa typ danych jako datę i czas.
        [Display(Name = "Rozpoczęcie sesji")] // Nazwa wyświetlana dla pola w interfejsie użytkownika.
        public DateTime StartTime { get; set; }

        // Właściwość przechowująca datę i czas zakończenia sesji.
        [Required(ErrorMessage = "Data i czas zakończenia są wymagane.")] // Określa, że pole jest wymagane.
        [DataType(DataType.DateTime)] // Określa typ danych jako datę i czas.
        [Display(Name = "Zakończenie sesji")] // Nazwa wyświetlana dla pola w interfejsie użytkownika.
        [Compare("StartTime", ErrorMessage = "Data zakończenia musi być późniejsza lub równa dacie rozpoczęcia.")] // Porównuje wartość z polem StartTime.
        public DateTime EndTime { get; set; }

        // Klucz obcy wskazujący na powiązanego użytkownika (IdentityUser).
        [Required] // Określa, że pole jest wymagane.
        public string UserId { get; set; }

        // Właściwość nawigacyjna do powiązanego użytkownika (IdentityUser).
        [ForeignKey("UserId")] // Określa klucz obcy dla tej relacji.
        public virtual IdentityUser? User { get; set; }

        // Właściwość nawigacyjna reprezentująca kolekcję powiązanych szczegółów treningu (TrainingDetail).
        public virtual ICollection<TrainingDetail>? TrainingDetails { get; set; }
    }
}
