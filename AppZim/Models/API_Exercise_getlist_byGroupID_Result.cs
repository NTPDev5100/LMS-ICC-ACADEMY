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
    
    public partial class API_Exercise_getlist_byGroupID_Result
    {
        public int ID { get; set; }
        public Nullable<int> ExerciseGroupID { get; set; }
        public string ExerciseTitle { get; set; }
        public string FileName { get; set; }
        public Nullable<int> Type { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<int> HaveDeadline { get; set; }
        public string LinkFile { get; set; }
        public int IsDone { get; set; }
        public int Remarked { get; set; }
        public Nullable<int> CourseID { get; set; }
    }
}
