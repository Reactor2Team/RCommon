﻿using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using RCommon.Configuration;
using RCommon.DependencyInjection;
using RCommon.ExceptionHandling.EnterpriseLibraryCore;
using RCommon.ExceptionHandling.EnterpriseLibraryCore.Handlers;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Security;
using System.Text;

namespace RCommon.ExceptionHandling.EnterpriseLibraryCore
{
    public class EhabExceptionHandlingConfiguration : IExceptionHandlingConfiguration
    {

        public EhabExceptionHandlingConfiguration()
        {

        }

        public void Configure(IContainerAdapter containerAdapter)
        {
            containerAdapter.AddTransient<IExceptionManager, EntLibExceptionManager>();
            //containerAdapter.AddSingleton<IConfigurationSource, DictionaryConfigurationSource>();

            
        }

        public void UsingDefaultExceptionPolicies()
        {
            // This is the fluent configuration version
            DictionaryConfigurationSource emptyConfigSource = new DictionaryConfigurationSource();

            ConfigurationSourceBuilder builder = new ConfigurationSourceBuilder();
            builder.ConfigureExceptionHandling()
                .GivenPolicyWithName("BasePolicy")          // This is a catch all policy
                .ForExceptionType<Exception>()              // For any type of exception
                .WrapWith<GeneralException>()               // Wrap with a better exception with more info
                    .UsingMessage("A handled exception occured and was processed by the BasePolicy")
                .HandleCustom<LoggingExceptonHandler>()     // Log the exception
                .ThenThrowNewException()                        // Rethrow the exception so the appropriate layer can handle it

                .GivenPolicyWithName("BusinessWrapPolicy")  // For wrapping all exceptions that bubble up to business layer
                .ForExceptionType<Exception>()
                .WrapWith<BusinessException>()
                .HandleCustom<LoggingExceptonHandler>()
                .ThenThrowNewException()

                .GivenPolicyWithName("BusinessReplacePolicy")// We use this for removing sensitive information from an exception
                .ForExceptionType<BusinessException>()
                .ReplaceWith<FriendlyBusinessException>()   // So let's simply make this a presentation friendly exception
                .ThenThrowNewException()

                .GivenPolicyWithName("ApplicationWrapPolicy")  // For wrapping all exceptions that bubble up to application layer
                .ForExceptionType<Exception>()
                .WrapWith<ApplicationTierException>()
                .HandleCustom<LoggingExceptonHandler>()
                .ThenThrowNewException()

                .GivenPolicyWithName("ApplicationReplacePolicy")// We use this for removing sensitive information from an exception
                .ForExceptionType<ApplicationTierException>()
                .ReplaceWith<FriendlyApplicationException>()   // So let's simply make this a presentation friendly exception
                .ThenThrowNewException()

                .GivenPolicyWithName("SecurityReplacePolicy")// We use this for removing sensitive information from an exception
                .ForExceptionType<SecurityException>()     // The assumption is that we've already handled the exception at a lower layer 
                .ReplaceWith<FriendlySecurityException>()   // So let's simply make this a presentation friendly exception
                .HandleCustom<LoggingExceptonHandler>()
                .ThenThrowNewException()                       // No need to log, just 

                .GivenPolicyWithName("PresentationReplacePolicy")// We use this for removing sensitive information from an exception
                .ForExceptionType<FriendlyApplicationException>()     // The assumption is that we've already handled the exception at a lower layer 
                .ThenDoNothing()                                // Already handled it
                .ForExceptionType<FriendlyBusinessException>()
                .ThenDoNothing()                                // Already handled it
                .ForExceptionType<FriendlySecurityException>()
                .ThenDoNothing();                               // Already handled it

            builder.UpdateConfigurationWithReplace(emptyConfigSource);

            // This is the configuration file version
            //IConfigurationSource config = new FileConfigurationSource(@"App.config"); //ConfigurationSourceFactory.Create(); // Requires that configuration file be in place.

            ExceptionPolicyFactory factory = new ExceptionPolicyFactory(emptyConfigSource);

            ExceptionManager exManager = factory.CreateManager();

            ExceptionPolicy.SetExceptionManager(factory.CreateManager());
        }
    }
}
