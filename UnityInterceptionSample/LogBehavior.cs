using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Interception.PolicyInjection.Pipeline;

namespace UnityInterceptionSample
{
	internal class LogBehavior : AsynchronousOperationInterceptionBehavior
	{
		public override bool WillExecute { get; } = true;

		public override IEnumerable<Type> GetRequiredInterfaces() => Type.EmptyTypes;

		protected override IMethodReturn? OnBeforeInvoke(IMethodInvocation input)
		{
			Console.WriteLine("args: {0}", string.Join(",", input.Arguments.Cast<object>().Select(o => o.ToString())));

			return null;
		}

		protected override IMethodReturn OnAfterInvoke(IMethodInvocation input, IMethodReturn executeResult)
		{
			if (executeResult.Exception != null)
			{
				Console.WriteLine(
					string.Format(
						"Method {0} threw exception {1} at {2}",
						input.MethodBase, executeResult.Exception.Message,
						DateTime.Now.ToLongTimeString()));
			}
			else
			{
				Console.WriteLine(
					string.Format(
						"Method {0} returned {1} at {2}",
						input.MethodBase, executeResult.ReturnValue,
						DateTime.Now.ToLongTimeString()));
			}

			return executeResult;
		}
	}
}
