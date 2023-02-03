using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class EmailTemplateTable
    {
        public static tbl_EmailTemplate InsertUpdate(int Id, int type, string title, string content, string CreatedBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_EmailTemplate ck = db.tbl_EmailTemplate.Where(n => n.ID == Id).FirstOrDefault();
                if (ck != null)
                {
                    ck.EmailTitle = title;
                    ck.EmailContent = content;
                    ck.Type = type;
                    switch (type)
                    {
                        case 1:
                            ck.TypeName = "Mẫu gửi học viên tham gia lớp test đầu vào";
                            break;
                        case 2:
                            ck.TypeName = "Mẫu gửi học viên kết quả test và lộ trình học";
                            break;
                        case 3:
                            ck.TypeName = "Mẫu xác nhận thanh toán";
                            break;
                        case 4:
                            ck.TypeName = "Mẫu gửi học viên nhận lớp";
                            break;
                        case 5:
                            ck.TypeName = "Mẫu gửi học viên tài khoản test";
                            break;
                        case 6:
                            ck.TypeName = "Mẫu gửi nhân viên khi tạo mới";
                            break;
                        case 7:
                            ck.TypeName = "Mẫu gửi giáo viên";
                            break;
                        case 8:
                            ck.TypeName = "Mẫu hợp đồng đào tạo";
                            break;
                        case 9:
                            ck.TypeName = "Mẫu phiếu lương";
                            break;
                        case 10:
                            ck.TypeName = "Mẫu cảnh báo doanh thu";
                            break;
                        default:
                            ck.TypeName = "Mẫu thông báo kết thúc lộ trình học";
                            break;
                    }
                    ck.ModifiedBy = CreatedBy;
                    ck.ModifiedDate = DateTime.Now;
                    db.SaveChanges();
                }
                else
                {
                    ck = new tbl_EmailTemplate();
                    ck.Type = type;
                    switch (type)
                    {
                        case 1:
                            ck.TypeName = "Mẫu gửi học viên tham gia lớp test đầu vào";
                            break;
                        case 2:
                            ck.TypeName = "Mẫu gửi học viên kết quả test và lộ trình học";
                            break;
                        case 3:
                            ck.TypeName = "Mẫu xác nhận thanh toán";
                            break;
                        case 4:
                            ck.TypeName = "Mẫu gửi học viên nhận lớp";
                            break;
                        case 5:
                            ck.TypeName = "Mẫu gửi học viên tài khoản test";
                            break;
                        case 6:
                            ck.TypeName = "Mẫu gửi nhân viên khi tạo mới";
                            break;
                        case 7:
                            ck.TypeName = "Mẫu gửi giáo viên";
                            break;
                        case 8:
                            ck.TypeName = "Mẫu hợp đồng đào tạo";
                            break;
                        case 9:
                            ck.TypeName = "Mẫu phiếu lương";
                            break;
                        default:
                            ck.TypeName = "Mẫu thông báo kết thúc lộ trình học";
                            break;
                    }
                    ck.EmailTitle = title;
                    ck.EmailContent = content;
                    ck.isHide = false;
                    ck.CreatedBy = CreatedBy;
                    ck.CreatedDate = DateTime.Now;
                    db.tbl_EmailTemplate.Add(ck);
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static List<tbl_EmailTemplate> GetAll()
        {
            using (var db = new ZimEntities())
            {
                List<tbl_EmailTemplate> ck = db.tbl_EmailTemplate.ToList();
                return ck;
            }
        }
        public static tbl_EmailTemplate GetByID(int id)
        {
            using (var db = new ZimEntities())
            {
                var  ck = db.tbl_EmailTemplate.Where(x => x.ID == id).FirstOrDefault();
                return ck;
            }
        }

        //public static tbl_FAQ UpdateIshide(int ID, bool isHide, string ModifiedBy)
        //{
        //    using (var db = new ZimEntities())
        //    {
        //        tbl_FAQ ck = db.tbl_FAQ.Where(n => n.ID == ID).FirstOrDefault();
        //        if (ck != null)
        //        {
        //            ck.isHide = isHide;
        //            ck.ModifiedBy = ModifiedBy;
        //            ck.ModifiedDate = GetDateTime.Now;
        //            db.SaveChanges();
        //        }
        //        return ck;
        //    }
        //}
    }
}