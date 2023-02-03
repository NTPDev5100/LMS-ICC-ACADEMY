using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class FAQTable
    {
        public static tbl_FAQ Insert(int role, string TitleQuestion, string AnswerContent, string CreatedBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_FAQ ck = new tbl_FAQ();
                ck.RoleID = role;
                switch(role)
                {
                    case 2:
                        ck.RoleName = "Trưởng phòng";
                        break;
                    case 3:
                        ck.RoleName = "Trưởng nhóm";
                        break;
                    case 4:
                        ck.RoleName = "Giáo viên";
                        break;
                    case 5:
                        ck.RoleName = "Học viên";
                        break;
                    case 6:
                        ck.RoleName = "Nhân viên(Tư vấn viên)";
                        break;
                    case 7:
                        ck.RoleName = "Học vụ";
                        break;
                    case 9:
                        ck.RoleName = "Kế toán";
                        break;
                    default:
                        ck.RoleName = "Tất cả";
                        break;
                }
                ck.TitleQuestion = TitleQuestion;
                ck.AnswerContent = AnswerContent;
                ck.isHide = false;
                ck.CreatedBy = CreatedBy;
                ck.CreatedDate = GetDateTime.Now;
                db.tbl_FAQ.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_FAQ UpdateIshide(int ID,bool isHide, string ModifiedBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_FAQ ck = db.tbl_FAQ.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.isHide = isHide;
                    ck.ModifiedBy = ModifiedBy;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        public static tbl_FAQ Update(int ID, int role, string TitleQuestion, string AnswerContent, string ModifiedBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_FAQ ck = db.tbl_FAQ.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.RoleID = role;
                    switch (role)
                    {
                        case 2:
                            ck.RoleName = "Trưởng phòng";
                            break;
                        case 3:
                            ck.RoleName = "Trưởng nhóm";
                            break;
                        case 4:
                            ck.RoleName = "Giáo viên";
                            break;
                        case 5:
                            ck.RoleName = "Học viên";
                            break;
                        case 6:
                            ck.RoleName = "Nhân viên(Tư vấn viên)";
                            break;
                        case 7:
                            ck.RoleName = "Học vụ";
                            break;
                        case 9:
                            ck.RoleName = "Kế toán";
                            break;
                        default:
                            ck.RoleName = "Tất cả";
                            break;
                    }
                    ck.TitleQuestion = TitleQuestion;
                    ck.AnswerContent = AnswerContent;
                    ck.ModifiedBy = ModifiedBy;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_FAQ GetByID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FAQ.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_FAQ> GetAll()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FAQ.ToList();
                return l;
            }
        }
    }
}