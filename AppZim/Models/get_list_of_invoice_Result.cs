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
    
    public partial class get_list_of_invoice_Result
    {
        public int ID { get; set; }
        public Nullable<int> SchoolID { get; set; }
        public string SchoolName { get; set; }
        public string StudentAddress { get; set; }
        public string StudentFullName { get; set; }
        public string StudentPhone { get; set; }
        public Nullable<int> StudentUID { get; set; }
        public Nullable<int> Price { get; set; }
        public Nullable<int> PriceLeft { get; set; }
        public string ReasonNote { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedInvoice { get; set; }
        public string QRCode { get; set; }
        public string Code { get; set; }
        public string IdentityCard { get; set; }
        public string IdentityCardCity { get; set; }
        public Nullable<System.DateTime> IdentityCardDate { get; set; }
        public Nullable<int> PaymentmethodID { get; set; }
        public string PaymentmethodName { get; set; }
        public Nullable<int> TotalRow { get; set; }
        public Nullable<double> TotalPrice { get; set; }
    }
}
