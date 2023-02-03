using AppZim.Areas.Admin.Controllers;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace AppZim.ZIM
{
    public class PushNotifyMobile
    {

        public static void SendNotifyMobile(string content, string[] IncludePlayerID)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            var request = WebRequest.Create("https://onesignal.com/api/v1/notifications") as HttpWebRequest;

            request.KeepAlive = true;
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            request.Headers.Add("authorization", "Basic YWM3NDdiZWUtZjEyZi00Y2Q5LTk5MDgtNjY0YTI5ODdjNDM0");

            var serializer = new JavaScriptSerializer();
            var obj = new
            {
                app_id = "2e611cb7-4715-48c6-b145-f256fa33a08f",
                headings = new { en = BaseController.ProjectName },
                contents = new { en = content },
                content_available = true,
                include_player_ids = IncludePlayerID
            };
            var param = serializer.Serialize(obj);
            byte[] byteArray = Encoding.UTF8.GetBytes(param);
            string responseContent = null;

            try
            {
                using (var writer = request.GetRequestStream())
                {
                    writer.Write(byteArray, 0, byteArray.Length);
                }

                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        responseContent = reader.ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
            }

            System.Diagnostics.Debug.WriteLine(responseContent);
        }
    }
}