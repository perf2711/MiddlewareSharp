using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

namespace MiddlewareSharp.Tests
{
    public class TestServiceProvider : IServiceContainer
    {
        private readonly Dictionary<Type, ServiceCreatorCallback> _services = new Dictionary<Type, ServiceCreatorCallback>();

        public object GetService(Type serviceType)
        {
            return _services[serviceType].Invoke(this, serviceType);
        }

        public void AddService(Type serviceType, ServiceCreatorCallback callback)
        {
            _services.Add(serviceType, callback);
        }

        public void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
        {
            throw new NotSupportedException();
        }

        public void AddService(Type serviceType, object serviceInstance)
        {
            _services.Add(serviceType, (container, type) => serviceInstance);
        }

        public void AddService(Type serviceType, object serviceInstance, bool promote)
        {
            throw new NotSupportedException();
        }

        public void RemoveService(Type serviceType)
        {
            _services.Remove(serviceType);
        }

        public void RemoveService(Type serviceType, bool promote)
        {
            throw new NotSupportedException();
        }
    }
}
