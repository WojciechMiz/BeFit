using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeFit.Data;
using BeFit.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims; // Importuje przestrzeń nazw dla User.FindFirstValue.
using Microsoft.AspNetCore.Identity; // Importuje przestrzeń nazw dla UserManager.

// Kontroler do zarządzania sesjami treningowymi. Wymaga autoryzacji użytkownika.
[Authorize]
public class TrainingSessionsController : Controller
{
    // Kontekst bazy danych.
    private readonly ApplicationDbContext _context;
    // Menedżer użytkowników Identity.
    private readonly UserManager<IdentityUser> _userManager;

    // Konstruktor wstrzykujący zależności (DbContext i UserManager).
    public TrainingSessionsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Akcja GET: Wyświetla listę sesji treningowych zalogowanego użytkownika.
    public async Task<IActionResult> Index()
    {
        // Pobiera ID zalogowanego użytkownika.
        var userId = _userManager.GetUserId(User);
        // Pobiera sesje treningowe należące do użytkownika, posortowane malejąco po dacie rozpoczęcia.
        var trainingSessions = await _context.TrainingSessions
            .Where(ts => ts.UserId == userId) // Filtruje sesje po ID użytkownika.
            .OrderByDescending(ts => ts.StartTime) // Sortuje sesje.
            .ToListAsync(); // Materializuje wyniki do listy.
        // Zwraca widok z listą sesji treningowych.
        return View(trainingSessions);
    }

    // Akcja GET: Wyświetla szczegóły sesji treningowej o podanym ID.
    public async Task<IActionResult> Details(int? id)
    {
        // Sprawdza, czy ID zostało podane.
        if (id == null) return NotFound();
        // Pobiera ID zalogowanego użytkownika.
        var userId = _userManager.GetUserId(User);
        // Wyszukuje sesję treningową o podanym ID, która należy do zalogowanego użytkownika.
        var trainingSession = await _context.TrainingSessions
            // Opcjonalnie: Dołącz powiązane szczegóły treningu.
            // .Include(ts => ts.TrainingDetails).ThenInclude(td => td.Exercise)
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId); // Sprawdza ID sesji i ID właściciela.

        // Sprawdza, czy sesja została znaleziona.
        if (trainingSession == null) return NotFound(); // Zwraca 404, jeśli sesja nie istnieje lub nie należy do użytkownika.

