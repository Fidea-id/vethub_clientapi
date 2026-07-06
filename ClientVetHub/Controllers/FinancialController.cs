using Application.Services.Contracts;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClientVetHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class FinancialController : ControllerBase
    {
        private readonly IFinancialService _financialService;

        public FinancialController(IFinancialService financialService)
        {
            _financialService = financialService;
        }

        [HttpPost("Transaction")]
        public async Task<IActionResult> CreateTransaction([FromBody] FinancialTransactionsRequest request)
        {
            var dbName = User.FindFirstValue("Entity");
            var result = await _financialService.CreateTransactionAsync(request, dbName);
            return Ok(result);
        }

        [HttpGet("Journal")]
        public async Task<IActionResult> GetJournal([FromQuery] FinancialTransactionsFilter filter)
        {
            var dbName = User.FindFirstValue("Entity");
            var result = await _financialService.GetJournalReportAsync(filter, dbName);
            return Ok(result);
        }

        [HttpGet("BalanceSheet")]
        public async Task<IActionResult> GetBalanceSheet([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var dbName = User.FindFirstValue("Entity");
            var result = await _financialService.GetBalanceSheetAsync(startDate, endDate, dbName);
            return Ok(result);
        }

        [HttpGet("ProfitLoss")]
        public async Task<IActionResult> GetProfitLoss([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var dbName = User.FindFirstValue("Entity");
            var result = await _financialService.GetProfitLossAsync(startDate, endDate, dbName);
            return Ok(result);
        }

        [HttpPost("SyncHistorical")]
        public async Task<IActionResult> SyncHistorical()
        {
            var dbName = User.FindFirstValue("Entity");
            var count = await _financialService.SyncHistoricalDataAsync(dbName);
            return Ok(new { Message = $"Successfully synced {count} records.", Count = count });
        }

        [HttpPost("Expense")]
        public async Task<IActionResult> CreateExpense([FromBody] ExpenseRequest request)
        {
            var dbName = User.FindFirstValue("Entity");
            await _financialService.CreateExpenseJournalAsync(dbName, request);
            return Ok(new { Message = "Expense recorded successfully." });
        }
    }
}
