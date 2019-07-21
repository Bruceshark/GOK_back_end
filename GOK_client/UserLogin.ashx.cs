using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.SessionState;

namespace GOK_client
{
    /// <summary>
    /// userlogin 的摘要说明
    /// </summary>
    public class UserLogin : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string username = context.Request["username"];
            string password = context.Request["password"];
            if (pub.QueryCheck(username) != "ok" || pub.QueryCheck(password) != "ok")
            {
                context.Response.Write("illegal");
            }
            else
            {
                string password_ = pub.md5(password);
                string username_ = pub.QuotedStr(username);
                string res = LoginQuery(username_, password_);
                context.Response.Write(res);
            }
        }

        private string LoginQuery(string username, string password)
        {
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand cmd = new SqlCommand("SELECT * FROM USER_TBL WHERE NAME = " + username, conn);
            SqlDataReader reader = cmd.ExecuteReader();//执行sql语句
            if (!reader.Read()) return "error";
            else
            {
                string msgTimeLong = DateTime.Now.ToLocalTime().ToString();
                string db_password = reader.GetString(reader.GetOrdinal("PASSWORD"));
                if (db_password != password) return "error";
                int userId = reader.GetInt32(reader.GetOrdinal("USER_ID"));
                string userName = reader.GetString(reader.GetOrdinal("NAME"));
                context.Session["user_id"] = userId.ToString();
                context.Session["user_name"] = userName;

                ///判断新老用户
                int old = reader.GetInt32(reader.GetOrdinal("OLD"));
                reader.Close();
                if (old == 1)
                {
                    SqlCommand act = new SqlCommand("UPDATE USER_ACTION_TIME_TBL SET TIME = '" + msgTimeLong + "' WHERE USER_ID = " + userId, conn);
                    act.ExecuteNonQuery();
                    conn.Close();//关闭数据库
                    return "old";
                }
                else
                {
                    SqlCommand cmdold = new SqlCommand("UPDATE USER_TBL SET old = 1 WHERE NAME = " + username, conn);
                    cmdold.ExecuteNonQuery();//执行sql语句
                    SqlCommand act = new SqlCommand("UPDATE USER_ACTION_TIME_TBL SET TIME = '" + msgTimeLong + "' WHERE USER_ID = " + userId, conn);
                    act.ExecuteNonQuery();
                    conn.Close();//关闭数据库
                    return "new";
                }
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}