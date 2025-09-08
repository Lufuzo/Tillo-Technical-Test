using Service_Layer.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Dependencies;
using Tillo_Technical_Test_Solution.Controller;

namespace Tillo_Technical_Test_Solution.App_Start
{
    public class SimpleResolver : IDependencyResolver
    {
        private readonly ITransactionService _service;

        public SimpleResolver(ITransactionService service)
        {
            _service = service;
        }

        public IDependencyScope BeginScope()
        {
            return this;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(TransactionsController))
                return new TransactionsController(_service);

            return null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return new List<object>();
        }

        public void Dispose()
        {
        }
    }
}