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
    
    public partial class tbl_SetPackage
    {
        public int ID { get; set; }
        public string PackageName { get; set; }
        public Nullable<int> Price { get; set; }
        public Nullable<bool> Show { get; set; }
        public Nullable<System.DateTime> Deadline { get; set; }
        public string PackageIMG { get; set; }
        public string PackageIMGThumbnail { get; set; }
        public string Note { get; set; }
        public Nullable<bool> Listening { get; set; }
        public Nullable<bool> Speaking { get; set; }
        public Nullable<bool> Reading { get; set; }
        public Nullable<bool> Writing { get; set; }
        public Nullable<bool> isHide { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
    }
}
