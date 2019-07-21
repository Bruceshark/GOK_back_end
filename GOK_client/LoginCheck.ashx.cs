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
    /// LoginCheck 的摘要说明:
    /// LoginCheck.ashx 主要用于给前端login页，defaultPage页和comeback页发送当前浏览器的session值，以让前端判断用户
    /// 是否登陆，若登陆则取得用户名显示，若没有登陆则退回login
    /// </summary>
    public class LoginCheck : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            HttpContext context_ = HttpContext.Current;
            if (context_.Session["user_name"] == null) context.Response.Write(0);
            else
            {
                string name = context_.Session["user_name"].ToString();
                context.Response.Write(name);
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