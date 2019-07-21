using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace GOKManage
{
    /// <summary>
    /// AdminLogout 的摘要说明
    /// </summary>
    public class AdminLogout : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            HttpContext contextid = HttpContext.Current;

            contextid.Session["admin_id"] = null;
            contextid.Session["admin_name"] = null;

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