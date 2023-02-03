using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace AppZim.TableSql
{
    public class GetDataPaypal
    {
        // get data when checkout success !^^
        public string GetPayPalResponse(string tx)
        {
            try
            {
                string authToken = WebConfigurationManager.AppSettings["Token"];
                string txToken = tx;
                string query = string.Format("cmd=_notify-synch&tx={0}&at={1}", txToken, authToken);
                string url = WebConfigurationManager.AppSettings["urlSubmitPayment"];
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = query.Length;
                StreamWriter outStreamWriter =
                new StreamWriter(req.GetRequestStream(), Encoding.ASCII);
                outStreamWriter.Write(query);
                outStreamWriter.Close();
                StreamReader reader = new StreamReader(req.GetResponse().GetResponseStream());
                string strResponse = reader.ReadToEnd();
                reader.Close();
                if (strResponse.StartsWith("SUCCESS"))
                    return strResponse;
                else
                    return String.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }


        public Order InformationOrder(string data)
        {
            string key, value;
            var order = new Order();
            order.ItemName = "Khóa học quản lý";
            order.GrossTotal = 100;
            return order;

            //try
            //{
            //    string[] strArray = data.Split('\n');
            //    for (int i = 1; i < strArray.Length - 1; i++)
            //    {
            //        string[] strArrayTemp = strArray[i].Split('=');
            //        key = strArrayTemp[0];
            //        value = HttpUtility.UrlDecode(strArrayTemp[1]);
            //        switch (key)
            //        {
            //            case "mc_gross":
            //                order.GrossTotal = double.Parse(value);
            //                break;
            //            case "invoice":
            //                order.InvoiceNumber = int.Parse(value);
            //                break;
            //            case "payment_status":
            //                order.PaymentStatus = value;
            //                break;
            //            case "first_name":
            //                order.PayerFirstName = value;
            //                break;
            //            case "mc_fee":
            //                order.PaymentFee = double.Parse(value);
            //                break;
            //            case "business":
            //                order.BusinessEmail = value;
            //                break;
            //            case "payer_email":
            //                order.PayerEmail = value;
            //                break;
            //            case "Tx Token":
            //                order.TxToken = value;
            //                break;
            //            case "last_name":
            //                order.PayerLastName = value;
            //                break;
            //            case "receiver_email":
            //                order.ReceiverEmail = value;
            //                break;
            //            case "item_name":
            //                order.ItemName = value;
            //                break;
            //            case "mc_currency":
            //                order.Currency = value;
            //                break;
            //            case "txn_id":
            //                order.ReceiverEmail = value;
            //                break;
            //            case "custom":
            //                order.Custom = value;
            //                break;
            //            case "subscr_id":
            //                order.SubscriberId = value;
            //                break;
            //        }
            //    }
            //    return order;
            //}
            //catch
            //{
            //    return null;
            //}
        }
    }
    public class Order
    {

        // get infor of order
        public double GrossTotal { get; set; }
        public int InvoiceNumber { get; set; }
        public string PaymentStatus { get; set; }
        public string PayerFirstName { get; set; }
        public double PaymentFee { get; set; }
        public string BusinessEmail { get; set; }
        public string PayerEmail { get; set; }
        public string TxToken { get; set; }
        public string PayerLastName { get; set; }
        public string ReceiverEmail { get; set; }
        public string ItemName { get; set; }
        public string Currency { get; set; }
        public string TransactionId { get; set; }
        public string SubscriberId { get; set; }
        public string Custom { get; set; }
    }
}