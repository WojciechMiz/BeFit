using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BeFit.Data;
using BeFit.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims; // Importuje przestrzeń nazw dla User.FindFirstValue.
using Microsoft.AspNetCore.Identity; // Importuje przestrzeń nazw dla UserManager.
using Microsoft.AspNetCore.Mvc.Rendering; // Importuje przestrzeń nazw dla SelectList.

namespace BeFit.Controllers
{
    // Kontroler do zarządzania szczegółami treningu. Wymaga autoryzacji użytkownika.
    [Authorize]
    public class TrainingDetailsController : Controller
    {
        // Kontekst bazy danych.
        private readonly ApplicationDbContext _context;
        // Menedżer użytkowników Identity.
        private readonly UserManager<IdentityUser> _userManager;

        // Konstruktor wstrzykujący zależności (DbContext i UserManager).
        public TrainingDetailsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Akcja GET: Wyświetla listę szczegółów treningu dla podanej sesji.
        public async Task<IActionResult> Index(int trainingSessionId)
        {
            // Pobiera ID zalogowanego użytkownika.
            var userId = _userManager.GetUserId(User);

            // Sprawdza, czy sesja o podanym ID należy do zalogowanego użytkownika.
            var sessionBelongsToUser = await _context.TrainingSessions
                                                 .AnyAsync(ts => ts.Id == trainingSessionId && ts.UserId == userId);

            // Jeśli sesja nie należy do użytkownika, zwraca błąd Forbid (403).
            if (!sessionBelongsToUser)
            {
                return Forbid();
            }

            // Pobiera szczegóły treningu dla danej sesji, dołączając powiązane ćwiczenia.
            var trainingDetails = await _context.TrainingDetails
                .Where(td => td.TrainingSessionId == trainingSessionId)
                .Include(td => td.Exercise) // Dołącza dane z tabeli Exercises.
                .OrderBy(td => td.Exercise.Name) // Sortuje wyniki po nazwie ćwiczenia.
                .ToListAsync();

            // Przekazuje ID sesji do widoku za pomocą ViewBag.
            ViewBag.TrainingSessionId = trainingSessionId;

            // Zwraca widok z listą szczegółów treningu.
            return View(trainingDetails);
        }

        // Akcja GET: Wyświetla formularz tworzenia nowego szczegółu treningu dla podanej sesji.
        public async Task<IActionResult> Create(int trainingSessionId)
        {
            // Pobiera ID zalogowanego użytkownika.
            var userId = _userManager.GetUserId(User);

            // Sprawdza, czy sesja o podanym ID istnieje i należy do zalogowanego użytkownika.
            var session = await _context.TrainingSessions
                                      .FirstOrDefaultAsync(ts => ts.Id == trainingSessionId && ts.UserId == userId);

            // Jeśli sesja nie istnieje lub nie należy do użytkownika, zwraca błąd Forbid (403).
            if (session == null)
            {
                return Forbid();
            }

            // Tworzy nowy obiekt TrainingDetail z ustawionym ID sesji.
            var trainingDetail = new TrainingDetail
            {
                TrainingSessionId = trainingSessionId
            };

            // Ładuje listę ćwiczeń do ViewBag dla listy rozwijanej w formularzu.
            await PopulateExercisesDropDownList();
            // Przekazuje ID sesji do widoku za pomocą ViewBag.
            ViewBag.TrainingSessionId = trainingSessionId;

            // Zwraca widok formularza tworzenia.
            return View(trainingDetail);
        }

        // Akcja POST: Przetwarza dane z formularza tworzenia szczegółu treningu.
        [HttpPost]
        [ValidateAntiForgeryToken] // Zabezpieczenie przed CSRF.
        // Określa, które pola modelu mają być powiązane z danymi z żądania.
        public async Task<IActionResult> Create([Bind("TrainingSessionId,ExerciseId,Load,Sets,Repetitions")] TrainingDetail trainingDetail)
        {
            // Pobiera ID zalogowanego użytkownika.
            var userId = _userManager.GetUserId(User);

            // Sprawdza, czy sesja podana w modelu należy do zalogowanego użytkownika.
            var sessionBelongsToUser = await _context.TrainingSessions
                                                 .AnyAsync(ts => ts.Id == trainingDetail.TrainingSessionId && ts.UserId == userId);

            // Jeśli sesja nie należy do użytkownika, zwraca błąd Forbid (403).
            if (!sessionBelongsToUser)
            {
                return Forbid();
            }

            // Usuwa potencjalne błędy walidacji dla właściwości nawigacyjnych, które nie są częścią formularza.
            ModelState.Remove("TrainingSession");
            ModelState.Remove("Exercise");

            // Sprawdza poprawność danych modelu.
            if (ModelState.IsValid)
            {
                // Dodaje nowy szczegół treningu do kontekstu.
                _context.Add(trainingDetail);
                // Zapisuje zmiany w bazie danych.
                await _context.SaveChangesAsync();
                // Przekierowuje do widoku szczegółów nadrzędnej sesji treningowej.
                return RedirectToAction("Details", "TrainingSessions", new { id = trainingDetail.TrainingSessionId });
            }

            // Jeśli dane są nieprawidłowe, ponownie ładuje listę ćwiczeń.
            await PopulateExercisesDropDownList(trainingDetail.ExerciseId);
            // Przekazuje ID sesji do widoku za pomocą ViewBag.
            ViewBag.TrainingSessionId = trainingDetail.TrainingSessionId;
            // Zwraca widok formularza z błędami walidacji.
            return View(trainingDetail);
        }

