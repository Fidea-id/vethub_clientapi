using Application.Services.Contracts;
using Application.Utils;
using Domain.Entities.DTOs;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClientVetHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChartOfAccountsController : ControllerBase
    {
        private readonly IChartOfAccountsService _chartOfAccountsService;
        private readonly IUnitOfWork _unitOfWork;

        public ChartOfAccountsController(IChartOfAccountsService chartOfAccountsService, IUnitOfWork unitOfWork)
        {
            _chartOfAccountsService = chartOfAccountsService;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] ChartOfAccountsFilter filters)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity") ?? string.Empty;
                var entities = await _chartOfAccountsService.GetEntitiesByFilter(filters, dbName);
                
                var balances = await _unitOfWork.ChartOfAccountsRepository.GetAccountBalancesAsync(dbName);
                var mappedData = Mapping.Mapper.Map<IEnumerable<ChartOfAccountsResponse>>(entities.Data);
                
                foreach (var item in mappedData)
                {
                    balances.TryGetValue(item.Id, out double rawBalance);
                    
                    if (item.Type == "Liability" || item.Type == "Equity" || item.Type == "Revenue")
                    {
                        item.Balance = -rawBalance;
                    }
                    else
                    {
                        item.Balance = rawBalance;
                    }
                }
                
                var result = new DataResultDTO<ChartOfAccountsResponse>
                {
                    Data = mappedData,
                    TotalData = entities.TotalData
                };
                
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity") ?? string.Empty;
                var data = await _chartOfAccountsService.ReadByIdAsync(id, dbName);
                if (data == null)
                {
                    return NotFound();
                }
                
                var balances = await _unitOfWork.ChartOfAccountsRepository.GetAccountBalancesAsync(dbName);
                var mapped = Mapping.Mapper.Map<ChartOfAccountsResponse>(data);
                
                balances.TryGetValue(mapped.Id, out double rawBalance);
                
                if (mapped.Type == "Liability" || mapped.Type == "Equity" || mapped.Type == "Revenue")
                {
                    mapped.Balance = -rawBalance;
                }
                else
                {
                    mapped.Balance = rawBalance;
                }
                
                return Ok(mapped);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChartOfAccountsRequest request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _chartOfAccountsService.CreateRequestAsync(request, dbName);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ChartOfAccountsRequest value)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var newData = await _chartOfAccountsService.UpdateAsync(id, value, dbName);
                return Ok(newData);
            }
            catch
            {
                throw;
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                await _chartOfAccountsService.DeleteAsync(id, dbName);
                return Ok();
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("MigratePendapatanOrder")]
        public async Task<IActionResult> MigratePendapatanOrder()
        {
            try
            {
                var dbName = User.FindFirstValue("Entity") ?? string.Empty;

                // Only proceed when tenant has legacy cash accounts but missing Pendapatan Order
                var kasTunai = await _unitOfWork.ChartOfAccountsRepository.WhereFirstQuery(dbName, "Name = 'Kas Tunai'");
                var kasKecil = await _unitOfWork.ChartOfAccountsRepository.WhereFirstQuery(dbName, "Name = 'Kas Kecil'");

                if (kasTunai == null || kasKecil == null)
                {
                    return Ok(new { updated = false, message = "Client does not have both Kas Tunai and Kas Kecil; skipping." });
                }

                var existing = await _unitOfWork.ChartOfAccountsRepository.WhereFirstQuery(dbName, "Name = 'Pendapatan Order'");
                if (existing != null)
                {
                    return Ok(new { updated = false, message = "Pendapatan Order already exists." });
                }

                var request = new ChartOfAccountsRequest
                {
                    Code = "4-1150",
                    Name = "Pendapatan Order",
                    Type = "Revenue",
                    SubType = null,
                    ParentId = null
                };

                var created = await _chartOfAccountsService.CreateRequestAsync(request, dbName);

                return Ok(new { updated = true, data = created });
            }
            catch (Exception ex)
            {
                // Do not throw; return failure so callers can continue
                return StatusCode(500, new { updated = false, message = ex.Message });
            }
        }
    }
}
