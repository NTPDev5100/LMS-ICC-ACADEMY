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
    
    public partial class API_doneexercise_getlist_Result
    {
        public int ID { get; set; }
        public Nullable<int> StudentID { get; set; }
        public string StudentName { get; set; }
        public Nullable<int> CourseID { get; set; }
        public string CourseName { get; set; }
        public string FullNameCreatedByExercise { get; set; }
        public string ExerciseQuestion { get; set; }
        public Nullable<double> Scores { get; set; }
        public Nullable<int> Total { get; set; }
        public string CreatedDate { get; set; }
    }
}
