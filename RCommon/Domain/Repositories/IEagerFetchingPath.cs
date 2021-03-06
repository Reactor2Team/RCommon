﻿namespace RCommon.Domain.Repositories
{
    using System;
    using System.Linq.Expressions;

    public interface IEagerFetchingPath<T>
    {
        IEagerFetchingPath<TChild> And<TChild>(Expression<Func<T, object>> path);
    }
}

