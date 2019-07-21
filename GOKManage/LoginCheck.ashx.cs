using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Web.SessionState;

namespace GOKManage
{
    /// <summary>
    /// LoginCheck 的摘要说明
    /// </summary>
    public class LoginCheck : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            HttpContext context_ = HttpContext.Current;
            if (context_.Session["admin_name"] == null) context.Response.Write(0);
            else
            {
                string name = context_.Session["admin_name"].ToString();
                context.Response.Write("name");
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