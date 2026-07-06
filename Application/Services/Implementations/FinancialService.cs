using Application.Services.Contracts;
using Application.Utils;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Domain.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Application.Services.Implementations
{
    public class FinancialService : IFinancialService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FinancialService> _logger;
        private readonly IConfiguration _configuration;

        public FinancialService(IUnitOfWork unitOfWork, ILogger<FinancialService> logger, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<FinancialTransactionsResponse> CreateTransactionAsync(FinancialTransactionsRequest request, string dbName)
        {
            // Validate Balanced Entries
            var totalDebit = request.Entries.Sum(x => x.Debit);
            var totalCredit = request.Entries.Sum(x => x.Credit);

            if (Math.Abs(totalDebit - totalCredit) > 0.001)
            {
                throw new Exception("Transaction is not balanced. Debit must equal Credit.");
            }

            // Generate Reference No if empty
            if (string.IsNullOrEmpty(request.ReferenceNo))
            {
                request.ReferenceNo = await GenerateVoucherNumber(dbName, request.TransactionDate);
            }

            var transaction = Mapping.Mapper.Map<FinancialTransactions>(request);
            FormatUtil.SetDateBaseEntity(transaction);
            FormatUtil.SetIsActive(transaction, true);

            var transactionId = await _unitOfWork.FinancialTransactionsRepository.Add(dbName, transaction);

            foreach (var entryRequest in request.Entries)
            {
                var entry = Mapping.Mapper.Map<JournalEntries>(entryRequest);
                entry.TransactionId = transactionId;
                FormatUtil.SetDateBaseEntity(entry);
                FormatUtil.SetIsActive(entry, true);
                await _unitOfWork.JournalEntriesRepository.Add(dbName, entry);
            }

            return Mapping.Mapper.Map<FinancialTransactionsResponse>(transaction);
        }

        public async Task<IEnumerable<FinancialTransactionsResponse>> GetJournalReportAsync(FinancialTransactionsFilter filter, string dbName)
        {
            var transactions = await _unitOfWork.FinancialTransactionsRepository.GetByFilter(dbName, filter);
            var result = Mapping.Mapper.Map<IEnumerable<FinancialTransactionsResponse>>(transactions.Data);

            foreach (var tx in result)
            {
                var entryFilter = new JournalEntriesFilter { TransactionId = tx.Id };
                var entries = await _unitOfWork.JournalEntriesRepository.GetByFilter(dbName, entryFilter);
                tx.Entries = Mapping.Mapper.Map<List<JournalEntriesResponse>>(entries.Data);

                // Populate Account Names
                foreach (var entry in tx.Entries)
                {
                    var account = await _unitOfWork.ChartOfAccountsRepository.GetById(dbName, entry.ChartOfAccountId);
                    if (account != null)
                    {
                        entry.AccountCode = account.Code;
                        entry.AccountName = account.Name;
                    }
                }
            }

            return result;
        }

        public async Task<FinancialReportResponse> GetBalanceSheetAsync(DateTime? startDate, DateTime? endDate, string dbName)
        {
            var start = startDate ?? DateTime.MinValue;
            var end = endDate ?? DateTime.Now;
            if (endDate.HasValue && endDate.Value.TimeOfDay == TimeSpan.Zero)
            {
                end = endDate.Value.Date.AddDays(1).AddTicks(-1);
            }

            var report = new FinancialReportResponse
            {
                Title = "Neraca (Balance Sheet)",
                StartDate = start,
                EndDate = end
            };

            var accounts = await _unitOfWork.ChartOfAccountsRepository.GetAll(dbName);
            var entries = await _unitOfWork.JournalEntriesRepository.GetAll(dbName);

            var groups = new List<string> { "Asset", "Liability", "Equity" };
            foreach (var groupName in groups)
            {
                var groupResponse = new FinancialReportGroupResponse { GroupName = groupName };
                var groupAccounts = accounts.Where(x => x.Type == groupName).ToList();

                foreach (var acc in groupAccounts)
                {
                    // Sum entries for this account in the date range (inclusive)
                    var accEntries = entries.Where(x => x.ChartOfAccountId == acc.Id && x.CreatedAt >= start && x.CreatedAt <= end);
                    var balance = accEntries.Sum(x => x.Debit) - accEntries.Sum(x => x.Credit);

                    // Assets are Debit positive, Liabilities/Equity are Credit positive
                    if (groupName != "Asset") balance = -balance;

                    if (Math.Abs(balance) > 0.001)
                    {
                        groupResponse.Items.Add(new FinancialReportItemResponse
                        {
                            Code = acc.Code,
                            Name = acc.Name,
                            Amount = balance
                        });
                    }
                }
                groupResponse.SubTotal = groupResponse.Items.Sum(x => x.Amount);
                report.Groups.Add(groupResponse);
            }

            report.NetTotal = report.Groups.FirstOrDefault(x => x.GroupName == "Asset")?.SubTotal ?? 0;
            return report;
        }

        public async Task<FinancialReportResponse> GetProfitLossAsync(DateTime? startDate, DateTime? endDate, string dbName)
        {
            var start = startDate ?? new DateTime(DateTime.Now.Year, 1, 1);
            var end = endDate ?? DateTime.Now;
            if (endDate.HasValue && endDate.Value.TimeOfDay == TimeSpan.Zero)
            {
                end = endDate.Value.Date.AddDays(1).AddTicks(-1);
            }

            var report = new FinancialReportResponse
            {
                Title = "Laba Rugi (Profit & Loss)",
                StartDate = start,
                EndDate = end
            };

            var accounts = await _unitOfWork.ChartOfAccountsRepository.GetAll(dbName);
            var entries = await _unitOfWork.JournalEntriesRepository.GetAll(dbName);

            var revenueGroup = new FinancialReportGroupResponse { GroupName = "Revenue" };
            var expenseGroup = new FinancialReportGroupResponse { GroupName = "Expense" };

            foreach (var acc in accounts)
            {
                if (acc.Type != "Revenue" && acc.Type != "Expense") continue;

                var accEntries = entries.Where(x => x.ChartOfAccountId == acc.Id && x.CreatedAt >= start && x.CreatedAt <= end);
                var balance = accEntries.Sum(x => x.Credit) - accEntries.Sum(x => x.Debit);

                if (acc.Type == "Expense") balance = -balance;

                if (Math.Abs(balance) > 0.001)
                {
                    var item = new FinancialReportItemResponse { Code = acc.Code, Name = acc.Name, Amount = balance };
                    if (acc.Type == "Revenue") revenueGroup.Items.Add(item);
                    else expenseGroup.Items.Add(item);
                }
            }

            revenueGroup.SubTotal = revenueGroup.Items.Sum(x => x.Amount);
            expenseGroup.SubTotal = expenseGroup.Items.Sum(x => x.Amount);

            report.Groups.Add(revenueGroup);
            report.Groups.Add(expenseGroup);
            report.NetTotal = revenueGroup.SubTotal - expenseGroup.SubTotal;

            return report;
        }

        public async Task<int> SyncHistoricalDataAsync(string dbName)
        {
            int syncCount = 0;

            // 1. Sync Paid Orders
            var paidOrders = await _unitOfWork.OrdersRepository.WhereQuery(dbName, "Status = 'Paid'");
            foreach (var order in paidOrders)
            {
                var exists = await _unitOfWork.FinancialTransactionsRepository.AnyQuery(dbName, $"Description LIKE '%Order {order.OrderNumber}%'");
                if (!exists)
                {
                    var amount = order.TotalDiscountedPrice > 0 ? order.TotalDiscountedPrice : order.TotalPrice;
                    await CreateIncomeJournalAsync(dbName, order.OrderNumber, amount, $"Historical Sync: Order {order.OrderNumber}");
                    syncCount++;
                }
            }

            // 2. Sync Paid Medical Records
            var paidMedicalRecords = await _unitOfWork.MedicalRecordsRepository.WhereQuery(dbName, "PaymentStatus = 'Paid'");
            foreach (var med in paidMedicalRecords)
            {
                var exists = await _unitOfWork.FinancialTransactionsRepository.AnyQuery(dbName, $"Description LIKE '%Medical Record {med.Code}%'");
                if (!exists)
                {
                    var amount = (med.TotalDiscounted ?? 0) > 0 ? med.TotalDiscounted ?? 0 : med.Total;
                    await CreateIncomeJournalAsync(dbName, med.Code, amount, $"Historical Sync: Medical Record {med.Code}");
                    syncCount++;
                }
            }

            return syncCount;
        }

        // Interface-compatible overload
        public async Task CreateIncomeJournalAsync(string dbName, string referenceNo, double amount, string description)
        {
            await CreateIncomeJournalAsync(dbName, referenceNo, amount, description, null, null);
        }

        public async Task CreateIncomeJournalAsync(string dbName, string referenceNo, double amount, string description, string paymentMethodName = null, string incomeAccountCode = null)
        {
            var clinicIncomeAccount = _configuration["FinanceSettings:ClinicIncomeAccount"] ?? "4-1100";
            var cashAccountCode = _configuration["FinanceSettings:CashAccount"] ?? "1-1100";
            var defaultCashAccount = _configuration["FinanceSettings:DefaultCashAccount"] ?? "1-1300";
            var incomeCode = incomeAccountCode ?? clinicIncomeAccount;

            if (!string.IsNullOrWhiteSpace(paymentMethodName) && paymentMethodName.Contains("cash", StringComparison.OrdinalIgnoreCase))
            {
                defaultCashAccount = cashAccountCode;
            }

            var incomeAcc = await GetAccountIdByCode(dbName, incomeCode);
            var cashAcc = await GetAccountIdByCode(dbName, defaultCashAccount);

            if (incomeAcc == 0 || cashAcc == 0)
            {
                _logger.LogWarning($"Could not find Chart of Accounts for Income ({clinicIncomeAccount}) or Cash ({defaultCashAccount}). Skipping journal entry.");
                return;
            }

            var request = new FinancialTransactionsRequest
            {
                TransactionDate = DateTime.Now,
                ReferenceNo = referenceNo,
                Description = description,
                TransactionType = "Income",
                TotalAmount = amount,
                Entries = new List<JournalEntriesRequest>
                {
                    new JournalEntriesRequest { ChartOfAccountId = cashAcc, Debit = amount, Credit = 0, Description = description },
                    new JournalEntriesRequest { ChartOfAccountId = incomeAcc, Debit = 0, Credit = amount, Description = description }
                }
            };

            await CreateTransactionAsync(request, dbName);
        }

        private async Task<int> GetAccountIdByCode(string dbName, string code)
        {
            var account = await _unitOfWork.ChartOfAccountsRepository.WhereFirstQuery(dbName, $"Code = '{code}'");
            return account?.Id ?? 0;
        }

        private async Task<string> GenerateVoucherNumber(string dbName, DateTime date)
        {
            var prefix = $"JV-{date:yyyyMM}-";
            var count = await _unitOfWork.FinancialTransactionsRepository.CountWithQuery(dbName, $"ReferenceNo LIKE '{prefix}%'");
            return $"{prefix}{(count + 1):D4}";
        }

        public async Task CreateExpenseJournalAsync(string dbName, ExpenseRequest request)
        {
            var expenseAccount = await _unitOfWork.ChartOfAccountsRepository.GetById(dbName, request.ExpenseAccountId);
            var paymentAccount = await _unitOfWork.ChartOfAccountsRepository.GetById(dbName, request.PaymentAccountId);

            if (expenseAccount == null || paymentAccount == null)
            {
                throw new Exception("Invalid account selected.");
            }

            // Create Voucher
            var voucherNo = $"JV-EXP-{DateTime.Now:yyyyMM}-{new Random().Next(1000, 9999)}";
            var transaction = new FinancialTransactions
            {
                VoucherNo = voucherNo,
                TransactionDate = request.TransactionDate,
                Description = request.Description,
                TransactionType = string.IsNullOrWhiteSpace(request.TransactionType) ? "Expense" : request.TransactionType,
                TotalAmount = request.Amount,
                Status = "Posted"
            };

            FormatUtil.SetIsActive(transaction, true);
            FormatUtil.SetDateBaseEntity(transaction);
            var transactionId = await _unitOfWork.FinancialTransactionsRepository.Add(dbName, transaction);

            // 1. Debit Entry (Expense)
            var debitEntry = new JournalEntries
            {
                TransactionId = transactionId,
                ChartOfAccountId = request.ExpenseAccountId,
                Debit = request.Amount,
                Credit = 0,
                Description = request.Description
            };
            FormatUtil.SetIsActive(debitEntry, true);
            FormatUtil.SetDateBaseEntity(debitEntry);
            await _unitOfWork.JournalEntriesRepository.Add(dbName, debitEntry);

            // 2. Credit Entry (Asset/Cash)
            var creditEntry = new JournalEntries
            {
                TransactionId = transactionId,
                ChartOfAccountId = request.PaymentAccountId,
                Debit = 0,
                Credit = request.Amount,
                Description = request.Description
            };
            FormatUtil.SetIsActive(creditEntry, true);
            FormatUtil.SetDateBaseEntity(creditEntry);
            await _unitOfWork.JournalEntriesRepository.Add(dbName, creditEntry);
        }
    }
}
