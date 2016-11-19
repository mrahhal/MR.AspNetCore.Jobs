using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MR.AspNetCore.Jobs
{
	public class InvocationData
	{
		public InvocationData(
		   string type, string method, string parameterTypes, string arguments)
		{
			Type = type;
			Method = method;
			ParameterTypes = parameterTypes;
			Arguments = arguments;
		}

		public string Type { get; }
		public string Method { get; }
		public string ParameterTypes { get; }
		public string Arguments { get; set; }

		public string Serialize() => Helper.ToJson(this);

		public MethodInvocation Deserialize()
		{
			try
			{
				var type = System.Type.GetType(Type, throwOnError: false, ignoreCase: true);
				if (type == null)
				{
					throw new JobLoadException("Could not load the job.");
				}

				var parameterTypes = Helper.FromJson<Type[]>(ParameterTypes);
				var method = GetNonOpenMatchingMethod(type, Method, parameterTypes);

				if (method == null)
				{
					throw new JobLoadException(string.Format(
						"The type '{0}' does not contain a method with signature '{1}({2})'",
						type.FullName,
						Method,
						string.Join(", ", parameterTypes.Select(x => x.Name))));
				}

				var serializedArguments = Helper.FromJson<string[]>(Arguments);
				var arguments = DeserializeArguments(method, serializedArguments);

				return new MethodInvocation(type, method, arguments);
			}
			catch (Exception ex) when (!(ex is JobLoadException))
			{
				throw new JobLoadException("Could not load the job. See inner exception for the details.", ex);
			}
		}

		public static InvocationData Serialize(MethodInvocation job)
		{
			return new InvocationData(
				job.Type.AssemblyQualifiedName,
				job.Method.Name,
				Helper.ToJson(job.Method.GetParameters().Select(x => x.ParameterType).ToArray()),
				Helper.ToJson(SerializeArguments(job.Args)));
		}

		internal static string[] SerializeArguments(IReadOnlyCollection<object> arguments)
		{
			var serializedArguments = new List<string>(arguments.Count);
			foreach (var argument in arguments)
			{
				var value = default(string);
				if (argument != null)
				{
					value = Helper.ToJson(argument);
				}

				serializedArguments.Add(value);
			}

			return serializedArguments.ToArray();
		}

		internal static object[] DeserializeArguments(MethodInfo methodInfo, string[] arguments)
		{
			var parameters = methodInfo.GetParameters();
			var result = new List<object>(arguments.Length);

			for (var i = 0; i < parameters.Length; i++)
			{
				var parameter = parameters[i];
				var argument = arguments[i];

				var value = DeserializeArgument(argument, parameter.ParameterType);

				result.Add(value);
			}

			return result.ToArray();
		}

		private static object DeserializeArgument(string argument, Type type)
		{
			return argument != null ? Helper.FromJson(argument, type) : null;
		}

		private static IEnumerable<MethodInfo> GetAllMethods(Type type)
		{
			var methods = new List<MethodInfo>(type.GetMethods());

			if (type.GetTypeInfo().IsInterface)
			{
				methods.AddRange(type.GetInterfaces().SelectMany(x => x.GetMethods()));
			}

			return methods;
		}

		private static MethodInfo GetNonOpenMatchingMethod(Type type, string name, Type[] parameterTypes)
		{
			var methodCandidates = GetAllMethods(type);

			foreach (var methodCandidate in methodCandidates)
			{
				if (!methodCandidate.Name.Equals(name, StringComparison.Ordinal))
				{
					continue;
				}

				var parameters = methodCandidate.GetParameters();
				if (parameters.Length != parameterTypes.Length)
				{
					continue;
				}

				var parameterTypesMatched = true;
				var genericArguments = new List<Type>();

				// Determining whether we can use this method candidate with
				// current parameter types.
				for (var i = 0; i < parameters.Length; i++)
				{
					var parameter = parameters[i];
					var parameterType = parameter.ParameterType;
					var actualType = parameterTypes[i];

					// Skipping generic parameters as we can use actual type.
					if (parameterType.IsGenericParameter)
					{
						genericArguments.Add(actualType);
						continue;
					}

					// Skipping non-generic parameters of assignable types.
					if (parameterType.IsAssignableFrom(actualType)) continue;

					parameterTypesMatched = false;
					break;
				}

				if (!parameterTypesMatched) continue;

				// Return first found method candidate with matching parameters.
				return methodCandidate.ContainsGenericParameters
					? methodCandidate.MakeGenericMethod(genericArguments.ToArray())
					: methodCandidate;
			}

			return null;
		}
	}
}
