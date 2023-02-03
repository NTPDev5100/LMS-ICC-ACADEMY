using AppZim.TableSql;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AppZim.Areas.Admin.Controllers
{
    public class MoneyExchangeRateController : BaseController
    {
        // GET: Admin/MoneyExchangeRate
        public ActionResult MoneyExchangeRate()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var l = MoneyExchangeRateTable.GetAll();
            return View(l);
        }
        public ActionResult Delete(int id)
        {
            try
            {
                var ck = MoneyExchangeRateTable.Delete(id);
                if(ck == null)
                {
                    SetAlert("Lỗi", "e");
                }
                else
                {
                    SetAlert("Thành công", "s");
                }
            }
            catch (Exception e)
            {
                SetAlert("Lỗi", "e");
            }
            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }
        [HttpPost]
        public ActionResult Insert(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var name = f.Get("name").ToString();
                var code = f.Get("code").ToString();
                var rate = f.Get("rate").Replace(",","").ToInt(0);
                var ck = MoneyExchangeRateTable.Insert(name, code, rate, aclog.FullName);
                if(ck == null)
                {
                    SetAlert("Lỗi", "e");
                }
                else
                {
                    SetAlert("Thành công", "s");
                }
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }
            catch (Exception e)
            {
                SetAlert("Lỗi", "e");
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }
        }
        [HttpPost]
        public ActionResult Edit(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                var id = f.Get("id").ToInt(0);
                var name = f.Get("name").ToString();
                var code = f.Get("code").ToString();
                var rate = f.Get("rate").Replace(",","").ToInt(0);
                var ck = MoneyExchangeRateTable.Update(id, name, code, rate);
                if (ck == null)
                {
                    SetAlert("Lỗi", "e");
                }
                else
                {
                    SetAlert("Thành công", "s");
                }
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }
            catch (Exception e)
            {
                SetAlert("Lỗi", "e");
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }
        }
    }
}