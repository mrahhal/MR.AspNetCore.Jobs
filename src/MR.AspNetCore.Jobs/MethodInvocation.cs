using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using MR.AspNetCore.Jobs.ExpressionUtil;

namespace MR.AspNetCore.Jobs
{
	public class MethodInvocation
	{
		public MethodInvocation(Type type, MethodInfo method, params object[] args)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));
			if (method == null) throw new ArgumentNullException(nameof(method));
			if (args == null) throw new ArgumentNullException(nameof(args));

			Type = type;
			Method = method;
			Args = args;
		}

		public Type Type { get; }

		public MethodInfo Method { get; }

		public IReadOnlyList<object> Args { get; }

		public static MethodInvocation FromExpression(Expression<Action> methodCall)
		{
			if (methodCall == null) throw new ArgumentNullException(nameof(methodCall));

			var callExpression = methodCall.Body as MethodCallExpression;
			if (callExpression == null)
			{
				throw new ArgumentException("Expression body should be of type `MethodCallExpression`", nameof(methodCall));
			}

			Type type;

			if (callExpression.Object != null)
			{
				var objectValue = GetExpressionValue(callExpression.Object);
				if (objectValue == null)
				{
					throw new InvalidOperationException("Expression object should be not null.");
				}

				type = objectValue.GetType();
			}
			else
			{
				type = callExpression.Method.DeclaringType;
			}

			return new MethodInvocation(
				type,
				callExpression.Method,
				GetExpressionValues(callExpression.Arguments));
		}

		public static MethodInvocation FromExpression<TType>(Expression<Action<TType>> methodCall)
		{
			if (methodCall == null) throw new ArgumentNullException(nameof(methodCall));

			var callExpression = methodCall.Body as MethodCallExpression;
			if (callExpression == null)
			{
				throw new ArgumentException("Expression body should be of type `MethodCallExpression`", nameof(methodCall));
			}

			return new MethodInvocation(
				typeof(TType),
				callExpression.Method,
				GetExpressionValues(callExpression.Arguments));
		}

		private static void Validate(
			Type type,
			string typeParameterName,
			MethodInfo method,
			string methodParameterName,
			int argumentCount,
			string argumentParameterName)
		{
			if (!method.IsPublic)
			{
				throw new NotSupportedException("Only public methods can be invoked in the background.");
			}

			if (method.ContainsGenericParameters)
			{
				throw new NotSupportedException("Job method can not contain unassigned generic type parameters.");
			}

			if (method.DeclaringType == null)
			{
				throw new NotSupportedException("Global methods are not supported. Use class methods instead.");
			}

			if (!method.DeclaringType.IsAssignableFrom(type))
			{
				throw new ArgumentException(
					String.Format("The type `{0}` must be derived from the `{1}` type.", method.DeclaringType, type),
					typeParameterName);
			}

			if (typeof(Task).IsAssignableFrom(method.ReturnType))
			{
				throw new NotSupportedException("Async methods are not supported. Please make them synchronous before using them in background.");
			}

			var parameters = method.GetParameters();

			if (parameters.Length != argumentCount)
			{
				throw new ArgumentException(
					"Argument count must be equal to method parameter count.",
					argumentParameterName);
			}

			foreach (var parameter in parameters)
			{
				if (parameter.IsOut)
				{
					throw new NotSupportedException(
						"Output parameters are not supported: there is no guarantee that specified method will be invoked inside the same process.");
				}

				if (parameter.ParameterType.IsByRef)
				{
					throw new NotSupportedException(
						"Parameters, passed by reference, are not supported: there is no guarantee that specified method will be invoked inside the same process.");
				}
			}
		}

		private static object[] GetExpressionValues(IEnumerable<Expression> expressions)
		{
			return expressions.Select(GetExpressionValue).ToArray();
		}

		private static object GetExpressionValue(Expression expression)
		{
			var constantExpression = expression as ConstantExpression;

			return constantExpression != null
				? constantExpression.Value
				: CachedExpressionCompiler.Evaluate(expression);
		}
	}
}
