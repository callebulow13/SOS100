using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOS100_LoanApi.Dtos;
using SOS100_LoansApi.Data;
using SOS100_LoansApi.Domain;

namespace SOS100_LoanAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoansController : ControllerBase
{
    private readonly LoanDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;

    public LoansController(LoanDbContext db, IHttpClientFactory httpClientFactory)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
    }

    // POST: api/loans
   // POST: api/loans
    [HttpPost]
    public async Task<IActionResult> CreateLoan(
        [FromBody] CreateLoanRequest req,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (string.IsNullOrWhiteSpace(req.BorrowerId))
            return BadRequest(new { message = "BorrowerId måste anges (tills auth är på plats)." });

        // =========================================================
        // NYTT STEG 1: Fråga Katalogen om prylen finns och är ledig
        // =========================================================
        var catalogClient = _httpClientFactory.CreateClient("KatalogClient");
        
        // Hämta prylen från ditt API
        var itemResponse = await catalogClient.GetAsync($"/api/items/{req.ItemId}", ct);
        
        if (itemResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return StatusCode(502, new { message = "LoanAPI kunde inte autentisera mot KatalogAPI (fel/saknad X-Api-Key)." });

        if (itemResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            return NotFound(new { message = "Prylen finns inte i katalogen. Kontrollera ID." });

        if (!itemResponse.IsSuccessStatusCode)
            return StatusCode(502, new { message = $"KatalogAPI fel: {(int)itemResponse.StatusCode} {itemResponse.StatusCode}" });

        var pryl = await itemResponse.Content.ReadFromJsonAsync<ItemDto>(cancellationToken: ct);

        // Om prylen inte har status 0 (Tillgänglig), avbryt!
        if (pryl == null || pryl.Status != 0)
        {
            return Conflict(new { message = "Prylen är tyvärr redan utlånad, saknas eller är trasig i katalogen." });
        }
        // =========================================================

        // Kompisens befintliga kod för att spara i sin egen databas börjar här...
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var alreadyActive = await _db.Loans
            .AnyAsync(l => l.ItemId == req.ItemId && l.Status == LoanStatus.Active, ct);

        if (alreadyActive)
            return Conflict(new { message = "Objektet är redan utlånat lokalt." });

        var loan = new Loan
        {
            ItemId = req.ItemId,
            BorrowerId = req.BorrowerId,
            LoanedAt = DateTimeOffset.UtcNow,
            DueAt = DateTimeOffset.UtcNow.AddDays(req.LoanDays),
            Status = LoanStatus.Active,
            ActiveItemKey = req.ItemId
        };

        _db.Loans.Add(loan);

        try
        {
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            var realError = ex.InnerException?.Message ?? ex.Message;

            // 1) Skriv ut till LoanAPI-konsolen (Run/Debug)
            Console.WriteLine("\n--- DB UPDATE ERROR ---");
            Console.WriteLine(realError);
            Console.WriteLine(ex.ToString());
            Console.WriteLine("-----------------------\n");

            // 2) Skicka tillbaka detaljer så MVC kan visa dem
            return Conflict(new
            {
                message = "Ett databasfel uppstod!",
                detaljer = realError
            });
        }

        // =========================================================
        // NYTT STEG 3: Säg till Katalogen att ändra status till Utlånad
        // =========================================================
        
        // Ändra statusen på kopian vi hämtade till 1 (eller vad Utlånad motsvarar i er enum)
        pryl.Status = 1; 

        // Skicka tillbaka den med en PUT-request till din uppdateringsmetod
        var updateResponse = await catalogClient.PutAsJsonAsync($"/api/items/{pryl.Id}", pryl, ct);

        if (!updateResponse.IsSuccessStatusCode)
        {
            // Läs det exakta felmeddelandet från ditt KatalogApi
            var errorText = await updateResponse.Content.ReadAsStringAsync();
            
            // Logga det tydligt i kompisens terminal
            Console.WriteLine($"\n--- FEL VID PUT TILL KATALOG ---");
            Console.WriteLine($"Statuskod: {updateResponse.StatusCode}");
            Console.WriteLine($"Felmeddelande: {errorText}");
            Console.WriteLine($"--------------------------------\n");
            
            // Tills vi har löst felet, avbryter vi lånet om katalogen inte kan uppdateras!
            return StatusCode(500, $"Lånet kunde inte slutföras eftersom Katalog-API vägrade uppdatera. Orsak: {errorText}");
        }
        // =========================================================

        return CreatedAtAction(
            nameof(GetLoanById),
            new { loanId = loan.Id },
            loan);
    }

    // GET: api/loans/{loanId}
    [HttpGet("{loanId:guid}")]
    public async Task<IActionResult> GetLoanById(
        Guid loanId,
        CancellationToken ct)
    {
        var loan = await _db.Loans
            .FirstOrDefaultAsync(l => l.Id == loanId, ct);

        if (loan is null)
            return NotFound();

        return Ok(loan);
    }

    // POST: api/loans/{loanId}/return
    [HttpPost("{loanId:guid}/return")]
    public async Task<IActionResult> ReturnLoan(Guid loanId, CancellationToken ct)
    {
        var loan = await _db.Loans.FirstOrDefaultAsync(l => l.Id == loanId, ct);
        if (loan is null)
            return NotFound();

        if (loan.Status != LoanStatus.Active)
            return Conflict(new { message = "Lånet är inte aktivt." });

        loan.Status = LoanStatus.Returned;
        loan.ReturnedAt = DateTimeOffset.UtcNow;

        // 🔓 släpper “låset” så item kan lånas igen
        loan.ActiveItemKey = null;

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Problem("Databasfel vid återlämning.", statusCode: 500);
        }

        return Ok(loan);
    }

    // GET: api/loans?status=Active&itemId=...
    [HttpGet]
    public async Task<IActionResult> Query(
        [FromQuery] LoanStatus? status,
        [FromQuery] int? itemId,
        CancellationToken ct)
    {
        var query = _db.Loans.AsQueryable();

        if (status is not null)
            query = query.Where(l => l.Status == status);

         if (itemId.HasValue)
            query = query.Where(l => l.ItemId == itemId);

        var result = await query
            .OrderByDescending(l => l.LoanedAt)
            .ToListAsync(ct);

        return Ok(result);
    }
    // --- HÄR KLISTRAR NI IN DEN NYA METODEN ISTÄLLET ---
    [HttpGet("test-hamta-pryl/{id}")]
    public async Task<IActionResult> TestFetch(int id)
    {
        var client = _httpClientFactory.CreateClient("KatalogClient");

        var pryl = await client.GetFromJsonAsync<ItemDto>($"/api/items/{id}");

        if (pryl == null)
        {
            return NotFound("Kunde inte hämta prylen från KatalogAPI.");
        }

        return Ok($"Hämtade {pryl.Name} som har status {pryl.Status}!");
    }
} // Här slutar hela LoansController-klassen!
