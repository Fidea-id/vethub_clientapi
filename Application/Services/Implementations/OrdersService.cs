using Application.Services.Contracts;
using Application.Utils;
using Domain.Entities.DTOs;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Domain.Utils;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Application.Services.Implementations
{
    public class OrdersService : GenericService<Orders, OrdersRequest, OrdersResponse, OrdersFilter>, IOrdersService
    {
        private readonly ILogger<OrdersService> _logger;

        public OrdersService(IUnitOfWork unitOfWork, IGenericRepository<Orders, OrdersFilter> repository,
            ILoggerFactory loggerFactory)
        : base(unitOfWork, repository)
        {
            _logger = loggerFactory.CreateLogger<OrdersService>();
        }
        public async Task<DashboardOrderResponse> GetOrderDashboardAsync(string dbName)
        {
            var data = await _unitOfWork.OrdersRepository.GetOrdersDashboard(dbName); 
            CultureInfo culture = new CultureInfo("id-ID"); // You can specify the culture you want here.
            data.IncomesAmountText = string.Format(culture, "Rp {0:N0}", data.IncomesAmount); 
            data.ExpensesAmountText = string.Format(culture, "Rp {0:N0}", data.ExpensesAmount);
            return data;
        }
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

        public async Task<OrdersPaymentResponse> AddOrdersPaymentAsync(OrdersPaymentRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<OrdersPayment>(request);
                FormatUtil.SetIsActive<OrdersPayment>(entity, true);
                FormatUtil.SetDateBaseEntity<OrdersPayment>(entity);

                var orderDetail = await _unitOfWork.OrdersRepository.GetOrderFull(dbName, request.OrderId);
                if (orderDetail == null) throw new Exception("Order not found");

                var paymentStatus = "Paid";
                entity.Status = paymentStatus;
                var newId = await _unitOfWork.OrdersPaymentRepository.Add(dbName, entity);
                entity.Id = newId;
                //update status
                var getTotalLastPayment = orderDetail.OrderPayments.Sum(x => x.Total);
                var totalPayments = getTotalLastPayment + request.Total;
                _logger.LogInformation("Last payment is " + getTotalLastPayment);
                if ((totalPayments) >= orderDetail.TotalPrice)
                {
                    var order = await _unitOfWork.OrdersRepository.GetById(dbName, request.OrderId);
                    order.Status = paymentStatus;
                    FormatUtil.SetDateBaseEntity<Orders>(order, true);
                    await _unitOfWork.OrdersRepository.Update(dbName, order);

                    //update stock
                    if (orderDetail.Type == "Incomes")
                    {
                        _logger.LogInformation("Start update stock of order type " + orderDetail.Type);
                        foreach (var item in orderDetail.OrderProducts)
                        {
                            _logger.LogInformation("Start update stock of " + item.ProductId + " by amount " + item.Quantity);
                            await _unitOfWork.ProductStockRepository.UpdateMinStock(item.ProductId, item.Quantity, dbName);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Start update stock of order type " + orderDetail.Type);
                        foreach (var item in orderDetail.OrderProducts)
                        {
                            _logger.LogInformation("Start update stock of " + item.ProductId + " by amount " + item.Quantity);
                            await _unitOfWork.ProductStockRepository.UpdateAddStock(item.ProductId, item.Quantity, dbName);
                        }
                    }
                }
                else
                {
                    paymentStatus = "Paid Less";
                }


                var result = new OrdersPaymentResponse
                {
                    Date = request.Date,
                    OrderId = orderDetail.Id,
                    PaymentMethodId = request.PaymentMethodId,
                    Total = request.Total,
                    LessTotal = orderDetail.TotalPrice - totalPayments,
                    Status = paymentStatus
                };
                return result;
            }
            catch (Exception ex)
            {
                ex.Source = $"OrderService.AddOrdersPaymentAsync";
                throw;
            }
        }

        public async Task<IEnumerable<OrderFullResponse>> GetOrderFullAsync(string dbName, bool thisMonth = false)
        {
            try
            {
                return await _unitOfWork.OrdersRepository.GetListOrderFull(dbName, thisMonth);
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

        public async Task<OrderFullResponse> CreateOrderFullAsync(OrderFullRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                Owners clientData;
                if(request.ClientId == 0)
                {
                    clientData = new Owners("Guest");
                }
                else
                {
                    clientData = await _unitOfWork.OwnersRepository.GetById(dbName, request.ClientId);
                    if (clientData == null) throw new Exception("Client not found");
                }
                var staff = await _unitOfWork.ProfileRepository.GetByGlobalId(dbName, request.StaffId);
                if (staff == null) throw new Exception("Staff not found");
                var getLatestCode = await _unitOfWork.OrdersRepository.GetLatestCode(dbName);
                var orderNumber = FormatUtil.GenerateOrdersNumber(getLatestCode);

                var countQty = request.OrderDetailItem.Sum(x => x.Quantity);
                var countDiscounted = request.OrderDetailItem.Sum(x => x.Discount);
                var countTotal = request.OrderDetailItem.Sum(x => x.TotalPrice);
                var newOrders = new Orders
                {
                    BranchId = 0,
                    ClientId = request.ClientId,
                    ClientName = clientData.Name,
                    Date = request.Date,
                    DueDate = request.DueDate,
                    Type = request.Type,
                    OrderNumber = orderNumber,
                    Status = "Unpaid",
                    StaffId = request.StaffId,
                    TotalQuantity = countQty,
                    TotalDiscountedPrice = countDiscounted ?? 0,
                    TotalPrice = countTotal
                };

                FormatUtil.SetIsActive<Orders>(newOrders, true);
                FormatUtil.SetDateBaseEntity<Orders>(newOrders);

                var newId = await _unitOfWork.OrdersRepository.Add(dbName, newOrders);
                newOrders.Id = newId;

                //add orders detail
                var detailList = new List<OrdersDetail>();
                foreach(var item in request.OrderDetailItem)
                {
                    var newOrdersDetail = new OrdersDetail
                    {
                        StaffId = item.StaffId,
                        Discount = item.Discount,
                        DiscountType = item.DiscountType,
                        OrderId = newOrders.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        TotalPrice = item.TotalPrice
                    };
                    FormatUtil.SetIsActive<OrdersDetail>(newOrdersDetail, true);
                    FormatUtil.SetDateBaseEntity<OrdersDetail>(newOrdersDetail);
                    detailList.Add(newOrdersDetail);
                }
                await _unitOfWork.OrdersDetailRepository.AddRange(dbName, detailList);

                var detailResponse = Mapping.Mapper.Map<List<OrdersDetailResponse>>(detailList);

                var response = new OrderFullResponse
                {
                    Id = newOrders.Id,
                    ClientId = newOrders.ClientId,
                    ClientName = newOrders.ClientName,
                    Date = newOrders.Date,
                    DueDate = newOrders.DueDate,
                    OrderNumber = newOrders.OrderNumber,
                    StaffId = newOrders.StaffId,
                    StaffName = staff.Name,
                    Status = newOrders.Status,
                    TotalQuantity = newOrders.TotalQuantity,
                    TotalDiscountedPrice = newOrders.TotalDiscountedPrice,
                    TotalPrice = newOrders.TotalPrice,
                    OrderProducts = detailResponse,
                    ClinicData = null
                };
                return response;
            }
            catch (Exception ex)
            {
                ex.Source = $"OrderService.CreateOrderFullAsync";
                throw;
            }
        }
    }
}