        // Akcja GET: Wyświetla formularz edycji szczegółu treningu o podanym ID.
        public async Task<IActionResult> Edit(int? id) // ID edytowanego szczegółu treningu.
        {
            // Sprawdza, czy ID zostało podane.
            if (id == null) return NotFound();

            // Pobiera ID zalogowanego użytkownika.
            var userId = _userManager.GetUserId(User);

            // Pobiera szczegół treningu z bazy, dołączając powiązaną sesję.
            var trainingDetail = await _context.TrainingDetails
                .Include(td => td.TrainingSession) // Dołącza dane z tabeli TrainingSessions.
                .FirstOrDefaultAsync(td => td.Id == id);

            // Sprawdza, czy szczegół treningu został znaleziony.
            if (trainingDetail == null) return NotFound();

            // Sprawdza, czy powiązana sesja należy do zalogowanego użytkownika.
            if (trainingDetail.TrainingSession?.UserId != userId)
            {
                return Forbid(); // Zwraca błąd Forbid (403), jeśli użytkownik nie jest właścicielem.
            }

            // Ładuje listę ćwiczeń do ViewBag dla listy rozwijanej.
            await PopulateExercisesDropDownList(trainingDetail.ExerciseId);
            // Zwraca widok formularza edycji z danymi szczegółu treningu.
            return View(trainingDetail);
        }

