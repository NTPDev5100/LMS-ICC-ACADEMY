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
    
    public partial class tbl_FeedBack
    {
        public int ID { get; set; }
        public Nullable<int> UID { get; set; }
        public string FullName { get; set; }
        public Nullable<bool> IsHidden { get; set; }
        public Nullable<int> TypeID { get; set; }
        public string TypeName { get; set; }
        public string Title { get; set; }
        public string ContentFeedBack { get; set; }
        public Nullable<bool> isNew { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<int> Rate { get; set; }
        public Nullable<int> SupportID { get; set; }
        public string SupportName { get; set; }
        public Nullable<System.DateTime> RateDate { get; set; }
        public Nullable<bool> RejectCheckFix { get; set; }
        public Nullable<System.DateTime> RejectCheckFixDate { get; set; }
    }
}