using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace AppZim.ZIM
{
    public class AssetCRM
    {
        public static string fromAddress = "tuyensinh@iccacademy.edu.vn";
        public static string mailPassword = "nymsmpwprjpkmlvl";       // Mail id password from where mail will be sent.

        /// <summary>
        /// Tạo chuỗi ký tự gồm số
        /// </summary>
        /// <param name="numberrandom">ĐỘ dài kí tự</param>
        /// <returns></returns>
        public static string RandomString(int numberrandom)
        {
            //var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var chars = "0123456789";
            var stringChars = new char[numberrandom];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);
            return finalString;
        }

        public static string RandomStringWithText(int numberrandom)
        {
            //var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var stringChars = new char[numberrandom];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);
            return finalString;
        }

        public static string UrlBeauty(string title)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9]");
            title = rgx.Replace(title, "-");
            return title;
        }
        public static void SendMailAmazone(string strTo, string title, string content)
        {
            try
            {


                string fromAddressAmazon = "AKIAQAFF5ORWHYZB7EMW";
                string mailPasswordAmazon = "BIY2OIqJAPEPiDOaDbiiB/AgNC/rgXID9+GcGx9gXb1i";
                // Replace sender@example.com with your "From" address. 
                // This address must be verified with Amazon SES.
                String FROM = "admin@asw.vn";
                String FROMNAME = "app.asw.vn";

                // Replace smtp_username with your Amazon SES SMTP user name.
                String SMTP_USERNAME = fromAddressAmazon;

                // Replace smtp_password with your Amazon SES SMTP password.
                String SMTP_PASSWORD = mailPasswordAmazon;

                // (Optional) the name of a configuration set to use for this message.
                // If you comment out this line, you also need to remove or comment out
                // the "X-SES-CONFIGURATION-SET" header below.
                //String CONFIGSET = "ConfigSet";

                // If you're using Amazon SES in a region other than US West (Oregon), 
                // replace email-smtp.us-west-2.amazonaws.com with the Amazon SES SMTP  
                // endpoint in the appropriate AWS Region.
                String HOST = "email-smtp.us-east-2.amazonaws.com";

                // The port you will connect to on the Amazon SES SMTP endpoint. We
                // are choosing port 587 because we will use STARTTLS to encrypt
                // the connection.
                int PORT = 587;

                // The subject line of the email
                String SUBJECT = title;

                // The body of the email
                String BODY = content;

                // Create and build a new MailMessage object
                MailMessage message = new MailMessage();
                message.IsBodyHtml = true;
                message.From = new MailAddress(FROM, FROMNAME);
                message.To.Add(new MailAddress(strTo));

                message.Subject = SUBJECT;
                message.Body = BODY;
                // Comment or delete the next line if you are not using a configuration set
                //message.Headers.Add("X-SES-CONFIGURATION-SET", CONFIGSET);

                //using (var client = new System.Net.Mail.SmtpClient(HOST, PORT))
                //{
                //    // Pass SMTP credentials
                //    client.Credentials =
                //        new NetworkCredential(SMTP_USERNAME, SMTP_PASSWORD);
                //    // Enable SSL encryption
                //    client.EnableSsl = true;
                //    client.Send(message);
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //public static void SendMail(string strTo, string title, string content)
        //{
        //    try
        //    {
        //        // Create smtp connection.
        //        SmtpClient client = new SmtpClient();
        //        client.Port = 587;//outgoing port for the mail.
        //        client.Host = "smtp.gmail.com";
        //        client.EnableSsl = true;
        //        client.Timeout = 10000;
        //        client.DeliveryMethod = SmtpDeliveryMethod.Network;
        //        client.UseDefaultCredentials = false;
        //        client.Credentials = new NetworkCredential(fromAddress, mailPassword);

        //        // Fill the mail form.
        //        var send_mail = new MailMessage();
        //        send_mail.IsBodyHtml = true;
        //        //address from where mail will be sent.
        //        send_mail.From = new MailAddress(fromAddress);
        //        //address to which mail will be sent.           
        //        send_mail.To.Add(new MailAddress(strTo));
        //        //subject of the mail.
        //        send_mail.Subject = title;
        //        send_mail.Body = content;
        //        client.Send(send_mail);
        //    }
        //    catch (Exception ex)
        //    {
        //        Writelog("SendMail", "AssetCRM", 0, ex.Message + ex.InnerException);
        //        throw ex;
        //    }
        //}

        public static void SendMail(string strTo, string title, string content)
        {
            try
            {
                // Create smtp connection.
                SmtpClient client = new SmtpClient();
                client.Port = 587;//outgoing port for the mail.
                client.Host = "smtp.office365.com";
                client.EnableSsl = true;
                client.Timeout = 20000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(fromAddress, mailPassword);

                // Fill the mail form.
                var send_mail = new MailMessage();
                send_mail.IsBodyHtml = true;
                //address from where mail will be sent.
                send_mail.From = new MailAddress(fromAddress);
                //address to which mail will be sent.           
                send_mail.To.Add(new MailAddress(strTo));
                //subject of the mail.
                send_mail.Subject = title;
                send_mail.Body = content;
                client.Send(send_mail);
            }
            catch (Exception ex)
            {
                Writelog("SendMail", "AssetCRM", 0, ex.Message + ex.InnerException);
                throw ex;
            }
        }

        public static bool SendMailMultiple(string strTo, string strSubject, string strMsg, string cc)
        {
            try
            {
                string strFrom = fromAddress;
                string pass = mailPassword;
                // Create the mail message
                MailMessage objMailMsg = new MailMessage(strFrom, strTo);

                objMailMsg.BodyEncoding = Encoding.UTF8;
                objMailMsg.Subject = strSubject;
                objMailMsg.CC.Add(cc);
                objMailMsg.IsBodyHtml = true;
                objMailMsg.Body = strMsg;
                SmtpClient objSMTPClient = new SmtpClient();

                objSMTPClient.Host = "smtp.office365.com";
                objSMTPClient.Port = 587;
                objSMTPClient.EnableSsl = true;
                objSMTPClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                objSMTPClient.UseDefaultCredentials = false;
                objSMTPClient.Credentials = new NetworkCredential(strFrom, pass);
                objSMTPClient.Timeout = 20000;
                objSMTPClient.Send(objMailMsg);
                return true;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static bool SendMailMultipleBcc(string strSubject, string strMsg, string emailListStr)
        {
            try
            {
                string strFrom = fromAddress;
                string pass = mailPassword;
                //Smtp
                SmtpClient objSMTPClient = new SmtpClient();
                objSMTPClient.Host = "smtp.office365.com";
                objSMTPClient.Port = 587;
                objSMTPClient.EnableSsl = true;
                objSMTPClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                objSMTPClient.UseDefaultCredentials = false;
                objSMTPClient.Credentials = new NetworkCredential(strFrom, pass);
                objSMTPClient.Timeout = 20000;
                // Create the mail message
                MailMessage objMailMsg = new MailMessage();
                objMailMsg.From = new MailAddress(strFrom);
                objMailMsg.BodyEncoding = Encoding.UTF8;
                objMailMsg.Subject = strSubject;
                objMailMsg.IsBodyHtml = true;
                objMailMsg.Body = strMsg;
                var emails = emailListStr.Split(',');
                foreach (string email in emails)
                {
                    objMailMsg.Bcc.Add(new MailAddress(email));
                }
                objSMTPClient.Send(objMailMsg);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        //tạo hình gift-card
        public static Bitmap CreateBarcode(string Code)
        {
            // Multiply the lenght of the code by 40 (just to have enough width)
            int w = Code.Length * 32;

            // Create a bitmap object of the width that we calculated and height of 100
            Bitmap oBitmap = new Bitmap(w, 100);

            // then create a Graphic object for the bitmap we just created.
            Graphics oGraphics = Graphics.FromImage(oBitmap);

            // Now create a Font object for the Barcode Font
            // (in this case the IDAutomationHC39M) of 18 point size
            Font oFont = new Font("IDAutomationHC39M", 18);

            // Let's create the Point and Brushes for the barcode
            PointF oPoint = new PointF(2f, 2f);
            SolidBrush oBrushWrite = new SolidBrush(Color.Black);
            SolidBrush oBrush = new SolidBrush(Color.White);

            // Now lets create the actual barcode image
            // with a rectangle filled with white color
            oGraphics.FillRectangle(oBrush, 0, 0, w, 100);

            // We have to put prefix and sufix of an asterisk (*),
            // in order to be a valid barcode
            oGraphics.DrawString("*" + Code + "*", oFont, oBrushWrite, oPoint);

            // Then we send the Graphics with the actual barcode
            return oBitmap;
        }

        //tạo QR code
        public static string CreateQRCode(string data, string name)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeGenerator.QRCode qrCode = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.M);
            System.Web.UI.WebControls.Image imgBarCode = new System.Web.UI.WebControls.Image();
            imgBarCode.Height = 150;
            imgBarCode.Width = 150;
            using (Bitmap bitMap = qrCode.GetGraphic(20))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    bitMap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] byteImage = ms.ToArray();
                    imgBarCode.ImageUrl = "data:image/png;base64," + Convert.ToBase64String(byteImage);

                    //Convert Base64 Encoded string to Byte Array.
                    byte[] imageBytes = Convert.FromBase64String(Convert.ToBase64String(byteImage));
                    string filePath = System.Web.HttpContext.Current.Server.MapPath("~/Upload/qrcode/" + name + ".jpg"); //xxx here
                    File.WriteAllBytes(filePath, imageBytes);
                }
                return "/Upload/qrcode/" + name + ".jpg";
            }
        }
        //tạo QR code
        public static string CreateQRCodeContract(string data, string name)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeGenerator.QRCode qrCode = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.M);
            System.Web.UI.WebControls.Image imgBarCode = new System.Web.UI.WebControls.Image();
            imgBarCode.Height = 150;
            imgBarCode.Width = 150;
            using (Bitmap bitMap = qrCode.GetGraphic(20))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    bitMap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] byteImage = ms.ToArray();
                    imgBarCode.ImageUrl = "data:image/png;base64," + Convert.ToBase64String(byteImage);

                    //Convert Base64 Encoded string to Byte Array.
                    byte[] imageBytes = Convert.FromBase64String(Convert.ToBase64String(byteImage));
                    string filePath = System.Web.HttpContext.Current.Server.MapPath("~/Upload/qrcodeCT/" + name + ".jpg");
                    File.WriteAllBytes(filePath, imageBytes);
                }
                return "/Upload/qrcodeCT/" + name + ".jpg";
            }
        }


        /// <summary>
        /// Chuyển số tiền sang chữ
        /// </summary>
        /// <param name="total">Số tiền cần đọc</param>
        /// <returns></returns>
        public static string MoneyToText(int total)
        {
            try
            {
                string rs = "";
                string[] ch = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
                string[] rch = { "lẻ", "mốt", "", "", "", "lăm" };
                string[] u = { "", "mươi", "trăm", "ngàn", "", "", "triệu", "", "", "tỷ", "", "", "ngàn", "", "", "triệu" };
                string nstr = total.ToString();

                int[] n = new int[nstr.Length];
                int len = n.Length;
                for (int i = 0; i < len; i++)
                {
                    n[len - 1 - i] = Convert.ToInt32(nstr.Substring(i, 1));
                }

                for (int i = len - 1; i >= 0; i--)
                {
                    if (i % 3 == 2)// số 0 ở hàng trăm
                    {
                        if (n[i] == 0 && n[i - 1] == 0 && n[i - 2] == 0) continue;//nếu cả 3 số là 0 thì bỏ qua không đọc
                    }
                    else if (i % 3 == 1) // số ở hàng chục
                    {
                        if (n[i] == 0)
                        {
                            if (n[i - 1] == 0) { continue; }// nếu hàng chục và hàng đơn vị đều là 0 thì bỏ qua.
                            else
                            {
                                rs += " " + rch[n[i]]; continue;// hàng chục là 0 thì bỏ qua, đọc số hàng đơn vị
                            }
                        }
                        if (n[i] == 1)//nếu số hàng chục là 1 thì đọc là mười
                        {
                            rs += " mười"; continue;
                        }
                    }
                    else if (i != len - 1)// số ở hàng đơn vị (không phải là số đầu tiên)
                    {
                        if (n[i] == 0)// số hàng đơn vị là 0 thì chỉ đọc đơn vị
                        {
                            if (i + 2 <= len - 1 && n[i + 2] == 0 && n[i + 1] == 0) continue;
                            rs += " " + (i % 3 == 0 ? u[i] : u[i % 3]);
                            continue;
                        }
                        if (n[i] == 1)// nếu là 1 thì tùy vào số hàng chục mà đọc: 0,1: một / còn lại: mốt
                        {
                            rs += " " + ((n[i + 1] == 1 || n[i + 1] == 0) ? ch[n[i]] : rch[n[i]]);
                            rs += " " + (i % 3 == 0 ? u[i] : u[i % 3]);
                            continue;
                        }
                        if (n[i] == 5) // cách đọc số 5
                        {
                            if (n[i + 1] != 0) //nếu số hàng chục khác 0 thì đọc số 5 là lăm
                            {
                                rs += " " + rch[n[i]];// đọc số 
                                rs += " " + (i % 3 == 0 ? u[i] : u[i % 3]);// đọc đơn vị
                                continue;
                            }
                        }
                    }
                    rs += (rs == "" ? " " : ", ") + ch[n[i]];// đọc số
                    rs += " " + (i % 3 == 0 ? u[i] : u[i % 3]);// đọc đơn vị
                }
                if (rs[rs.Length - 1] != ' ')
                    rs += " đồng";
                else
                    rs += "đồng";

                if (rs.Length > 2)
                {
                    string rs1 = rs.Substring(0, 2);
                    rs1 = rs1.ToUpper();
                    rs = rs.Substring(2);
                    rs = rs1 + rs;
                }
                //return rs.Trim().Replace("lẻ,", "lẻ").Replace("mươi,", "mươi").Replace("trăm,", "trăm").Replace("mười,", "mười");
                return rs.Trim().Replace(",", "");
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Tạo mã màu random
        /// </summary>
        /// <returns></returns>
        public static string RandomColor()
        {
            var random = new Random();
            var CustomerColor = String.Format("#{0:X6}", random.Next(0x1000000));
            string color = CustomerColor.ToString();
            return color;
        }

        public static string RemoveHTMLTags(string content)
        {
            var cleaned = string.Empty;
            try
            {
                StringBuilder textOnly = new StringBuilder();
                using (var reader = XmlNodeReader.Create(new System.IO.StringReader("<xml>" + content + "</xml>")))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Text)
                            textOnly.Append(reader.ReadContentAsString());
                    }
                }
                cleaned = textOnly.ToString();
            }
            catch
            {
                //A tag is probably not closed. fallback to regex string clean.
                string textOnly = string.Empty;
                Regex tagRemove = new Regex(@"<[^>]*(>|$)");
                Regex compressSpaces = new Regex(@"[\s\r\n]+");
                textOnly = tagRemove.Replace(content, string.Empty);
                textOnly = compressSpaces.Replace(textOnly, " ");
                cleaned = textOnly;
            }

            return cleaned;
        }
        /// <summary>
        /// Xóa dấu tiếng việt
        /// </summary>
        /// <param name="text">chuỗi cần xóa</param>
        /// <returns></returns>
        public static string RemoveUnicode(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";
            string[] arr1 = new string[] { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
            "đ",
            "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",
            "í","ì","ỉ","ĩ","ị",
            "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",
            "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",
            "ý","ỳ","ỷ","ỹ","ỵ",};

            string[] arr2 = new string[] { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
            "d",
            "e","e","e","e","e","e","e","e","e","e","e",
            "i","i","i","i","i",
            "o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o",
            "u","u","u","u","u","u","u","u","u","u","u",
            "y","y","y","y","y",};
            for (int i = 0; i < arr1.Length; i++)
            {
                text = text.Replace(arr1[i], arr2[i]);
                text = text.Replace(arr1[i].ToUpper(), arr2[i].ToUpper());
            }
            return text;
        }
        public static bool CheckUnicode(string text)
        {
            string[] arr1 = new string[] { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
            "đ",
            "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",
            "í","ì","ỉ","ĩ","ị",
            "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",
            "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",
            "ý","ỳ","ỷ","ỹ","ỵ",};
            bool check = false;
            for (int i = 0; i < arr1.Length; i++)
            {
                if (text.Contains(arr1[i]))
                    check = true;
            }
            return check;
        }

        //Push noti desktop
        public class jsonNoti
        {
            public string title { get; set; }
            public string message { get; set; }
            public string icon { get; set; }
            public string link { get; set; }
        }
        public static string vapidPublicKey = "BLV67mH2vJ089lrdChQhSzSwJgWXvpKBdwgZ-AzuDpmlKGlZPtCbH_AD28gDnd7u42srBlEQLmbRYf46thgGIzI";
        public static string vapidPrivateKey = "RTQLMmzY3Ey72ELoJpW-_gDJbC-v_sG7d8r9JKalM0c";
        //public static void PushNotiDesktop(int UID, string title, string link)// đẩy thông báo xuống desktop trình duyệt
        //{
        //    //var ltoken = DeviceBrowserTable.getbyuid(UID);
        //    //if (ltoken != null)
        //    //{
        //    //    //var noti = NotificationTable.GetByID(NotiID);
        //    //    //string notiPush = "{\"title\":\"PM.MONA.MEDIA\",\"message\":\"" + noti.NotificationContent + "\"}";

        //    //    jsonNoti n = new jsonNoti();
        //    //    n.title = "app.zim.vn";
        //    //    //n.message = noti.NotificationTitle;
        //    //    n.message = title;
        //    //    n.icon = "https://app.zim.vn/app-assets/favicon-zim.jpg";
        //    //    n.link = link;
        //    //    var notiPush = new JavaScriptSerializer().Serialize(n);
        //    //    foreach (var item in ltoken)
        //    //    {
        //    //        try
        //    //        {
        //    //            var pushSubscription = new PushSubscription(item.PushEndpoint, item.PushP256DH, item.PushAuth);
        //    //            var vapidDetails = new VapidDetails("mailto:vun4m389@gmail.com", vapidPublicKey, vapidPrivateKey);
        //    //            var webPushClient = new WebPushClient();
        //    //            webPushClient.SendNotification(pushSubscription, notiPush, vapidDetails);
        //    //        }
        //    //        catch
        //    //        {
        //    //            DeviceBrowserTable.updatehide(item.ID, "auto");
        //    //        }
        //    //    }
        //    //}
        //}
        /// <summary>
        /// Tạo hình thubnail
        /// </summary>
        /// <param name="sourcefile">File gốc</param>
        /// <param name="destinationfile">Đương dẫn luu hình nhỏ</param>
        /// <param name="width">kích thước</param>
        public static void GenerateThumbNail(string sourcefile, string destinationfile, int width, bool post)
        {
            System.Drawing.Image image = System.Drawing.Image.FromFile(HttpContext.Current.Server.MapPath(sourcefile));
            int srcWidth = image.Width;
            int srcHeight = image.Height;
            int thumbWidth = width;
            int thumbHeight;
            Bitmap bmp;
            if (srcHeight > srcWidth)
            {

                thumbWidth = width;
                double percent = srcHeight * 1.0 / thumbWidth * 1.0;
                thumbHeight = Convert.ToInt32(Math.Round(srcHeight * 1.0 / percent));
                bmp = new Bitmap(thumbWidth, thumbHeight);
            }
            else
            {
                //thumbHeight = thumbWidth;
                //thumbWidth = (srcWidth / srcHeight) * thumbHeight;
                //bmp = new Bitmap(thumbWidth, thumbHeight);
                if (post)
                {
                    double percent = srcWidth * 1.0 / width * 1.0;
                    thumbHeight = Convert.ToInt32(Math.Round(srcHeight * 1.0 / percent));
                    bmp = new Bitmap(width, thumbHeight);
                }
                else
                {
                    thumbHeight = width;
                    double percent = srcHeight * 1.0 / thumbHeight * 1.0;
                    thumbWidth = Convert.ToInt32(Math.Round(srcWidth * 1.0 / percent));
                    bmp = new Bitmap(thumbWidth, thumbHeight);
                }
            }

            System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bmp);
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            gr.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            System.Drawing.Rectangle rectDestination = new System.Drawing.Rectangle(0, 0, thumbWidth, thumbHeight);
            gr.DrawImage(image, rectDestination, 0, 0, srcWidth, srcHeight, GraphicsUnit.Pixel);
            bmp.Save(HttpContext.Current.Server.MapPath(destinationfile));
            bmp.Dispose();
            image.Dispose();
        }
        #region Kiểm tra có phải là hình hay ko
        private enum ImageFileExtension
        {
            none = 0,
            jpg = 1,
            jpeg = 2,
            bmp = 3,
            gif = 4,
            png = 5
        }
        public enum FileType
        {
            Image = '1',
            Video = '2',
            PDF = '3',
            Text = '4',
            DOC = '5',
            DOCX = '6',
            PPT = '7',
        }

        public static bool isValidFile(byte[] bytFile, string flType, String FileContentType)
        {
            bool isvalid = false;
            if (flType == ".jpg" || flType == ".jpeg" || flType == ".png")
            {
                isvalid = isValidImageFile(bytFile, FileContentType);//we are going call this method
            }
            //else if (flType == FileType.Video)
            //{
            //    isvalid = isValidVideoFile(bytFile, FileContentType);
            //}
            //else if (flType == FileType.PDF)
            //{
            //    isvalid = isValidPDFFile(bytFile, FileContentType);
            //}
            return isvalid;
        }


        public static bool isValidImageFile(byte[] bytFile, String FileContentType)
        {
            bool isvalid = false;

            byte[] chkBytejpg = { 255, 216, 255, 224 };
            byte[] chkBytebmp = { 66, 77 };
            byte[] chkBytegif = { 71, 73, 70, 56 };
            byte[] chkBytepng = { 137, 80, 78, 71 };

            ImageFileExtension imgfileExtn = ImageFileExtension.none;

            if (FileContentType.Contains("jpg") | FileContentType.Contains("jpeg"))
            {
                imgfileExtn = ImageFileExtension.jpg;
            }
            else if (FileContentType.Contains("png"))
            {
                imgfileExtn = ImageFileExtension.png;
            }
            else if (FileContentType.Contains("bmp"))
            {
                imgfileExtn = ImageFileExtension.bmp;
            }
            else if (FileContentType.Contains("gif"))
            {
                imgfileExtn = ImageFileExtension.gif;
            }

            if (imgfileExtn == ImageFileExtension.jpg || imgfileExtn == ImageFileExtension.jpeg)
            {
                if (bytFile.Length >= 4)
                {
                    int j = 0;
                    for (Int32 i = 0; i <= 3; i++)
                    {
                        if (bytFile[i] == chkBytejpg[i])
                        {
                            j = j + 1;
                            if (j == 3)
                                isvalid = true;
                        }
                    }
                }
            }
            if (imgfileExtn == ImageFileExtension.png)
            {
                if (bytFile.Length >= 4)
                {
                    int j = 0;
                    for (Int32 i = 0; i <= 3; i++)
                    {
                        if (bytFile[i] == chkBytepng[i])
                        {
                            j = j + 1;
                            if (j == 3)
                                isvalid = true;
                        }
                    }
                }
            }
            if (imgfileExtn == ImageFileExtension.bmp)
            {
                if (bytFile.Length >= 4)
                {
                    int j = 0;
                    for (Int32 i = 0; i <= 1; i++)
                    {
                        if (bytFile[i] == chkBytebmp[i])
                        {
                            j = j + 1;
                            if (j == 2)
                                isvalid = true;
                        }
                    }
                }
            }

            if (imgfileExtn == ImageFileExtension.gif)
            {
                if (bytFile.Length >= 4)
                {
                    int j = 0;
                    for (Int32 i = 0; i <= 1; i++)
                    {
                        if (bytFile[i] == chkBytegif[i])
                        {
                            j = j + 1;
                            if (j == 3)
                                isvalid = true;
                        }
                    }
                }
            }
            return isvalid;
        }

        #endregion

        /// <summary>
        /// Ghi file log lỗi hằng ngày
        /// </summary>
        /// <param name="page">Trag lỗi, controller</param>
        /// <param name="function">Hàm, view lỗi</param>
        /// <param name="loginUID">ID đăng đăng nhập</param>
        /// <param name="contenterror">Thông báo lỗi</param>
        public static void Writelog(string page, string function, int loginUID, string contenterror)
        {
            string name = GetDateTime.Now.ToString("dd-MM-yyyy");
            string content = "PageError: " + page + "    FunctionError: " + function + "    loginUID: " + loginUID + "    MessageError: " + contenterror + "    " + GetDateTime.Now.ToString("dd/MM/yyyy HH:mm") + "" + Environment.NewLine;
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "/FileLog/log" + name + ".txt";
            File.AppendAllText(filePath, content);
        }
        public static int GetVideoDuration(string filePath)
        {
            //var player = new WindowsMediaPlayer();
            //var clip = player.newMedia(filePath);
            //TimeSpan span = TimeSpan.FromSeconds(clip.duration);
            //int totalMinutes = Math.Round(span.TotalMinutes, 0).ToString().ToInt(0);
            //return totalMinutes;

            //// Microsoft.WindowsAPICodePack - Shell NuGet package
            //using (var shell = ShellObject.FromParsingName(filePath))
            //{
            //    IShellProperty prop = shell.Properties.System.Media.Duration;
            //    var t = (ulong)prop.ValueAsObject;
            //    TimeSpan span = TimeSpan.FromTicks((long)t);
            //    int totalMinutes = Math.Round(span.TotalMinutes, 0).ToString().ToInt(0);
            //    return totalMinutes;
            //}

            TagLib.File file = TagLib.File.Create(filePath);
            int s_time = (int)file.Properties.Duration.TotalSeconds;
            int s_minutes = s_time / 60;
            int s_seconds = s_time % 60;
            if (s_seconds > 20)
                s_minutes += 1;
            return s_minutes;
        }
        public static bool isValidImageAndVideo(string flType)
        {
            bool isvalid = false;
            if (flType == ".jpg" || flType == ".jpeg" || flType == ".png" || flType == ".bmp" || flType == ".mp4" || flType == ".flv" || flType == ".mpeg" || flType == ".mov")
            {
                isvalid = true;
            }
            return isvalid;
        }
        //làm tròn điểm thi 
        public static double RoundScore(double score)
        {
            double sodutemp = score - (int)score;
            double sodu = 0;
            int songuyen = (int)score;
            if (sodutemp < 0.125)
            {
                sodu = 0;
            }
            else if (sodutemp >= 0.125 && sodutemp < 0.375)
            {
                sodu = 0.25;
            }
            else if (sodutemp >= 0.375 && sodutemp < 0.625)
            {
                sodu = 0.5;
            }
            else if (sodutemp >= 0.625 && sodutemp < 0.875)
            {
                sodu = 0.75;
            }
            else { sodu = 1; }
            return songuyen + sodu;
        }
        public static bool isValidFileCustom(string flType)
        {
            bool isvalid = false;
            if (flType == ".jpg" || flType == ".jpeg" || flType == ".png" || flType == ".bmp")
            {
                isvalid = true;
            }
            return isvalid;
        }
        public static string CalcPersent(int thangnay, int thangtruoc)
        {
            string df = "";
            double per = 0.0;
            if (thangtruoc == 0)
            {
                return "<span class='text-success d-flex'><i class='fas fa-level-up-alt'></i>" + (thangnay * 100).ToString() + "%</span>";
            }
            if (thangnay == 0)
            {
                return "<span class='text-danger d-flex'><i class='fas fa-level-down-alt'></i>" + (thangtruoc * 100).ToString() + "%</span>";
            }
            if (thangnay >= thangtruoc)
            {
                per = Math.Round((double)(thangnay - thangtruoc) / thangtruoc, 2);
                per = per * 100;
                df = "<span class='text-success d-flex'><i class='fas fa-level-up-alt'></i>" + per.ToString() + "%</span>";
            }
            else
            {
                per = Math.Round((double)(thangtruoc - thangnay) / thangtruoc, 2);
                per = per * 100;
                df = "<span class='text-danger d-flex'><i class='fas fa-level-down-alt'></i>" + per.ToString() + "%</span>";
            }
            return df;
        }
        public static string DateToText(DateTime from, DateTime to)
        {
            int totalDays = Convert.ToInt32(to.Subtract(from).TotalDays);
            int years = Convert.ToInt32(totalDays / 365);
            int months = (totalDays - (years * 365)) / 30;
            int days = totalDays - (years * 365) - (months * 30);
            if (years > 0)
            {
                return years + " năm " + months + " tháng " + days + " ngày";
            }
            if (months > 0)
            {
                return months + " tháng " + days + " ngày";
            }
            if (days > 0)
            {
                return days + " ngày";
            }
            return from.ToString("dd/MM/yyyy");
        }
    }
}