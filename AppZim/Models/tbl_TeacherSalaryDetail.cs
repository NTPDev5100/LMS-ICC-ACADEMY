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
    
    public partial class tbl_TeacherSalaryDetail
    {
        public int ID { get; set; }
        public Nullable<int> CourseID { get; set; }
        public Nullable<int> CoursesScheduleID { get; set; }
        public Nullable<int> TeacherID { get; set; }
        public Nullable<double> Salary { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public Nullable<int> Type { get; set; }
        public Nullable<bool> isClosing { get; set; }
        public Nullable<bool> isHide { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ReasonNote { get; set; }
        public Nullable<bool> IsReject { get; set; }
    }
}
