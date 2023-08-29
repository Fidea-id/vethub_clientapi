﻿using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class OrdersPaymentRepository : GenericRepository<OrdersPayment, OrdersPaymentFilter>, IOrdersPaymentRepository
    {
        public OrdersPaymentRepository(IDBFactory context) : base(context)
        {
        }
    }
}