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

			Validate(type, method, args.Length);

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
			return FromExpressionCore(methodCall);
		}

		public static MethodInvocation FromExpression<TType>(Expression<Action<TType>> methodCall)
		{
			if (methodCall == null) throw new ArgumentNullException(nameof(methodCall));
			return FromExpressionCore<TType>(methodCall);
		}

		public static MethodInvocation FromExpression(Expression<Func<Task>> methodCall)
		{
			if (methodCall == null) throw new ArgumentNullException(nameof(methodCall));
			return FromExpressionCore(methodCall);
		}

		public static MethodInvocation FromExpression<TType>(Expression<Func<TType, Task>> methodCall)
		{
			var callExpression = methodCall.Body as MethodCallExpression;
			return FromExpressionCore<TType>(methodCall);
		}

		private static MethodInvocation FromExpressionCore(LambdaExpression methodCall)
		{
			var callExpression = methodCall.Body as MethodCallExpression;
			if (callExpression == null)
			{
				throw new ArgumentException("Expression body should be of type `MethodCallExpression`", nameof(methodCall));
			}

			var type = default(Type);
			if (callExpression.Object != null)
			{
				var objectValue = GetExpressionValue(callExpression.Object);
				if (objectValue == null)
				{
					throw new InvalidOperationException("Expression object should not be null.");
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

		private static MethodInvocation FromExpressionCore<TType>(LambdaExpression methodCall)
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
			MethodInfo method,
			int argumentCount)
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
					string.Format("The type `{0}` must be derived from the `{1}` type.", method.DeclaringType, type),
					nameof(type));
			}

			var parameters = method.GetParameters();

			if (parameters.Length != argumentCount)
			{
				throw new ArgumentException(
					"Argument count must be equal to method parameter count.",
					"args");
			}

			foreach (var parameter in parameters)
			{
				if (parameter.IsOut || parameter.ParameterType.IsByRef)
				{
					throw new NotSupportedException("out and ref parameters are not supported.");
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
