using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOS100_LoanApi.Dtos;
using SOS100_LoanAPI.Data;
using SOS100_LoanAPI.Domain;
using SOS100_LoanAPI.Infrastructure;

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

        
        // Frågar Katalogen om prylen finns och är ledig
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

        // KatalogAPI:s befintliga kod för att spara i sin egen databas börjar här...
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var alreadyActive = await _db.Loans
            .AnyAsync(l => l.ItemId == req.ItemId && l.ReturnedAt == null, ct);

        if (alreadyActive)
            return Conflict(new { message = "Objektet är redan utlånat lokalt." });
        

        var loan = new Loan
        {
            ItemId = req.ItemId,
            BorrowerId = req.BorrowerId,
            LoanedAt = DateTimeOffset.UtcNow,
            DueAt = DateTimeOffset.UtcNow.AddDays(req.LoanDays),
            ItemName = pryl.Name,
        };

        _db.Loans.Add(loan);
        
        // Uppdaterar eller skapa statistikrad
        var stat = await _db.LoanUserItemStats
            .FirstOrDefaultAsync(s =>
                s.BorrowerId == req.BorrowerId &&
                s.ItemId == req.ItemId, ct);

        if (stat == null)
        {
            stat = new LoanUserItemStat
            {
                BorrowerId = req.BorrowerId,
                ItemId = req.ItemId,
                ItemName = pryl.Name,
                TotalLoans = 1,
                LateReturns = 0
            };

            _db.LoanUserItemStats.Add(stat);
        }
        else
        {
            stat.TotalLoans += 1;
            stat.ItemName = pryl.Name;
        }

        try
        {
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.IsSqliteUniqueConstraintViolation())
        {
            await tx.RollbackAsync(ct);

            // Detta är "förväntade" fel: någon försöker skapa ett aktivt lån
            // fast item redan har ett aktivt lån. DB-indexet stoppar det.
            return Conflict(new
            {
                message = "Det finns redan ett aktivt lån för detta item.",
                code = "ACTIVE_LOAN_EXISTS"
            });
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync(ct);

            var realError = ex.InnerException?.Message ?? ex.Message;

            Console.WriteLine("\n--- DB UPDATE ERROR ---");
            Console.WriteLine(realError);
            Console.WriteLine(ex.ToString());
            Console.WriteLine("----------------------\n");

            return Conflict(new
            {
                message = "Ett databasfel uppstod!",
                detaljer = realError
            });
        }
        
        // Ändrar statusen på kopian vi hämtade till 1 (eller vad Utlånad motsvarar i er enum)
        pryl.Status = 1; 

        // Skickar tillbaka den med en PUT-request till din uppdateringsmetod
        var updateResponse = await catalogClient.PutAsJsonAsync($"/api/items/{pryl.Id}", pryl, ct);

        if (!updateResponse.IsSuccessStatusCode)
        {
            // Läser det exakta felmeddelandet från KatalogApi
            var errorText = await updateResponse.Content.ReadAsStringAsync();
            
            // Logga det tydligt i kompisens terminal
            Console.WriteLine($"\n--- FEL VID PUT TILL KATALOG ---");
            Console.WriteLine($"Statuskod: {updateResponse.StatusCode}");
            Console.WriteLine($"Felmeddelande: {errorText}");
            Console.WriteLine($"--------------------------------\n");
            
            // Tills felet är löst, avbryts lånet om katalogen inte kan uppdateras!
            return StatusCode(500, $"Lånet kunde inte slutföras eftersom Katalog-API vägrade uppdatera. Orsak: {errorText}");
        }
        // =========================================================
        try
        {
            var reminderClient = _httpClientFactory.CreateClient("ReminderApi");

            var reminderResponse = await reminderClient.PostAsJsonAsync("/api/reminders", new
            {
                userId = req.BorrowerId,
                itemId = req.ItemId,
                loanId = loan.Id.ToString(),
                itemTitle = pryl.Name,
                dueDate = loan.DueAt.UtcDateTime,
                isSent = false
            }, ct);

            if (!reminderResponse.IsSuccessStatusCode)
            {
                var reminderError = await reminderResponse.Content.ReadAsStringAsync(ct);

                Console.WriteLine("\n--- FEL VID POST TILL REMINDER API ---");
                Console.WriteLine($"Statuskod: {reminderResponse.StatusCode}");
                Console.WriteLine($"Felmeddelande: {reminderError}");
                Console.WriteLine("--------------------------------------\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ReminderApi nere för lån {loan.Id}: {ex.Message}");
        }

        return CreatedAtAction(
            nameof(GetLoanById),
            new { loanId = loan.Id },
            loan);
    }
    
    // GET: api/loans/stats
