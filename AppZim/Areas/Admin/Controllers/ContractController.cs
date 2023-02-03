using AppZim.Models;
using AppZim.TableSql;
using AppZim.ZIM;
using MB.Extensions;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebUI.Business;

namespace AppZim.Areas.Admin.Controllers
{
    public class ContractController : BaseController
    {
        // GET: Admin/Contract
        [HttpGet]
        public ActionResult Contract()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên,giaos vien bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var kq = ContractTable.getbyid(1);
                if (kq == null)
                    ViewBag.Content = "";
                else
                    ViewBag.Content = kq.ContractContent;
                return View();
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("ContractController", "Contract", Session["UID"].ToString().ToInt(0), ex.Message);
                SetAlert("Sự cố bất ngờ, vui lòng thử lại", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
        }
        
        [HttpGet]
        public ActionResult SampleTeacherContract()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên,giaos vien bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var kq = ContractTable.getbyid(2);
                if (kq == null)
                    ViewBag.Content = "";
                else
                    ViewBag.Content = kq.ContractContent;
                return View();
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("ContractController", "Contract", Session["UID"].ToString().ToInt(0), ex.Message);
                SetAlert("Sự cố bất ngờ, vui lòng thử lại", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult AddContract(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });


            var content = f.Get("txt-content").ToString();
            if (string.IsNullOrEmpty(AssetCRM.RemoveHTMLTags(content)))
            {
                SetAlert("Vui lòng nhập nội dung", "e");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            ContractTable.update(1, content, aclog.UserName);
            SetAlert("Thành công", "s");
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult UpdateTeacherContract(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });


            var content = f.Get("txt-content").ToString();
            if (string.IsNullOrEmpty(AssetCRM.RemoveHTMLTags(content)))
            {
                SetAlert("Vui lòng nhập nội dung", "e");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            ContractTable.update(2, content, aclog.UserName);
            SetAlert("Thành công", "s");
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }

        [HttpGet]
        public ActionResult CustomerContractList(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                var df = DateTime.ParseExact("01/01/1001", "dd/MM/yyyy", null).Date;
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });

                string search = "";
                ViewBag.Search = Request.QueryString["search"];
                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                    search = Request.QueryString["search"].ToString().Trim();
                //page size
                int uid = 0;
                if (aclog.RoleID == 6)// tư vấn viên thì chỉ lấy đước nó
                    uid = aclog.ID;
                int pageSize = 20;
                //lấy hoc viên có hợp đồng
                SqlParameter[] parameters =
                {
                    new SqlParameter("@qsearch", SqlDbType.NVarChar,2000) { Value = search },
                    new SqlParameter("@UID", SqlDbType.Int) { Value = uid },
                };
                var rs = SqlHelper.ExecuteDataTable("get_contract", parameters);
                IEnumerable<tbl_StudentAppointment> l = (from rw in rs.AsEnumerable()
                                                         select new tbl_StudentAppointment()
                                                         {
                                                             ID = Convert.ToInt32(rw["ID"]),
                                                             StudentUID = Convert.ToInt32(rw["StudentUID"]),
                                                             StudentName = Convert.ToString(rw["StudentName"]),
                                                             ClassName = Convert.ToString(rw["ClassName"]),
                                                             SchoolID = Convert.ToInt32(rw["SchoolID"]),
                                                             SchoolName = Convert.ToString(rw["SchoolName"]),
                                                             Note = Convert.ToString(rw["Note"]),
                                                             CreatedDate = rw["CreatedDate"] == DBNull.Value ? df : Convert.ToDateTime(rw["CreatedDate"])

                                                         }).ToList();

