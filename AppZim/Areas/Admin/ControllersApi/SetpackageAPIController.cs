using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using AppZim.Areas.Admin.Controllers;
using AppZim.Models;
using AppZim.TableSql;
using MB.Extensions;

namespace AppZim.Areas.Admin.ControllersApi
{
    public class SetpackageAPIController: ApiController
    {
        [HttpPost]
        public StatusApi ConfirmOrder([FromBody]BodyPackage body)
        {
            StatusApi t = new StatusApi();
            IEnumerable<string> head_key;
            if (Request.Headers.TryGetValues("key", out head_key))
            {
                string key = head_key.FirstOrDefault().ToString();
                if(key != "SHkgxqFpIEh5IMOgLCDEkcOieSBsw6AgQVBJIG5ow6k")
                {
                    t.Code = 201;
                    t.Message = "Fail";
                    return t;
                }
            }
            else
            {
                t.Code = 201;
                t.Message = "Fail";
                return t;
            }         
            int OrderID = body.OrderID;
            var setpackage = SetPackageStudentTable.getbyid(OrderID);
            if (setpackage == null)
            {
                t.Code = 201;
                t.Message = "Fail";
                return t;
            }
            if (setpackage.Paid.Value)
            {
                string url = HttpContext.Current.Request.Url.Authority;
                t.Code = 200;
                t.Message = url + "/StudentSet/MySet";
                return t;
            }
            var ac = AccountTable.getbyID(setpackage.StudentID.Value);
            if (ac == null)
            {

                t.Code = 201;
                t.Message = "Fail";
                return t;
            }
          
            int confirm = body.Confirm.ToInt(0);
            if (confirm == 1 )
            {
                var confirmpackage = SetPackageStudentTable.updatePaid(setpackage.ID, true, "API/ConfirmOrder");
                if (confirmpackage != null)
                {
                    string name = "";
                    name = "7-" + confirmpackage.ID + "-" + GetDateTime.Now.ToString("ddMMyyyyHHmmss") + "";//7 Thanh toán set
                    string nameencode = AppZim.ZIM.Encryptor.EncryptURL(name);
                    string strPathAndQuery = HttpContext.Current.Request.Url.PathAndQuery;
                    string strUrl = HttpContext.Current.Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                    string datalink = "" + strUrl + "invoice/?code=" + nameencode;
                    var barcode = AppZim.ZIM.AssetCRM.CreateQRCode(datalink, name);
                    tbl_Invoices iv = CashierController.InBill(0,6, confirmpackage.ID, ac, confirmpackage.SetPrice.Value, "OnePay","", 0, "", new tbl_School() {ID=0,SchoolName="" },new tbl_Account() { ID=1}, confirmpackage.PaymentType.Value, "OnePay", name, nameencode, datalink);         
                    t.Code = 200;
                    t.Message = datalink;                
                    return t;
                }
                else
                {
                    t.Code = 201;
                    t.Message = "Fail";
                    return t;
                }
            }
            else
            {
                t.Code = 201;
                t.Message = "Fail";
                return t;
            }         
        }
   
    }    
    public class BodyPackage
    {
        public int OrderID { get; set; }
        public string Confirm { get; set; }


    }
    public class StatusApi
    {
        public int Code { get; set; }
        public string Message { get; set; }
    }
}