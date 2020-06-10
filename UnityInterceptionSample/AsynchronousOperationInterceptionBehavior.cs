using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity.Interception.InterceptionBehaviors;
using Unity.Interception.PolicyInjection.Pipeline;

namespace UnityInterceptionSample
{
	using WrapperAsyncFunctions = ConcurrentDictionary<Type, Func<object?, IMethodInvocation, object?>>;

	public abstract class AsynchronousOperationInterceptionBehavior : IInterceptionBehavior
	{
		private readonly WrapperAsyncFunctions m_WrapperCreators = new WrapperAsyncFunctions();

		public abstract bool WillExecute { get; }

		public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
		{
			return GetWarpperMethod(input, getNext)();
		}

		public abstract IEnumerable<Type> GetRequiredInterfaces();

		private IMethodReturn OtherWrapperMethod(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
		{
			var beforeResult = OnBeforeInvoke(input);

			if (beforeResult != null)
				return beforeResult;

			return OnAfterInvoke(input, getNext()(input, getNext));
		}

		private Func<IMethodReturn> GetWarpperMethod(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
		{
			var method = input.MethodBase as MethodInfo;
			if (method == null)
				return () => OtherWrapperMethod(input, getNext);

			if (typeof(Task).IsAssignableFrom(method.ReturnType)
				|| method.ReturnType == typeof(ValueTask)
				|| method.ReturnType.IsGenericType
				&& method.ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
			{
				var wrapperMethod = this.m_WrapperCreators.GetOrAdd(
					method.ReturnType,
					(Type t) =>
					{
						if (t == typeof(Task))
						{
							return CreateNormalWrapper<Task>(CreateWrapperTaskAsync);
						}
						else if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Task<>))
						{
							var funcType = typeof(Func<,,>).MakeGenericType(t, typeof(IMethodInvocation), t);

							var method = typeof(AsynchronousOperationInterceptionBehavior)
								.GetMethod(nameof(CreateGenericWrapperTaskAsync), BindingFlags.Instance | BindingFlags.NonPublic)
								!.MakeGenericMethod(t.GenericTypeArguments[0])
								.CreateDelegate(funcType, this);

							return (Func<object?, IMethodInvocation, object?>)typeof(AsynchronousOperationInterceptionBehavior)
								.GetMethod(nameof(CreateNormalWrapper), BindingFlags.Instance | BindingFlags.NonPublic)
								!.MakeGenericMethod(t)
								.Invoke(this, new[] { method! })!;
						}
						else if (t == typeof(ValueTask))
						{
							return CreateNormalWrapper<ValueTask>(CreateWrapperValueTaskAsync);
						}
						else if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ValueTask<>))
						{
							var funcType = typeof(Func<,,>).MakeGenericType(t, typeof(IMethodInvocation), t);

							var method = typeof(AsynchronousOperationInterceptionBehavior)
								.GetMethod(nameof(CreateGenericWrapperValueTaskAsync), BindingFlags.Instance | BindingFlags.NonPublic)
								!.MakeGenericMethod(t.GenericTypeArguments[0])
								.CreateDelegate(funcType, this);

							return (Func<object?, IMethodInvocation, object?>)typeof(AsynchronousOperationInterceptionBehavior)
								.GetMethod(nameof(CreateNormalWrapper), BindingFlags.Instance | BindingFlags.NonPublic)
								!.MakeGenericMethod(t)
								.Invoke(this, new[] { method! })!;
						}
						else
							return new Func<object?, IMethodInvocation, object?>((t, _) => t);
					});


				return () =>
				{
					var beforeResult = OnBeforeInvoke(input);

					if (beforeResult != null)
						return beforeResult;

					var value = getNext()(input, getNext);

					return input.CreateMethodReturn(
						wrapperMethod(value.ReturnValue, input),
						value.Outputs);
				};
			}
			else
				return () => OtherWrapperMethod(input, getNext);

		}

		protected virtual IMethodReturn? OnBeforeInvoke(IMethodInvocation input)
		{
			return null;
		}

		protected virtual IMethodReturn OnAfterInvoke(IMethodInvocation input, IMethodReturn executeResult)
		{
			return executeResult;
		}

		private async ValueTask CreateWrapperValueTaskAsync(ValueTask task, IMethodInvocation input)
		{
			await task.ConfigureAwait(false);

			var afterResult = OnAfterInvoke(input, input.CreateMethodReturn(null));

			if (afterResult.Exception != null)
				throw afterResult.Exception;
		}

		private async Task CreateWrapperTaskAsync(Task task, IMethodInvocation input)
		{
			await task.ConfigureAwait(false);

			var afterResult = OnAfterInvoke(input, input.CreateMethodReturn(null));

			if (afterResult.Exception != null)
				throw afterResult.Exception;
		}

		private Func<object?, IMethodInvocation, object?> CreateNormalWrapper<TTask>(Func<TTask, IMethodInvocation, TTask> func)
		{
			return (o, input) => func((TTask)o!, input);
		}

		private async ValueTask<T> CreateGenericWrapperValueTaskAsync<T>(ValueTask<T> task, IMethodInvocation input)
		{
			var result = await task.ConfigureAwait(false);

			var afterResult = OnAfterInvoke(input, input.CreateMethodReturn(result));

			if (afterResult.Exception != null)
				throw afterResult.Exception;

			return (T)afterResult.ReturnValue;
		}

		private async Task<T> CreateGenericWrapperTaskAsync<T>(Task<T> task, IMethodInvocation input)
		{
			var result = await task.ConfigureAwait(false);

			var afterResult = OnAfterInvoke(input, input.CreateMethodReturn(result));

			if (afterResult.Exception != null)
				throw afterResult.Exception;

			return (T)afterResult.ReturnValue;
		}
	}
}