                if (Request.QueryString["center"].ToInt(0) != 0)
                {
                    int center = ViewBag.CenterID = Request.QueryString["center"].ToInt(0);
                    l = l.Where(n => n.SchoolID == center).ToList();
                }
                // lấy ngày đầu và ngày cuối tháng
                DateTime date = DateTime.Now;
                var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                // thời gian bắt đầu từ
                if (!string.IsNullOrEmpty(Request.QueryString["from"]))
                {
                    try
                    {
                        DateTime fromdate = DateTime.ParseExact(Request.QueryString["from"].ToString(), "dd/MM/yyyy", null);
                        l = from ex in l
                            where ex.CreatedDate.Value.Date >= fromdate.Date
                            select ex;
                        ViewBag.From = fromdate.ToString("dd/MM/yyyy");
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        DateTime fromdate = firstDayOfMonth;
                        l = from ex in l
                            where ex.CreatedDate.Value.Date >= fromdate.Date
                            select ex;
                        ViewBag.From = fromdate.ToString("dd/MM/yyyy");
                    }
                    catch { }
                }
                // đến
                if (!string.IsNullOrEmpty(Request.QueryString["to"]))
                {
                    try
                    {
                        DateTime todate = DateTime.ParseExact(Request.QueryString["to"].ToString(), "dd/MM/yyyy", null);
                        l = from ex in l
                            where ex.CreatedDate.Value.Date <= todate.Date
                            select ex;
                        ViewBag.To = todate.ToString("dd/MM/yyyy");
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        DateTime todate = lastDayOfMonth;
                        l = from ex in l
                            where ex.CreatedDate.Value.Date <= todate.Date
                            select ex;
                        ViewBag.To = todate.ToString("dd/MM/yyyy");
                    }
                    catch { }
                }
                //sort
                int sort = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
                {
                    sort = Request.QueryString["sort"].ToInt(0);

                    if (sort == 1)
                        l = l.OrderBy(n => n.StudentName).ToList();
                    if (sort == 2)
                        l = l.OrderByDescending(n => n.StudentName).ToList();
                    if (sort == 3)
                        l = l.OrderBy(n => n.ClassName).ToList();
                    if (sort == 4)
                        l = l.OrderByDescending(n => n.ClassName).ToList();
                    if (sort == 5)
                        l = l.OrderBy(n => n.CreatedDate).ToList();
                    if (sort == 6)
                        l = l.OrderByDescending(n => n.CreatedDate).ToList();
                }
                ViewBag.Sort = sort;
                ViewBag.Total = l.Count();
                return View(l.ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("ContractController", "CustomerContractList", uid, ex.Message);
                SetAlert("Sự cố bất ngờ, vui lòng thử lại", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
        }
        [HttpGet]
        public ActionResult EditContractCustomer(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên,giaos vien bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var kq = ContractTable.getbyid(1);
                string ct = "";
                if (kq != null)
                    ct = kq.ContractContent;

                var check = ContractCustomerTable.getbystudentAppointmentID(id);
                if (check != null)
                {
                    if (check.Status == 2)
                    {
                        SetAlert("Hợp đồng đã được duyệt, bạn ko thể sửa!", "e");
                        return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
                    }
                    else
                    {
                        //đổ dữ liệu
                        tbl_StudentAppointment cs = StudentAppointmentTable.getbyid(id);
                        if (cs == null)
                        {
                            SetAlert("Không tìm thấy dữ liệu", "e");
                            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
                        }
                        ViewBag.CSID = cs.ID;
                        ViewBag.Content = check.ContractContent;
                        ViewBag.Note = check.Note == null ? "" : check.Note;
                        return View();
                    }

                }
                else
                {
                    //đổ dữ liệu
                    tbl_StudentAppointment cs = StudentAppointmentTable.getbyid(id);
                    using (var db = new ZimEntities())
                    {
                        var studentAppoinment = db.tbl_StudentAppointment.SingleOrDefault(x => x.ID == cs.ID);
                        studentAppoinment.isCreateContract = true;
                        db.SaveChanges();
                    }
                    if (cs == null)
                    {
                        SetAlert("Không tìm thấy dữ liệu", "e");
                        return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
                    }
                    ct = ct.Replace("{ngay}", DateTime.Now.ToString("dd"));
                    ct = ct.Replace("{thang}", DateTime.Now.ToString("MM"));
                    ct = ct.Replace("{nam}", DateTime.Now.ToString("yyyy"));
                    //thông tin học viên
                    var student = AccountTable.getbyID(cs.StudentUID.Value);
                    ct = ct.Replace("{so-dien-thoai}", student.Phone == null ? "N/A" : student.Phone);
                    ct = ct.Replace("{email}", student.Email == null ? "N/A" : student.Email);
                    ct = ct.Replace("{hoc-vien}", student.FullName == null ? "N/A" : student.FullName);
                    ViewBag.CSID = cs.ID;
                    ViewBag.Content = ct;
                    return View();
                }
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("ContractController", "EditContractCustomer", Session["UID"].ToString().ToInt(0), ex.Message);
                SetAlert("Sự cố bất ngờ, vui lòng thử lại", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
        }

        [HttpGet]
        public ActionResult EditContractTeacher(int id)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên,giaos vien bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });

                var contract = ContractTeacherTable.getbyid(id);
                if (contract == null)
                {
                    SetAlert("Không tìm thấy hợp đồng này!", "e");
                    return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
                }

