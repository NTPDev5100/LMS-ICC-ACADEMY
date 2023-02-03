using AppZim.Models;
using AppZim.TableSql;
using AppZim.ZIM;
using MB.Extensions;
using Org.BouncyCastle.Asn1.Ocsp;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static AppZim.Areas.Admin.Controllers.CourseController;

namespace AppZim.Areas.Admin.Controllers
{
    public class CourseOnlineController : Controller
    {
        /// <summary>
        /// Danh sách khóa học online
        /// </summary>
        /// <returns></returns>
        public ActionResult CourseOnline()
        {
            return View();
        }
        /// <summary>
        /// Danh sách từng phần theo từng khóa học
        /// </summary>
        /// <returns></returns>
        public ActionResult Section()
        {
            return View();
        }
        /// <summary>
        /// Danh sách bài học theo từng phần
        /// </summary>
        /// <returns></returns>
        public ActionResult Lesson()
        {
            return View();
        }
        /// <summary>
        /// Mã giảm giá cho từng khóa học
        /// </summary>
        /// <returns></returns>
        public ActionResult DiscountCode()
        {
            return View();
        }
        /// <summary>
        /// Danh sách đơn hàng mua khóa học online
        /// </summary>
        /// <returns></returns>
        public ActionResult Order()
        {
            return View();
        }
    }
}