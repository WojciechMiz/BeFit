using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BeFit.Data; // Przestrzeń nazw dla ApplicationDbContext

var builder = WebApplication.CreateBuilder(args);

// --- Rejestracja serwisów w kontenerze DI ---

// Rejestracja ApplicationDbContext do użycia z Entity Framework Core i SQL Server.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Dodaje stronę błędów związanych z migracjami bazy danych (dla developmentu).
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Konfiguracja systemu tożsamości (Identity) z domyślnym użytkownikiem (IdentityUser).
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        // Ustawienie, czy konto musi być potwierdzone.
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>() // Dodaje obsługę ról do Identity.
    .AddEntityFrameworkStores<ApplicationDbContext>(); // Określa magazyn danych dla Identity.

// Dodaje usługi wymagane dla aplikacji MVC (kontrolery i widoki).
builder.Services.AddControllersWithViews();

// --- Budowanie aplikacji ---
var app = builder.Build();

// --- Konfiguracja potoku przetwarzania żądań HTTP ---

// Konfiguracja dla środowiska deweloperskiego.
if (app.Environment.IsDevelopment())
{
    // Umożliwia zarządzanie migracjami EF Core przez przeglądarkę.
    app.UseMigrationsEndPoint();
}
// Konfiguracja dla środowiska innego niż deweloperskie (np. produkcyjnego).
else
{
    // Ustawia globalną obsługę wyjątków.
    app.UseExceptionHandler("/Home/Error");
    // Dodaje nagłówek HSTS wymuszający HTTPS.
    app.UseHsts();
}

// Przekierowuje żądania HTTP na HTTPS.
app.UseHttpsRedirection();
// Umożliwia serwowanie plików statycznych (np. CSS, JS).
app.UseStaticFiles();

// Dodaje middleware routingu do potoku.
app.UseRouting();

// Dodaje middleware uwierzytelniania (wymagane dla Identity).
app.UseAuthentication();
// Dodaje middleware autoryzacji (wymagane dla atrybutów [Authorize]).
app.UseAuthorization();

// Mapuje domyślną trasę dla kontrolerów MVC.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Mapuje strony Razor Pages (używane np. przez domyślny interfejs Identity).
app.MapRazorPages();

// --- Uruchomienie aplikacji ---
app.Run();
