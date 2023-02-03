using AppZim.api;
using AppZim.Models;
using AppZim.TableSql;
using MB.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace AppZim.Areas.api.package
{
    public class packageController : ApiController
    {
        public string getToken()
        {
            try
            {
                IEnumerable<string> headerValues = Request.Headers.GetValues("token");
                return headerValues.FirstOrDefault();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        [HttpGet]
        public HttpResponseMessage store(string search, int type, int page = 1)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 5)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                if (search == null)
                    search = "";
                int pageSize = 10;
                var l = new List<API_Package_getList_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.API_Package_getList(aclog.ID, search, type, (page - 1) * pageSize, pageSize).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = l, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage getFreePackage()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 5)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                var packageid = HttpContext.Current.Request.Form.Get("PackageID").ToInt(0);
                var setpackagestudent = SetPackageStudentTable.get_by_setID_and_studentID(packageid, aclog.ID);
                if (setpackagestudent != null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                var setpackage = SetPackageTable.getbyid(packageid);
                if (setpackage == null || setpackage.Price > 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                List<int> lexaminerID = SetPackageExaminersTable.getlistexaminer().OrderBy(n => n.TeacherID).Select(n => n.TeacherID.Value).ToList();
                if (lexaminerID.Count <= 0)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }
                int startID = 0;
                tbl_SetPackageStudent firstset = SetPackageStudentTable.getall().OrderByDescending(n => n.ID).FirstOrDefault();
                if (firstset != null)
                {
                    startID = firstset.ExaminerID.Value;
                }
                List<int> lteacherid = new List<int>();
                lteacherid = lexaminerID.Where(n => n > startID).ToList();
                if (lteacherid.Count <= 0)
                {
                    lteacherid.AddRange(lexaminerID);
                }
                int UIDFix = lteacherid[0];
                setpackagestudent = SetPackageStudentTable.insert(aclog.ID, aclog.FullName, packageid, setpackage.Price.Value, 0, 0, 0, UIDFix, aclog.UserName);
                setpackagestudent = SetPackageStudentTable.updatePaid(setpackagestudent.ID, true, aclog.UserName);// Cập nhật đã thanh toán rồi=> xác nhận sở hữu => cho làm bài
                if (setpackagestudent == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                else
                    return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message + "||" + ex.InnerException });
            }
        }
        [HttpPost]
        public HttpResponseMessage checkout()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 5)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                int ID = HttpContext.Current.Request.Form.Get("PackageID").ToInt(0);
                int paymenttype = 1;//tiền mặt
                var pack = SetPackageTable.getbyid(ID);
                if (pack == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }
                //giáo viên nào sẽ là người chấm bài cho người này bây giờ?
                List<int> lexaminerID = SetPackageExaminersTable.getlistexaminer().OrderBy(n => n.TeacherID).Select(n => n.TeacherID.Value).ToList();
                if (lexaminerID.Count <= 0)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }
                int startID = 0;
                tbl_SetPackageStudent firstset = SetPackageStudentTable.getall().OrderByDescending(n => n.ExaminerID).FirstOrDefault();
                if (firstset != null)
                {
                    startID = firstset.ExaminerID.Value;
                }
                List<int> lteacherid = new List<int>();
                lteacherid = lexaminerID.Where(n => n > startID).ToList();
                if (lexaminerID.Count > 0)
                {
                    lteacherid.AddRange(lexaminerID);
                }
                int UIDFix = lteacherid[0];
                var setpackagestudent = SetPackageStudentTable.get_by_setID_and_studentID(ID, aclog.ID);
                if (setpackagestudent == null)
                {
                    setpackagestudent = SetPackageStudentTable.insert(aclog.ID, aclog.FullName, ID, pack.Price.Value, paymenttype, 0, 0, UIDFix, aclog.UserName);
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message + "||" + ex.InnerException });
            }
        }
        [HttpGet]
        public HttpResponseMessage mypackage(string search, int type, int page = 1)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 5)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                if (search == null)
                    search = "";
                int pageSize = 10;
                var l = new List<API_Package_getMyPackageList_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.API_Package_getMyPackageList(aclog.ID, search, type, (page - 1) * pageSize, pageSize).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = l, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getPackageSkill(int packageid)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 5)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                var l = new List<API_Skill_GetList_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.API_Skill_GetList(aclog.ID, packageid).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = l, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage startTestWriting()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 5)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                int skillid = HttpContext.Current.Request.Form.Get("SkillID").ToInt(0);
                var setdetail = SetPackageDetailTable.getbyid(skillid);
                if (setdetail == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });

                if (setdetail.isHide.Value)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });

                tbl_SetPackage set = SetPackageTable.getbyid(setdetail.SetPackageID.Value);
                var pack = SetPackageStudentTable.get_by_setID_and_studentID(setdetail.SetPackageID.Value, aclog.ID);
                if (pack == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });

                if (setdetail.TypeDetail == 3)
                {
                    List<tbl_SetPackageStudentExercise> sse = SetPackageStudentExerciseTable.getall(set.ID, aclog.ID).Where(x => x.PartNumber == 4 || x.PartNumber == 5).ToList();//kiểm tra có bài tập chưa,
                    if (sse.Count == 0)
                    {
                        List<int> listtopicID = TopicTable.getall().Select(n => n.ID).ToList();
                        Random random = new Random();

                        tbl_Topic topic = TopicTable.getbyid(listtopicID[random.Next(0, listtopicID.Count - 1)]);
                        if (topic == null)
                            return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });

                        //ds câu hỏi của topic
                        List<tbl_Exercise> listExTopic = ExerciseTable.getListTopicQuestion(topic.ID).Where(n => n.isHide != true).ToList();

                        List<int> taskone = listExTopic.Where(n => n.PartNumber == 4).Select(n => n.ID).ToList();

                        List<int> tasktwo = listExTopic.Where(n => n.PartNumber == 5).Select(n => n.ID).ToList();

                        if (listExTopic.Count == 0 || taskone.Count == 0 || tasktwo.Count == 0)
                            return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });

                        //insert task one
                        tbl_Exercise ex_taskone = ExerciseTable.getbyid(taskone[random.Next(0, taskone.Count - 1)]);
                        SetPackageStudentExerciseTable.insert(set.ID, aclog.ID, ex_taskone.ID, ex_taskone.ExerciseTitle, 4, aclog.UserName);
                        //insert task two
                        tbl_Exercise ex_tasktwo = ExerciseTable.getbyid(tasktwo[random.Next(0, tasktwo.Count - 1)]);
                        SetPackageStudentExerciseTable.insert(set.ID, aclog.ID, ex_tasktwo.ID, ex_tasktwo.ExerciseTitle, 5, aclog.UserName);
                    }
                    var taskonelist = SetPackageStudentExerciseTable.getall(set.ID, aclog.ID).Where(n => n.PartNumber == 4 || n.PartNumber == 5).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = taskonelist, message = ApiMessage.Success });
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage startTestSpeaking()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 5)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                int skillid = HttpContext.Current.Request.Form.Get("SkillID").ToInt(0);
                var setdetail = SetPackageDetailTable.getbyid(skillid);
                if (setdetail == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });

                if (setdetail.isHide.Value)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });

                tbl_SetPackage set = SetPackageTable.getbyid(setdetail.SetPackageID.Value);
                var pack = SetPackageStudentTable.get_by_setID_and_studentID(setdetail.SetPackageID.Value, aclog.ID);
                if (pack == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });

                if (setdetail.TypeDetail == 4)
                {
                    List<tbl_SetPackageStudentExercise> sse = SetPackageStudentExerciseTable.getall(set.ID, aclog.ID).Where(x => x.PartNumber == 1 || x.PartNumber == 2 || x.PartNumber == 3).ToList();//kiểm tra có bài tập chưa
                    if (sse.Count == 0)
                    {
                        List<int> listtopicID = TopicTable.getall().Select(n => n.ID).ToList();
                        Random random = new Random();

                        tbl_Topic topic = TopicTable.getbyid(listtopicID[random.Next(0, listtopicID.Count - 1)]);
                        if (topic == null)
                            return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });

                        //ds câu hỏi của topic
                        List<tbl_Exercise> listExTopic = ExerciseTable.getListTopicQuestion(topic.ID).Where(n => n.isHide != true).ToList();

                        List<int> part1 = listExTopic.Where(n => n.PartNumber == 1).Select(n => n.ID).ToList();

                        List<int> part2 = listExTopic.Where(n => n.PartNumber == 2).Select(n => n.ID).ToList();

                        List<int> part3 = listExTopic.Where(n => n.PartNumber == 3).Select(n => n.ID).ToList();

                        if (listExTopic.Count == 0 || part1.Count == 0 || part2.Count == 0 || part3.Count == 0)
                            return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });

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
                    }
                    var list = SetPackageStudentExerciseTable.getall(set.ID, aclog.ID).Where(n => n.PartNumber == 1 || n.PartNumber == 2 || n.PartNumber == 3).OrderBy(x => x.PartNumber).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = list, message = ApiMessage.Success });
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage submitWriting()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 5)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                int setdetailid = HttpContext.Current.Request.Form.Get("SkillID").ToInt(0);
                string result = HttpContext.Current.Request.Form.Get("Result");
                List<SubmitTask> listresult = JsonConvert.DeserializeObject<List<SubmitTask>>(result);
                //lấy setdetail ID 
                var setdetail = SetPackageDetailTable.getbyid(setdetailid);
                if (setdetail.TypeDetail == 4)
                {
                    setdetail = SetPackageDetailTable.getbysetid(setdetail.SetPackageID.Value).Where(n => n.TypeDetail == 3 && n.isHide != true).FirstOrDefault();
                }
                var package = SetPackageTable.getbyid(setdetail.SetPackageID.Value);
                var times = 0;
                var checkTime = SetPackageHistoryTable.getbyStudentIDandSetDetailID(aclog.ID, setdetail.ID);
                if(checkTime != null)
                {
                    times = checkTime.Times.Value;
                }    
                //lưu lại lịch sử làm bài
                var sethistory = SetPackageHistoryTable.insert(aclog.ID, setdetail.SetPackageID.Value, setdetail.ID, aclog.UserName, times + 1);

                List<tbl_ExerciseGroup> questiongroup = ExerciseGroupTable.getbysetdetail(setdetail.ID);
                foreach (var item in questiongroup)
                {
                    SetPackageHistoryDetailTable.insert(sethistory.ID, item.ID, item.SetPackageQuestion, aclog.UserName);
                }
                tbl_SetPackageStudent setst = SetPackageStudentTable.get_by_setID_and_studentID(setdetail.SetPackageID.Value, aclog.ID);
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
                    var rs = ExerciseAddonStudentTable.insert(aclog, new tbl_Courses() { ID = 0, CourseName = "Writing Task " + tasknum }, new tbl_SetPackage() { ID = package.ID, PackageName = package.PackageName }, ex, "", answer, 0, setst.ExaminerID.Value, aclog.UserName);
                    if (rs != null)
                    {
                        if (setst.ExaminerID.Value != 0)// gửi thông báo cho giáo viên
                        {
                            var nt = NotificationTable.insertsingle("", "Thông báo sửa bài", "Có bài tập mới từ khóa " + rs.CourseName, rs.UIDFix.Value, aclog.UserName, 6, rs.CourseID.Value);
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                    }
                }
                SetPackageHistoryTable.updatedone(sethistory.ID, 0, 0, aclog.UserName);
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage submitSpeaking()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 5)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                int setdetailid = HttpContext.Current.Request.Form.Get("SkillID").ToInt(0);
                string result = HttpContext.Current.Request.Form.Get("Result");
                int test = 0;
                List<SubmitTask> listresult = JsonConvert.DeserializeObject<List<SubmitTask>>(result);
                //lấy setdetail ID 
                var setdetail = SetPackageDetailTable.getbyid(setdetailid);
                var setpackage = SetPackageTable.getbyid(setdetail.SetPackageID.Value);

                int idfix = 0;
                tbl_SetPackageStudent setst = SetPackageStudentTable.get_by_setID_and_studentID(setpackage.ID, aclog.ID);
                idfix = setst.ExaminerID.Value;
                var rs = ExerciseAddonStudentTable.insert(aclog, new tbl_Courses() { ID = 0, CourseName = "Speaking Test", }, setpackage, new tbl_Exercise() { ID = 0, Type = 3, ExerciseGroupID = 0, ExerciseTitle = "" }, "", "", 0, idfix, aclog.UserName);
                if (rs != null)
                {
                    foreach (var item in listresult)
                    {
                        int questionid = item.TaskID;
                        tbl_SetPackageStudentExercise sse = SetPackageStudentExerciseTable.getbyid(questionid);
                        tbl_Exercise ex = ExerciseTable.getbyid(sse.ExerciseID.Value);
                        tbl_ExerciseAddonStudentOnSetPackage onset = ExerciseAddonStudentOnSetPackageTable.insert(rs.ID, ex.PartNumber.Value, ex.ID, ex.ExerciseTitle, setpackage.ID, setpackage.PackageName, aclog.UserName);
                        test += 1;
                        if (!string.IsNullOrEmpty(item.Answer))
                            ExerciseAddonStudentFileAudioTable.insertOnSetPackage(rs.ID, onset.ID, item.Answer, aclog.UserName);
                    }
                }
                var times = 0;
                var checkTime = SetPackageHistoryTable.getbyStudentIDandSetDetailID(aclog.ID, setdetail.ID);
                if (checkTime != null)
                {
                    times = checkTime.Times.Value;
                }
                var sethistory = SetPackageHistoryTable.insert(aclog.ID, setdetail.SetPackageID.Value, setdetail.ID, aclog.UserName, times + 1);
                List<tbl_ExerciseGroup> questiongroup = ExerciseGroupTable.getbysetdetail(setdetail.ID);
                foreach (var item in questiongroup)
                {
                    SetPackageHistoryDetailTable.insert(sethistory.ID, item.ID, item.SetPackageQuestion, aclog.UserName);
                }

                if (idfix != 0)// gửi thông báo cho giáo viên
                {
                    var nt = NotificationTable.insertsingle("", "Thông báo sửa bài", "Có bài tập mới từ khóa " + rs.CourseName, rs.UIDFix.Value, aclog.UserName, 6, rs.CourseID.Value);
                }
                SetPackageHistoryTable.updatedone(sethistory.ID, 0, 0, aclog.UserName);
                return Request.CreateResponse(HttpStatusCode.OK, new { test, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage saveAudio()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 5)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                var fileupload = HttpContext.Current.Request.Files.Get("File");
                string link = "";
                if (fileupload != null)
                {
                    string name = Path.GetFileNameWithoutExtension(fileupload.FileName);
                    string ext = Path.GetExtension(fileupload.FileName);
                    if (ext.Contains(".js") || ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1") || ext.Contains(".exe"))
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.InvalidFile });
                    }
                    var fileName = Guid.NewGuid() + ext;
                    var path = Path.Combine(HttpContext.Current.Server.MapPath("~/Upload/exercise/"), fileName);
                    link = "/Upload/exercise/" + fileName;
                    fileupload.SaveAs(path);
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { link, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        // lấy danh sách các bài làm của học viên
        [HttpGet]
        public HttpResponseMessage viewEssayPackageResult(int skillID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 5)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                var setdetail = SetPackageDetailTable.getbyid(skillID);
                if (setdetail == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }
                var packhistory = SetPackageHistoryTable.getbyStudentIDandSetDetailID(aclog.ID, setdetail.ID);
                if (packhistory == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { skill = setdetail, skillHistory = packhistory, message = ApiMessage.NotFoundInformation });
                }
                var list = new List<tbl_ExerciseAddonStudent>();
                if (setdetail.TypeDetail == 3)
                {
                    list = ExerciseAddonStudentTable.GetDoneList(aclog.ID, setdetail.SetPackageID.Value, 1);
                }
                if (setdetail.TypeDetail == 4)
                {
                    list = ExerciseAddonStudentTable.GetDoneList(aclog.ID, setdetail.SetPackageID.Value, 3);
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = list, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        // xem chi tiết bài làm tự luận của học viên
        [HttpGet]
        public HttpResponseMessage getExerciseDoneDetailInPackage(int resultID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 5)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                var l = ExerciseAddonStudentTable.getbyid(resultID);
                if (l == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }
                var lfix = ExerciseAddonStudentFixTable.getbyexestudent(l.ID);
                var cmtList = new List<Comment>();
                for (int i = 0; i < lfix.Count; i++)
                {
                    var cmt = new Comment();
                    cmt.CommentID = lfix[i].ID;
                    var position = lfix[i].DataCMT.Split('|');
                    cmt.Text = position[0];
                    cmt.Start = position[1];
                    cmt.End = position[2];
                    cmt.CommentContent = lfix[i].FixNote;
                    cmtList.Add(cmt);
                }
                var audio = new List<API_ExerciseResult_GetAudioLink_Result>();
                using (var db = new ZimEntities())
                {
                    audio = db.API_ExerciseResult_GetAudioLink(l.ID, l.SetPackageID.Value).ToList();
                }
                var lscore = ExerciseScoreDetailTable.getbyexeid(l.ID).FirstOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, new { exercise = l, comment = cmtList, score = lscore, audio, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        // lấy danh sách các bài làm trắc nghiệm của học viên
        [HttpGet]
        public HttpResponseMessage viewMultipleChoicePackageResult(int skillID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 5)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                var history = SetPackageHistoryTable.getbyListStudentIDandSetDetailID(aclog.ID, skillID);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = history, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
    }
}
