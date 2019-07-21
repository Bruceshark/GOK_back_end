using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Web.SessionState;

namespace GOK_client
{
    /// <summary>
    /// UserLogout 的摘要说明
    /// </summary>
    public class UserLogout : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            string msgTimeLong = DateTime.Now.ToLocalTime().ToString();

            string userId = "";
            HttpContext contextid = HttpContext.Current;
            if (contextid.Session["user_id"] == null ) context.Response.Write(0);
            else
            {
                userId = contextid.Session["user_id"].ToString();
            }

            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand act = new SqlCommand("UPDATE USER_ACTION_TIME_TBL SET TIME = '" + msgTimeLong + "' WHERE USER_ID = " + userId, conn);
            act.ExecuteNonQuery();
            conn.Close();

            contextid.Session["user_id"] = null;
            contextid.Session["user_name"] = null;

            context.Response.ContentType = "text/plain";
            context.Response.Write("ok");
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