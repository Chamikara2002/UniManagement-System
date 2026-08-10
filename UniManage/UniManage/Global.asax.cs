using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using UniManage.Data;

namespace UniManage
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Seed roles and admin user
            using (var context = new UniManageDbContext())
            {
                IdentitySeeder.Seed(context);
            }
        }
    }
}