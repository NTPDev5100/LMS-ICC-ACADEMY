using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace AppZim
{
    public static class PhanTrang
    {
        private static string GetPageUrl(int currentPage, string pageUrl)
        {

            pageUrl = Regex.Replace(pageUrl, "(\\?|\\&)*" + "page=" + currentPage, "");
            if (pageUrl.IndexOf("?") > 0)
            {
                pageUrl += "&page={0}";
            }
            else
            {
                pageUrl += "?page={0}";
            }
            return pageUrl;
        }
        public static MvcHtmlString PhanTrangCustom(int pagesToOutput, int currentPage, int pageCount, string currentPageUrl, string[] strText)
        {
            //Nếu Số trang hiển thị là số lẻ thì tăng thêm 1 thành chẵn
            if (pagesToOutput % 2 != 0)
            {
                pagesToOutput++;
            }
            //Một nửa số trang để đầu ra, đây là số lượng hai bên.
            int pagesToOutputHalfed = pagesToOutput / 2;
            //Url của trang
            string pageUrl = GetPageUrl(currentPage, currentPageUrl);
            //Trang đầu tiên
            int startPageNumbersFrom = currentPage - pagesToOutputHalfed; 
            //Trang cuối cùng
            int stopPageNumbersAt = currentPage + pagesToOutputHalfed; 
            StringBuilder output = new StringBuilder();
            //Nối chuỗi phân trang
            //output.Append("<div class=\"paging\">");
            output.Append("<ul class=\"pagination mg-b-0 justify-content-end\">");
            //Link First(Trang đầu) và Previous(Trang trước)
            if (currentPage > 1)
            {
                
                output.Append("<li class=\"\" ><a title=\"" + strText[1] + "\" href=\"" + string.Format(pageUrl, currentPage - 1) + "\">Previous</a></li>");

            }
            /******************Xác định startPageNumbersFrom & stopPageNumbersAt**********************/
            if (startPageNumbersFrom < 1)
            {
                startPageNumbersFrom = 1;

                //As page numbers are starting at one, output an even number of pages.  
                stopPageNumbersAt = pagesToOutput;
            }

            if (stopPageNumbersAt > pageCount)
            {
                stopPageNumbersAt = pageCount;
            }

            if ((stopPageNumbersAt - startPageNumbersFrom) < pagesToOutput)
            {
                startPageNumbersFrom = stopPageNumbersAt - pagesToOutput;
                if (startPageNumbersFrom < 1)
                {
                    startPageNumbersFrom = 1;
                }
            }
            /******************End: Xác định startPageNumbersFrom & stopPageNumbersAt**********************/

            //Các dấu ... chỉ những trang phía trước  
            if (startPageNumbersFrom > 1)
            {
                output.Append("<li class=\"\"><a href=\"" + string.Format(GetPageUrl(currentPage - 1, pageUrl), startPageNumbersFrom - 1) + "\">&hellip;</a></li>");
            }

            //Duyệt vòng for hiển thị các trang
            for (int i = startPageNumbersFrom; i <= stopPageNumbersAt; i++)
            {
                if (currentPage == i)
                {
                    output.Append("<li class=\"active\" ><a >" + i.ToString() + "</a> </li>");
                }
                else
                {
                    output.Append("<li><a href=\"" + string.Format(pageUrl, i) + "\">" + i.ToString() + "</a> </li>");
                }
            }

            //Các dấu ... chỉ những trang tiếp theo  
            if (stopPageNumbersAt < pageCount)
            {
                output.Append("<li class=\"\" ><a href=\"" + string.Format(pageUrl, stopPageNumbersAt + 1) + "\">&hellip;</a></li>");
            }
            //Link Next(Trang tiếp) và Last(Trang cuối)
            if (currentPage != pageCount)
            {

                output.Append("<li class=\"\" ><a title=\"" + strText[2] + "\" href=\"" + string.Format(pageUrl, currentPage + 1) + "\">Next</a></li>");

            }
            output.Append("</ul>");
   
            return MvcHtmlString.Create(output.ToString());
        }
    }
}