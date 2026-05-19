using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib.Tools;
using MonoMod.RuntimeDetour;

namespace HarmonyLib.Internal.RuntimeFixes
{
	internal static class StackTraceFixes
	{
		private static bool _applied;

		private static readonly Dictionary<MethodBase, MethodBase> RealMethodMap = new Dictionary<MethodBase, MethodBase>();

		private static Func<Assembly> _realGetAss;

		private static Func<StackFrame, MethodBase> _origGetMethod;

		private static Action<object> _origRefresh;

		public static void Install()
		{
			if (!_applied)
			{
				try
				{
					_origRefresh = new Detour(AccessTools.Method(AccessTools.Inner(typeof(ILHook), "Context"), "Refresh"), AccessTools.Method(typeof(StackTraceFixes), "OnILChainRefresh")).GenerateTrampoline<Action<object>>();
					_origGetMethod = new Detour(AccessTools.Method(typeof(StackFrame), "GetMethod"), AccessTools.Method(typeof(StackTraceFixes), "GetMethodFix")).GenerateTrampoline<Func<StackFrame, MethodBase>>();
					_realGetAss = new NativeDetour(AccessTools.Method(typeof(Assembly), "GetExecutingAssembly"), AccessTools.Method(typeof(StackTraceFixes), "GetAssemblyFix")).GenerateTrampoline<Func<Assembly>>();
				}
				catch (Exception ex)
				{
					Logger.LogText(Logger.LogChannel.Error, "Failed to apply stack trace fix: (" + ex.GetType().FullName + ") " + ex.Message);
				}
				_applied = true;
			}
		}

		private static MethodBase GetMethodFix(StackFrame self)
		{
			MethodBase methodBase = _origGetMethod(self);
			if (methodBase == null)
			{
				return null;
			}
			lock (RealMethodMap)
			{
				MethodBase value;
				return RealMethodMap.TryGetValue(methodBase, out value) ? value : methodBase;
			}
		}

		private static Assembly GetAssemblyFix()
		{
			return new StackFrame(1).GetMethod()?.Module.Assembly ?? _realGetAss();
		}

		private static void OnILChainRefresh(object self)
		{
			_origRefresh(self);
			if (!(AccessTools.Field(self.GetType(), "Detour").GetValue(self) is Detour detour))
			{
				return;
			}
			lock (RealMethodMap)
			{
				RealMethodMap[detour.Target] = detour.Method;
			}
		}
	}
}
