using System;
using Library;
using Unity;
using Unity.Interception;
using Unity.Interception.Interceptors;
using Unity.Interception.Interceptors.InstanceInterceptors.InterfaceInterception;
using Unity.Interception.Interceptors.TypeInterceptors.VirtualMethodInterception;

namespace UnityInterceptionSample
{
	class Program
	{
		static void Main(string[] args)
		{
			// 不使用container的應用
			var interfaceInterceptor = new InterfaceInterceptor();

			var warppedInstance = interfaceInterceptor.CreateProxy(typeof(IUserService), new UserService());
			warppedInstance.AddInterceptionBehavior(new LogBehavior());

			var serviceWithoutDI = (IUserService)warppedInstance;

			serviceWithoutDI.AddUser("tester", "aa@bb.cc");

			var virtualMethodInterceptor = new VirtualMethodInterceptor();

			var warppedType = virtualMethodInterceptor.CreateProxyType(typeof(NotifyService));

			var warpper = (IInterceptingProxy)Activator.CreateInstance(warppedType)!;

			warpper.AddInterceptionBehavior(new LogBehavior());

			((NotifyService)warpper).Send();

			Console.ReadKey();
		}
	}
}
