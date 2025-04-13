using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeFit.Data;
using BeFit.Models;
using System.Threading.Tasks;
using System.Linq; // Importuje przestrzeń nazw dla metod LINQ, np. Any(), OrderBy().

// Kontroler do zarządzania typami ćwiczeń.
// Wymaga roli "Administrator" dla akcji modyfikujących dane.
[Authorize(Roles = "Administrator")]
public class ExercisesController : Controller
{
    // Kontekst bazy danych używany do operacji na danych.
    private readonly ApplicationDbContext _context;

    // Konstruktor wstrzykujący kontekst bazy danych.
    public ExercisesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Akcja GET: Wyświetla listę wszystkich ćwiczeń.
    // Dostępna dla wszystkich użytkowników (również niezalogowanych).
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        // Pobiera posortowaną listę ćwiczeń i przekazuje ją do widoku.
        return View(await _context.Exercises.OrderBy(e => e.Name).ToListAsync());
    }

    // Akcja GET: Wyświetla szczegóły ćwiczenia o podanym ID.
    // Dostępna dla wszystkich użytkowników.
    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        // Sprawdza, czy ID zostało podane.
        if (id == null)
        {
            return NotFound(); // Zwraca 404, jeśli ID jest null.
        }

        // Wyszukuje ćwiczenie w bazie danych po ID.
        var exercise = await _context.Exercises
            .FirstOrDefaultAsync(m => m.Id == id);
        // Sprawdza, czy ćwiczenie zostało znalezione.
        if (exercise == null)
        {
            return NotFound(); // Zwraca 404, jeśli ćwiczenie nie istnieje.
        }

        // Zwraca widok ze szczegółami ćwiczenia.
        return View(exercise);
    }

    // Akcja GET: Wyświetla formularz tworzenia nowego ćwiczenia.
    // Wymaga roli "Administrator".
    [Authorize(Roles = "Administrator")]
    public IActionResult Create()
    {
        // Zwraca widok formularza.
        return View();
    }

    // Akcja POST: Przetwarza dane z formularza tworzenia ćwiczenia.
    // Wymaga roli "Administrator".
    [HttpPost]
    [ValidateAntiForgeryToken] // Zabezpieczenie przed CSRF.
    [Authorize(Roles = "Administrator")]
    // Określa, które pola modelu mają być powiązane z danymi z żądania.
    public async Task<IActionResult> Create([Bind("Name,Description")] Exercise exercise)
    {
        // Sprawdza poprawność danych modelu.
        if (ModelState.IsValid)
        {
            // Dodaje nowe ćwiczenie do kontekstu.
            _context.Add(exercise);
            // Zapisuje zmiany w bazie danych.
            await _context.SaveChangesAsync();
            // Przekierowuje do listy ćwiczeń.
            return RedirectToAction(nameof(Index));
        }
        // Zwraca widok formularza z błędami walidacji.
        return View(exercise);
    }

    // Akcja GET: Wyświetla formularz edycji ćwiczenia o podanym ID.
    // Wymaga roli "Administrator".
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Edit(int? id)
    {
        // Sprawdza, czy ID zostało podane.
        if (id == null)
        {
            return NotFound();
        }

        // Wyszukuje ćwiczenie w bazie danych po ID.
        var exercise = await _context.Exercises.FindAsync(id);
        // Sprawdza, czy ćwiczenie zostało znalezione.
        if (exercise == null)
        {
            return NotFound();
        }
        // Zwraca widok formularza edycji z danymi ćwiczenia.
        return View(exercise);
    }

    // Akcja POST: Przetwarza dane z formularza edycji ćwiczenia.
    // Wymaga roli "Administrator".
    [HttpPost]
    [ValidateAntiForgeryToken] // Zabezpieczenie przed CSRF.
    [Authorize(Roles = "Administrator")]
    // Określa, które pola modelu mają być powiązane z danymi z żądania.
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Exercise exercise)
    {
        // Sprawdza, czy ID z trasy zgadza się z ID w modelu.
        if (id != exercise.Id)
        {
            return NotFound();
        }

        // Sprawdza poprawność danych modelu.
        if (ModelState.IsValid)
        {
            try
            {
                // Oznacza encję jako zmodyfikowaną.
                _context.Update(exercise);
                // Zapisuje zmiany w bazie danych.
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Obsługa konfliktu współbieżności.
                if (!ExerciseExists(exercise.Id))
                {
                    return NotFound(); // Zwraca 404, jeśli encja została usunięta.
                }
                else
                {
                    throw; // Rzuca wyjątek dalej, jeśli wystąpił inny problem.
                }
            }
            // Przekierowuje do listy ćwiczeń.
            return RedirectToAction(nameof(Index));
        }
        // Zwraca widok formularza edycji z błędami walidacji.
        return View(exercise);
    }

    // Akcja GET: Wyświetla stronę potwierdzenia usunięcia ćwiczenia.
    // Wymaga roli "Administrator".
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(int? id)
    {
        // Sprawdza, czy ID zostało podane.
        if (id == null)
        {
            return NotFound();
        }

        // Wyszukuje ćwiczenie w bazie danych po ID.
        var exercise = await _context.Exercises
            .FirstOrDefaultAsync(m => m.Id == id);
        // Sprawdza, czy ćwiczenie zostało znalezione.
        if (exercise == null)
        {
            return NotFound();
        }

        // Zwraca widok potwierdzenia usunięcia.
        return View(exercise);
    }

    // Akcja POST: Potwierdza i wykonuje usunięcie ćwiczenia.
    // Wymaga roli "Administrator".
    [HttpPost, ActionName("Delete")] // Używa aliasu "Delete" dla tej akcji.
    [ValidateAntiForgeryToken] // Zabezpieczenie przed CSRF.
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        // Wyszukuje ćwiczenie w bazie danych po ID.
        var exercise = await _context.Exercises.FindAsync(id);
        // Sprawdza, czy ćwiczenie istnieje.
        if (exercise != null)
        {
            // Usuwa ćwiczenie z kontekstu.
            _context.Exercises.Remove(exercise);
            // Zapisuje zmiany w bazie danych.
            await _context.SaveChangesAsync();
        }
        else
        {
             // Zwraca 404, jeśli ćwiczenie nie zostało znalezione.
             return NotFound();
        }
        // Przekierowuje do listy ćwiczeń.
        return RedirectToAction(nameof(Index));
    }

    // Metoda pomocnicza sprawdzająca istnienie ćwiczenia o podanym ID.
    private bool ExerciseExists(int id)
    {
        // Sprawdza, czy w bazie danych istnieje ćwiczenie o podanym ID.
        return _context.Exercises.Any(e => e.Id == id);
    }
}
