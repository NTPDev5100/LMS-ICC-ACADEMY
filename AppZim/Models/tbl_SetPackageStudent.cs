//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AppZim.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tbl_SetPackageStudent
    {
        public int ID { get; set; }
        public Nullable<int> StudentID { get; set; }
        public string StudentName { get; set; }
        public Nullable<int> SetPackageID { get; set; }
        public Nullable<int> SetPrice { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<int> DiscountID { get; set; }
        public Nullable<int> DiscountPrice { get; set; }
        public Nullable<bool> IsHide { get; set; }
        public Nullable<int> PaymentType { get; set; }
        public Nullable<bool> Paid { get; set; }
        public Nullable<int> ExaminerID { get; set; }
    }
}
