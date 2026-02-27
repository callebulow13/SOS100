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

    public LoansController(LoanDbContext db)
    {
        _db = db;
    }

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

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var alreadyActive = await _db.Loans
            .AnyAsync(l =>
                l.ItemId == req.ItemId &&
                l.Status == LoanStatus.Active,
                ct);

        if (alreadyActive)
            return Conflict(new { message = "Objektet är redan utlånat." });

        var now = DateTimeOffset.UtcNow;

        var loan = new Loan
        {
            ItemId = req.ItemId,
            BorrowerId = req.BorrowerId,
            LoanedAt = DateTimeOffset.UtcNow,
            DueAt = DateTimeOffset.UtcNow.AddDays(req.LoanDays),
            Status = LoanStatus.Active,

            // 🔒 VATTENTÄT REGEL
            ActiveItemKey = req.ItemId
        };

        _db.Loans.Add(loan);

        try
        {
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Detta händer t.ex. när unique index på ActiveItemKey triggas
            return Conflict(new { message = "Objektet är redan utlånat." });
        }

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
        [FromQuery] string? itemId,
        CancellationToken ct)
    {
        var query = _db.Loans.AsQueryable();

        if (status is not null)
            query = query.Where(l => l.Status == status);

        if (!string.IsNullOrWhiteSpace(itemId))
            query = query.Where(l => l.ItemId == itemId);

        var result = await query
            .OrderByDescending(l => l.LoanedAt)
            .ToListAsync(ct);

        return Ok(result);
    }
}