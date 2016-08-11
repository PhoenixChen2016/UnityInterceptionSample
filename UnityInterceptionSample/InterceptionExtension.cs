using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity.InterceptionExtension;
using Microsoft.Practices.Unity;

namespace UnityInterceptionSample
{
    class InterceptionExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {
            this.Container.AddNewExtension<Interception>();

            this.Context.Registering += Context_Registering;
            this.Context.RegisteringInstance += Context_RegisteringInstance;
        }

        private void Context_RegisteringInstance(object sender, RegisterInstanceEventArgs e)
        {
            if (e.RegisteredType.IsInterface)
            {
                e.LifetimeManager.SetValue(Intercept.ThroughProxy(
                    e.RegisteredType,
                    e.Instance,
                    new InterfaceInterceptor(),
                    new[] { new LogBehavior() }));
            }

            if (e.RegisteredType.IsMarshalByRef)
            {
                e.LifetimeManager.SetValue(Intercept.ThroughProxy(
                    e.RegisteredType,
                    e.Instance,
                    new TransparentProxyInterceptor(),
                    new[] { new LogBehavior() }));
            }
        }

        private void Context_Registering(object sender, RegisterEventArgs e)
        {
            if (e.TypeTo.IsInterface)
            {
                var interceptor = new Interceptor<InterfaceInterceptor>();
                interceptor.AddPolicies(e.TypeFrom, e.TypeTo, e.Name, this.Context.Policies);
            }

            if (e.TypeTo.IsMarshalByRef)
            {
                var interceptor = new Interceptor<TransparentProxyInterceptor>();
                interceptor.AddPolicies(e.TypeFrom, e.TypeTo, e.Name, this.Context.Policies);
            }

            var behavior = new InterceptionBehavior<LogBehavior>();
            behavior.AddPolicies(e.TypeFrom, e.TypeTo, e.Name, this.Context.Policies);
        }
    }
}