// Hämtar statistik till React-sidan från tabellen LoanUserItemStats
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var stats = await _db.LoanUserItemStats
            .Select(s => new
            {
                id = s.Id,
                itemId = s.ItemId,
                itemName = s.ItemName,
                borrowerId = s.BorrowerId,
                totalLoans = s.TotalLoans,
                lateReturns = s.LateReturns
            })
            .ToListAsync(ct);

        return Ok(stats);
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

        return await CompleteReturnAsync(loan, ct);
    }
   
    
    // POST: api/loans/return-by-item/{itemId}
    [HttpPost("return-by-item/{itemId:int}")]
    public async Task<IActionResult> ReturnLoanByItemId(int itemId, CancellationToken ct)
    {
        var loan = await _db.Loans
            .FirstOrDefaultAsync(l => l.ItemId == itemId && l.ReturnedAt == null, ct);

        if (loan is null)
            return NotFound(new { message = "Det finns inget aktivt lån för detta item." });

        return await CompleteReturnAsync(loan, ct);
    }



    // GET: api/loans/active-by-item/{itemId}
    [HttpGet("active-by-item/{itemId:int}")]
    public async Task<IActionResult> GetActiveLoanByItemId(int itemId, CancellationToken ct)
    {
        var loan = await _db.Loans
            .FirstOrDefaultAsync(l => l.ItemId == itemId && l.ReturnedAt == null, ct);

        if (loan is null)
            return NotFound(new { message = "Det finns inget aktivt lån för detta item." });

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

        if (status is not null)
        {
            query = status switch
            {
                LoanStatus.Active => query.Where(l => l.ReturnedAt == null),
                LoanStatus.Returned => query.Where(l => l.ReturnedAt != null),
                _ => query
            };
        }
        
        if (itemId is not null)
            query = query.Where(l => l.ItemId == itemId.Value);

        // 1. Hämtar datan från SQLite först (utan sortering)
        var result = await query.ToListAsync(ct);

        // 2. Sorterar listan i minnet istället (C# klarar DateTimeOffset galant!)
        var sortedResult = result
            .OrderByDescending(l => l.LoanedAt)
            .ToList();

        return Ok(sortedResult);
    }
    
    private async Task<IActionResult> CompleteReturnAsync(Loan loan, CancellationToken ct)
{
    if (loan.ReturnedAt is not null)
        return Conflict(new { message = "Lånet är inte aktivt." });

    loan.ReturnedAt = DateTimeOffset.UtcNow;
    
    // Registrerar sen återlämning i statistik
    if (loan.ReturnedAt > loan.DueAt)
    {
        var stat = await _db.LoanUserItemStats
            .FirstOrDefaultAsync(s =>
                s.BorrowerId == loan.BorrowerId &&
                s.ItemId == loan.ItemId, ct);

        if (stat != null)
        {
            stat.LateReturns += 1;
        }
    }

    var catalogClient = _httpClientFactory.CreateClient("KatalogClient");

    var itemResponse = await catalogClient.GetAsync($"/api/items/{loan.ItemId}", ct);

    if (itemResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        return StatusCode(502, new { message = "LoanAPI kunde inte autentisera mot KatalogAPI vid återlämning." });

    if (itemResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
        return NotFound(new { message = "Itemet finns inte i katalogen." });

    if (!itemResponse.IsSuccessStatusCode)
        return StatusCode(502, new { message = $"KatalogAPI fel vid hämtning: {(int)itemResponse.StatusCode} {itemResponse.StatusCode}" });

    var pryl = await itemResponse.Content.ReadFromJsonAsync<ItemDto>(cancellationToken: ct);

    if (pryl == null)
        return StatusCode(502, new { message = "LoanAPI kunde inte läsa item från KatalogAPI." });

    pryl.Status = 0;

    var updateResponse = await catalogClient.PutAsJsonAsync($"/api/items/{pryl.Id}", pryl, ct);

    if (updateResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        return StatusCode(502, new { message = "LoanAPI kunde inte autentisera mot KatalogAPI vid statusuppdatering." });

    if (!updateResponse.IsSuccessStatusCode)
    {
        var errorText = await updateResponse.Content.ReadAsStringAsync(ct);

        return StatusCode(502, new
        {
            message = $"KatalogAPI fel vid uppdatering: {(int)updateResponse.StatusCode} {updateResponse.StatusCode}",
            details = errorText
        });
    }

    try
    {
        await _db.SaveChangesAsync(ct);
    }
    catch (DbUpdateException)
    {
        return Problem("Databasfel vid återlämning.", statusCode: 500);
    }
// Tar bort påminnelse från ReminderApi när lånet återlämnas
    // =========================================================
    try
    {
        var reminderClient = _httpClientFactory.CreateClient("ReminderApi");
        
        var remindersResponse = await reminderClient
            .GetAsync($"/api/reminders?userId={loan.BorrowerId}", ct);
        
        if (remindersResponse.IsSuccessStatusCode)
        {
            var reminders = await remindersResponse.Content
                .ReadFromJsonAsync<List<ReminderDto>>(cancellationToken: ct);
            
            var reminder = reminders?
                .FirstOrDefault(r => r.LoanId == loan.Id.ToString());

            if (reminder != null)
            {
                var updated = new
                {
                    isSent    = true,
                    dueDate   = reminder.DueDate,
                    itemTitle = reminder.ItemTitle
                };
                var putContent = JsonContent.Create(updated);
                await reminderClient.PutAsync(
                    $"/api/reminders/{reminder.Id}", putContent, ct);
                Console.WriteLine($"✅ Reminder {reminder.Id} markerad som återlämnad!");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Kunde inte ta bort påminnelse: {ex.Message}");
    }
    var response = new ReturnLoanResponse
    {
        LoanId = loan.Id,
        ItemId = loan.ItemId,
        ReturnedAt = loan.ReturnedAt!.Value,
        Message = "Lånet är återlämnat."
    };

    return Ok(response);
}
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
    // PUT: api/loans/{loanId}/due-date
    // Låter en admin ändra förfallodatumet på ett lån
    [HttpPut("{loanId:guid}/due-date")]
    public async Task<IActionResult> UpdateLoanDueDate(Guid loanId, [FromBody] DateTime newDueDate, CancellationToken ct)
    {
        var loan = await _db.Loans.FindAsync(new object[] { loanId }, ct);
        
        if (loan == null)
            return NotFound(new { message = "Lånet hittades inte." });

        // Uppdatera till det nya datumet
        loan.DueAt = new DateTimeOffset(newDueDate, TimeSpan.Zero);
        
        await _db.SaveChangesAsync(ct);
        
        return Ok(loan);
    }

    // DELETE: api/loans/{loanId}
    // Raderar ett lån helt från databasen
    [HttpDelete("{loanId:guid}")]
    public async Task<IActionResult> DeleteLoan(Guid loanId, CancellationToken ct)
    {
        var loan = await _db.Loans.FindAsync(new object[] { loanId }, ct);
        
        if (loan == null)
            return NotFound(new { message = "Lånet hittades inte." });

        // VIKTIGT: Om lånet fortfarande är aktivt, måste vi släppa prylen fri i katalogen!
        if (loan.ReturnedAt == null)
        {
            var catalogClient = _httpClientFactory.CreateClient("KatalogClient");
            var itemResponse = await catalogClient.GetAsync($"/api/items/{loan.ItemId}", ct);
            
            if (itemResponse.IsSuccessStatusCode)
            {
                var pryl = await itemResponse.Content.ReadFromJsonAsync<ItemDto>(cancellationToken: ct);
                if (pryl != null)
                {
                    pryl.Status = 0; // 0 = Tillgänglig
                    await catalogClient.PutAsJsonAsync($"/api/items/{pryl.Id}", pryl, ct);
                }
            }
        }

        _db.Loans.Remove(loan);
        await _db.SaveChangesAsync(ct);
        
        return NoContent();
    }
} // Här slutar hela LoansController-klassen!
public record ReminderDto(
    int Id,
    string LoanId,
    string UserId,
    string ItemTitle,
    DateTime DueDate
);
