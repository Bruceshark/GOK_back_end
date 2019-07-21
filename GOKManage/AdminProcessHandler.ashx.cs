using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

namespace GOKManage
{
    /// <summary>
    /// AdminProcessHandler 的摘要说明
    /// </summary>
    public class AdminProcessHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string id = context.Request["id"];

            //post输入的id需要进行参数校验
            if (pub.QueryCheck(id) != "ok") context.Response.Write("illegal");
            else
            {
                process(id);
                context.Response.Write("ok");
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private void process(string id)
        {
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand cmd = new SqlCommand("UPDATE ADMIN_MSG_TBL SET IS_PROCESSED = 1 WHERE MSG_ID = " + id, conn);
            cmd.ExecuteNonQuery();
            conn.Close();
        }
    }
}