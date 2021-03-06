﻿namespace RCommon.ObjectAccess.EFCore
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using RCommon;
    using RCommon.DataServices;
    using RCommon.DataServices.Transactions;
    using RCommon.Domain.Repositories;
    using RCommon.Expressions;
    using RCommon.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// A concrete implementation for Entity Framework Core version 3.x. This class
    /// currently exposes much of the functionality of EF with the exception of change tracking and peristance models. We expose IQueryable to layers down stream
    /// so that complex joins can be utilized and then managed at the domain level. This implementation makes special considerations for managing the lifetime of the
    /// <see cref="DbContext"/> specifically when it applies to the <see cref="UnitOfWorkScope"/>. 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class EFCoreRepository<TEntity> : FullFeaturedRepositoryBase<TEntity>, IEFCoreRepository<TEntity> where TEntity : class
    {
        private readonly List<string> _includes;
        private readonly Dictionary<Type, object> _objectSets;
        private readonly IDataStoreProvider _dataStoreProvider;
        private readonly ILogger _logger;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        //private DbSet<TEntity> _objectSet;
        private IQueryable<TEntity> _repositoryQuery;
        private bool _tracking;



        /// <summary>
        /// The default constructor for the repository. 
        /// </summary>
        /// <param name="dbContext">The <see cref="TDataStore"/> is injected with scoped lifetime so it will always return the same instance of the <see cref="DbContext"/>
        /// througout the HTTP request or the scope of the thread.</param>
        /// <param name="logger">Logger used throughout the application.</param>
        public EFCoreRepository(IDataStoreProvider dataStoreProvider, ILoggerFactory logger, IUnitOfWorkManager unitOfWorkManager)
        {
            this._dataStoreProvider = dataStoreProvider;
            this._logger = logger.CreateLogger(this.GetType().Name);
            this._unitOfWorkManager = unitOfWorkManager;
            this._includes = new List<string>();
            //this._objectSet = null;
            this._repositoryQuery = null;
            this._tracking = true;
            this._objectSets = new Dictionary<Type, object>();
        }

        public override TEntity Add(TEntity entity)
        {
            var updatedEntity = this.ObjectSet.Add(entity).Entity;
            this.Save();
            return updatedEntity;
        }


        protected override void ApplyFetchingStrategy(Expression[] paths)
        {
            Guard.Against<ArgumentNullException>((paths == null) || (paths.Length == 0), "Expected a non-null and non-empty array of Expression instances representing the paths to eagerly load.");
            string currentPath = string.Empty;
            paths.ForEach<System.Linq.Expressions.Expression>(delegate (System.Linq.Expressions.Expression path) {
                MemberAccessPathVisitor visitor = new MemberAccessPathVisitor();
                visitor.Visit(path);
                currentPath = !string.IsNullOrEmpty(currentPath) ? (currentPath + "." + visitor.Path) : visitor.Path;
                ((EFCoreRepository<TEntity>)this)._includes.Add(currentPath);
            });
        }



        public IQueryable<TEntity> CreateQuery()
        {
            return this.ObjectSet.AsQueryable<TEntity>();
        }

        public override void Delete(TEntity entity)
        {
            this.ObjectSet.Remove(entity);
            this.Save();
        }


        public override ICollection<TEntity> Find(ISpecification<TEntity> specification)
        {
            return this.FindCore(specification.Predicate).ToList();
        }

        public override ICollection<TEntity> Find(Expression<Func<TEntity, bool>> expression)
        {
            return this.FindCore(expression).ToList();
        }

        public override IQueryable<TEntity> FindQuery(ISpecification<TEntity> specification)
        {
            return this.FindCore(specification.Predicate);
        }

        public override IQueryable<TEntity> FindQuery(Expression<Func<TEntity, bool>> expression)
        {
            return this.FindCore(expression);
        }

        private IQueryable<TEntity> FindCore(Expression<Func<TEntity, bool>> expression)
        {
            IQueryable<TEntity> queryable;
            try
            {
                Guard.Against<NullReferenceException>(this.RepositoryQuery == null, "RepositoryQuery is null");

                queryable = this.RepositoryQuery.Where<TEntity>(expression);
            }
            catch (ApplicationException exception)
            {
                this._logger.LogError(exception, "Error in Repository.FindCore: " + base.GetType().ToString() + " while executing a query on the Context.", expression);
                throw new RepositoryException("Error in Repository: " + base.GetType().ToString() + " while executing a query on the Context.", exception.GetBaseException());
            }
            return queryable;
        }




        public override int GetCount(ISpecification<TEntity> selectSpec) =>
            this.Find(selectSpec).Count<TEntity>();

        public override int GetCount(Expression<Func<TEntity, bool>> expression) =>
            this.Find(expression).Count<TEntity>();

        /*private EntityKey GetEntityKey(TEntity entity)
        {
            List<EntityKeyMember> entityKeyValues = new List<EntityKeyMember>();
            using (ReadOnlyMetadataCollection<EdmMember>.Enumerator enumerator = this.ObjectSet.EntitySet.ElementType.KeyMembers.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    EdmMember current = enumerator.Current;
                    object keyValue = entity.GetType().GetProperty(current.Name).GetValue(entity, null);
                    entityKeyValues.Add(new EntityKeyMember(current.Name, keyValue));
                }
            }
            return new EntityKey(this.ObjectContext.DefaultContainerName + "." + this.ObjectSet.EntitySet.Name, entityKeyValues);
        }*/

        /*private DbSet<T> GetObjectSet<T>() where T : class
        {
            object obj2 = null;
            if (!this._objectSets.TryGetValue(typeof(T), out obj2))
            {
                obj2 = this.ObjectContext.Set<T>();// CreateObjectSet<T>();
                this._objectSets.Add(typeof(T), obj2);
            }
            return (DbSet<T>)obj2;
        }*/

        /*private ObjectStateEntry GetObjectStateEntry(TEntity entity)
        {
            ObjectStateEntry entry = null;
            this.ObjectContext.ObjectStateManager.TryGetObjectStateEntry(this.GetEntityKey(entity), out entry);
            return entry;
        }

        private ObjectStateEntry GetObjectStateEntry(EntityKey entityKey)
        {
            ObjectStateEntry entry = null;
            this.ObjectContext.ObjectStateManager.TryGetObjectStateEntry(entityKey, out entry);
            return entry;
        }*/


        private int Save()
        {
            int affected = 0;
            if (this._unitOfWorkManager.CurrentUnitOfWork == null)
            {
                affected = this.ObjectContext.SaveChanges(true);
            }
            return affected;
        }

        public override void Update(TEntity entity)
        {
            this.ObjectSet.Update(entity);
            this.Save();
        }

        public async override Task<ICollection<TEntity>> FindAsync(ISpecification<TEntity> specification)
        {
            return await this.FindCore(specification.Predicate).ToListAsync();
        }

        public async override Task<ICollection<TEntity>> FindAsync(Expression<Func<TEntity, bool>> expression)
        {
            return await this.FindCore(expression).ToListAsync();
        }

        public async override Task<int> GetCountAsync(ISpecification<TEntity> selectSpec)
        {
            return await this.FindCore(selectSpec.Predicate).CountAsync();
        }

        public async override Task<int> GetCountAsync(Expression<Func<TEntity, bool>> expression)
        {
            return await this.FindCore(expression).CountAsync();
        }

        private async Task<int> SaveAsync()
        {
            int affected = 0;
            if (this._unitOfWorkManager.CurrentUnitOfWork == null)
            {
                affected = await this.ObjectContext.SaveChangesAsync(true);
            }
            return affected;
        }

        public override void Attach(TEntity entity)
        {
            this.ObjectContext.Attach<TEntity>(entity);
            this.Save();
        }

        public override TEntity Find(object primaryKey)
        {
            
            return this.ObjectSet.Find(primaryKey);
        }

        public override TEntity FindSingleOrDefault(Expression<Func<TEntity, bool>> expression)
        {
            return this.FindCore(expression).SingleOrDefault();
        }

        public override TEntity FindSingleOrDefault(ISpecification<TEntity> specification)
        {
            return this.FindCore(specification.Predicate).SingleOrDefault();
        }

        public override async Task AddAsync(TEntity entity)
        {
            await this.ObjectSet.AddAsync(entity);
            await this.SaveAsync();
        }

        public async override Task DeleteAsync(TEntity entity)
        {
            this.ObjectSet.Remove(entity);
            await this.SaveAsync();
        }

        public async override Task UpdateAsync(TEntity entity)
        {
            this.ObjectSet.Update(entity);
            await this.SaveAsync();
        }

        public override async Task<TEntity> FindAsync(object primaryKey)
        {
            return await this.ObjectSet.FindAsync(primaryKey);
        }

        public override async Task<TEntity> FindSingleOrDefaultAsync(Expression<Func<TEntity, bool>> expression)
        {
            return await this.FindCore(expression).SingleOrDefaultAsync();
        }

        public override async Task<TEntity> FindSingleOrDefaultAsync(ISpecification<TEntity> specification)
        {
            return await this.FindCore(specification.Predicate).SingleOrDefaultAsync();
        }

        public async override Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression)
        {
            return await this.FindCore(expression).AnyAsync();
        }

        public async override Task<bool> AnyAsync(ISpecification<TEntity> specification)
        {
            return await this.FindCore(specification.Predicate).AnyAsync();
        }

        protected internal DbContext ObjectContext
        {
            get
            {

                if (this._unitOfWorkManager.CurrentUnitOfWork != null)
                {

                    return this._dataStoreProvider.GetDataStore<RCommonDbContext>(this._unitOfWorkManager.CurrentUnitOfWork.TransactionId.Value, this.DataStoreName);

                }
                return this._dataStoreProvider.GetDataStore<RCommonDbContext>(this.DataStoreName);
            }
        }



        protected DbSet<TEntity> ObjectSet
        {
            get
            {
                /*if (ReferenceEquals(this._objectSet, null))
                {
                    
                    var objectSet = this.GetObjectSet<TEntity>();
                    
                    
                    this._objectSet = objectSet;
                }*/
                return this.ObjectContext.Set<TEntity>();
                //return this._objectSet;
            }
        }

        public bool Tracking
        {
            get =>
                this._tracking;
            set
            {
                this._tracking = value;
            }

        }

        protected override IQueryable<TEntity> RepositoryQuery
        {
            get
            {
                IQueryable<TEntity> query;
                if (ReferenceEquals(this._repositoryQuery, null))
                {
                    query = this.CreateQuery();
                }
                else
                {
                    query = _repositoryQuery;
                }

                // Start Eagerloading
                
                
                if (this._includes.Count > 0)
                {
                    Action<string> action = null;
                    if (action == null)
                    {
                        action = delegate (string m) {
                            query = query.Include(m);
                        };
                    }
                    this._includes.ForEach(action); // Build the eagerloaded query
                    this._includes.Clear(); // clear the eagerloading paths so we don't duplicate efforts if another repository method is called
                }
                this._repositoryQuery = query;

                return this._repositoryQuery;
            }
        }

        protected string EntitySetName =>
            this.ObjectContext.GetType().GetProperties().Single<PropertyInfo>(delegate (PropertyInfo p)
            {
                bool flag1;
                if (!p.PropertyType.IsGenericType)
                {
                    flag1 = false;
                }
                else
                {
                    Type[] typeArguments = new Type[] { typeof(TEntity) };
                    flag1 = typeof(IQueryable<>).MakeGenericType(typeArguments).IsAssignableFrom(p.PropertyType);
                }
                return flag1;
            }).Name;


    }
}

