using AppZim.Models;
using AppZim.TableSql;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static AppZim.Areas.Admin.Controllers.CourseController;

namespace AppZim.Areas.Admin.Controllers
{
    public class CourseNewController : Controller
    {
        // GET: Admin/CourseNew
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoadTietHoc(int numberOfLesson, string Date, string daySelected, int teacherId, string caSelected)
        {
            try
            {
                DateTime sDate = DateTime.ParseExact(Date, "dd/MM/yyyy", null).AddDays(-1); //ngày bắt đầu
                DateTime eDate = DateTime.Now; //ngày kết thúc được tính dựa vào ngày cuối cùng của lịch

                List<NgayHoc> list = new List<NgayHoc>();
                int index = 0;
                var teacher = AccountTable.getbyID(teacherId);
                int[] days = daySelected.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                int[] cahocs = caSelected.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                var offs = DayOffTable.getall().Select(x => x.DayOff).ToArray();
                do
                {
                    sDate = sDate.AddDays(1);
                    // kiểm tra xem có phải ngày nghỉ hay không
                    if (Array.IndexOf(offs, sDate) != -1)
                        continue;
                    // kiểm tra xem có phải ngày được chọn hay không
                    if (Array.IndexOf(days, Convert.ToInt32(sDate.DayOfWeek)) == -1)
                        continue;
                    //kiểm tra xem này này có ca được chọn là ca nào
                    int cahocindex = Array.FindIndex(days, 0, x => x == Convert.ToInt32(sDate.DayOfWeek));
                    var cahoc = cahocs[cahocindex];
                    var study = StudyTimeTable.getbyid(cahoc);
                    if(study == null) 
                        continue;
                    string[] stime = study.sTime.Split(':');
                    TimeSpan sth = new TimeSpan(stime[0].ToInt(), stime[1].ToInt(0), 0);
                    DateTime st = DateTime.Now.Date.Add(sth);
                    string[] etime = study.eTime.Split(':');
                    TimeSpan eth = new TimeSpan(etime[0].ToInt(), etime[1].ToInt(0), 0);
                    DateTime et = DateTime.Now.Date.Add(eth);
                    var check = checkteacher(teacherId, sDate, st, et, cahoc);
                    if (!check)
                    {
                        NgayHoc lt = new NgayHoc();
                        List<TietHoc> t = new List<TietHoc>();
                        lt.ID = index + 1;
                        lt.date = sDate.ToString("yyyy-MM-dd");
                        lt.eventName = sDate.ToString("dd/MM") + " - [" + study.sTime + "-" + study.eTime + "]";
                        lt.Color = "orange";
                        lt.calendar = "Đã chọn ngày học";
                        lt.TeacherID = teacher.ID;
                        lt.TeacherName = teacher.FullName;
                        lt.CaID = study.ID;
                        lt.CaName = "Ca: " + study.sTime + ":" + study.eTime + "";
                        index += 1;
                        list.Add(lt);
                    }
                }
                while (list.Count < numberOfLesson);
                eDate = sDate.Date;//lấy ngày kết thúc khóa học
                int totalca = 1; //số ca 1 ngày học
                return Json(new { rs = true, d = list, limit = totalca, edate = eDate.ToString("dd/MM/yyyy") }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { rs = false, message = ex.Message + "||" + ex.InnerException }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult LoadTietHocNTD(int GiaoTrinhID, string Date, string caId, string daySelected, int schoolId)
        {
            try
            {
                string[] arrayCaId = caId.Split(',');
                DateTime sDate = DateTime.ParseExact(Date, "dd/MM/yyyy", null); //ngày bắt đầu
                DateTime eDate = DateTime.Now; //ngày kết thúc được tính dựa vào ngày cuối cùng của lịch
                var giaotrinh = CurriculumTable.getbyID(GiaoTrinhID);
                var chitietGT = CurriculumDetailTable.getbycurriculumid(giaotrinh.ID);
                List<NgayHoc> l = new List<NgayHoc>();
                var DSGV = new List<DsGiaoVien>();
                int ID = 1;
                if (chitietGT != null)
                {
                    int totalStudy = arrayCaId.Count(); //số ca nó đã chọn
                    int soca1ngay = totalStudy;
                    int vitriCaId = 0;
                    int vitriMonId = 0;
                    List<NgaySelected> dates = LoadNgayHocNTD(soca1ngay, giaotrinh.TotalLesson.Value, daySelected, sDate);
                    int vitriDates = 0;
                    for (int i = 0; i < chitietGT.Count; i += 1)
                    {
                        // xử lý tìm ca học theo ID
                        var ca = StudyTimeTable.getbyid(Convert.ToInt32(arrayCaId[vitriCaId]));
                        if (ca == null)
                        {
                            continue;
                        }

                        // Lấy ds GV được dạy
                        var lTe = TaiGiaoVienNTD(schoolId, chitietGT[vitriMonId].SubjectID.Value, Convert.ToInt32(arrayCaId[vitriCaId]), dates[vitriDates].Date.Date).ToList();
                        DSGV = lTe;
                        if (lTe.Count == 0)
                        {
                            i = i - 1; //nếu không tìm được GV thì giảm i để chạy tiếp
                            if (vitriCaId == arrayCaId.Count() - 1) //nếu vị trí ca bằng số item trong array thì reset vị trí nó.
                            {
                                vitriCaId = 0;
                            }
                            else
                            {
                                vitriCaId += 1;
                            }
                            vitriDates += 1; //nếu ngày này không có thì tăng ngày lên để tìm GV cho có
                            continue;
                        }
                        else
                        {
                            vitriMonId += 1;
                        }

                        if (vitriCaId == arrayCaId.Count() - 1) //nếu vị trí ca bằng số item trong array thì reset vị trí nó.
                        {
                            vitriCaId = 0;
                        }
                        else
                        {
                            vitriCaId += 1;
                        }

                        NgayHoc lt = new NgayHoc();
                        List<TietHoc> t = new List<TietHoc>();

                        lt.ID = ID;
                        lt.date = dates[vitriDates].Date.ToString("yyyy-MM-dd");
                        lt.eventName = "" + dates[vitriDates].Date.ToString("dd/MM") + " - [" + ca.sTime + "-" + ca.eTime + "]";
                        lt.Color = "orange";
                        lt.calendar = "Đã chọn ngày học";
                        lt.TeacherID = lTe[0].id;
                        lt.TeacherName = lTe[0].name + "[" + ca.sTime + "-" + ca.eTime + "]";
                        lt.CaID = ca.ID;
                        lt.CaName = "Ca: " + ca.sTime + ":" + ca.eTime + "";
                       
                        l.Add(lt);
                        ID += 1;
                        vitriDates += 1;
                    }
                    l = l.OrderBy(n => n.ID).ToList();
                    eDate = dates[vitriDates - 1].Date;//lấy ngày kết thúc khóa học
                }
                int totalca = arrayCaId.Count(); //số ca 1 ngày học
                return Json(new { rs = true, d = l, dsgv = DSGV, limit = totalca, edate = eDate.ToString("dd/MM/yyyy") }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { rs = false, message = ex.Message + "||" + ex.InnerException }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Lấy ngẫu nhiên 1 giáo viên dạy được
        /// </summary>
        /// <param name="SchoolID"></param>
        /// <param name="MonID"></param>
        /// <param name="CaID"></param>
        /// <param name="Date"></param>
        /// <returns></returns>
        public static List<DsGiaoVien> TaiGiaoVienNTD(int SchoolID, int MonID, int CaID, DateTime Date)
        {
            List<DsGiaoVien> l = new List<DsGiaoVien>();
            var mon = SubjectTable.getbyIDs(MonID);

            using (var db = new ZimEntities())
            {
                //lất tất cả GV được tick dạy môn đó rồi đem check xem nó có rảnh để dạy không
                var dsGVticked = db.tbl_AssignTeacherClass.Where(i => i.ClassID == mon.ClassID && i.SubjectID == mon.ID && i.isHide == false).Select(i => i.UID).ToList();
                if (dsGVticked.Count() != 0)
                {
                    foreach (var teacherId in dsGVticked)
                    {
                        var teacher = db.tbl_Account.Where(x => x.ID == teacherId.Value && x.Status == 2 && x.IsDelete == false).Select(i => new { ID = i.ID, FullName = i.FullName }).FirstOrDefault();
                        if (teacher == null)
                            continue;
                        //kiểm tra xem giáo viên có đăng ký nghỉ hay ko
                        bool checkdayoffteacher = DayOffTeacherTable.CheckGVCoRanhKhong(teacherId.Value, Date.Date, CaID);
                        if (checkdayoffteacher == false) //nó bận rồi, người tiếp theo
                            continue;
                        //xem thằng đó có rảnh ko
                        bool ckteacher = checkteacherNTD(teacher.ID, Date.Date, CaID);
                        if (ckteacher == false)
                        {
                            DsGiaoVien a = new DsGiaoVien();
                            a.id = teacher.ID;
                            a.name = teacher.FullName;
                            l.Add(a);
                        }
                    }
                }
            }
            if (l.Count() != 0)
            {
                return l; //lấy ngẫu nhiên 1 GV thôi
            }
            return null;
        }

        /// <summary>
        /// Check GV có bị đụng ca dạy không (true:có đụng)
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="Date"></param>
        /// <param name="studyid"></param>
        /// <returns></returns>
        public static bool checkteacherNTD(int UID, DateTime Date, int studyid)//kiểm tra giáo viên có đụng ko
        {
            using (var db = new ZimEntities())
            {
                var study = StudyTimeTable.getbyid(studyid);
                if (study == null)
                    return true;
                bool check = false;
                string[] stime = study.sTime.Split(':');
                TimeSpan sth = new TimeSpan(stime[0].ToInt(), stime[1].ToInt(0), 0);
                DateTime st = GetDateTime.Now.Date.Add(sth);

                string[] etime = study.eTime.Split(':');
                TimeSpan eth = new TimeSpan(etime[0].ToInt(), etime[1].ToInt(0), 0);
                DateTime et = GetDateTime.Now.Date.Add(eth);

                var l = db.tbl_CoursesSchedule.Where(n => n.TeacherID == UID && n.IsHide != true && (n.Date.Value.Day == Date.Day && n.Date.Value.Month == Date.Month && n.Date.Value.Year == Date.Year)).ToList();
                if (l.Count() != 0)
                {
                    foreach (var item in l)
                    {
                        var stu = StudyTimeTable.getbyid(item.StudyTimeID.Value);
                        if (stu == null)
                            continue;

                        string[] istime = stu.sTime.Split(':');
                        TimeSpan isth = new TimeSpan(istime[0].ToInt(), istime[1].ToInt(0), 0);
                        DateTime ist = GetDateTime.Now.Date.Add(isth);

                        string[] ietime = stu.eTime.Split(':');
                        TimeSpan ieth = new TimeSpan(ietime[0].ToInt(), ietime[1].ToInt(0), 0);
                        DateTime iet = GetDateTime.Now.Date.Add(ieth);

                        if (st == ist || et == iet)// thời gian bắt đầu, kết thúc cùng nhau => ko được
                        {
                            check = true;
                            break;
                        }

                        if ((st <= ist && ist < et) || (st < iet && iet <= et))// thời gian bắt đầu, kết thúc nằm trong ca khác => ko được
                        {
                            check = true;
                            break;
                        }

                        if ((st < ist && iet < et) || (st > ist && iet > et))// ca bị bao hoặc bao 1 ca khác => ko được
                        {
                            check = true;
                            break;
                        }
                    }
                    if (check == true)
                        return check;
                }
                return check;
            }
        }
        public static bool checkteacher(int UID, DateTime Date, DateTime st, DateTime et, int studytime)//kiểm tra giáo viên có đụng ko
        {
            using (var db = new ZimEntities())
            {
                var off = db.tbl_DayOffTeacher.Where(x => x.isHide == false && x.UID == UID && x.DayOff == Date && x.StudyTimeID == studytime).FirstOrDefault();
                if (off != null)
                    return true;
                var l = db.tbl_CoursesSchedule.Where(n => n.TeacherID == UID && n.IsHide != true && (n.Date.Value.Day == Date.Day && n.Date.Value.Month == Date.Month && n.Date.Value.Year == Date.Year)).ToList();
                if (l.Count() != 0)
                {
                    foreach (var item in l)
                    {
                        var stu = StudyTimeTable.getbyid(item.StudyTimeID.Value);
                        if (stu == null)
                            continue;

                        string[] istime = stu.sTime.Split(':');
                        TimeSpan isth = new TimeSpan(istime[0].ToInt(), istime[1].ToInt(0), 0);
                        DateTime ist = DateTime.Now.Date.Add(isth);

                        string[] ietime = stu.eTime.Split(':');
                        TimeSpan ieth = new TimeSpan(ietime[0].ToInt(), ietime[1].ToInt(0), 0);
                        DateTime iet = DateTime.Now.Date.Add(ieth);

                        if (st == ist || et == iet)// thời gian bắt đầu, kết thúc cùng nhau => ko được
                        {
                            return true;
                        }

                        if ((st <= ist && ist < et) || (st < iet && iet <= et))// thời gian bắt đầu, kết thúc nằm trong ca khác => ko được
                        {
                            return true;
                        }

                        if ((st < ist && iet < et) || (st > ist && iet > et))// ca bị bao hoặc bao 1 ca khác => ko được
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        [HttpPost]
        public ActionResult TaiNgayHocNTD(string CaID, string Date, string daySelected)//lấy ra những ngày học được
        {
            string[] arrca = CaID.Split(',');
            // chuỗi json {"id": 17,limit: 1, eventName: 'Blank Event', calendar: 'Blank', color: 'blank', date: '2018-02-09'}
            List<Ngay> l = new List<Ngay>();

            DateTime sDate = DateTime.ParseExact(Date, "dd/MM/yyyy", null);
            DateTime Day = sDate.Date;
            for (int i = 1; i <= 180; i++)
            {
                Ngay t = new Ngay();
                if (i != 1)
                    Day = Day.AddDays(1);

                var check = CheckDaySelected(Day, daySelected);
                if (check == false)
                {
                    continue;
                }

                var checkdayoff = DayOffTable.getbydate(Day);
                if (checkdayoff != null)
                    continue;

                int limit = arrca.Count(); //chọn bao nhiêu ca thì nó sẽ trả lai view bấy nhiêu ca
                t.Day = "{\"id\":\"d-" + i + "\",\"limit\":" + limit + ", \"eventName\":\"Blank Event\", \"calendar\":\"blank\", \"color\":\"blank\", \"date\":\"" + Day.ToString("yyyy-MM-dd") + "\"}";
                if (limit > 0)
                    l.Add(t);
            }
            l = l.ToList();
            return Json(new { d = l }, JsonRequestBehavior.AllowGet);
        }
        public List<NgaySelected> LoadNgayHocNTD(int soca1ngay, int sobuoihoc, string daySelected, DateTime ngaybatdau)//lấy ra những ngày học được
        {
            List<NgaySelected> l = new List<NgaySelected>();
            //DateTime sDate = DateTime.ParseExact(ngaybatdau, "dd/MM/yyyy", null);
            //DateTime sDate = Convert.ToDateTime(Date);

            DateTime Day = ngaybatdau.AddDays(1).Date;
            for (int i = 1; i <= 180; i++)
            {
                NgaySelected day = new NgaySelected();
                if (i != 1)
                {
                    Day = Day.AddDays(1);
                    day.Date = Day;
                }
                else
                {
                    day.Date = Day;
                }

                var check = CheckDaySelected(day.Date, daySelected); //check xem ngày đó có trong ds đã chọn không
                if (check == false)
                {
                    continue;
                }

                var checkdayoff = DayOffTable.getbydate(Day);
                if (checkdayoff != null)
                    continue;

                for (var d = 0; d < soca1ngay; d++) //lặp lại ngày tương đương số ca 1 ngày
                {
                    l.Add(day);
                }
            }
            return l.OrderBy(x => x.Date).ToList();
        }
        public class NgaySelected
        {
            public DateTime Date { get; set; }
        }
        /// <summary>
        /// Check Day có trong DS ngày đã chọn không
        /// </summary>
        /// <param name="Day"></param>
        /// <param name="daySelected"></param>
        /// <returns></returns>
        public bool CheckDaySelected(DateTime Day, string daySelected)
        {
            bool check = false;
            string[] ngayhocdachon = daySelected.Split(','); //những ngày học nó đã chọn trong 1 tuần
            for (var j = 0; j < ngayhocdachon.Length; j++)
            {
                if ((int)Day.DayOfWeek == ngayhocdachon[j].ToInt())
                {
                    check = true;
                    break;
                }
            }
            return check;
        }
    }
}