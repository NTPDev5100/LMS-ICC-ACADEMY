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
    
    public partial class tbl_CoursesAttendanceStudentWarning
    {
        public int ID { get; set; }
        public Nullable<int> CoursesAttendanceStudentID { get; set; }
        public string ContentFeedback { get; set; }
        public Nullable<bool> isHide { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<int> StudentUID { get; set; }
        public Nullable<int> StatusCMT { get; set; }
        public Nullable<int> TypeComment { get; set; }
    }
}