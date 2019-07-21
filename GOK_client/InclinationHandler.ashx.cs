using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Web.SessionState;

namespace GOK_client
{
    /// <summary>
    /// InclinationHandler 的摘要说明
    /// </summary>
    public class InclinationHandler : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string user1 = "";
            string user1Name = "";
            HttpContext contextid = HttpContext.Current;
            if (contextid.Session["user_id"] == null || contextid.Session["user_name"] == null) context.Response.Write(8);
            else
            {
                user1 = contextid.Session["user_id"].ToString();
                user1Name = contextid.Session["user_name"].ToString();
            }
            string inclination = context.Request["inclination"];
            switch (inclination)
            {
                case "good":
                    InclGood(user1, user1Name);
                    break;
                case "bad":
                    InclBad(user1, user1Name);
                    break;
                case "middle":
                    InclMiddle(user1, user1Name);
                    break;
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private void InclGood(string user1, string user1Name)
        {
            string msgTime = DateTime.Now.ToShortTimeString().ToString();
            string msgTimeLong = DateTime.Now.ToLocalTime().ToString();
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand cmd = new SqlCommand("UPDATE INCL_TBL SET INCL_ID = 2 WHERE USER_ID = " + user1 +
                                            ";INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                                                 "VALUES(" + user1 + ", 0, '倾向选择成功', '你成功选择了正义倾向', '" + msgTime + "')" +
                                           "UPDATE USER_ACTION_TIME_TBL SET TIME = '" + msgTimeLong + "' WHERE USER_ID = " + user1, conn);
            cmd.ExecuteNonQuery();
            conn.Close();
            context.Response.Write("ok");
        }

        private void InclBad(string user1, string user1Name)
        {
            string msgTime = DateTime.Now.ToShortTimeString().ToString();
            string msgTimeLong = DateTime.Now.ToLocalTime().ToString();
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand cmd = new SqlCommand("UPDATE INCL_TBL SET INCL_ID = 3 WHERE USER_ID = " + user1 +
                                            ";INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                                                 "VALUES(" + user1 + ", 0, '倾向选择成功', '你成功选择了邪恶倾向', '" + msgTime + "')" +
                                           "UPDATE USER_ACTION_TIME_TBL SET TIME = '" + msgTimeLong + "' WHERE USER_ID = " + user1, conn);
            cmd.ExecuteNonQuery();
            conn.Close();
            context.Response.Write("ok");
        }

        private void InclMiddle(string user1, string user1Name)
        {
            string msgTime = DateTime.Now.ToShortTimeString().ToString();
            string msgTimeLong = DateTime.Now.ToLocalTime().ToString();
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand cmd = new SqlCommand("UPDATE INCL_TBL SET INCL_ID = 4 WHERE USER_ID = " + user1 +
                                            ";INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                                                 "VALUES(" + user1 + ", 0, '倾向选择成功', '你成功选择了中立倾向', '" + msgTime + "')" +
                                           "UPDATE USER_ACTION_TIME_TBL SET TIME = '" + msgTimeLong + "' WHERE USER_ID = " + user1, conn);
            cmd.ExecuteNonQuery();
            conn.Close();
            context.Response.Write("ok");
        }
    }
}