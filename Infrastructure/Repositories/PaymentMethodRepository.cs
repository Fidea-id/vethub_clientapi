using Domain.Entities.Filters;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class PaymentMethodRepository : GenericRepository<PaymentMethod, NameBaseEntityFilter>, IPaymentMethodRepository
    {
        public PaymentMethodRepository(IDBFactory context) : base(context)
        {
        }
    }
}
