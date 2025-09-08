using Repository_Layer.Transaction_Repo;
using Service_Layer.Repository;
using SimpleInjector.Integration.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using Tillo_Technical_Test_Solution.App_Start;

namespace Tillo_Technical_Test_Solution
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
       
            GlobalConfiguration.Configure(WebApiConfig.Register);

            var connStr = System.Web.Configuration.WebConfigurationManager
                .ConnectionStrings["DefaultConnection"].ConnectionString;

   

            var repo = new TransactionRepository(connStr);
            repo.Initialize(); // ensure tables exist

            var service = new TransactionService(repo);

            GlobalConfiguration.Configuration.DependencyResolver =
                new SimpleResolver(service);
        }
    }
}