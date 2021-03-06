// ------------------------------------------------------------------------------------------------

// <auto-generated>
// ReSharper disable CheckNamespace
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable DoNotCallOverridableMethodsInConstructor
// ReSharper disable EmptyNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable NotAccessedVariable
// ReSharper disable PartialMethodWithSinglePart
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantCast
// ReSharper disable RedundantNameQualifier
// ReSharper disable RedundantOverridenMember
// ReSharper disable UseNameofExpression
// ReSharper disable UsePatternMatching
#pragma warning disable 1591    //  Ignore "Missing XML Comment" warning

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RCommon.ObjectAccess.EFCore.Tests
{
    // MonthlySalesSummary
    public partial class MonthlySalesSummary
    {
        public int Year { get; set; } // Year (Primary key)
        public int Month { get; set; } // Month (Primary key)
        public int SalesPersonId { get; set; } // SalesPersonId (Primary key)
        public decimal? Amount { get; set; } // Amount
        public string Currency { get; set; } // Currency (length: 255)
        public string SalesPersonFirstName { get; set; } // SalesPersonFirstName (length: 255)
        public string SalesPersonLastName { get; set; } // SalesPersonLastName (length: 255)

        public MonthlySalesSummary()
        {
            InitializePartial();
        }

        partial void InitializePartial();
    }

}
// </auto-generated>