        // Zwraca widok ze szczegółami sesji.
        return View(trainingSession);
    }

    // Akcja GET: Wyświetla formularz tworzenia nowej sesji treningowej.
    public IActionResult Create()
    {
        // Zwraca widok formularza.
        return View();
    }

    // Akcja POST: Przetwarza dane z formularza tworzenia sesji treningowej.
    [HttpPost]
    [ValidateAntiForgeryToken] // Zabezpieczenie przed CSRF.
    // Określa, które pola modelu mają być powiązane z danymi z żądania.
    public async Task<IActionResult> Create([Bind("StartTime,EndTime")] TrainingSession trainingSession)
    {
        // Pobiera ID zalogowanego użytkownika.
        var userId = _userManager.GetUserId(User);
        // Przypisuje ID użytkownika do tworzonej sesji.
        trainingSession.UserId = userId;

        // Usuwa potencjalne błędy walidacji dla pól nieobecnych w formularzu.
        ModelState.Remove("UserId");
        ModelState.Remove("User");
        ModelState.Remove("TrainingDetails");

        // Sprawdza, czy data zakończenia jest późniejsza niż data rozpoczęcia.
        if (trainingSession.EndTime <= trainingSession.StartTime)
        {
             // Dodaje błąd walidacji do ModelState.
             ModelState.AddModelError("EndTime", "Data zakończenia musi być późniejsza niż data rozpoczęcia.");
        }

        // Sprawdza poprawność danych modelu.
        if (ModelState.IsValid)
        {
            // Dodaje nową sesję do kontekstu.
            _context.Add(trainingSession);
            // Zapisuje zmiany w bazie danych.
            await _context.SaveChangesAsync();
            // Przekierowuje do listy sesji.
            return RedirectToAction(nameof(Index));
        }
        // Zwraca widok formularza z błędami walidacji.
        return View(trainingSession);
    }

    // Akcja GET: Wyświetla formularz edycji sesji treningowej o podanym ID.
    public async Task<IActionResult> Edit(int? id)
    {
        // Sprawdza, czy ID zostało podane.
        if (id == null) return NotFound();

        // Pobiera ID zalogowanego użytkownika.
        var userId = _userManager.GetUserId(User);
        // Wyszukuje sesję treningową o podanym ID, która należy do zalogowanego użytkownika.
        var trainingSession = await _context.TrainingSessions.FirstOrDefaultAsync(ts => ts.Id == id && ts.UserId == userId);

        // Sprawdza, czy sesja została znaleziona.
        if (trainingSession == null)
        {
            // Zwraca 404, jeśli sesja nie istnieje lub nie należy do użytkownika.
            return NotFound();
        }
        // Zwraca widok formularza edycji z danymi sesji.
        return View(trainingSession);
    }

    // Akcja POST: Przetwarza dane z formularza edycji sesji treningowej.
    [HttpPost]
    [ValidateAntiForgeryToken] // Zabezpieczenie przed CSRF.
    // Określa, które pola modelu mają być powiązane z danymi z żądania.
    public async Task<IActionResult> Edit(int id, [Bind("Id,StartTime,EndTime")] TrainingSession trainingSession)
    {
        // Sprawdza, czy ID z trasy zgadza się z ID w modelu.
        if (id != trainingSession.Id) return NotFound();

        // Pobiera ID zalogowanego użytkownika.
        var userId = _userManager.GetUserId(User);

        // Pobiera oryginalną sesję z bazy (bez śledzenia) w celu weryfikacji właściciela.
        var originalSession = await _context.TrainingSessions
                                        .AsNoTracking() // Wyłącza śledzenie zmian dla tej encji.
                                        .FirstOrDefaultAsync(ts => ts.Id == id);

        // Sprawdza, czy oryginalna sesja istnieje.
        if (originalSession == null) return NotFound();

        // Sprawdza, czy oryginalna sesja należy do zalogowanego użytkownika.
        if (originalSession.UserId != userId) return Forbid(); // Zwraca 403, jeśli użytkownik nie jest właścicielem.

        // Przypisuje ID użytkownika do edytowanej sesji, aby zapobiec jego zmianie.
        trainingSession.UserId = userId;
        // Usuwa potencjalne błędy walidacji dla pól nieobecnych w formularzu.
        ModelState.Remove("UserId");
        ModelState.Remove("User");
        ModelState.Remove("TrainingDetails");

        // Sprawdza, czy data zakończenia jest późniejsza niż data rozpoczęcia.
        if (trainingSession.EndTime <= trainingSession.StartTime)
        {
             // Dodaje błąd walidacji do ModelState.
             ModelState.AddModelError("EndTime", "Data zakończenia musi być późniejsza niż data rozpoczęcia.");
        }

        // Sprawdza poprawność danych modelu.
        if (ModelState.IsValid)
        {
            try
            {
                // Oznacza encję jako zmodyfikowaną.
                _context.Update(trainingSession);
                // Zapisuje zmiany w bazie danych.
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Obsługa konfliktu współbieżności.
                // Sprawdza, czy sesja nadal istnieje i należy do użytkownika.
                if (!await TrainingSessionExistsAndBelongsToUser(trainingSession.Id, userId))
                {
                    return NotFound(); // Zwraca 404, jeśli sesja została usunięta lub zmieniono właściciela.
                }
                else
                {
                    throw; // Rzuca wyjątek dalej.
                }
            }
            // Przekierowuje do listy sesji.
            return RedirectToAction(nameof(Index));
        }
        // Zwraca widok formularza edycji z błędami walidacji.
        return View(trainingSession);
    }

    // Akcja GET: Wyświetla stronę potwierdzenia usunięcia sesji treningowej.
    public async Task<IActionResult> Delete(int? id)
    {
        // Sprawdza, czy ID zostało podane.
        if (id == null) return NotFound();

        // Pobiera ID zalogowanego użytkownika.
        var userId = _userManager.GetUserId(User);
        // Wyszukuje sesję treningową o podanym ID, która należy do zalogowanego użytkownika.
        var trainingSession = await _context.TrainingSessions
            // Opcjonalnie: Dołącz powiązane szczegóły treningu.
            // .Include(ts => ts.TrainingDetails)
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

        // Sprawdza, czy sesja została znaleziona.
        if (trainingSession == null) return NotFound(); // Zwraca 404, jeśli sesja nie istnieje lub nie należy do użytkownika.

        // Zwraca widok potwierdzenia usunięcia.
        return View(trainingSession);
    }

    // Akcja POST: Potwierdza i wykonuje usunięcie sesji treningowej.
    [HttpPost, ActionName("Delete")] // Używa aliasu "Delete" dla tej akcji.
    [ValidateAntiForgeryToken] // Zabezpieczenie przed CSRF.
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        // Pobiera ID zalogowanego użytkownika.
        var userId = _userManager.GetUserId(User);
        // Wyszukuje sesję treningową o podanym ID, która należy do zalogowanego użytkownika.
        var trainingSession = await _context.TrainingSessions.FirstOrDefaultAsync(ts => ts.Id == id && ts.UserId == userId);

        // Sprawdza, czy sesja została znaleziona i należy do użytkownika.
        if (trainingSession != null)
        {
            // Uwaga: Należy obsłużyć usuwanie powiązanych TrainingDetails, jeśli nie ma Cascade Delete.
            // var detailsToDelete = await _context.TrainingDetails.Where(td => td.TrainingSessionId == id).ToListAsync();
            // _context.TrainingDetails.RemoveRange(detailsToDelete);

            // Usuwa sesję z kontekstu.
            _context.TrainingSessions.Remove(trainingSession);
            // Zapisuje zmiany w bazie danych.
            await _context.SaveChangesAsync();
        }
        // Jeśli sesja nie została znaleziona lub nie należy do użytkownika, przekierowuje do listy.
        // Nie zwraca błędu, aby nie ujawniać informacji o istnieniu sesji.
        return RedirectToAction(nameof(Index));
    }

    // Metoda pomocnicza sprawdzająca, czy sesja istnieje i należy do danego użytkownika.
    private async Task<bool> TrainingSessionExistsAndBelongsToUser(int id, string userId)
    {
        // Sprawdza istnienie sesji o podanym ID i należącej do użytkownika o podanym userId.
        return await _context.TrainingSessions.AnyAsync(e => e.Id == id && e.UserId == userId);
    }

    // Metoda pomocnicza sprawdzająca istnienie sesji o podanym ID.
    private bool TrainingSessionExists(int id)
    {
        // Sprawdza, czy w bazie danych istnieje sesja o podanym ID.
        return _context.TrainingSessions.Any(e => e.Id == id);
    }
}
