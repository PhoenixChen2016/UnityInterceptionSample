using System;
using Library;
using Unity;
using Unity.Interception;
using Unity.Interception.Interceptors.InstanceInterceptors.InterfaceInterception;
using Unity.Interception.Interceptors.TypeInterceptors.VirtualMethodInterception;

namespace UnityInterceptionSample
{
	class Program
	{
		static void Main(string[] args)
		{
			// 不使用container的應用
			var serviceWithoutDI = Intercept.ThroughProxy<IUserService>(
				new UserService(),
				new InterfaceInterceptor(),
				new[] { new LogBehavior() });

			serviceWithoutDI.AddUser("tester", "aa@bb.cc");

			Console.ReadKey();
		}
	}
}
