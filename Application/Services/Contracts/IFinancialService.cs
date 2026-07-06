using Domain.Entities.Filters.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;

namespace Application.Services.Contracts
{
    public interface IFinancialService
    {
        Task<FinancialTransactionsResponse> CreateTransactionAsync(FinancialTransactionsRequest request, string dbName);
        Task<IEnumerable<FinancialTransactionsResponse>> GetJournalReportAsync(FinancialTransactionsFilter filter, string dbName);
        Task<FinancialReportResponse> GetBalanceSheetAsync(DateTime? startDate, DateTime? endDate, string dbName);
        Task<FinancialReportResponse> GetProfitLossAsync(DateTime? startDate, DateTime? endDate, string dbName);
        Task<int> SyncHistoricalDataAsync(string dbName);
        Task CreateIncomeJournalAsync(string dbName, string referenceNo, double amount, string description, string paymentMethodName = null, string incomeAccountCode = null);
        Task CreateExpenseJournalAsync(string dbName, ExpenseRequest request);
    }
}
