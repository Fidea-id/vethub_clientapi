﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Contracts
{
    public interface ICurrentUserService
    {
        public Task<int> UserId { get; }
    }
}
