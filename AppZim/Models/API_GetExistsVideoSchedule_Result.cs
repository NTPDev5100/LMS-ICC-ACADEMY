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
    
    public partial class API_GetExistsVideoSchedule_Result
    {
        public int ID { get; set; }
        public Nullable<int> CourseID { get; set; }
        public Nullable<int> SubjectID { get; set; }
        public Nullable<int> CurriculumsDetailsID { get; set; }
        public Nullable<int> StudyTimeID { get; set; }
        public string SubjectName { get; set; }
        public string TimeName { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public string Files { get; set; }
    }
}