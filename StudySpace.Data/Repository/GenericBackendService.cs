using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Data.Repository
{

    public class GenericBackendService
    {
        private readonly IServiceProvider _serviceProvider;
        public GenericBackendService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public T? Resolve<T>()
        {
            return (T)_serviceProvider.GetService(typeof(T))!;
        }
    }
}
