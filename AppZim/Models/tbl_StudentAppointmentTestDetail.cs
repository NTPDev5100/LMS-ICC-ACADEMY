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
    
    public partial class tbl_StudentAppointmentTestDetail
    {
        public int ID { get; set; }
        public Nullable<int> StudentAppointmentTestID { get; set; }
        public Nullable<int> AdvisoryUID { get; set; }
        public Nullable<int> TeacherTestID { get; set; }
        public string Listening { get; set; }
        public string Speaking { get; set; }
        public string Reading { get; set; }
        public string Writing { get; set; }
        public string Vocab { get; set; }
        public Nullable<int> ConsultingPrice { get; set; }
        public string Note { get; set; }
        public Nullable<bool> isHide { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<int> ReasonType { get; set; }
        public Nullable<int> ReasonID { get; set; }
        public Nullable<bool> IsSendMail { get; set; }
    }
}
