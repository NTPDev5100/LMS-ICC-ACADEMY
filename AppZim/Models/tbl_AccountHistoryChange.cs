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
    
    public partial class tbl_AccountHistoryChange
    {
        public int ID { get; set; }
        public Nullable<int> UID { get; set; }
        public string AccountName { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public string ContenChange { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string ChangeName { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
    }
}
