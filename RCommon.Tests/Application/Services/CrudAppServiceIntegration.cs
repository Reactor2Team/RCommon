﻿using AutoMapper;
using AutoMapper.Configuration;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using RCommon.Domain.Repositories;
using RCommon.ObjectAccess.EFCore;
using RCommon.ObjectAccess.EFCore.Tests;
using RCommon.TestBase;
using RCommon.Tests.Application.DTO;
using RCommon.Tests.Domain.Services;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace RCommon.Tests.Application.Services
{
    [TestFixture]
    public class CrudAppServiceIntegration : EFCoreTestBase
    {
        private TestDbContext _context;
        //private MapperConfiguration _mapperConfiguration;

        public CrudAppServiceIntegration() :base()
        {
            var services = new ServiceCollection();

            services.AddTransient<IEagerFetchingRepository<Customer>, EFCoreRepository<Customer>>();
            services.AddTransient<IEagerFetchingRepository<Order>, EFCoreRepository<Order>>();

            services.AddTransient<ITestDomainService, TestDomainService>();
            services.AddTransient<ITestAppService, TestAppService>();

            //IMapperConfigurationExpression expressionMapper = new MapperConfigurationExpression();
            
            //var exp = expressionMapper.CreateMap<CustomerDto, Customer>();
            //expressionMapper.CreateMap<Customer, CustomerDto>();

            services.AddAutoMapper(x =>
            {
                x.CreateMap<CustomerDto, Customer>();
                x.CreateMap<Customer, CustomerDto>();
            });

            this.InitializeRCommon(services);

        }

        [OneTimeSetUp]
        public void InitialSetup()
        {
            this.Logger.LogInformation("Beginning Onetime setup", null);
            //this.ContainerAdapter.Register<DbContext, TestDbContext>(typeof(TestDbContext).AssemblyQualifiedName);

        }

        [SetUp]
        public void Setup()
        {
            this.CreateWebRequest();

            //_context = this.ServiceProvider.GetService<RCommonDbContext>();
            this.Logger.LogInformation("Beginning New Test Setup", null);


        }

        [TearDown]
        public void TearDown()
        {
            this.Logger.LogInformation("Tearing down Test", null);

            if (_context != null)
            {
                _context.Database.ExecuteSqlInterpolated($"DELETE OrderItems");
                _context.Database.ExecuteSqlInterpolated($"DELETE Products");
                _context.Database.ExecuteSqlInterpolated($"DELETE Orders");
                _context.Database.ExecuteSqlInterpolated($"DELETE Customers");
                _context.Dispose();
            }
        }

        [Test]
        public void Can_Create_Async()
        {
            _context = new TestDbContext(this.Configuration);
            var testData = new EFTestData(_context);
            var testDataActions = new EFTestDataActions(testData);
            var customer = testDataActions.CreateCustomerStub(x=>x.FirstName = "Albus");

            var service = this.ServiceProvider.GetService<ITestAppService>();
            var mapper = this.ServiceProvider.GetService<IMapper>();

            var customerDto = mapper.Map<CustomerDto>(customer);
            var result =  service.CreateAsync(customerDto);

            Customer savedCustomer = null;
            testData.Batch(action => savedCustomer = action.GetFirstCustomer(x=>x.FirstName == "Albus"));

            Assert.IsNotNull(savedCustomer);
            Assert.AreEqual(savedCustomer.FirstName, customer.FirstName);
        }

        [Test]
        public void Can_Create()
        {
            _context = new TestDbContext(this.Configuration);
            var testData = new EFTestData(_context);
            var testDataActions = new EFTestDataActions(testData);
            var customer = testDataActions.CreateCustomerStub(x => x.FirstName = "Severus");

            var service = this.ServiceProvider.GetService<ITestAppService>();
            var mapper = this.ServiceProvider.GetService<IMapper>();

            var customerDto = mapper.Map<CustomerDto>(customer);
            var result = service.Create(customerDto);

            Customer savedCustomer = null;
            testData.Batch(action => savedCustomer = action.GetFirstCustomer(x => x.FirstName == "Severus"));

            Assert.IsNotNull(savedCustomer);
            Assert.AreEqual(savedCustomer.FirstName, customer.FirstName);
            Assert.AreEqual(result.DataResult.FirstName, customer.FirstName);
        }

        [Test]
        public void Can_Update_Async()
        {
            _context = new TestDbContext(this.Configuration);
            var testData = new EFTestData(_context);
            var testDataActions = new EFTestDataActions(testData);
            var customer = testDataActions.CreateCustomer();

            var service = this.ServiceProvider.GetService<ITestAppService>();
            var mapper = this.ServiceProvider.GetService<IMapper>();

            var customerDto = mapper.Map<CustomerDto>(customer);
            var firstName = new Faker().Name.FirstName();
            customerDto.FirstName = firstName;
            var result = service.UpdateAsync(customerDto);

            _context.Dispose(); // Refresh the cache
            _context = new TestDbContext(this.Configuration);
            testData = new EFTestData(_context);

            Customer savedCustomer = null;
            testData.Batch(action => savedCustomer = action.GetCustomerById(customer.CustomerId));

            Assert.IsNotNull(savedCustomer);
            Assert.AreEqual(savedCustomer.FirstName, firstName);
        }

        [Test]
        public void Can_Update()
        {
            _context = new TestDbContext(this.Configuration);
            var testData = new EFTestData(_context);
            var testDataActions = new EFTestDataActions(testData);
            var customer = testDataActions.CreateCustomer();

            var service = this.ServiceProvider.GetService<ITestAppService>();
            var mapper = this.ServiceProvider.GetService<IMapper>();

            var customerDto = mapper.Map<CustomerDto>(customer);
            var firstName = new Faker().Name.FirstName();
            customerDto.FirstName = firstName;
            var result = service.Update(customerDto);

            _context.Dispose(); // Refresh the cache
            _context = new TestDbContext(this.Configuration);
            testData = new EFTestData(_context);

            Customer savedCustomer = null;
            testData.Batch(action => savedCustomer = action.GetCustomerById(customer.CustomerId));

            Assert.IsNotNull(savedCustomer);
            Assert.AreEqual(savedCustomer.FirstName, firstName);
        }

        [Test]
        public void Can_Delete_Async()
        {
            _context = new TestDbContext(this.Configuration);
            var testData = new EFTestData(_context);
            var testDataActions = new EFTestDataActions(testData);
            var customer = testDataActions.CreateCustomer();

            var service = this.ServiceProvider.GetService<ITestAppService>();
            var mapper = this.ServiceProvider.GetService<IMapper>();

            var customerDto = mapper.Map<CustomerDto>(customer);
            var result = service.DeleteAsync(customerDto);

            Customer savedCustomer = null;
            testData.Batch(action => savedCustomer = action.GetCustomerById(customer.CustomerId));

            Assert.IsNull(savedCustomer);
        }

        [Test]
        public void Can_Delete()
        {
            _context = new TestDbContext(this.Configuration);
            var testData = new EFTestData(_context);
            var testDataActions = new EFTestDataActions(testData);
            var customer = testDataActions.CreateCustomer();

            var service = this.ServiceProvider.GetService<ITestAppService>();
            var mapper = this.ServiceProvider.GetService<IMapper>();

            var customerDto = mapper.Map<CustomerDto>(customer);
            var result = service.Delete(customerDto);

            Customer savedCustomer = null;
            testData.Batch(action => savedCustomer = action.GetCustomerById(customer.CustomerId));

            Assert.IsNull(savedCustomer);
        }

        [Test]
        public void Can_GetById_Async()
        {
            _context = new TestDbContext(this.Configuration);
            var testData = new EFTestData(_context);
            var testDataActions = new EFTestDataActions(testData);
            var customer = testDataActions.CreateCustomer();

            var service = this.ServiceProvider.GetService<ITestAppService>();

            var result = service.GetByIdAsync(customer.CustomerId).Result;

            Assert.IsNotNull(result.DataResult);
            Assert.IsTrue(customer.FirstName == result.DataResult.FirstName);
        }

        [Test]
        public void Can_GetById()
        {
            _context = new TestDbContext(this.Configuration);
            var testData = new EFTestData(_context);
            var testDataActions = new EFTestDataActions(testData);
            var customer = testDataActions.CreateCustomer();

            var service = this.ServiceProvider.GetService<ITestAppService>();

            var result = service.GetById(customer.CustomerId);

            Assert.IsNotNull(result.DataResult);
            Assert.IsTrue(customer.FirstName == result.DataResult.FirstName);
        }
    }
}
