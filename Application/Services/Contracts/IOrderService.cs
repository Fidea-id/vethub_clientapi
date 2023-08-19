﻿using Domain.Entities.DTOs;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;

namespace Application.Services.Contracts
{
    public interface IOrdersService : IGenericService<Orders, OrdersRequest, OrdersResponse, OrdersFilter>
    {
        Task<DataResultDTO<OrdersResponse>> GetOrdersList(OrdersFilter filters, string dbName);

        //OrderFullResponse
        Task<IEnumerable<OrderFullResponse>> GetOrderFullAsync(string dbName);
        Task<OrderFullResponse> GetOrderFullByIdAsync(int id, string dbName);

        //OrderDetail
        Task<OrdersDetail> AddOrdersDetailAsync(OrdersDetailRequest request, string dbName);

        //OrderPayment
        Task<OrdersPayment> AddOrdersPaymentAsync(OrdersPaymentRequest request, string dbName);
    }
}
