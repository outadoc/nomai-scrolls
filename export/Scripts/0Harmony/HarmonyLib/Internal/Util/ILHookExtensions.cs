using System;
using System.Reflection;
using System.Reflection.Emit;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace HarmonyLib.Internal.Util
{
	internal static class ILHookExtensions
	{
		private static readonly MethodInfo IsAppliedSetter;

		private static Func<ILHook, Detour> GetAppliedDetour;

		static ILHookExtensions()
		{
			IsAppliedSetter = AccessTools.PropertySetter(typeof(ILHook), "IsApplied");
			DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition("GetDetour", typeof(Detour), new Type[1] { typeof(ILHook) });
			ILGenerator iLGenerator = dynamicMethodDefinition.GetILGenerator();
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Call, AccessTools.PropertyGetter(typeof(ILHook), "_Ctx"));
			iLGenerator.Emit(OpCodes.Ldfld, AccessTools.Field(AccessTools.Inner(typeof(ILHook), "Context"), "Detour"));
			iLGenerator.Emit(OpCodes.Ret);
			GetAppliedDetour = dynamicMethodDefinition.Generate().CreateDelegate<Func<ILHook, Detour>>();
		}

		public static ILHook MarkApply(this ILHook hook, bool apply)
		{
			if (hook == null)
			{
				return null;
			}
			IsAppliedSetter.Invoke(hook, new object[1] { !apply });
			return hook;
		}

		public static MethodBase GetCurrentTarget(this ILHook hook)
		{
			return GetAppliedDetour(hook).Target;
		}
	}
}
