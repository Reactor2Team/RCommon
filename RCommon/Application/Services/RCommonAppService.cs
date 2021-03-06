﻿using Microsoft.Extensions.Logging;
using RCommon.DataServices.Transactions;
using RCommon.ExceptionHandling;
using System;
using System.Collections.Generic;
using System.Text;

namespace RCommon.Application.Services
{
    public class RCommonAppService : RCommonService
    {


        public RCommonAppService(ILogger logger, IExceptionManager exceptionManager, IUnitOfWorkScopeFactory unitOfWorkScopeFactory)
            : base(logger)
        {
            ExceptionManager = exceptionManager;
            UnitOfWorkScopeFactory = unitOfWorkScopeFactory;



        }

        public IExceptionManager ExceptionManager { get; }

        protected IUnitOfWorkScopeFactory UnitOfWorkScopeFactory { get; }
    }
}
