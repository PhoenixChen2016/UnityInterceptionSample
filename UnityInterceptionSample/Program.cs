using Library;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityInterceptionSample
{
	class Program
	{
		static void Main(string[] args)
		{
			var container = new UnityContainer();
			container.AddNewExtension<InterceptionExtension>();

			container.RegisterType<IUserService, UserService>();

			var service = container.Resolve<IUserService>();

			service.AddUser("tester", "aa@bb.cc");
		}
	}
}
