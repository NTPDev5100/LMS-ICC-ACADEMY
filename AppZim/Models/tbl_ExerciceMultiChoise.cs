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
    
    public partial class tbl_ExerciceMultiChoise
    {
        public int ID { get; set; }
        public Nullable<int> ExerciseID { get; set; }
        public string AnswerTitle { get; set; }
        public Nullable<bool> isTrue { get; set; }
        public Nullable<bool> isHide { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<int> ExerciseGroupID { get; set; }
        public Nullable<bool> IsNoise { get; set; }
    }
}
