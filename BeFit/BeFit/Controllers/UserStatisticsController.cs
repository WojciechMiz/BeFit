using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeFit.Data;
using BeFit.Models; // Importuje przestrzeń nazw dla modeli bazowych.
using BeFit.ViewModels; // Importuje przestrzeń nazw dla ViewModeli.
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims; // Importuje przestrzeń nazw dla User.FindFirstValue.
using Microsoft.AspNetCore.Identity; // Importuje przestrzeń nazw dla UserManager.
using System.Collections.Generic; // Importuje przestrzeń nazw dla List.

namespace BeFit.Controllers
{
    // Kontroler do wyświetlania statystyk użytkownika. Wymaga autoryzacji.
    [Authorize]
    public class UserStatisticsController : Controller
    {
        // Kontekst bazy danych.
        private readonly ApplicationDbContext _context;
        // Menedżer użytkowników Identity.
        private readonly UserManager<IdentityUser> _userManager;

        // Konstruktor wstrzykujący zależności (DbContext i UserManager).
        public UserStatisticsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Akcja GET: Wyświetla statystyki treningowe zalogowanego użytkownika.
        public async Task<IActionResult> Index()
        {
            // Pobiera ID zalogowanego użytkownika.
            var userId = _userManager.GetUserId(User);
            // Oblicza datę sprzed 4 tygodni od teraz (w UTC).
            var fourWeeksAgo = DateTime.UtcNow.AddDays(-28);

            // Pobiera szczegóły treningowe użytkownika z ostatnich 4 tygodni.
            var recentTrainingDetails = await _context.TrainingDetails
                .Include(td => td.TrainingSession) // Dołącza dane sesji.
                .Include(td => td.Exercise)       // Dołącza dane ćwiczenia.
                // Filtruje po ID użytkownika i dacie rozpoczęcia sesji.
                .Where(td => td.TrainingSession.UserId == userId && td.TrainingSession.StartTime >= fourWeeksAgo)
                .ToListAsync(); // Materializuje wyniki do listy.

            // Sprawdza, czy znaleziono jakiekolwiek dane treningowe.
            if (!recentTrainingDetails.Any())
            {
                // Ustawia komunikat dla widoku, jeśli brak danych.
                ViewBag.Message = "Brak danych treningowych z ostatnich 4 tygodni do wygenerowania statystyk.";
                // Zwraca widok z pustą listą statystyk.
                return View(new List<ExerciseStatisticViewModel>());
            }

            // Oblicza statystyki, grupując dane po typie ćwiczenia.
            var statistics = recentTrainingDetails
                .GroupBy(td => td.Exercise) // Grupuje szczegóły treningu według obiektu Exercise.
                // Tworzy nowy obiekt ExerciseStatisticViewModel dla każdej grupy.
                .Select(g => new ExerciseStatisticViewModel
                {
                    // Pobiera nazwę ćwiczenia z klucza grupy.
                    ExerciseName = g.Key.Name,
                    // Liczy liczbę wykonanych ćwiczeń danego typu.
                    TimesPerformed = g.Count(),
                    // Oblicza sumę całkowitej liczby powtórzeń (serie * powtórzenia).
                    TotalReps = g.Sum(td => td.Sets * td.Repetitions),
                    // Oblicza średnie obciążenie (rzutowane na double).
                    AverageLoad = g.Any() ? g.Average(td => (double)td.Load) : 0.0,
                    // Znajduje maksymalne użyte obciążenie.
                    MaximumLoad = g.Any() ? g.Max(td => td.Load) : 0m
                })
                .OrderBy(s => s.ExerciseName) // Sortuje statystyki alfabetycznie po nazwie ćwiczenia.
                .ToList(); // Materializuje wyniki do listy.

            // Zwraca widok z obliczonymi statystykami.
            return View(statistics);
        }
    }
}
