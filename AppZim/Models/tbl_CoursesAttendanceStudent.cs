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
    
    public partial class tbl_CoursesAttendanceStudent
    {
        public int ID { get; set; }
        public Nullable<int> SchoolID { get; set; }
        public string SchoolName { get; set; }
        public Nullable<int> StudentID { get; set; }
        public string StudentName { get; set; }
        public string StudentPhone { get; set; }
        public Nullable<int> CourseID { get; set; }
        public string CourseName { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public string Note { get; set; }
        public Nullable<bool> isHide { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<int> AttendanceID { get; set; }
        public string AttendanceName { get; set; }
        public Nullable<int> LearningID { get; set; }
        public string LearningName { get; set; }
        public Nullable<bool> Warning { get; set; }
        public Nullable<int> StudyTimeID { get; set; }
        public string TeacherName { get; set; }
        public string CmtNew { get; set; }
        public Nullable<int> TeacherLeadID { get; set; }
        public string TeacherLeadName { get; set; }
        public Nullable<System.DateTime> sTimeJoinMeeting { get; set; }
        public Nullable<System.DateTime> eTimeJoinMeeting { get; set; }
        public Nullable<int> TotalJoinMeeting { get; set; }
    }
}
