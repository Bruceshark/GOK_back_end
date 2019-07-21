using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

namespace GOKManage
{
    /// <summary>
    /// AdminMethodHandler 的摘要说明
    /// </summary>
    public class AdminMethodHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string method = context.Request["method"];

            //post输入的user2需要进行参数校验
            if (pub.QueryCheck(method) != "ok") context.Response.Write("illegal");
            else
            {
                SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
                conn.Open();//打开数据库
                switch (method)
                {
                    case "begin":
                        SqlCommand cmd1 = new SqlCommand("UPDATE GAME_PROCESS_TBL SET SWITCH = 1 WHERE PROCESS_NAME = 'STATE'", conn);
                        cmd1.ExecuteNonQuery();
                        context.Response.Write("ok");
                        break;
                    case "pause":
                        SqlCommand cmd2 = new SqlCommand("UPDATE GAME_PROCESS_TBL SET SWITCH = 0 WHERE PROCESS_NAME = 'STATE'", conn);
                        cmd2.ExecuteNonQuery();
                        context.Response.Write("ok");
                        break;
                    case "gunBan":
                        SqlCommand cmd3 = new SqlCommand("UPDATE GAME_PROCESS_TBL SET SWITCH = -1 WHERE PROCESS_NAME = 'STATE'", conn);
                        cmd3.ExecuteNonQuery();
                        context.Response.Write("ok");
                        break;
                }
                conn.Close();
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