        // Akcja POST: Przetwarza dane z formularza edycji szczegółu treningu.
        [HttpPost]
        [ValidateAntiForgeryToken] // Zabezpieczenie przed CSRF.
        // Określa, które pola modelu mają być powiązane z danymi z żądania.
        public async Task<IActionResult> Edit(int id, [Bind("Id,TrainingSessionId,ExerciseId,Load,Sets,Repetitions")] TrainingDetail trainingDetail)
        {
            // Sprawdza, czy ID z trasy zgadza się z ID w modelu.
            if (id != trainingDetail.Id) return NotFound();

            // Pobiera ID zalogowanego użytkownika.
            var userId = _userManager.GetUserId(User);

            // Pobiera oryginalny szczegół treningu z bazy (bez śledzenia) w celu weryfikacji właściciela.
            var originalDetail = await _context.TrainingDetails
                                         .Include(td => td.TrainingSession)
                                         .AsNoTracking() // Wyłącza śledzenie zmian dla tej encji.
                                         .FirstOrDefaultAsync(td => td.Id == id);

            // Sprawdza, czy oryginalny szczegół istnieje.
            if (originalDetail == null) return NotFound();

            // Sprawdza, czy oryginalny szczegół należał do sesji zalogowanego użytkownika.
            if (originalDetail.TrainingSession?.UserId != userId)
            {
                return Forbid();
            }
            // Sprawdza, czy ID sesji nie zostało zmienione na sesję innego użytkownika.
            if (originalDetail.TrainingSessionId != trainingDetail.TrainingSessionId)
            {
                 // Sprawdza, czy nowa sesja (jeśli zmiana jest dozwolona) należy do użytkownika.
                 var newSessionBelongsToUser = await _context.TrainingSessions
                                                        .AnyAsync(ts => ts.Id == trainingDetail.TrainingSessionId && ts.UserId == userId);
                 if (!newSessionBelongsToUser) return Forbid();
            }

            // Usuwa potencjalne błędy walidacji dla właściwości nawigacyjnych.
            ModelState.Remove("TrainingSession");
            ModelState.Remove("Exercise");

            // Sprawdza poprawność danych modelu.
            if (ModelState.IsValid)
            {
                try
                {
                    // Oznacza encję jako zmodyfikowaną.
                    _context.Update(trainingDetail);
                    // Zapisuje zmiany w bazie danych.
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Obsługa konfliktu współbieżności.
                    // Sprawdza, czy szczegół nadal istnieje i należy do sesji użytkownika.
                    if (!await TrainingDetailExistsAndBelongsToUserSession(trainingDetail.Id, userId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; // Rzuca wyjątek dalej.
                    }
                }
                // Przekierowuje do widoku szczegółów nadrzędnej sesji treningowej.
                return RedirectToAction("Details", "TrainingSessions", new { id = trainingDetail.TrainingSessionId });
            }

            // Jeśli dane są nieprawidłowe, ponownie ładuje listę ćwiczeń.
            await PopulateExercisesDropDownList(trainingDetail.ExerciseId);
            // Zwraca widok formularza edycji z błędami walidacji.
            return View(trainingDetail);
        }

        // Akcja GET: Wyświetla stronę potwierdzenia usunięcia szczegółu treningu.
        public async Task<IActionResult> Delete(int? id)
        {
             // Sprawdza, czy ID zostało podane.
             if (id == null) return NotFound();

            // Pobiera ID zalogowanego użytkownika.
            var userId = _userManager.GetUserId(User);

            // Pobiera szczegół treningu z bazy, dołączając powiązaną sesję i ćwiczenie.
            var trainingDetail = await _context.TrainingDetails
                .Include(td => td.TrainingSession) // Dołącza dane z tabeli TrainingSessions.
                .Include(td => td.Exercise)      // Dołącza dane z tabeli Exercises.
                .FirstOrDefaultAsync(td => td.Id == id);

            // Sprawdza, czy szczegół treningu został znaleziony.
            if (trainingDetail == null) return NotFound();

            // Sprawdza, czy powiązana sesja należy do zalogowanego użytkownika.
            if (trainingDetail.TrainingSession?.UserId != userId)
            {
                return Forbid(); // Zwraca błąd Forbid (403), jeśli użytkownik nie jest właścicielem.
            }

            // Zwraca widok potwierdzenia usunięcia.
            return View(trainingDetail);
        }

        // Akcja POST: Potwierdza i wykonuje usunięcie szczegółu treningu.
        [HttpPost, ActionName("Delete")] // Używa aliasu "Delete" dla tej akcji.
        [ValidateAntiForgeryToken] // Zabezpieczenie przed CSRF.
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Pobiera ID zalogowanego użytkownika.
            var userId = _userManager.GetUserId(User);

            // Pobiera szczegół treningu z bazy, dołączając powiązaną sesję w celu weryfikacji właściciela.
             var trainingDetail = await _context.TrainingDetails
                .Include(td => td.TrainingSession) // Dołącza dane z tabeli TrainingSessions.
                .FirstOrDefaultAsync(td => td.Id == id);

            // Sprawdza, czy szczegół treningu istnieje.
            if (trainingDetail == null)
            {
                 // Jeśli nie istnieje, przekierowuje do listy sesji.
                 return RedirectToAction("Index", "TrainingSessions");
            }

            // Sprawdza, czy powiązana sesja należy do zalogowanego użytkownika.
            if (trainingDetail.TrainingSession?.UserId != userId)
            {
                return Forbid(); // Zwraca błąd Forbid (403), jeśli użytkownik nie jest właścicielem.
            }

            // Zapisuje ID sesji przed usunięciem obiektu, aby móc przekierować.
            var trainingSessionId = trainingDetail.TrainingSessionId;

            // Usuwa szczegół treningu z kontekstu.
            _context.TrainingDetails.Remove(trainingDetail);
            // Zapisuje zmiany w bazie danych.
            await _context.SaveChangesAsync();

            // Przekierowuje do widoku szczegółów nadrzędnej sesji treningowej.
            return RedirectToAction("Details", "TrainingSessions", new { id = trainingSessionId });
        }

        // Metoda pomocnicza do ładowania listy ćwiczeń do ViewBag.
        private async Task PopulateExercisesDropDownList(object? selectedExercise = null)
        {
            // Tworzy zapytanie pobierające posortowane ćwiczenia.
            var exercisesQuery = from e in _context.Exercises
                                 orderby e.Name
                                 select e;
            // Tworzy SelectList i przypisuje ją do ViewBag.ExerciseId.
            ViewBag.ExerciseId = new SelectList(await exercisesQuery.AsNoTracking().ToListAsync(), "Id", "Name", selectedExercise);
        }

        // Metoda pomocnicza sprawdzająca, czy szczegół treningu istnieje i należy do sesji danego użytkownika.
        private async Task<bool> TrainingDetailExistsAndBelongsToUserSession(int id, string userId)
        {
            // Sprawdza istnienie szczegółu o podanym ID, którego sesja należy do użytkownika o podanym userId.
            return await _context.TrainingDetails
                                 .Include(td => td.TrainingSession)
                                 .AnyAsync(e => e.Id == id && e.TrainingSession.UserId == userId);
        }

         // Metoda pomocnicza sprawdzająca istnienie szczegółu treningu o podanym ID.
        private bool TrainingDetailExists(int id)
        {
            // Sprawdza, czy w bazie danych istnieje szczegół treningu o podanym ID.
            return _context.TrainingDetails.Any(e => e.Id == id);
        }
    }
}
