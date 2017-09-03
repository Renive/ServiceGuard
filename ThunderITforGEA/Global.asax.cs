using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ThunderITforGEA
{
    public class MvcApplication : System.Web.HttpApplication
    {
        //private static CacheItemRemovedCallback OnCacheRemove; 
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
          //  private static CacheItemRemovedCallback OnCacheRemove; 
        }
    }
}
