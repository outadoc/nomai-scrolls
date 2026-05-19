using System;
using System.Reflection;
using HarmonyLib.Internal.Util;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace HarmonyLib.Public.Patching
{
	public class ManagedMethodPatcher : MethodPatcher
	{
		private static readonly MethodInfo IsAppliedSetter = AccessTools.PropertySetter(typeof(ILHook), "IsApplied");

		private static readonly Action<ILHook, bool> SetIsApplied = IsAppliedSetter.CreateDelegate<Action<ILHook, bool>>();

		private Mono.Cecil.Cil.MethodBody hookBody;

		private ILHook ilHook;

		public ManagedMethodPatcher(MethodBase original)
			: base(original)
		{
		}

		public override DynamicMethodDefinition PrepareOriginal()
		{
			return null;
		}

		public override MethodBase DetourTo(MethodBase replacement)
		{
			if (ilHook == null)
			{
				ilHook = new ILHook(base.Original, Manipulator, new ILHookConfig
				{
					ManualApply = true
				});
			}
			SetIsApplied(ilHook, arg2: false);
			try
			{
				ilHook.Apply();
			}
			catch (Exception ex)
			{
				throw HarmonyException.Create(ex, hookBody);
			}
			return ilHook.GetCurrentTarget();
		}

		public override DynamicMethodDefinition CopyOriginal()
		{
			return new DynamicMethodDefinition(base.Original);
		}

		private void Manipulator(ILContext ctx)
		{
			hookBody = ctx.Body;
			HarmonyManipulator.Manipulate(base.Original, base.Original.GetPatchInfo(), ctx);
		}

		public static void TryResolve(object sender, PatchManager.PatcherResolverEventArgs args)
		{
			if (args.Original.GetMethodBody() != null)
			{
				args.MethodPatcher = new ManagedMethodPatcher(args.Original);
			}
		}
	}
}
