using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ExerciseGroupTable
    {
        /// <summary>
        /// thêm dữ liệu gói câu hỏi của giáo trình hoặc khóa, ko phải SetPakage
        /// </summary>
        /// <param name="curid"></param>
        /// <param name="courseid"></param>
        /// <param name="type"></param>
        /// <param name="groupname"></param>
        /// <param name="createby"></param>
        /// <returns></returns>
        public static tbl_ExerciseGroup insert(int curid, int courseid, int type, string groupname, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciseGroup ck = new tbl_ExerciseGroup();
                ck.CurriculumID = curid;
                ck.CourseID = courseid;
                ck.TypeGroup = type;
                if (type == 1)
                    ck.TypeGroupName = "Tự luận";
                if (type == 2)
                    ck.TypeGroupName = "Trắc nghiệm";
                if (type == 3)
                    ck.TypeGroupName = "Nói";
                ck.ExerciseGroupName = groupname;
                ck.CountExe = 0;
                ck.isHide = false;
                ck.IsDelete = false;
                //thông tin package
                ck.SetPackage = false;
                ck.TypeMultiChoice = 0;
                ck.SetPackageQuestion = "";
                //
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_ExerciseGroup.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        /// <summary>
        /// cập nhật group câu hỏi
        /// </summary>
        /// <param name="id"></param>
        /// <param name="groupname"></param>
        /// <param name="createby"></param>
        /// <returns></returns>
        public static tbl_ExerciseGroup update(int id, string groupname, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciseGroup ck = db.tbl_ExerciseGroup.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.ExerciseGroupName = groupname;
                    if (ck.TypeGroup == 1)
                        ck.TypeGroupName = "Essay";
                    if (ck.TypeGroup == 2)
                        ck.TypeGroupName = "Multiple-choice";
                    if (ck.TypeGroup == 3)
                        ck.TypeGroupName = "Speaking";
                    ck.CountExe = 0;
                    ck.isHide = false;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_ExerciseGroup Delete(int id, bool IsDelete, string ModifiedBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciseGroup ck = db.tbl_ExerciseGroup.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.IsDelete = IsDelete;
                    ck.ModifiedBy = ModifiedBy;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        /// <summary>
        /// cập nhật số lượng câu hỏi của group
        /// </summary>
        /// <param name="id"></param>
        /// <param name="createby"></param>
        public static void updatecount(int id, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciseGroup ck = db.tbl_ExerciseGroup.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.CountExe = ck.CountExe + 1;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
            }
        }
        /// <summary>
        /// lấy theo ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static tbl_ExerciseGroup getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciseGroup l = db.tbl_ExerciseGroup.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }
        /// <summary>
        /// Lấy theo giáo trình và dạng bài
        /// </summary>
        /// <param name="currid">id khóa</param>
        /// <param name="type">dạng bài: 1 Essay, 2 Multiple-choice, 3 speaking</param>
        /// <returns></returns>
        public static List<tbl_ExerciseGroup> getbycurrid(int currid, int type)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseGroup.Where(n => n.CurriculumID == currid && n.TypeGroup == type && n.IsDelete == false).ToList();
                return l;
            }
        }
        /// <summary>
        /// Lấy theo giáo trình
        /// </summary>        
        /// <param name="type">dạng bài: 1 Essay, 2 Multiple-choice, 3 speaking</param>
        /// <returns></returns>
        public static List<tbl_ExerciseGroup> getbycurriculumid(int currid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseGroup.Where(n => n.CurriculumID == currid).ToList();
                return l;
            }
        }
        /// <summary>
        /// Lấy theo khóa học và dạng bài
        /// </summary>
        /// <param name="courseid">id khóa</param>
        /// <param name="type">dạng bài: 1 Essay, 2 Multiple-choice, 3 speaking</param>
        /// <returns></returns>
        public static List<tbl_ExerciseGroup> getbycourseidvstype(int courseid, int type)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseGroup.Where(n => n.CourseID == courseid && n.TypeGroup == type && n.IsDelete == false).ToList();
                return l;
            }
        }
        /// <summary>
        /// Lấy theo khóa học
        /// </summary>
        /// <param name="courseid">id khóa</param>        
        /// <returns></returns>
        public static List<tbl_ExerciseGroup> getbycourseid(int courseid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseGroup.Where(n => n.CourseID == courseid).ToList();
                return l;
            }
        }
        //======================================================= GÓI BÀI =============================================================================
        /// <summary>
        /// insert groub package
        /// </summary>
        /// <param name="setpackageid">id của set</param>
        /// <param name="type">1 Essay, 2 Multiple-choice, 3 speaking hiện tại chỉ có chắc nghiệm</param>
        /// <param name="groupname">tên nhóm bài</param>
        /// <param name="typemultichoice">1 choice, 2 drag, 3 input, 4 multi-choice, 5 map</param>
        /// <param name="des">mô tả về nhóm bài</param>
        /// <param name="question">câu hỏi tổng, đề bài cho các câu hỏi con</param>      
        /// <param name="exgrid">câu hỏi cha thuộc nhóm nào</param>
        /// <param name="createby">người tạo</param>
        /// <returns></returns>
        public static tbl_ExerciseGroup insertsetpackage(tbl_SetPackageDetail detail, int type, string groupname, int typemultichoice, string question, string introduction, int exgrid, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciseGroup ck = new tbl_ExerciseGroup();
                ck.CurriculumID = 0;
                ck.CourseID = 0;
                ck.TypeGroup = type;
                if (type == 1)
                    ck.TypeGroupName = "Essay";
                if (type == 2)
                    ck.TypeGroupName = "Multiple-choice";
                if (type == 3)
                    ck.TypeGroupName = "Speaking";
                ck.ExerciseGroupName = groupname;
                ck.CountExe = 0;
                ck.isHide = false;
                //thông tin package
                ck.SetPackage = true;
                ck.TypeMultiChoice = typemultichoice;//1 choice, 2 drag, 3 input, 4 multi-choice, 5 map
                ck.Introduction = introduction; //lời dẫn
                ck.SetPackageQuestion = question;
                ck.SetPackageDetailID = detail.ID;
                ck.SetPackageID = detail.SetPackageID.Value;
                ck.ExerciseGroupID = exgrid;
                //
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_ExerciseGroup.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static tbl_ExerciseGroup updatesetpackage(int id, string groupname, string question, string introduction, int egid, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciseGroup ck = db.tbl_ExerciseGroup.Where(n => n.ID == id).FirstOrDefault();
                if (ck == null)
                    return null;
                ck.CurriculumID = 0;
                ck.CourseID = 0;
                if (!string.IsNullOrEmpty(groupname))
                    ck.ExerciseGroupName = groupname;
                ck.CountExe = 0;
                ck.ExerciseGroupID = egid;
                ck.Introduction = introduction;
                ck.isHide = false;
                //thông tin package
                ck.SetPackage = true;
                ck.SetPackageQuestion = question;

                //
                ck.ModifiedBy = createby;
                ck.ModifiedDate = GetDateTime.Now;
                db.SaveChanges();
                return ck;
            }
        }
        /// <summary>
        /// Lấy theo set gói
        /// </summary>
        /// <param name="setid">id khóa</param>        
        /// <returns></returns>
        public static List<tbl_ExerciseGroup> getbysetid(int setid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseGroup.Where(n => n.SetPackageID == setid && n.SetPackage == true && n.isHide != true).ToList();
                return l;
            }
        }
        /// <summary>
        /// Lấy theo set detail => các câu hỏi thuộc kĩ năng đó
        /// </summary>
        /// <param name="setdetail"></param>
        /// <returns></returns>
        public static List<tbl_ExerciseGroup> getbysetdetail(int setdetail)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseGroup.Where(n => n.SetPackageDetailID == setdetail && n.SetPackage == true && n.isHide == false).ToList();
                return l;
            }
        }
        /// <summary>
        /// xóa nhóm câu hỏi
        /// </summary>
        /// <param name="groupid">id nhóm cần xóa</param>
        /// <param name="createby">người xóa</param>
        /// <returns></returns>
        public static tbl_ExerciseGroup deletegroup(int groupid, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseGroup.Where(n => n.ID == groupid).FirstOrDefault();
                if (l != null)
                {
                    l.isHide = true;
                    l.ModifiedBy = createby;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return l;
            }
        }
        public static tbl_ExerciseGroup updateExercise(int id, int curriculumnDetailID)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciseGroup ck = db.tbl_ExerciseGroup.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    if (curriculumnDetailID != 0)
                        ck.CurriculumnDetailID = curriculumnDetailID;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        /// <summary>
        /// cập nhật group câu hỏi
        /// </summary>
        /// <param name="id"></param>
        /// <param name="groupname"></param>
        /// <param name="createby"></param>
        /// <returns></returns>
        public static tbl_ExerciseGroup update(int id, string groupname, int curriculumnDetailID, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciseGroup ck = db.tbl_ExerciseGroup.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.ExerciseGroupName = groupname;
                    if (ck.TypeGroup == 1)
                        ck.TypeGroupName = "Essay";
                    if (ck.TypeGroup == 2)
                        ck.TypeGroupName = "Multiple-choice";
                    if (ck.TypeGroup == 3)
                        ck.TypeGroupName = "Speaking";
                    ck.CountExe = 0;
                    if (curriculumnDetailID != 0)
                        ck.CurriculumnDetailID = curriculumnDetailID;
                    ck.isHide = false;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
    }
}