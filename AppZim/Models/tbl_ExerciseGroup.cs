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
    
    public partial class tbl_ExerciseGroup
    {
        public int ID { get; set; }
        public Nullable<int> CourseID { get; set; }
        public Nullable<int> CurriculumID { get; set; }
        public Nullable<int> TypeGroup { get; set; }
        public string TypeGroupName { get; set; }
        public string ExerciseGroupName { get; set; }
        public Nullable<int> CountExe { get; set; }
        public Nullable<bool> isHide { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<int> SetPackageID { get; set; }
        public Nullable<int> TypeSkill { get; set; }
        public string LinkAudio { get; set; }
        public Nullable<int> TimeOut { get; set; }
        public string SetPackageQuestion { get; set; }
        public Nullable<int> TypeMultiChoice { get; set; }
        public string SetPackageDescription { get; set; }
        public Nullable<bool> SetPackage { get; set; }
        public Nullable<int> SetPackageDetailID { get; set; }
        public Nullable<int> ExerciseGroupID { get; set; }
        public string Introduction { get; set; }
        public Nullable<bool> IsDelete { get; set; }
        public Nullable<int> CurriculumnDetailID { get; set; }
    }
}
