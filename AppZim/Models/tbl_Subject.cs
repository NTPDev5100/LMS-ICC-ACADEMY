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
    
    public partial class tbl_Subject
    {
        public int ID { get; set; }
        public Nullable<int> GradeID { get; set; }
        public string GradeName { get; set; }
        public Nullable<int> ClassID { get; set; }
        public string ClassName { get; set; }
        public string SubjectName { get; set; }
        public Nullable<bool> isHide { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<bool> SpecialSubject { get; set; }
        public string Code { get; set; }
    }
}
