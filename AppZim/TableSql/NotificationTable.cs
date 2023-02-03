using AppZim.Models;
using MB.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;

namespace AppZim.TableSql
{
    public class NotificationTable
    {
        //thêm dữ liệu
        public static tbl_Notification insert(string NotificationTitle, string NotificationContent, int Role, bool pushmail, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_Notification n = new tbl_Notification();
                n.NotificationTitle = NotificationTitle;// tiêu đề
                n.NotificationContent = NotificationContent;// nội dung
                n.NotificationRoleID = Role;
                n.isHide = false;
                n.PushDone = false;
                n.PushEmail = pushmail;
                n.CreatedBy = n.ModifiedBy = createby;
                n.CreatedDate = n.ModifiedDate = GetDateTime.Now;
                db.tbl_Notification.Add(n);
                db.SaveChanges();
                return n;
            }
        }

        public static tbl_Notification updatepushdoneparent(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Notification.Where(n => n.ID == ID).FirstOrDefault();
                if (l == null)
                    return null;
                l.PushDone = true;
                l.ModifiedDate = GetDateTime.Now;
                db.SaveChanges();
                return l;
            }
        }

        public static List<tbl_Notification> GetAllPT()
        {
            using (var db = new ZimEntities())
            {
                var n = db.tbl_Notification.OrderByDescending(x => x.ID).ToList();
                return n;
            }
        }
        /// <summary>
        /// 1 = /Admin/CourseDetail/NotificationCourse/{id}
        //2 = /Admin/Customer/CustomerWarning
        //3 = /Admin/CourseDetail/ScheduleCourse/
        //4 = /Admin/CourseDetail/ExerciseCourse/
        //5 = news-feed? group =
        //6 = / Admin / TeacherExercise / ExerciseProcess ? search = rs.CourseName
        //7 = / StudentExrcise / ExrciseDetailEssayFix /
        //8 = / Admin / FeedBack / FeedBackDetail /
        //9 = Media.Host + "invoice/?code=" + nameencode
        //10 = Admin/Cashier/RefundList
        //11 = /Admin/Notification/Notification
        //12 = /Admin/TeacherExercise/ExerciseDetail/
        //13 = /Admin/Customer/CustomerDetai/
        //14 = /Admin/Customer/CourseStudent/
        //15 = /Admin/Customer/WarningDetail/
        //16 = /Admin/Course/CourseList? search =
        //17 = /StudentCourse/SignContract
        //18 = Admin/CourseDetail/FeedbackSchedule
        //19 = Admin/TeacherCourse/FeedbackList

        /// </summary>
        /// <param name="NotificationID"></param>
        /// <param name="Title"></param>
        /// <param name="Content"></param>
        /// <param name="UID"></param>
        /// <param name="createby"></param>
        /// <param name="type"></param>
        /// <param name="idvalue"></param>
        /// <returns></returns>
        //thêm dữ liệu
        public static tbl_NotificationSingle insertsingle(string NotificationID, string Title, string Content, int UID, string createby, int type, int idvalue)
        {
            using (var db = new ZimEntities())
            {
                tbl_NotificationSingle n = new tbl_NotificationSingle();
                if (!string.IsNullOrEmpty(NotificationID))
                    n.NotificationID = NotificationID.ToInt();// loại thông báo
                n.NotificationTitle = Title;// danh sách role nhận
                n.NotificationContent = Content;
                n.IDValue = idvalue;// lưu id của các đường dẫn detail
                n.UID = UID;
                n.Status = 1;//1 chưa xem, 2 đã xem               
                n.NotiType = type;//
                n.QickView = false;
                n.CreatedBy = n.ModifiedBy = createby;
                n.CreatedDate = n.ModifiedDate = GetDateTime.Now;
                db.tbl_NotificationSingle.Add(n);
                db.SaveChanges();
                return n;
            }
        }
        public static tbl_NotificationSingle GetByID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_NotificationSingle.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }
        public static List<tbl_NotificationSingle> GetByUID(int UID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_NotificationSingle.Where(n => n.UID == UID).OrderByDescending(n => n.CreatedDate).ToList();
                return l;
            }
        }
        public static List<tbl_NotificationSingle> GetByUIDNew(int UID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_NotificationSingle.Where(n => n.UID == UID && n.Status == 1).OrderByDescending(n => n.CreatedDate).ToList();
                return l;
            }
        }
        
        public async static Task<List<tbl_NotificationSingle>> GetByUIDNewAsync(int UID)
        {
            using (var db = new ZimEntities())
            {
                var l = await db.tbl_NotificationSingle.Where(n => n.UID == UID && n.Status == 1).OrderByDescending(n => n.CreatedDate).ToListAsync();
                return l;
            }
        }

        public static List<tbl_NotificationSingle> GetAll(int UID)
        {
            using (var db = new ZimEntities())
            {
                var n = db.tbl_NotificationSingle.Where(x => x.UID == UID).OrderByDescending(x => x.ID).ToList();
                return n;
            }
        }

        public static void UpdateQuickView(int UID, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_NotificationSingle.Where(n => n.UID == UID && n.QickView != true).ToList();
                for (int i = 0; i < l.Count; i++)
                {
                    var n = l[i];
                    n.QickView = true;
                    n.ModifiedDate = GetDateTime.Now;
                    n.ModifiedBy = createby;
                    db.SaveChanges();
                }
            }
        }

        public static tbl_NotificationSingle updateView(int ID, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_NotificationSingle.Where(n => n.ID == ID).FirstOrDefault();
                if (l == null)
                    return null;
                l.Status = 2;
                l.ModifiedDate = GetDateTime.Now;
                l.ModifiedBy = createby;
                db.SaveChanges();
                return l;
            }
        }
    }
}