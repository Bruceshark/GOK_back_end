using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Web.SessionState;


namespace GOKManage
{
    /// <summary>
    /// AdminLogin 的摘要说明
    /// </summary>
    public class AdminLogin : IHttpHandler, IRequiresSessionState
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

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private string LoginQuery(string username, string password)
        {
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand cmd = new SqlCommand("SELECT * FROM ADMIN_TBL WHERE NAME = " + username, conn);
            SqlDataReader reader = cmd.ExecuteReader();//执行sql语句
            if (!reader.Read()) return "error";
            else
            {
                string db_password = reader.GetString(reader.GetOrdinal("PASSWORD"));
                if (db_password != password)
                {
                    reader.Close();
                    conn.Close();
                    return "error";
                }
                else
                {
                    int userId = reader.GetInt32(reader.GetOrdinal("ADMIN_ID"));
                    string userName = reader.GetString(reader.GetOrdinal("NAME"));

                    context.Session["admin_id"] = userId.ToString();
                    context.Session["admin_name"] = userName;
                    reader.Close();
                    conn.Close();
                    return "ok";
                }
            }
        }

    }
}