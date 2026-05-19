using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib.Internal.Patching;
using HarmonyLib.Internal.Util;
using HarmonyLib.Public.Patching;
using HarmonyLib.Tools;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace HarmonyLib
{
	internal static class PatchFunctions
	{
		internal static List<MethodInfo> GetSortedPatchMethods(MethodBase original, Patch[] patches, bool debug)
		{
			return new PatchSorter(patches, debug).Sort(original);
		}

		internal static Patch[] GetSortedPatchMethodsAsPatches(MethodBase original, Patch[] patches, bool debug)
		{
			return new PatchSorter(patches, debug).SortAsPatches(original);
		}

		internal static MethodInfo UpdateWrapper(MethodBase original, PatchInfo patchInfo)
		{
			MethodPatcher methodPatcher = original.GetMethodPatcher();
			DynamicMethodDefinition dynamicMethodDefinition = methodPatcher.PrepareOriginal();
			if (dynamicMethodDefinition != null)
			{
				ILContext ctx = new ILContext(dynamicMethodDefinition.Definition);
				HarmonyManipulator.Manipulate(original, patchInfo, ctx);
			}
			try
			{
				return methodPatcher.DetourTo(dynamicMethodDefinition?.Generate()) as MethodInfo;
			}
			catch (Exception ex)
			{
				throw HarmonyException.Create(ex, dynamicMethodDefinition?.Definition?.Body);
			}
		}

		internal static MethodInfo ReversePatch(HarmonyMethod standin, MethodBase original, MethodInfo postTranspiler, MethodInfo postManipulator)
		{
			if (standin == null)
			{
				throw new ArgumentNullException("standin");
			}
			if ((object)standin.method == null)
			{
				throw new ArgumentNullException("standin.method");
			}
			bool debug = standin.debug == true;
			List<MethodInfo> transpilers = new List<MethodInfo>();
			List<MethodInfo> ilmanipulators = new List<MethodInfo>();
			if (standin.reversePatchType == HarmonyReversePatchType.Snapshot)
			{
				Patches patchInfo = Harmony.GetPatchInfo(original);
				transpilers.AddRange(GetSortedPatchMethods(original, patchInfo.Transpilers.ToArray(), debug));
				ilmanipulators.AddRange(GetSortedPatchMethods(original, patchInfo.ILManipulators.ToArray(), debug));
			}
			if ((object)postTranspiler != null)
			{
				transpilers.Add(postTranspiler);
			}
			if ((object)postManipulator != null)
			{
				ilmanipulators.Add(postManipulator);
			}
			Logger.Log(Logger.LogChannel.Info, delegate
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("Reverse patching " + standin.method.FullDescription() + " with " + original.FullDescription());
				PrintInfo(stringBuilder, transpilers, "Transpiler");
				PrintInfo(stringBuilder, ilmanipulators, "Manipulators");
				return stringBuilder.ToString();
			}, debug);
			Mono.Cecil.Cil.MethodBody patchBody = null;
			ILHook iLHook = new ILHook(standin.method, delegate(ILContext ctx)
			{
				if (original is MethodInfo methodInfo2)
				{
					patchBody = ctx.Body;
					MethodPatcher methodPatcher = methodInfo2.GetMethodPatcher();
					DynamicMethodDefinition dynamicMethodDefinition = methodPatcher.CopyOriginal();
					if (dynamicMethodDefinition == null)
					{
						throw new NullReferenceException("Cannot reverse patch " + methodInfo2.FullDescription() + ": method patcher (" + methodPatcher.GetType().FullDescription() + ") can't copy original method body");
					}
					ILManipulator iLManipulator = new ILManipulator(dynamicMethodDefinition.Definition.Body, debug);
					ctx.Body.Variables.Clear();
					foreach (VariableDefinition variable in iLManipulator.Body.Variables)
					{
						ctx.Body.Variables.Add(new VariableDefinition(ctx.Module.ImportReference(variable.VariableType)));
					}
					foreach (MethodInfo item in transpilers)
					{
						iLManipulator.AddTranspiler(item);
					}
					iLManipulator.WriteTo(ctx.Body, standin.method);
					HarmonyManipulator.ApplyILManipulators(ctx, original, ilmanipulators, null);
					Instruction instruction = null;
					foreach (Instruction item2 in ctx.Instrs.Where((Instruction i) => i.OpCode == Mono.Cecil.Cil.OpCodes.Ret))
					{
						if (instruction == null)
						{
							instruction = ctx.IL.Create(Mono.Cecil.Cil.OpCodes.Ret);
						}
						item2.OpCode = Mono.Cecil.Cil.OpCodes.Br;
						item2.Operand = instruction;
					}
					if (instruction != null)
					{
						ctx.IL.Append(instruction);
					}
					Logger.Log(Logger.LogChannel.IL, () => "Generated reverse patcher (" + ctx.Method.FullName + "):\n" + ctx.Body.ToILDasmString(), debug);
				}
			}, new ILHookConfig
			{
				ManualApply = true
			});
			try
			{
				iLHook.Apply();
			}
			catch (Exception ex)
			{
				throw HarmonyException.Create(ex, patchBody);
			}
			MethodInfo methodInfo = iLHook.GetCurrentTarget() as MethodInfo;
			PatchTools.RememberObject(standin.method, methodInfo);
			return methodInfo;
			void PrintInfo(StringBuilder sb, ICollection<MethodInfo> methods, string name)
			{
				if (methods.Count <= 0)
				{
					return;
				}
				sb.AppendLine(name + ":");
				foreach (MethodInfo method in methods)
				{
					sb.AppendLine("  * " + method.FullDescription());
				}
			}
		}

		internal static IEnumerable<CodeInstruction> ApplyTranspilers(MethodBase methodBase, ILGenerator generator, int maxTranspilers = 0)
		{
			MethodPatcher methodPatcher = methodBase.GetMethodPatcher();
			DynamicMethodDefinition dynamicMethodDefinition = methodPatcher.CopyOriginal();
			if (dynamicMethodDefinition == null)
			{
				throw new NullReferenceException("Cannot reverse patch " + methodBase.FullDescription() + ": method patcher (" + methodPatcher.GetType().FullDescription() + ") can't copy original method body");
			}
			ILManipulator iLManipulator = new ILManipulator(dynamicMethodDefinition.Definition.Body, debug: false);
			PatchInfo patchInfo = methodBase.GetPatchInfo();
			if (patchInfo != null)
			{
				List<MethodInfo> sortedPatchMethods = GetSortedPatchMethods(methodBase, patchInfo.transpilers, debug: false);
				for (int i = 0; i < maxTranspilers && i < sortedPatchMethods.Count; i++)
				{
					iLManipulator.AddTranspiler(sortedPatchMethods[i]);
				}
			}
			return iLManipulator.GetInstructions(generator, methodBase);
		}
	}
}