                if (contract.Status == 2)
                {
                    SetAlert("Hợp đồng đã được duyệt, bạn ko thể sửa!", "e");
                    return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
                }
                ViewBag.ID = contract.ID;
                ViewBag.Content = contract.ContractContent;
                return View();
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("ContractController", "EditContractCustomer", Session["UID"].ToString().ToInt(0), ex.Message);
                SetAlert("Sự cố bất ngờ, vui lòng thử lại", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult AddContractCustomer(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var st = StudentAppointmentTable.getbyid(f.Get("hdf-appointment-student-id").ToInt(0));
            if (st == null)
            {
                SetAlert("Không tìm thấy dữ liệu", "w");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            var content = f.Get("txt-content").ToString();
            if (string.IsNullOrEmpty(AssetCRM.RemoveHTMLTags(content)))
            {
                SetAlert("Vui lòng nhập nội dung", "e");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            string note = "";
            if (!string.IsNullOrEmpty(f.Get("txt-note")))
            {
                note = f.Get("txt-note").ToString();
            }
            string name = "code-" + st.ID + "-" + GetDateTime.Now.ToString("ddMMyyyyHHmmss") + "";
            string code = AppZim.ZIM.Encryptor.EncryptURL(name);
            string strPathAndQuery = Request.Url.PathAndQuery;
            string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
            string datalink = "" + strUrl + "print-contract?code=" + code;
            var barcode = AppZim.ZIM.AssetCRM.CreateQRCodeContract(datalink, name);
            content = content.Replace("http://app.vietcoco.monamedia.net/app-assets/zimv2/assets/img/barcode.jpg", barcode);
            ContractCustomerTable.insert(st.StudentUID.Value, st.ID, content, note, code, aclog.UserName);
            SetAlert("Thành công", "s");
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult EditContract(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var st = ContractTeacherTable.getbyid(f.Get("hdid").ToInt(0));
            if (st == null)
            {
                SetAlert("Không tìm thấy hợp đồng", "e");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            var content = f.Get("txt-content").ToString();
            if (string.IsNullOrEmpty(AssetCRM.RemoveHTMLTags(content)))
            {
                SetAlert("Vui lòng nhập nội dung", "e");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }

            ContractTeacherTable.insert(st.TeacherID.Value, content, st.Status.Value, st.isHide.Value, aclog.UserName);
            SetAlert("Thành công", "s");
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }

        [HttpGet]
        public ActionResult CensorshipContract(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                var df = DateTime.ParseExact("01/01/1001", "dd/MM/yyyy", null).Date;
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });

                string search = "";
                ViewBag.Search = Request.QueryString["search"];
                if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                    search = Request.QueryString["search"].ToString().Trim();
                //page size              
                int pageSize = 20;
                //lấy hoc viên có hợp đồng
                SqlParameter[] parameters =
                {
                    new SqlParameter("@qsearch", SqlDbType.NVarChar,2000) { Value = search }
                };
                var rs = SqlHelper.ExecuteDataTable("get_contract_censorship", parameters);
                IEnumerable<tbl_ContractCustomer> l = (from rw in rs.AsEnumerable()
                                                       select new tbl_ContractCustomer()
                                                       {
                                                           ID = Convert.ToInt32(rw["ID"]),
                                                           StudentID = Convert.ToInt32(rw["StudentID"]),
                                                           StudentAppointmentID = Convert.ToInt32(rw["StudentAppointmentID"]),
                                                           ContractContent = Convert.ToString(rw["ContractContent"]),
                                                           Status = Convert.ToInt32(rw["Status"]),
                                                           isHide = Convert.ToBoolean(rw["isHide"]),
                                                           Note = rw["Note"].ToString(),
                                                           CreatedBy = Convert.ToString(rw["CreatedBy"]),
                                                           CodeCheck = Convert.ToString(rw["CodeCheck"]),
                                                           CreatedDate = rw["CreatedDate"] == DBNull.Value ? df : Convert.ToDateTime(rw["CreatedDate"]),
                                                           ModifiedBy = Convert.ToString(rw["ModifiedBy"]),
                                                           ModifiedDate = rw["ModifiedDate"] == DBNull.Value ? df : Convert.ToDateTime(rw["ModifiedDate"]),
                                                           Signed = Convert.ToBoolean(rw["Signed"])
                                                       }).ToList();
                //sort
                int sort = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
                {
                    sort = Request.QueryString["sort"].ToInt(0);
                    if (sort == 1)
                        l = l.OrderBy(n => n.Status).ToList();
                    if (sort == 2)
                        l = l.OrderByDescending(n => n.Status).ToList();
                }
                ViewBag.Sort = sort;
                ViewBag.Total = l.Count();
                return View(l.ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("ContractController", "CustomerContractList", uid, ex.Message);
                SetAlert("Sự cố bất ngờ, vui lòng thử lại", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
        }
        /// <summary>
        /// cập nhật trạng thái về hợp đồng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AcceptContract(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var cc = ContractCustomerTable.getbyid(id);
            var content = cc.ContractContent.Replace("../../", ProjectDomain);
            if (cc.Status == 1)//chấp nhận
            {
                ContractCustomerTable.updateaccept(cc.ID, 2, aclog.UserName);
                var student = AccountTable.getbyID(cc.StudentID.Value);
                if (student != null)
                {
                    AssetCRM.SendMail(student.Email, ProjectName + " gửi bạn hợp đồng cam kết chất lượng", content);

                    var te = DeviceMobileTable.getbyUID(student.ID)?.IncludePlayerID;
                    PushNotifyMobile.SendNotifyMobile("Bạn đã có 1 hợp đồng cam kết chất lượng, cùng vào xem nha", new[] { te });
                    NotificationTable.insertsingle("", ProjectName + " gửi bạn hợp đồng cam kết chất lượng", "Bạn đã có 1 hợp đồng cam kết chất lượng, cùng vào xem nha", student.ID, aclog.UserName, 17, 0);
                }
            }
            else// ko duyệt
                ContractCustomerTable.updateaccept(cc.ID, 1, aclog.UserName);
            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public ActionResult AcceptContractTeacher(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var cc = ContractTeacherTable.getbyid(id);
            var content = cc.ContractContent.Replace("../../", ProjectDomain);
            if (cc.Status == 1)//chấp nhận
            {
                ContractTeacherTable.insert(cc.TeacherID.Value, cc.ContractContent, 2, false, aclog.UserName);
                var teacher = AccountTable.getbyID(cc.TeacherID.Value);
                if (teacher != null)
                {
                    AssetCRM.SendMail(teacher.Email, ProjectName + " gửi bạn hợp đồng", content);
                }
            }

            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult PrintContract(string code)
        {
            string decode = AppZim.ZIM.Encryptor.DecryptURL(code);
            string[] arr = decode.Split('-');
            if (arr.Length != 3)
                return RedirectToAction("ErrorNotFound", "Home", new { area = "Admin" });
            var contract = ContractCustomerTable.getbystudentAppointmentID(arr[1].ToInt(0));
            if (contract == null)
                return RedirectToAction("ErrorNotFound", "Home", new { area = "Admin" });
            if (contract.Status != 2)
                return RedirectToAction("ErrorNotFound", "Home", new { area = "Admin" });
            return View(contract);
        }
        [HttpGet]
        public ActionResult PrintTeacherContract(int id)
        {
            var contract = ContractTeacherTable.getbyid(id);
            if (contract == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            if (contract.Status != 2)
                return RedirectToAction("Signin", "Login", new { area = "" });
            return View("PrintContract", new tbl_ContractCustomer() { ContractContent = contract.ContractContent });
        }
        public ActionResult TermOfService()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên,giaos vien bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var kq = TermOfServiceTable.getfirst();
                if (kq == null)
                    ViewBag.Content = "";
                else
                    ViewBag.Content = kq.TermContent;
                return View();
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("TermOfService", "Contract", Session["UID"].ToString().ToInt(0), ex.Message);
                SetAlert("Sự cố bất ngờ, vui lòng thử lại", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult SystemTermOfService()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult AddTerm(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var content = f.Get("txt-content").ToString();
            if (string.IsNullOrEmpty(AssetCRM.RemoveHTMLTags(content)))
            {
                SetAlert("Vui lòng nhập nội dung", "e");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            TermOfServiceTable.insert(content, aclog.UserName);
            SetAlert("Thành công", "s");
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }
        [HttpGet]
        public ActionResult TeacherContract(int page = 1)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                //page size              
                int pageSize = 20;
                int teacherId = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["slTeacher"]))
                {
                    teacherId = Request.QueryString["slTeacher"].ToInt(0);
                }
                ViewBag.Teacher = teacherId;
                //sort
                int sort = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
                {
                    sort = Request.QueryString["sort"].ToInt(0);
                }
                ViewBag.Sort = sort;
                var l = new List<ContractTeacer_GetList_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.ContractTeacer_GetList(teacherId, sort, (page - 1) * pageSize, pageSize).ToList();
                }
                ViewBag.StartIndex = (page - 1) * pageSize;
                int total = 0;
                if (l.Count > 0)
                {
                    total = l[0].TotalRow.Value;
                }
                ViewBag.Total = total;
                int PageCount = 0;
                if (total % pageSize == 0)
                    PageCount = total / pageSize;
                else
                    PageCount = total / pageSize + 1;
                ViewBag.Page = page;
                ViewBag.PageCount = PageCount;
                return View(l);
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("ContractController", "TeacherContract", 0, ex.Message + "||" + ex.InnerException);
                SetAlert("Đã xảy ra lỗi, vui lòng liên hệ Mona Media", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
        }
    }
}