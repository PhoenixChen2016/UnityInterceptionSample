using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace UnityInterceptionSample
{
	class LogBehavior : IInterceptionBehavior
	{
		public bool WillExecute
		{
			get
			{
				return true;
			}
		}

		public IEnumerable<Type> GetRequiredInterfaces()
		{
			return Type.EmptyTypes;
		}

		public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
		{
			Console.WriteLine("args: {0}", string.Join(",", input.Arguments.Cast<object>().Select(o => o.ToString())));

			var result = getNext()(input, getNext);

			// After invoking the method on the original target.
			if (result.Exception != null)
			{
				Console.WriteLine(String.Format(
				  "Method {0} threw exception {1} at {2}",
				  input.MethodBase, result.Exception.Message,
				  DateTime.Now.ToLongTimeString()));
			}
			else
			{
				Console.WriteLine(String.Format(
				  "Method {0} returned {1} at {2}",
				  input.MethodBase, result.ReturnValue,
				  DateTime.Now.ToLongTimeString()));
			}

			return result;
		}
	}
}
