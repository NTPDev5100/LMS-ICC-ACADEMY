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
    
    public partial class tbl_AcademicLevel
    {
        public int ID { get; set; }
        public string LevelName { get; set; }
        public Nullable<int> Position { get; set; }
        public Nullable<int> Language { get; set; }
        public string LanguageName { get; set; }
        public string LevelContent { get; set; }
        public Nullable<bool> IsHide { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
    }
}