using Application.Services.Contracts;
using Domain.Entities.Models;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Implementations
{
    public class OwnersService : GenericService<Owners, Owners>, IOwnersService
    {
        public OwnersService(IUnitOfWork unitOfWork, IGenericRepository<Owners> repository)
        : base(unitOfWork, repository)
        { }
    }
}
