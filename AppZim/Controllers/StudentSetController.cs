using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AppZim.TableSql;
using MB.Extensions;
using AppZim.Models;
using PagedList;
using AppZim.ZIM;
using AppZim.Areas.Admin.Controllers;
using Newtonsoft.Json;
namespace AppZim.Controllers
{
    public class StudentSetController : BaseController
    {
        // GET: StudentSet
        public ActionResult MySet(int page = 1)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 5 && aclog.RoleID != 1)
                {
                    return RedirectToAction("Signin", "Login", new { area = "" });
                }
                IEnumerable<tbl_SetPackageStudent> l = SetPackageStudentTable.getbyStudentID(aclog.ID).OrderByDescending(n => n.ID);
                int pageSize = 10;
                //if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                //{
                //    ViewBag.Search = Request.QueryString["search"];
                //    string search = Request.QueryString["search"].ToString();
                //    l = from ex in l
                //        where AssetCRM.RemoveUnicode(ex.PackageName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                //        select ex;
                //}
                int setType = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["setType"]))
                {
                    setType = Request.QueryString["setType"].ToInt(0);
                    if (setType == 1)
                        l = l.Where(n => n.SetPrice == 0).ToList();
                    if (setType == 2)
                        l = l.Where(n => n.SetPrice > 0).ToList();
                }
                ViewBag.SetType = setType;
                int sort = 0;
                if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
                {
                    sort = Request.QueryString["sort"].ToInt(0);
                    //if (sort == 1)
                    //    l = l.OrderBy(n => n.PackageName).ToList();
                    //if (sort == 2)
                    //    l = l.OrderByDescending(n => n.PackageName).ToList();
                    if (sort == 3)
                        l = l.OrderBy(n => n.CreatedDate).ToList();
                    if (sort == 4)
                        l = l.OrderByDescending(n => n.CreatedDate).ToList();
                }
                ViewBag.Sort = sort;
                ViewBag.TotalCount = l.Count();
                return View(l.ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("StudentSetController", "MySet", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult SetPackage(int ID = 0)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 5)
                {
                    return RedirectToAction("Signin", "Login", new { area = "" });
                }
                var pack = SetPackageStudentTable.get_by_setID_and_studentID(ID, aclog.ID);
                if (pack == null)
                {
                    SetAlert("You not have this set!", "e");
                    return RedirectToAction("MySet", "StudentSet", new { area = "" });
                }
                ViewBag.SetID = ID;
                return View(pack);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("StudentSetController", "SetPackage", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult DoingTest(int id = 0)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 5)
                {
                    return RedirectToAction("Signin", "Login", new { area = "" });
                }
                var setdetail = SetPackageDetailTable.getbyid(id);
                if (setdetail == null)
                {
                    SetAlert("You not have this set!", "e");
                    return RedirectToAction("MySet", "StudentSet", new { area = "" });
                }
                if (setdetail.isHide.Value)
                {
                    SetAlert("You not have this set!", "e");
                    return RedirectToAction("MySet", "StudentSet", new { area = "" });
                }
                tbl_SetPackage set = SetPackageTable.getbyid(setdetail.SetPackageID.Value);
                var pack = SetPackageStudentTable.get_by_setID_and_studentID(setdetail.SetPackageID.Value, aclog.ID);
                if (pack == null)
                {
                    SetAlert("You not have this set!", "e");
                    return RedirectToAction("MySet", "StudentSet", new { area = "" });
                }
                if (setdetail.TypeDetail == 3 || setdetail.TypeDetail == 4)
                {

                    List<int> lexaminerID = SetPackageExaminersTable.getlistexaminer().Select(n => n.TeacherID.Value).ToList();
                    if (lexaminerID.Count <= 0)
                    {
                        SetAlert("This set has no examiner! Please contact Admin", "e");
                        return RedirectToAction("MySet", "StudentSet", new { area = "" });
                    }
                    List<tbl_SetPackageStudentExercise> sse = SetPackageStudentExerciseTable.getall(set.ID, aclog.ID);//kiểm tra có bài tập chưa,
                    if (sse.Count == 0)
                    {
                        List<int> listtopicID = TopicTable.getall().Select(n => n.ID).ToList();
                        Random random = new Random();

                        tbl_Topic topic = TopicTable.getbyid(listtopicID[random.Next(0, listtopicID.Count - 1)]);
                        if (topic == null)
                        {
                            SetAlert("Topic not found, please contact admin!", "e");
                            return RedirectToAction("MySet", "StudentSet", new { area = "" });
                        }
                        //ds câu hỏi của topic
                        List<tbl_Exercise> listExTopic = ExerciseTable.getListTopicQuestion(topic.ID).Where(n => n.isHide != true).ToList();

                        List<int> part1 = listExTopic.Where(n => n.PartNumber == 1).Select(n => n.ID).ToList();

                        List<int> part2 = listExTopic.Where(n => n.PartNumber == 2).Select(n => n.ID).ToList();

                        List<int> part3 = listExTopic.Where(n => n.PartNumber == 3).Select(n => n.ID).ToList();

                        List<int> taskone = listExTopic.Where(n => n.PartNumber == 4).Select(n => n.ID).ToList();

                        List<int> tasktwo = listExTopic.Where(n => n.PartNumber == 5).Select(n => n.ID).ToList();

                        if (listExTopic.Count == 0 || part1.Count == 0 || part2.Count == 0 || part3.Count == 0 || taskone.Count == 0 || tasktwo.Count == 0)
                        {
                            SetAlert("This topic has no question, please contact admin!!", "e");
                            return Redirect(Request.UrlReferrer.AbsoluteUri);
                        }
                        //insert part 1 
                        //12-15 câu
                        int part1count = random.Next(12, 15);
                        if (part1.Count <= part1count)
                        {
                            foreach (var item in part1)
                            {
                                tbl_Exercise ex = ExerciseTable.getbyid(item);
                                SetPackageStudentExerciseTable.insert(set.ID, aclog.ID, ex.ID, ex.ExerciseTitle, 1, aclog.UserName);
                            }
                        }
                        else
                        {
                            HashSet<int> listExPart1 = new HashSet<int>();
                            while (listExPart1.Count < part1count)
                            {
                                listExPart1.Add(part1[random.Next(0, part1.Count - 1)]);
                            }
                            foreach (var item in listExPart1)
                            {
                                tbl_Exercise ex = ExerciseTable.getbyid(item);
                                SetPackageStudentExerciseTable.insert(set.ID, aclog.ID, ex.ID, ex.ExerciseTitle, 1, aclog.UserName);
                            }
                        }

                        //insert part 2
                        //1 câu
                        tbl_Exercise ex_part2 = ExerciseTable.getbyid(part2[random.Next(0, part2.Count - 1)]);
                        SetPackageStudentExerciseTable.insert(set.ID, aclog.ID, ex_part2.ID, ex_part2.ExerciseTitle, 2, aclog.UserName);
                        //insert part3
                        //3-5 cau
                        int part3count = random.Next(3, 5);
                        if (part3.Count <= part3count)
                        {
                            foreach (var item in part3)
                            {
                                tbl_Exercise ex = ExerciseTable.getbyid(item);
                                SetPackageStudentExerciseTable.insert(set.ID, aclog.ID, ex.ID, ex.ExerciseTitle, 3, aclog.UserName);
                            }
                        }
                        else
                        {
                            HashSet<int> listexpart3 = new HashSet<int>();
                            while (listexpart3.Count < part3count)
                            {
                                listexpart3.Add(part3[random.Next(0, part3.Count - 1)]);
                            }
                            foreach (var item in listexpart3)
                            {
                                tbl_Exercise ex = ExerciseTable.getbyid(item);
                                SetPackageStudentExerciseTable.insert(set.ID, aclog.ID, ex.ID, ex.ExerciseTitle, 3, aclog.UserName);
                            }
                        }
                        //tbl_SetPackageDetail speaking = SetPackageDetailTable.getbysetid(set.ID).Where(n => n.TypeDetail == 4 && n.isHide != true).FirstOrDefault();
                        //tbl_SetPackageDetail writing = SetPackageDetailTable.getbysetid(set.ID).Where(n => n.TypeDetail == 3 && n.isHide != true).FirstOrDefault();
                        //insert task one
                        tbl_Exercise ex_taskone = ExerciseTable.getbyid(taskone[random.Next(0, taskone.Count - 1)]);
                        SetPackageStudentExerciseTable.insert(set.ID, aclog.ID, ex_taskone.ID, ex_taskone.ExerciseTitle, 4, aclog.UserName);
                        //insert task two
                        tbl_Exercise ex_tasktwo = ExerciseTable.getbyid(tasktwo[random.Next(0, tasktwo.Count - 1)]);
                        SetPackageStudentExerciseTable.insert(set.ID, aclog.ID, ex_tasktwo.ID, ex_tasktwo.ExerciseTitle, 5, aclog.UserName);

                        //if (speaking != null && writing != null)//set có 2 kĩ năng => essay, id chung là ID speaking 
                        //{

                        //    string metatitle = "Essay-" + AppZim.ZIM.AssetCRM.UrlBeauty(AssetCRM.RemoveUnicode(set.PackageName.ToLower()));
                        //    string linkdoing = "/doing-test/" + speaking.ID + "/" + metatitle + "";
                        //    return Redirect(linkdoing);
                        //}
                        //else
                        //{
                        //    if (speaking != null)
                        //    {
                        //        string metatitle = AppZim.ZIM.AssetCRM.UrlBeauty(AssetCRM.RemoveUnicode(speaking.PackageDetailName.ToLower()));
                        //        string linkdoing = "/doing-test/" + speaking.ID + "/" + metatitle + "";
                        //        return Redirect(linkdoing);
                        //    }
                        //    if (writing != null)
                        //    {
                        //        string metatitle = AppZim.ZIM.AssetCRM.UrlBeauty(AssetCRM.RemoveUnicode(writing.PackageDetailName.ToLower()));
                        //        string linkdoing = "/doing-test/" + writing.ID + "/" + metatitle + "";
                        //        return Redirect(linkdoing);
                        //    }
                        //}
                        if (setdetail.TypeDetail == 4)
                        {
                            string metatitle = AppZim.ZIM.AssetCRM.UrlBeauty(AssetCRM.RemoveUnicode(setdetail.PackageDetailName.ToLower()));
                            string linkdoing = "/doing-test/" + setdetail.ID + "/" + metatitle + "";
                            return Redirect(linkdoing);
                        }
                        if (setdetail.TypeDetail == 3)
                        {
                            string metatitle = AppZim.ZIM.AssetCRM.UrlBeauty(AssetCRM.RemoveUnicode(setdetail.PackageDetailName.ToLower()));
                            string linkdoing = "/doing-test/" + setdetail.ID + "/" + metatitle + "";
                            return Redirect(linkdoing);
                        }
                    }
                }
                //if (setdetail.TypeDetail == 3)
                //{
                //    var sdspeaking = SetPackageDetailTable.getbysetid(set.ID).Where(n => n.TypeDetail == 4 && n.isHide != true).FirstOrDefault();
                //    if (sdspeaking != null)
                //    {
                //        string metatitle = AppZim.ZIM.AssetCRM.UrlBeauty(AssetCRM.RemoveUnicode(sdspeaking.PackageDetailName.ToLower()));
                //        string linkdoing = "/doing-test/" + sdspeaking.ID + "/" + metatitle + "";
                //        return Redirect(linkdoing);
                //    }
                //}
                var sethistory = SetPackageHistoryTable.getbyStudentIDandSetDetailID(aclog.ID, setdetail.ID);

                if (sethistory != null)
                {
                    if (sethistory.IsDone == true)
                    {
                        string linkdone = HttpContext.Request.Url.AbsoluteUri.Replace("doing-test", "done-test");
                        return Redirect(linkdone);
                    }
                }
                return View(setdetail);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("StudentSetController", "DoingTest", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
            }

        }
        [HttpPost]
        public ActionResult StartTest()
        {
            try
            {
                string linkredirect = Url.Action("MySet", "StudentSet", new { area = "" });
                if (Session["UID"] == null)
                    return Json(new { rs = false, type = 1, link = linkredirect, message = "Some thing error!" }, JsonRequestBehavior.AllowGet);
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 5)
                {
                    return Json(new { rs = false, type = 1, link = linkredirect, message = "Some thing error!" }, JsonRequestBehavior.AllowGet);
                }
                // lấy id của setdetail theo url đã cấu hình
                int setdeatilid = HttpContext.Request.UrlReferrer.Segments[2].Replace("/", "").ToInt(0);
                var setdetail = SetPackageDetailTable.getbyid(setdeatilid);
                if (setdetail == null)
                {
                    return Json(new { rs = false, type = 1, link = linkredirect, message = "You not have this set!" }, JsonRequestBehavior.AllowGet);
                }
                var pack = SetPackageStudentTable.get_by_setID_and_studentID(setdetail.SetPackageID.Value, aclog.ID);
                if (pack == null)
                {
                    return Json(new { rs = false, type = 1, link = linkredirect, message = "You not have this set!" }, JsonRequestBehavior.AllowGet);
                }
                if (setdetail.TypeDetail == 3 || setdetail.TypeDetail == 4)
                {
                    List<int> lexaminerID = SetPackageExaminersTable.getlistexaminer().Select(n => n.TeacherID.Value).ToList();
                    if (lexaminerID.Count <= 0)
                    {
                        return Json(new { rs = false, type = 1, link = linkredirect, message = "This set has not Examiner! Please contact admin" }, JsonRequestBehavior.AllowGet);
                    }
                }
                int time = setdetail.Minute.Value;
                if (setdetail.TypeDetail == 3)
                {
                    time = 60;
                }
                string linkaudio = "";
                if (!string.IsNullOrEmpty(setdetail.LinkAudio))
                    linkaudio = setdetail.LinkAudio;
                var sethistory = SetPackageHistoryTable.getbyStudentIDandSetDetailID(aclog.ID, setdetail.ID);
                //kiểm tra xong part chưa -- lưu trong bản setpackagehistorydetail
                //tbl_SetPackageDetail sdwriting = SetPackageDetailTable.getbysetid(setdetail.SetPackageID.Value).Where(n => n.TypeDetail == 3 && n.isHide != true).FirstOrDefault();

                if (sethistory != null)//đã bấm start ít nhất 1 lần
                {
                    tbl_SetPackageHistoryDetail part1 = SetPackageHistoryDetailTable.getbyHistoryIDandExerciseGroupID(sethistory.ID, 1);
                    tbl_SetPackageHistoryDetail part2 = SetPackageHistoryDetailTable.getbyHistoryIDandExerciseGroupID(sethistory.ID, 2);
                    tbl_SetPackageHistoryDetail part3 = SetPackageHistoryDetailTable.getbyHistoryIDandExerciseGroupID(sethistory.ID, 3);
                    tbl_SetPackageHistoryDetail taskwriting = SetPackageHistoryDetailTable.getbyHistoryIDandExerciseGroupID(sethistory.ID, 4);
                    DateTime timestart = sethistory.CreatedDate.Value;
                    TimeSpan timeSpan = GetDateTime.Now - timestart;
                    double minute = timeSpan.TotalMinutes;
                    if (sethistory.IsDone == false)//chưa xong
                    {
                        if (setdetail.TypeDetail == 4)
                        {

                            if (string.IsNullOrEmpty(part1.Result))//chưa lưu gì là chưa submit part1
                            {
                                time = 14;
                                timestart = part1.CreatedDate.Value;
                                timeSpan = GetDateTime.Now - timestart;
                                minute = timeSpan.TotalMinutes;
                            }
                            else if (string.IsNullOrEmpty(part2.Result))
                            {
                                time = 3;
                                timestart = part2.CreatedDate.Value;
                                timeSpan = GetDateTime.Now - timestart;
                                minute = timeSpan.TotalMinutes;
                            }
                            else
                            {
                                time = 5;
                                timestart = part3.CreatedDate.Value;
                                timeSpan = GetDateTime.Now - timestart;
                                minute = timeSpan.TotalMinutes;
                                //if (sdwriting != null)
                                //{
                                //    if (taskwriting != null)
                                //    {
                                //        time = 60;
                                //        timestart = taskwriting.CreatedDate.Value;
                                //        timeSpan = GetDateTime.Now - timestart;
                                //        minute = timeSpan.TotalMinutes;
                                //    }
                                //    else
                                //    {
                                //        time = 5;
                                //        timestart = part3.CreatedDate.Value;
                                //        timeSpan = GetDateTime.Now - timestart;
                                //        minute = timeSpan.TotalMinutes;
                                //    }
                                //}
                                //else
                                //{
                                //    time = 5;
                                //    timestart = part3.CreatedDate.Value;
                                //    timeSpan = GetDateTime.Now - timestart;
                                //    minute = timeSpan.TotalMinutes;
                                //}
                            }
                        }
                        if (minute >= time) //quá thời gian
                        {
                            if (setdetail.TypeDetail == 4)
                            {
                                if (string.IsNullOrEmpty(part1.Result))//part 1 chưa submit => khóa part1 , next qua part2
                                {
                                    part1 = SetPackageHistoryDetailTable.updateGroupResult(part1.ID, "Done", aclog.UserName);
                                    part2 = SetPackageHistoryDetailTable.insert(sethistory.ID, 2, "Part 2", aclog.UserName);
                                    return Json(new { rs = true, time = time - minute, linkaudio = linkaudio, part = 2 }, JsonRequestBehavior.AllowGet);
                                }
                                else if (string.IsNullOrEmpty(part2.Result)) //part 2 chưa submit => khóa part2 , next qua part3
                                {
                                    part2 = SetPackageHistoryDetailTable.updateGroupResult(part2.ID, "Done", aclog.UserName);
                                    part3 = SetPackageHistoryDetailTable.insert(sethistory.ID, 3, "Part 3", aclog.UserName);
                                    return Json(new { rs = true, time = time - minute, linkaudio = linkaudio, part = 3 }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(part3.Result)) //part 3 chưa submit => khóa part3 
                                    {
                                        //ko có => xong bài

                                        part3 = SetPackageHistoryDetailTable.updateGroupResult(part3.ID, "Done", aclog.UserName);
                                        sethistory = SetPackageHistoryTable.updatedone(sethistory.ID, 0, 0, aclog.UserName);
                                        string linkdone = HttpContext.Request.UrlReferrer.AbsoluteUri.Replace("doing-test", "done-test");
                                        return Json(new { rs = false, type = 1, link = linkdone, linkaudio = linkaudio, time = time }, JsonRequestBehavior.AllowGet);
                                        //if (sdwriting != null)//nếu set này có kĩ năng viết => combo essay => next essay
                                        //{
                                        //    if (taskwriting == null)//chưa insert
                                        //    {
                                        //        taskwriting = SetPackageHistoryDetailTable.insert(sethistory.ID, 4, "Task writing", aclog.UserName);
                                        //        return Json(new { rs = true, time = time - minute, linkaudio = linkaudio, part = 4 }, JsonRequestBehavior.AllowGet);
                                        //    }
                                        //    else
                                        //    {
                                        //        //insert rồi mà vào đây => xong bài
                                        //        taskwriting = SetPackageHistoryDetailTable.updateGroupResult(taskwriting.ID, "Done", aclog.UserName);
                                        //        sethistory = SetPackageHistoryTable.updatedone(sethistory.ID, aclog.UserName);
                                        //        string linkdone = HttpContext.Request.UrlReferrer.AbsoluteUri.Replace("doing-test", "done-test");
                                        //        return Json(new { rs = false, type = 1, link = linkdone, linkaudio = linkaudio, time = time }, JsonRequestBehavior.AllowGet);
                                        //    }
                                        //}
                                        //else
                                        //{
                                        //    //ko có => xong bài
                                        //    sethistory = SetPackageHistoryTable.updatedone(sethistory.ID, aclog.UserName);
                                        //    string linkdone = HttpContext.Request.UrlReferrer.AbsoluteUri.Replace("doing-test", "done-test");
                                        //    return Json(new { rs = false, type = 1, link = linkdone, linkaudio = linkaudio, time = time }, JsonRequestBehavior.AllowGet);
                                        //}
                                    }
                                    else
                                    {
                                        string linkdone = HttpContext.Request.UrlReferrer.AbsoluteUri.Replace("doing-test", "done-test");
                                        return Json(new { rs = false, type = 1, link = linkdone, linkaudio = linkaudio, time = time }, JsonRequestBehavior.AllowGet);
                                        //if (sdwriting != null)//nếu set này có kĩ năng viết => combo essay => next essay
                                        //{
                                        //    taskwriting = SetPackageHistoryDetailTable.updateGroupResult(taskwriting.ID, "Done", aclog.UserName);
                                        //    sethistory = SetPackageHistoryTable.updatedone(sethistory.ID, aclog.UserName);
                                        //    string linkdone = HttpContext.Request.UrlReferrer.AbsoluteUri.Replace("doing-test", "done-test");
                                        //    return Json(new { rs = false, type = 1, link = linkdone, linkaudio = linkaudio, time = time }, JsonRequestBehavior.AllowGet);
                                        //}
                                        //else
                                        //{
                                        //    string linkdone = HttpContext.Request.UrlReferrer.AbsoluteUri.Replace("doing-test", "done-test");
                                        //    return Json(new { rs = false, type = 1, link = linkdone, linkaudio = linkaudio, time = time }, JsonRequestBehavior.AllowGet);
                                        //}
                                    }
                                }
                            }
                            else
                            {

                                sethistory = SetPackageHistoryTable.updatedone(sethistory.ID, 0, 0, aclog.UserName);
                                string linkdone = HttpContext.Request.UrlReferrer.AbsoluteUri.Replace("doing-test", "done-test");
                                return Json(new { rs = false, type = 1, link = linkdone, linkaudio = linkaudio, time = time }, JsonRequestBehavior.AllowGet);
                            }

                        }
                        else
                        {
                            if (setdetail.TypeDetail == 4)
                            {
                                if (string.IsNullOrEmpty(part1.Result))
                                {
                                    return Json(new { rs = true, time = time - minute, linkaudio = linkaudio, part = 1 }, JsonRequestBehavior.AllowGet);
                                }
                                else if (string.IsNullOrEmpty(part2.Result))
                                {
                                    return Json(new { rs = true, time = time - minute, linkaudio = linkaudio, part = 2 }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(part3.Result))
                                    {
                                        string linkdone = HttpContext.Request.UrlReferrer.AbsoluteUri.Replace("doing-test", "done-test");
                                        return Json(new { rs = false, type = 1, link = linkdone, linkaudio = linkaudio, time = time }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        return Json(new { rs = true, time = time - minute, linkaudio = linkaudio, part = 3 }, JsonRequestBehavior.AllowGet);
                                    }

                                }
                            }
                            else
                            {
                                return Json(new { rs = true, time = time - minute, linkaudio = linkaudio }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    else//đã xong
                    {
                        string linkdone = HttpContext.Request.UrlReferrer.AbsoluteUri.Replace("doing-test", "done-test");
                        return Json(new { rs = false, type = 1, link = linkdone, linkaudio = linkaudio, time = time }, JsonRequestBehavior.AllowGet);
                    }
                }
                else//em vừa bấm Start xong luôn 
                {
                    var times = 0;
                    var checkTime = SetPackageHistoryTable.getbyStudentIDandSetDetailID(aclog.ID, setdetail.ID);
                    if (checkTime != null)
                    {
                        times = checkTime.Times.Value;
                    }
                    sethistory = SetPackageHistoryTable.insert(aclog.ID, setdetail.SetPackageID.Value, setdetail.ID, aclog.UserName, times + 1);
                    if (setdetail.TypeDetail == 4)//insert part1 trước
                    {
                        time = 14;
                        SetPackageHistoryDetailTable.insert(sethistory.ID, 1, "Part 1", aclog.UserName);
                        //insert bài nộp nhưng ẩn đi + không gửi thông báo cho giáo viên cho đến khi nó làm xong
                        tbl_SetPackage set = SetPackageTable.getbyid(pack.SetPackageID.Value);
                        var exaddon = ExerciseAddonStudentTable.insert(aclog, new tbl_Courses() { ID = 0, CourseName = "Speaking Test", }, set, new tbl_Exercise() { ID = 0, Type = 3, ExerciseGroupID = 0, ExerciseTitle = "" }, "", "", 0, pack.ExaminerID.Value, aclog.UserName);
                        tbl_Account creator = AccountTable.getbyusername(pack.CreatedBy);
                        if (creator == null)
                        {
                            creator = new tbl_Account() { ID = 0, FullName = "" };
                        }
                        ExerciseAddonStudentTable.updateCreateBy(exaddon.ID, creator.ID, creator.FullName);
                    }
                    if (setdetail.TypeDetail == 3)
                    {
                        time = 60;
                        SetPackageHistoryDetailTable.insert(sethistory.ID, 1, "Task 1", aclog.UserName);
                        SetPackageHistoryDetailTable.insert(sethistory.ID, 2, "Task 2", aclog.UserName);
                    }
                    List<tbl_ExerciseGroup> questiongroup = ExerciseGroupTable.getbysetdetail(setdetail.ID);
                    foreach (var item in questiongroup)
                    {
                        SetPackageHistoryDetailTable.insert(sethistory.ID, item.ID, item.SetPackageQuestion, aclog.UserName);
                    }
                    return Json(new { rs = true, time = time, linkaudio = linkaudio }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("StudentSetController", "StartTest", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public class QandA
        {
            public int QuesID { get; set; }
            public string AnsID { get; set; }
        }
        public class TestResult
        {
            public int Type { get; set; }
            public int Group { get; set; }
            public string QuestionRecord { get; set; }
            public List<QandA> ListQA { get; set; }
        }
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitTest(FormCollection f)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 5)
                {
                    return RedirectToAction("Signin", "Login", new { area = "" });
                }
                //lấy setdetail ID 
                int setdetailid = HttpContext.Request.UrlReferrer.Segments[2].Replace("/", "").ToInt(0);
                var setdetail = SetPackageDetailTable.getbyid(setdetailid);
                if (setdetail == null)
                {
                    SetAlert("Bạn không có quyền truy cập gói này!", "e");
                    return RedirectToAction("MySet", "StudentSet", new { area = "" });
                }
                var setpackage = SetPackageTable.getbyid(setdetail.SetPackageID.Value);
                var pack = SetPackageStudentTable.get_by_setID_and_studentID(setdetail.SetPackageID.Value, aclog.ID);
                if (pack == null)
                {
                    SetAlert("Bạn không có quyền truy cập gói này!", "e");
                    return RedirectToAction("MySet", "StudentSet", new { area = "" });
                }
                var sethistory = SetPackageHistoryTable.getbyStudentIDandSetDetailID(aclog.ID, setdetail.ID);
                string result = f.Get("hdfResult");
                //List<TestResult> listTestResult = new List<TestResult>();
                List<TestResult> listTestResult = JsonConvert.DeserializeObject<List<TestResult>>(result).ToList();
                //á

                foreach (TestResult grouptest in listTestResult)
                {
                    string questionrecord = grouptest.QuestionRecord;
                    if (grouptest.Type == 1 || grouptest.Type == 4 || grouptest.Type == 5)//nhóm câu hỏi trắc nghiệm 1 câu, và nhiều câu
                    {
                        if (grouptest.ListQA != null)
                        {
                            if (grouptest.ListQA.Count > 0)
                            {
                                for (int i = 0; i < grouptest.ListQA.Count; i++)
                                {
                                    QandA qa = grouptest.ListQA[i];
                                    var question = ExerciseTable.getbyid(qa.QuesID);
                                    if (grouptest.Type == 5)
                                    {
                                        List<string> listans = qa.AnsID.Split(',').ToList();
                                        List<tbl_ExerciceMultiChoise> listanswer = new List<tbl_ExerciceMultiChoise>();
                                        foreach (var item in listans)
                                        {
                                            if (!string.IsNullOrEmpty(item))
                                            {
                                                var answers = ExerciceMultiChoiseTable.getbyid(item.ToInt(0));
                                                if (answers != null)
                                                {
                                                    listanswer.Add(answers);
                                                }
                                            }
                                        }
                                        tbl_ExerciceMultiChoise answer = listanswer.FirstOrDefault();//chọn đúng hay sai thì cũng quy về thằng này
                                        tbl_ExerciceMultiChoise checkcorrect = listanswer.Where(n => n.ExerciseID == question.ID && n.isTrue == true).FirstOrDefault();
                                        if (checkcorrect != null)
                                        {
                                            answer = checkcorrect;
                                        }
                                        if (answer == null)
                                            answer = new tbl_ExerciceMultiChoise() { ID = 0, AnswerTitle = "", isTrue = false };

                                        if (question != null)
                                        {
                                            ExerciseAddonStudentTable.insert(aclog, new tbl_Courses() { ID = 0, CourseName = "", }, new tbl_SetPackage() { ID = setpackage.ID, PackageName = setpackage.PackageName }, question, "", answer.AnswerTitle, answer.ID, 0, aclog.UserName);
                                            var sethistoryDetail = SetPackageHistoryDetailTable.getbyHistoryIDandExerciseGroupID(sethistory.ID, grouptest.Group);
                                            sethistoryDetail = SetPackageHistoryDetailTable.updateGroupResult(sethistoryDetail.ID, questionrecord, aclog.UserName);
                                        }
                                    }
                                    else
                                    {
                                        var answer = ExerciceMultiChoiseTable.getbyid(qa.AnsID.ToInt(0));
                                        if (answer == null)
                                            answer = new tbl_ExerciceMultiChoise() { ID = 0, AnswerTitle = "", isTrue = false };
                                        if (question != null)
                                        {
                                            ExerciseAddonStudentTable.insert(aclog, new tbl_Courses() { ID = 0, CourseName = "", }, new tbl_SetPackage() { ID = setpackage.ID, PackageName = setpackage.PackageName }, question, "", answer.AnswerTitle, answer.ID, 0, aclog.UserName);
                                            var sethistoryDetail = SetPackageHistoryDetailTable.getbyHistoryIDandExerciseGroupID(sethistory.ID, grouptest.Group);
                                            sethistoryDetail = SetPackageHistoryDetailTable.updateGroupResult(sethistoryDetail.ID, questionrecord, aclog.UserName);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (grouptest.Type == 2)//nhóm câu hỏi DRAG
                    {
                        if (grouptest.ListQA != null)
                        {
                            if (grouptest.ListQA.Count > 0)
                            {
                                for (int i = 0; i < grouptest.ListQA.Count; i++)
                                {
                                    QandA qa = grouptest.ListQA[i];
                                    var question = ExerciseTable.getbytitleidvsgroupid(grouptest.Group, qa.QuesID.ToString());
                                    var answer = ExerciceMultiChoiseTable.getbyid(qa.AnsID.ToInt(0));
                                    if (answer == null)
                                        answer = new tbl_ExerciceMultiChoise() { ID = 0, AnswerTitle = "", isTrue = false };
                                    if (question != null)
                                    {


                                        ExerciseAddonStudentTable.insert(aclog, new tbl_Courses() { ID = 0, CourseName = "", }, new tbl_SetPackage() { ID = setpackage.ID, PackageName = setpackage.PackageName }, question, "", answer.AnswerTitle, answer.ID, 0, aclog.UserName);
                                        var sethistoryDetail = SetPackageHistoryDetailTable.getbyHistoryIDandExerciseGroupID(sethistory.ID, grouptest.Group);
                                        sethistoryDetail = SetPackageHistoryDetailTable.updateGroupResult(sethistoryDetail.ID, questionrecord, aclog.UserName);
                                    }
                                }
                            }
                        }
                    }
                    else if (grouptest.Type == 3)//Nhóm câu hỏi input
                    {
                        if (grouptest.ListQA != null)
                        {
                            if (grouptest.ListQA.Count > 0)
                            {
                                for (int i = 0; i < grouptest.ListQA.Count; i++)
                                {
                                    QandA qa = grouptest.ListQA[i];
                                    var question = ExerciseTable.getbytitleidvsgroupid(grouptest.Group, qa.QuesID.ToString());
                                    var answer = ExerciceMultiChoiseTable.getbytitle(qa.AnsID.Trim().ToString(), question.ID);
                                    if (answer == null)
                                        answer = new tbl_ExerciceMultiChoise() { ID = 0, AnswerTitle = qa.AnsID.Trim(), isTrue = false };
                                    if (question != null)
                                    {

                                        ExerciseAddonStudentTable.insert(aclog, new tbl_Courses() { ID = 0, CourseName = "", }, new tbl_SetPackage() { ID = setpackage.ID, PackageName = setpackage.PackageName }, question, "", answer.AnswerTitle, answer.ID, 0, aclog.UserName);
                                        var sethistoryDetail = SetPackageHistoryDetailTable.getbyHistoryIDandExerciseGroupID(sethistory.ID, grouptest.Group);
                                        sethistoryDetail = SetPackageHistoryDetailTable.updateGroupResult(sethistoryDetail.ID, questionrecord, aclog.UserName);
                                    }
                                }
                            }
                        }
                    }
                }
                //tính điểm
                int correct = 0;
                List<tbl_ExerciseGroup> questiongroup = ExerciseGroupTable.getbysetdetail(setdetail.ID);
                foreach (var item in questiongroup)
                {
                    if (item != null)
                    {
                        List<tbl_ExerciseAddonStudent> lexa = ExerciseAddonStudentTable.getbyUIDStudent_ExeGroupID(aclog.ID, item.ID);
                        List<int> ListExID = lexa.Select(n => n.ExerciseID.Value).Distinct().ToList();
                        for (int i = 0; i < ListExID.Count; i++)
                        {
                            List<tbl_Exercise> checkCauHoiCon = ExerciseTable.getListHasParent(ListExID[i]);
                            if (checkCauHoiCon.Count > 0)
                            {
                                //ds câu trả lời - cả con lẫn cha
                                List<tbl_ExerciseAddonStudent> exstudentchoise = ExerciseAddonStudentTable.getListbyuidvsexeid(aclog.ID, ListExID[i]);
                                List<tbl_ExerciceMultiChoise> cautraloi = new List<tbl_ExerciceMultiChoise>();
                                foreach (var exs in exstudentchoise)
                                {
                                    tbl_ExerciceMultiChoise exsc = ExerciceMultiChoiseTable.getbyid(exs.ExerciseMultipleChoiseID.Value);
                                    if (exsc != null)
                                    {
                                        cautraloi.Add(exsc);
                                    }
                                }
                                List<tbl_ExerciceMultiChoise> dscautraloidung = ExerciceMultiChoiseTable.getbyexeid(ListExID[i]).Where(n => n.isTrue == true && n.isHide != true).ToList();
                                List<tbl_ExerciceMultiChoise> dscautraloichadung = ExerciceMultiChoiseTable.getbyexeid(ListExID[i]).Where(n => n.isTrue == true && n.isHide != true).ToList();
                                List<tbl_ExerciceMultiChoise> dscautraloidungCon = new List<tbl_ExerciceMultiChoise>();
                                foreach (var con in checkCauHoiCon)
                                {
                                    dscautraloidungCon = ExerciceMultiChoiseTable.getbyexeid(con.ID).Where(n => n.isTrue == true && n.isHide != true).ToList();
                                    dscautraloidung.AddRange(dscautraloidungCon);
                                }
                                bool status = true;
                                IEnumerable<tbl_ExerciceMultiChoise> check = (from exmID in cautraloi
                                                                              join tldung in dscautraloichadung on exmID.ID equals tldung.ID
                                                                              select tldung).ToList();
                                if (check.Count() <= 0)
                                {
                                    status = false;
                                }
                                if (cautraloi.Count > dscautraloidung.Count)
                                {
                                    status = false;
                                }
                                if (status == true)
                                {
                                    correct++;
                                }
                                bool statuscon = true;
                                // dùng để đếm số câu đúng sai, thay đổi trạng thái màu
                                foreach (var contemp in checkCauHoiCon)
                                {
                                    statuscon = true;
                                    // tăng câu đúng khi câu con đúng

                                    dscautraloidungCon = ExerciceMultiChoiseTable.getbyexeid(contemp.ID).Where(n => n.isTrue == true && n.isHide != true).ToList();

                                    IEnumerable<tbl_ExerciceMultiChoise> checkcon = (from exmID in cautraloi
                                                                                     join tldung in dscautraloidungCon on exmID.ID equals tldung.ID
                                                                                     select tldung).ToList();
                                    if (checkcon.Count() <= 0)
                                    {
                                        statuscon = false;
                                    }
                                    if (cautraloi.Count > dscautraloidung.Count)
                                    {
                                        statuscon = false;
                                    }
                                    if (statuscon == true)
                                    {
                                        correct++;
                                    }
                                }
                                if (status != statuscon && status != true)
                                {
                                    status = true;
                                }
                            }
                            else
                            {
                                List<tbl_ExerciceMultiChoise> dscautraloidung = ExerciceMultiChoiseTable.getbyexeid(ListExID[i]).Where(n => n.isTrue == true && n.isHide != true).ToList();
                                List<string> dscautraloi = dscautraloidung.Select(n => n.AnswerTitle).Distinct().ToList();
                                bool status = true;
                                List<tbl_ExerciseAddonStudent> exstudentchoise = ExerciseAddonStudentTable.getListbyuidvsexeid(aclog.ID, ListExID[i]);
                                List<tbl_ExerciceMultiChoise> cautraloi = new List<tbl_ExerciceMultiChoise>();
                                foreach (var exs in exstudentchoise)
                                {
                                    tbl_ExerciceMultiChoise exsc = ExerciceMultiChoiseTable.getbyid(exs.ExerciseMultipleChoiseID.Value);
                                    if (exsc != null)
                                    {
                                        cautraloi.Add(exsc);
                                    }
                                }
                                IEnumerable<tbl_ExerciceMultiChoise> check = from exmID in cautraloi
                                                                             join tldung in dscautraloidung on exmID.ID equals tldung.ID
                                                                             select tldung;
                                List<string> cautlcuaban = cautraloi.Select(n => n.AnswerTitle).Distinct().ToList();

                                if (check.Count() == 0 || check.Count() != dscautraloidung.Count())
                                {
                                    status = false;
                                    if (item.TypeMultiChoice == 3)
                                    {
                                        if (check.Count() > 0)
                                        {
                                            status = true;
                                        }
                                    }
                                }
                                if (cautlcuaban.Count != dscautraloi.Count && item.TypeMultiChoice != 3)
                                {
                                    status = false;
                                }

                                if (status == true)
                                {
                                    correct++;
                                }
                            }
                        }
                    }
                }
                //tính tổng số câu hỏi
                int totalExercise = 0;
                using (var db = new ZimEntities())
                {
                    var gr = db.tbl_ExerciseGroup.Where(x => x.SetPackageDetailID == setdetail.ID).Select(x => x.ID).ToList();
                    if (gr.Count() != 0)
                    {
                        foreach (var i in gr)
                        {
                            totalExercise += db.tbl_Exercise.Where(x => x.ExerciseGroupID == i).Select(x => x.ID).Count();
                        }
                    }
                }
                //double totalPoint = SetPackagePointConfigTable.GetPoint(correct);
                double totalPoint = 0;
                double persentCorrect = 0;
                if (totalExercise != 0)
                {
                    totalPoint = correct * 10 / totalExercise;
                    totalPoint = Math.Round(totalPoint, 2);

                    persentCorrect = correct * 100 / totalExercise;
                    persentCorrect = Math.Round(persentCorrect, 0);
                }
                sethistory = SetPackageHistoryTable.updatedone(sethistory.ID, correct, totalPoint, aclog.UserName);
                string linkdone = HttpContext.Request.UrlReferrer.AbsoluteUri.Replace("doing-test", "done-test");
                return Redirect(linkdone);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("StudentSetController", "SubmitTest", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
                //return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public class Result
        {
            public int QuestionID { get; set; }
            public List<string> ListRecord { get; set; }
        }
        public class SubmitTask
        {
            public int TaskID { get; set; }
            public string Answer { get; set; }
        }
        [HttpPost]
        public ActionResult SubmitPart(int part, string result)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            List<Result> listresult = JsonConvert.DeserializeObject<List<Result>>(result);
            //lấy setdetail ID 
            int setdetailid = HttpContext.Request.UrlReferrer.Segments[2].Replace("/", "").ToInt(0);
            var setdetail = SetPackageDetailTable.getbyid(setdetailid);
            var setpackage = SetPackageTable.getbyid(setdetail.SetPackageID.Value);
            var h = SetPackageHistoryTable.getbyStudentIDandSetDetailID(aclog.ID, setdetailid);
            if (h == null)
            {
                return Json(new { rs = false, message = "Fail to submit" }, JsonRequestBehavior.AllowGet);
            }
            List<int> lexaminerID = SetPackageExaminersTable.getlistexaminer().Select(n => n.TeacherID.Value).OrderBy(n => n).ToList();
            if (lexaminerID.Count == 0)
            {
                return Json(new { rs = false, message = "This set has no Examiner, please contact admin!" }, JsonRequestBehavior.AllowGet);
            }
            tbl_SetPackageHistoryDetail part1 = SetPackageHistoryDetailTable.getbyHistoryIDandExerciseGroupID(h.ID, 1);
            tbl_SetPackageHistoryDetail part2 = SetPackageHistoryDetailTable.getbyHistoryIDandExerciseGroupID(h.ID, 2);
            tbl_SetPackageHistoryDetail part3 = SetPackageHistoryDetailTable.getbyHistoryIDandExerciseGroupID(h.ID, 3);
            int idfix = 0;
            tbl_SetPackageStudent setst = SetPackageStudentTable.get_by_setID_and_studentID(setpackage.ID, aclog.ID);
            idfix = setst.ExaminerID.Value;
            if (part == 1)
            {
                if (part1 != null)
                {
                    if (!string.IsNullOrEmpty(part1.Result))
                    {
                        return Json(new { rs = false, message = "This part had been submited" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { rs = false, message = "Fail to submit" }, JsonRequestBehavior.AllowGet);
                }

            }
            if (part == 2)
            {
                if (part2 != null)
                {
                    if (!string.IsNullOrEmpty(part2.Result))
                    {
                        return Json(new { rs = false, message = "This part had been submited" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { rs = false, message = "Fail to submit" }, JsonRequestBehavior.AllowGet);
                }
            }
            if (part == 3)
            {
                if (part3 != null)
                {
                    if (!string.IsNullOrEmpty(part3.Result))
                    {
                        return Json(new { rs = false, message = "This part had been submited" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { rs = false, message = "Fail to submit" }, JsonRequestBehavior.AllowGet);
                }
            }

            tbl_ExerciseAddonStudent rs = ExerciseAddonStudentTable.getbyUIDStudent_SetPackageID_Type(aclog.ID, setpackage.ID, 3);
            if (rs != null)
            {
                foreach (var item in listresult)
                {
                    int questionid = item.QuestionID;
                    tbl_SetPackageStudentExercise sse = SetPackageStudentExerciseTable.getbyid(questionid);
                    tbl_Exercise ex = ExerciseTable.getbyid(sse.ExerciseID.Value);
                    tbl_ExerciseAddonStudentOnSetPackage onset = ExerciseAddonStudentOnSetPackageTable.insert(rs.ID, ex.PartNumber.Value, ex.ID, ex.ExerciseTitle, setpackage.ID, setpackage.PackageName, aclog.UserName);

                    foreach (var record in item.ListRecord)
                    {
                        if (!string.IsNullOrEmpty(record))
                            ExerciseAddonStudentFileAudioTable.insertOnSetPackage(rs.ID, onset.ID, record, aclog.UserName);

                    }
                }
            }
            else
            {
                return Json(new { rs = false, message = "Fail to submit" }, JsonRequestBehavior.AllowGet);
            }

            if (part == 1)
            {
                //lấy sethistorydetail part1 
                var shd = SetPackageHistoryDetailTable.getbyHistoryIDandExerciseGroupID(h.ID, 1);
                SetPackageHistoryDetailTable.updateGroupResult(shd.ID, "Done", aclog.UserName);
                var shd2 = SetPackageHistoryDetailTable.insert(h.ID, 2, "Part 2", aclog.UserName);
                return Json(new { rs = true, timenext = 3, message = "Next! Part 2" }, JsonRequestBehavior.AllowGet);
            }
            if (part == 2)
            {
                var shd = SetPackageHistoryDetailTable.getbyHistoryIDandExerciseGroupID(h.ID, 2);
                SetPackageHistoryDetailTable.updateGroupResult(shd.ID, "Done", aclog.UserName);

                var shd2 = SetPackageHistoryDetailTable.insert(h.ID, 3, "Part 3", aclog.UserName);
                return Json(new { rs = true, timenext = 5, message = "Next! Part 3" }, JsonRequestBehavior.AllowGet);
            }
            if (part == 3)
            {
                var shd = SetPackageHistoryDetailTable.getbyHistoryIDandExerciseGroupID(h.ID, 3);
                SetPackageHistoryDetailTable.updateGroupResult(shd.ID, "Done", aclog.UserName);
                if (idfix != 0)// gửi thông báo cho giáo viên
                {
                    var nt = NotificationTable.insertsingle("", "Thông báo sửa bài", "Có bài tập mới từ khóa " + rs.CourseName, rs.UIDFix.Value, aclog.UserName, 6, rs.CourseID.Value);
                    
                }
                h = SetPackageHistoryTable.updatedone(h.ID, 0, 0, aclog.UserName);
                return Json(new { rs = true, timenext = 0, message = "Done" }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { rs = true, }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SubmitWriting(string result)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            List<SubmitTask> listresult = JsonConvert.DeserializeObject<List<SubmitTask>>(result);
            //lấy setdetail ID 
            int setdetailid = HttpContext.Request.UrlReferrer.Segments[2].Replace("/", "").ToInt(0);
            var setdetail = SetPackageDetailTable.getbyid(setdetailid);
            if (setdetail.TypeDetail == 4)
            {
                setdetail = SetPackageDetailTable.getbysetid(setdetail.SetPackageID.Value).Where(n => n.TypeDetail == 3 && n.isHide != true).FirstOrDefault();
            }
            var setpackage = SetPackageTable.getbyid(setdetail.SetPackageID.Value);
            var h = SetPackageHistoryTable.getbyStudentIDandSetDetailID(aclog.ID, setdetailid);

            if (h == null)
            {
                return Json(new { rs = false, message = "Fail to submit" }, JsonRequestBehavior.AllowGet);
            }
            if (h.IsDone.Value)
            {
                return Json(new { rs = false, message = "This task has been submitted!" }, JsonRequestBehavior.AllowGet);
            }
            tbl_SetPackageStudent setst = SetPackageStudentTable.get_by_setID_and_studentID(setpackage.ID, aclog.ID);
            int idfix = setst.ExaminerID.Value;
            foreach (var item in listresult)
            {
                tbl_SetPackageStudentExercise sse = SetPackageStudentExerciseTable.getbyid(item.TaskID);
                tbl_Exercise ex = ExerciseTable.getbyid(sse.ExerciseID.Value);

                string answer = item.Answer;

                string tasknum = "One";
                if (sse.PartNumber.Value == 4)
                {
                    tasknum = "One";
                }
                if (sse.PartNumber.Value == 5)
                {
                    tasknum = "Two";
                }
                var rs = ExerciseAddonStudentTable.insert(aclog, new tbl_Courses() { ID = 0, CourseName = "Writing Task " + tasknum }, new tbl_SetPackage() { ID = setpackage.ID, PackageName = setpackage.PackageName }, ex, "", answer, 0, idfix, aclog.UserName);
                if (rs != null)
                {
                    if (idfix != 0)// gửi thông báo cho giáo viên
                    {
                        var nt = NotificationTable.insertsingle("", "Thông báo sửa bài", "Có bài tập mới từ khóa " + rs.CourseName, rs.UIDFix.Value, aclog.UserName, 6, rs.CourseID.Value);
                        
                    }
                }
                else
                {
                    return Json(new { rs = false, message = "Fail to submit" }, JsonRequestBehavior.AllowGet);
                }
            }
            h = SetPackageHistoryTable.updatedone(h.ID, 0, 0, aclog.UserName);
            string linkdone = HttpContext.Request.UrlReferrer.AbsoluteUri.Replace("doing-test", "done-test");
            return Json(new { rs = true, message = "Submit success!", linkdone = linkdone }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DoneTest(int id = 0)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 5)
                {
                    return RedirectToAction("Signin", "Login", new { area = "" });
                }
                var setdetail = SetPackageDetailTable.getbyid(id);

                if (setdetail == null)
                {
                    SetAlert("You not have this set!", "e");
                    return RedirectToAction("MySet", "StudentSet", new { area = "" });
                }
                tbl_SetPackage set = SetPackageTable.getbyid(setdetail.SetPackageID.Value);
                var packhistory = SetPackageHistoryTable.getbyStudentIDandSetDetailID(aclog.ID, setdetail.ID);
                if (packhistory == null)
                {
                    string link = HttpContext.Request.Url.AbsoluteUri.Replace("done-test", "doing-test");
                    SetAlert("No record!", "e");
                    return Redirect(link);
                }
                DateTime currenttime = GetDateTime.Now;
                TimeSpan min = currenttime - packhistory.CreatedDate.Value;
                double timeup = (setdetail.Minute.Value) * 1.0 - min.TotalMinutes;
                if (timeup > 0 && packhistory.IsDone == false)
                {
                    string link = HttpContext.Request.Url.AbsoluteUri.Replace("done-test", "doing-test");
                    SetAlert("No record!", "e");
                    return Redirect(link);
                }
                //if (setdetail.TypeDetail == 3 || setdetail.TypeDetail == 4)
                //{
                //    SetAlert("Your test is grading! Please check notification", "s");
                //    return RedirectToAction("SetPackages", "StudentSetCustom", new { ID = setdetail.SetPackageID });
                //}
                //xong
                return View(setdetail);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("StudentSetController", "DoneTest", uid, ex.Message);
                return RedirectToAction("Signin", "Login", new { area = "" });
            }

        }

        [HttpPost]
        public ActionResult CheckTimeUp()
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                int setdetailid = HttpContext.Request.UrlReferrer.Segments[2].Replace("/", "").ToInt(0);
                var setdetail = SetPackageDetailTable.getbyid(setdetailid);
                if (setdetail == null)
                {
                    return Json(new { rs = false, message = "You not have this set!" }, JsonRequestBehavior.AllowGet);
                }
                var sethistory = SetPackageHistoryTable.getbyStudentIDandSetDetailID(aclog.ID, setdetail.ID);
                if (sethistory == null)
                {
                    return Json(new { rs = false, message = "You not have this set!" }, JsonRequestBehavior.AllowGet);
                }
                DateTime currenttime = GetDateTime.Now;
                TimeSpan min = currenttime - sethistory.CreatedDate.Value;
                int Minute = setdetail.Minute.Value;
                if (setdetail.TypeDetail == 4)
                {
                    //kiểm tra xong part chưa -- lưu trong bản setpackagehistorydetail
                    tbl_SetPackageHistoryDetail part1 = SetPackageHistoryDetailTable.getbyHistoryIDandExerciseGroupID(sethistory.ID, 1);
                    tbl_SetPackageHistoryDetail part2 = SetPackageHistoryDetailTable.getbyHistoryIDandExerciseGroupID(sethistory.ID, 2);
                    tbl_SetPackageHistoryDetail part3 = SetPackageHistoryDetailTable.getbyHistoryIDandExerciseGroupID(sethistory.ID, 3);
                    tbl_SetPackageHistoryDetail taskwriting = SetPackageHistoryDetailTable.getbyHistoryIDandExerciseGroupID(sethistory.ID, 4);

                    if (string.IsNullOrEmpty(part1.Result))//chưa lưu gì là chưa submit part1
                    {
                        min = currenttime - part1.CreatedDate.Value;
                        Minute = 14;
                    }
                    else if (string.IsNullOrEmpty(part2.Result))
                    {
                        min = currenttime - part2.CreatedDate.Value;
                        Minute = 3;
                    }
                    else
                    {
                        tbl_SetPackageDetail sdwriting = SetPackageDetailTable.getbysetid(setdetail.SetPackageID.Value).Where(n => n.TypeDetail == 3 && n.isHide != true).FirstOrDefault();
                        if (sdwriting != null)
                        {
                            if (string.IsNullOrEmpty(part3.Result))
                            {
                                min = currenttime - part3.CreatedDate.Value;
                                Minute = 5;
                            }
                            else
                            {
                                min = currenttime - taskwriting.CreatedDate.Value;
                                Minute = 60;
                            }
                        }
                        else
                        {
                            min = currenttime - part3.CreatedDate.Value;
                            Minute = 5;
                        }
                    }
                }
                double timeup = (Minute) * 1.0 - min.TotalMinutes;
                if (setdetail.TypeDetail == 4)
                {
                    if (timeup <= 0)
                    {
                        return Json(new { rs = true, }, JsonRequestBehavior.AllowGet);
                    }
                    else if (timeup <= 2.0)
                    {
                        return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { rs = false, message = "You not have this set!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    if (timeup <= 0)
                    {
                        return Json(new { rs = true, }, JsonRequestBehavior.AllowGet);
                    }
                    else if (timeup <= 5.0)
                    {
                        return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { rs = false, message = "You not have this set!" }, JsonRequestBehavior.AllowGet);
                    }
                }


            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("StudentSetController", "CheckTimeUp", uid, ex.Message);
                //return RedirectToAction("Signin", "Login", new { area = "" });
                return Json(new { rs = false, message = "Some thing error!" }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}

