using System;
using System.Web.Mvc;
using UniManage.Data;
using UniManage.Models;

namespace UniManage.Filters
{
    public class CustomHandleErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            try
            {
                var ex = filterContext.Exception;
                // log to AuditLog table if available
                using (var db = new UniManageDbContext())
                {
                    db.AuditLogs.Add(new AuditLog
                    {
                        Event = "UnhandledException",
                        Message = ex.Message,
                        Details = ex.ToString(),
                        CreatedOn = DateTime.UtcNow
                    });
                    db.SaveChanges();
                }
            }
            catch
            {
                // swallow logging exceptions
            }

            base.OnException(filterContext);
        }
    }
}