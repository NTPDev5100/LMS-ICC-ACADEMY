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
    
    public partial class API_ticket_getlist_Result
    {
        public int ID { get; set; }
        public Nullable<int> Total { get; set; }
        public Nullable<int> UID { get; set; }
        public string FullName { get; set; }
        public Nullable<bool> IsHidden { get; set; }
        public Nullable<int> TypeID { get; set; }
        public string TypeName { get; set; }
        public string Title { get; set; }
        public string ContentFeedBack { get; set; }
        public Nullable<bool> isNew { get; set; }
        public string CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<int> Rate { get; set; }
        public Nullable<int> SupportID { get; set; }
        public string SupportName { get; set; }
        public string RateDate { get; set; }
        public Nullable<bool> RejectCheckFix { get; set; }
        public string RejectCheckFixDate { get; set; }
    }
}
