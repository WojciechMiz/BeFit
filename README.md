# Projekt BeFit

BeFit to aplikacja ASP.NET Core zaprojektowana, aby pomagać użytkownikom śledzić ich aktywności fitness, w tym ćwiczenia i sesje treningowe. Aplikacja zapewnia przyjazny interfejs do zarządzania typami ćwiczeń, sesjami treningowymi oraz szczegółowymi statystykami opartymi na aktywnościach użytkownika.

## Funkcjonalności

-   **Zarządzanie Typami Ćwiczeń**: Użytkownicy mogą przeglądać typy ćwiczeń. Tylko administratorzy mają uprawnienia do tworzenia, edycji i usuwania typów ćwiczeń.
-   **Zarządzanie Sesjami Treningowymi**: Zarejestrowani użytkownicy mogą tworzyć, przeglądać, edytować i usuwać *własne* sesje treningowe. Użytkownicy nie mają dostępu do sesji treningowych utworzonych przez innych.
-   **Szczegóły Treningu**: Użytkownicy mogą powiązać wykonane ćwiczenia ze swoimi sesjami treningowymi, określając szczegóły takie jak użyte obciążenie, liczba serii i liczba powtórzeń. Dostęp do tych szczegółów jest ograniczony do właściciela sesji.
-   **Statystyki Użytkownika**: Aplikacja udostępnia widok statystyk dla zalogowanego użytkownika, pokazujący jego aktywności z ostatnich czterech tygodni. Dla każdego typu ćwiczenia prezentowana jest liczba jego wykonań, łączna liczba powtórzeń, średnie oraz maksymalne użyte obciążenie.

## Modele Danych

1.  **Exercise** (`Exercise.cs`): Reprezentuje typ ćwiczenia (np. "Wyciskanie sztangi", "Przysiady"). Zawiera co najmniej nazwę ćwiczenia.
2.  **TrainingSession** (`TrainingSession.cs`): Reprezentuje pojedynczą sesję treningową. Zawiera datę i czas rozpoczęcia oraz zakończenia sesji, a także powiązanie z użytkownikiem (`UserId`).
3.  **TrainingDetail** (`TrainingDetail.cs`): Łączy typ ćwiczenia (`Exercise`) z sesją treningową (`TrainingSession`). Zawiera informacje o wykonaniu danego ćwiczenia w ramach sesji: użyte obciążenie (`Load`), liczbę serii (`Sets`) i liczbę powtórzeń w serii (`Repetitions`).

## Kontrolery

-   **ExercisesController**: Zarządza operacjami CRUD dla typów ćwiczeń (`Exercise`) z kontrolą dostępu opartą na rolach (Administrator dla CUD, dostęp anonimowy dla R).
-   **TrainingSessionsController**: Zarządza operacjami CRUD dla sesji treningowych (`TrainingSession`), zapewniając, że użytkownicy mogą zarządzać tylko własnymi sesjami.
-   **TrainingDetailsController**: Zarządza operacjami CRUD dla szczegółów treningu (`TrainingDetail`), zapewniając dostęp tylko właścicielowi powiązanej sesji.
-   **UserStatisticsController**: Odpowiada za pobieranie i przygotowywanie danych statystycznych dla zalogowanego użytkownika.

## Wymagania Wstępne

-   Zainstalowany .NET SDK (wersja zgodna z projektem, np. .NET 6.0 lub nowsza).
-   Dostęp do instancji SQL Server (np. LocalDB, SQL Server Express).

## Konfiguracja i Uruchomienie

1.  Sklonuj repozytorium: `git clone <adres-repozytorium>`
2.  Przejdź do katalogu projektu: `cd BeFit`
3.  Przywróć zależności projektu: `dotnet restore`
4.  Zaktualizuj parametr połączenia (`ConnectionString`) w pliku `appsettings.json`, aby wskazywał na Twoją bazę danych SQL Server. Upewnij się, że nazwa bazy danych (`Database=BeFit`) jest poprawna.
5.  Zastosuj migracje bazy danych (utworzy to bazę danych i tabele, jeśli nie istnieją): `dotnet ef database update`
6.  Uruchom aplikację: `dotnet run`
7.  Otwórz przeglądarkę i przejdź pod adres wskazany w konsoli (zazwyczaj `https://localhost:xxxx` i `http://localhost:yyyy`).

## Pierwsze Uruchomienie - Rola Administratora

Aby móc zarządzać typami ćwiczeń, jeden z użytkowników musi mieć przypisaną rolę "Administrator". Aplikacja w obecnej formie nie zawiera wbudowanego mechanizmu do automatycznego tworzenia tej roli ani przypisywania jej. Należy to zrobić ręcznie, np. poprzez bezpośrednią modyfikację bazy danych lub dodając tymczasowy kod inicjalizujący role i użytkowników przy starcie aplikacji (np. w `Program.cs`).

## Wkład w Projekt

Wszelkie kontrybucje są mile widziane! Jeśli masz sugestie lub chcesz wprowadzić ulepszenia, prosimy o utworzenie zgłoszenia (issue) lub przesłanie pull requesta.

## Licencja

Ten projekt jest udostępniany na licencji MIT. Zobacz plik `LICENSE`, aby uzyskać więcej szczegółów.
