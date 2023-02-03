using AppZim.TableSql;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.Hubs
{
    public class ZimRealTime : Hub
    {
        // trạng thái online
        public void useronline(int UID, string classs)
        {
            bool onl = false;
            if (classs.Contains("active"))
                onl = true;
            AccountTable.updatestatussnline(UID, onl);
            Clients.All.getstatusonline(UID, classs);
        }

        //gửi, nhận tin nhắn
        public void SendMess(int uidFrom, int uidTo, string message)//đẩy tin nhắn lên
        {
            var aclog = AccountTable.getbyID(uidFrom);
            var m = MessageTable.insert(aclog.ID, uidTo, message, aclog.UserName);
            if (m != null)// đẩy tin nhắn xuống
            {
                Clients.All.addNewMessageToPage(m);
            }
        }

        //thông báo số lượng thông báo, tin nhắn đang đợi
        public void countsmsvsnoti(int uid)//đẩy tin nhắn lên
        {
            int lmess = MessageTable.getbyreceiveID(uid).Where(n => n.MessageNew == true).ToList().Select(n => n.FromUID).Distinct().Count();
            if (lmess > 0)// đẩy tin nhắn xuống
            {
                Clients.All.showcountsmsvsnoti(uid, lmess);
            }
        }

        //Thông báo nhanh
        public void PushNoti(int uid, string message)
        {

            var account = AccountTable.getbyID(uid);
            if (account != null)
            {
                if (account.RoleID == 1)
                    Clients.All.shownotification(uid, message);
            }
        }
    }
}