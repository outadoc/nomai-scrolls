using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Mono.Cecil;
using MonoMod.Utils;

namespace HarmonyLib
{
	public class DelegateTypeFactory
	{
		private class DelegateEntry
		{
			public CallingConvention? callingConvention;

			public Type delegateType;
		}

		private static int counter;

		private static readonly Dictionary<MethodInfo, List<DelegateEntry>> TypeCache = new Dictionary<MethodInfo, List<DelegateEntry>>();

		private static readonly MethodBase CallingConvAttr = AccessTools.Constructor(typeof(UnmanagedFunctionPointerAttribute), new Type[1] { typeof(CallingConvention) });

		public static readonly DelegateTypeFactory instance = new DelegateTypeFactory();

		public Type CreateDelegateType(Type returnType, Type[] argTypes)
		{
			return CreateDelegateType(returnType, argTypes, null);
		}

		public Type CreateDelegateType(Type returnType, Type[] argTypes, CallingConvention? convention)
		{
			counter++;
			AssemblyDefinition assemblyDefinition = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition($"HarmonyDTFAssembly{counter}", new Version(1, 0)), $"HarmonyDTFModule{counter}", ModuleKind.Dll);
			ModuleDefinition module = assemblyDefinition.MainModule;
			TypeDefinition typeDefinition = new TypeDefinition("", $"HarmonyDTFType{counter}", Mono.Cecil.TypeAttributes.Public | Mono.Cecil.TypeAttributes.Sealed)
			{
				BaseType = module.ImportReference(typeof(MulticastDelegate))
			};
			module.Types.Add(typeDefinition);
			if (convention.HasValue)
			{
				CustomAttribute customAttribute = new CustomAttribute(module.ImportReference(CallingConvAttr));
				customAttribute.ConstructorArguments.Add(new CustomAttributeArgument(module.ImportReference(typeof(CallingConvention)), convention.Value));
				typeDefinition.CustomAttributes.Add(customAttribute);
			}
			MethodDefinition methodDefinition = new MethodDefinition(".ctor", Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.HideBySig | Mono.Cecil.MethodAttributes.RTSpecialName, module.ImportReference(typeof(void)))
			{
				ImplAttributes = Mono.Cecil.MethodImplAttributes.CodeTypeMask
			};
			methodDefinition.Parameters.AddRange(new ParameterDefinition[2]
			{
				new ParameterDefinition(module.ImportReference(typeof(object))),
				new ParameterDefinition(module.ImportReference(typeof(IntPtr)))
			});
			typeDefinition.Methods.Add(methodDefinition);
			MethodDefinition methodDefinition2 = new MethodDefinition("Invoke", Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.Virtual | Mono.Cecil.MethodAttributes.HideBySig, module.ImportReference(returnType))
			{
				ImplAttributes = Mono.Cecil.MethodImplAttributes.CodeTypeMask
			};
			methodDefinition2.Parameters.AddRange(argTypes.Select((Type t) => new ParameterDefinition(module.ImportReference(t))));
			typeDefinition.Methods.Add(methodDefinition2);
			return ReflectionHelper.Load(assemblyDefinition.MainModule).GetType($"HarmonyDTFType{counter}");
		}

		public Type CreateDelegateType(MethodInfo method)
		{
			return CreateDelegateType(method, null);
		}

		public Type CreateDelegateType(MethodInfo method, CallingConvention? convention)
		{
			DelegateEntry delegateEntry;
			if (TypeCache.TryGetValue(method, out var value) && (delegateEntry = value.FirstOrDefault((DelegateEntry e) => e.callingConvention == convention)) != null)
			{
				return delegateEntry.delegateType;
			}
			if (value == null)
			{
				value = (TypeCache[method] = new List<DelegateEntry>());
			}
			delegateEntry = new DelegateEntry
			{
				delegateType = CreateDelegateType(method.ReturnType, method.GetParameters().Types().ToArray(), convention),
				callingConvention = convention
			};
			value.Add(delegateEntry);
			return delegateEntry.delegateType;
		}
	}
}
