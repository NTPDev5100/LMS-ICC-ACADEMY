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
    
    public partial class CoursesSchedule_GetNotAttendance_Result
    {
        public int ID { get; set; }
        public string Email { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public string sTime { get; set; }
        public string eTime { get; set; }
        public Nullable<int> CourseID { get; set; }
        public string CourseName { get; set; }
    }
}
