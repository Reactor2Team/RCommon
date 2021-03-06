﻿using RCommon.Application.DTO;
using RCommon.Collections;
using Samples.Domain.Entities;
using System.Threading.Tasks;

namespace Samples.Domain.DomainServices
{
    public interface IUserService
    {
        Task<CommandResult<IPaginatedList<ApplicationUser>>> SearchUsersAsync(string searchTerms, int pageIndex, int pageSize);
    }
}