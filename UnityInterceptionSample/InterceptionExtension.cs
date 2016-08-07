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
		}

		private void Context_Registering(object sender, RegisterEventArgs e)
		{
			var interceptor = new Interceptor<InterfaceInterceptor>();
			interceptor.AddPolicies(e.TypeFrom, e.TypeTo, e.Name, this.Context.Policies);

			var behavior = new InterceptionBehavior<LogBehavior>();
			behavior.AddPolicies(e.TypeFrom, e.TypeTo, e.Name, this.Context.Policies);
		}
	}
}
