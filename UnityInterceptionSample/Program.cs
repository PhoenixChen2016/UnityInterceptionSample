using Library;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace UnityInterceptionSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new UnityContainer();
            container.AddNewExtension<InterceptionExtension>();

            // Interface proxy
            container.RegisterType<IUserService, UserService>();
            // MarshalByRefObject proxy
            container.RegisterType<Mail, Mail>();

            var service = container.Resolve<IUserService>();
            var mail = container.Resolve<Mail>();

            service.AddUser("tester", "aa@bb.cc");
            mail.Send("from@bb.cc", "to@bb.cc", "subject", "body");

            // 不使用container的應用
            var serviceWithoutDI = Intercept.ThroughProxy<IUserService>(
                new UserService(),
                new InterfaceInterceptor(),
                new[] { new LogBehavior() });

            serviceWithoutDI.AddUser("tester", "aa@bb.cc");

            var mailWithoutDI = Intercept.ThroughProxy(
                new Mail(),
                new TransparentProxyInterceptor(),
                new[] { new LogBehavior() });

            mailWithoutDI.Send("from@bb.cc", "to@bb.cc", "subject", "body");

            Console.ReadKey();
        }
    }
}
