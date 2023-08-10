using Application.Services.Contracts;
using Application.Utils;
using Domain.Entities.DTOs;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Domain.Utils;

namespace Application.Services.Implementations
{
    public class OrdersService : GenericService<Orders, OrdersRequest, OrdersResponse, OrdersFilter>, IOrdersService
    {
        public OrdersService(IUnitOfWork unitOfWork, IGenericRepository<Orders, OrdersFilter> repository)
        : base(unitOfWork, repository)
        { }

        public async Task<DataResultDTO<OrdersResponse>> GetOrdersList(OrdersFilter filters, string dbName)
        {
            var data = await _unitOfWork.OrdersRepository.GetOrdersList(dbName, filters);
            return data;
        }

        public async Task<OrdersDetail> AddOrdersDetailAsync(OrdersDetailRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<OrdersDetail>(request);
                FormatUtil.SetIsActive<OrdersDetail>(entity, true);
                FormatUtil.SetDateBaseEntity<OrdersDetail>(entity);

                var newId = await _unitOfWork.OrdersDetailRepository.Add(dbName, entity);
                entity.Id = newId;
                return entity;
            }
            catch (Exception ex)
            {
                ex.Source = $"OrderService.AddOrdersDetailAsync";
                throw;
            }
        }

        public async Task<OrdersPayment> AddOrdersPaymentAsync(OrdersPaymentRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<OrdersPayment>(request);
                FormatUtil.SetIsActive<OrdersPayment>(entity, true);
                FormatUtil.SetDateBaseEntity<OrdersPayment>(entity);

                var newId = await _unitOfWork.OrdersPaymentRepository.Add(dbName, entity);
                entity.Id = newId;
                return entity;
            }
            catch (Exception ex)
            {
                ex.Source = $"OrderService.AddOrdersPaymentAsync";
                throw;
            }
        }

        public async Task<IEnumerable<OrderFullResponse>> GetOrderFullAsync(string dbName)
        {
            try
            {
                return await _unitOfWork.OrdersRepository.GetListOrderFull(dbName);
            }
            catch (Exception ex)
            {
                ex.Source = $"OrderService.GetOrderFullAsync";
                throw;
            }
        }

        public async Task<OrderFullResponse> GetOrderFullByIdAsync(int id, string dbName)
        {
            try
            {
                return await _unitOfWork.OrdersRepository.GetOrderFull(dbName, id);
            }
            catch (Exception ex)
            {
                ex.Source = $"OrderService.GetOrderFullByIdAsync";
                throw;
            }
        }

    }
}
