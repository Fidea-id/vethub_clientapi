using Dapper;
using Domain.Entities.DTOs;
using Domain.Entities.Models;
using Domain.Interfaces;
using Infrastructure.Data;
using System;
using System.Data;

namespace Infrastructure.Repositories
{
    public class ProfileRepository : GenericRepository<Services>, IProfileRepository
    {
        public ProfileRepository(IDBFactory context) : base(context)
        {
        }
    }
}

