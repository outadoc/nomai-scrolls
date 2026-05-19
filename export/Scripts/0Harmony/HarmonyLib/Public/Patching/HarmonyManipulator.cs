using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib.Internal.Patching;
using HarmonyLib.Internal.Util;
using HarmonyLib.Tools;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace HarmonyLib.Public.Patching
{
	public static class HarmonyManipulator
	{
		internal class PatchContext
		{
			public MethodInfo method;

			public bool wrapTryCatch;
		}

		private class ArgumentBoxInfo
		{
			public int index;

			public VariableDefinition tmpVar;

			public Type type;

			public bool isByRef;
		}

		private static readonly string INSTANCE_PARAM = "__instance";

		private static readonly string ORIGINAL_METHOD_PARAM = "__originalMethod";

		private static readonly string RUN_ORIGINAL_PARAM = "__runOriginal";

		private static readonly string RESULT_VAR = "__result";

		private static readonly string STATE_VAR = "__state";

		private static readonly string EXCEPTION_VAR = "__exception";

		private static readonly string PARAM_INDEX_PREFIX = "__";

		private static readonly string INSTANCE_FIELD_PREFIX = "___";

		private static readonly MethodInfo GetMethodFromHandle1 = typeof(MethodBase).GetMethod("GetMethodFromHandle", new Type[1] { typeof(RuntimeMethodHandle) });

		private static readonly MethodInfo GetMethodFromHandle2 = typeof(MethodBase).GetMethod("GetMethodFromHandle", new Type[2]
		{
			typeof(RuntimeMethodHandle),
			typeof(RuntimeTypeHandle)
		});

		private static MethodInfo LogPatchExceptionMethod = AccessTools.Method(typeof(HarmonyManipulator), "LogPatchException");

		private static void LogPatchException(object errorObject, string patch)
		{
			Logger.LogText(Logger.LogChannel.Error, $"Error while running {patch}. Error: {errorObject}");
		}

		private static void SortPatches(MethodBase original, PatchInfo patchInfo, out List<PatchContext> prefixes, out List<PatchContext> postfixes, out List<PatchContext> transpilers, out List<PatchContext> finalizers, out List<PatchContext> ilmanipulators)
		{
			bool debugging = patchInfo.Debugging;
			Patch[] patches;
			Patch[] patches2;
			Patch[] patches3;
			Patch[] patches4;
			Patch[] patches5;
			lock (patchInfo)
			{
				patches = patchInfo.prefixes.ToArray();
				patches2 = patchInfo.postfixes.ToArray();
				patches3 = patchInfo.transpilers.ToArray();
				patches4 = patchInfo.finalizers.ToArray();
				patches5 = patchInfo.ilmanipulators.ToArray();
			}
			prefixes = Sort(original, patches, debugging);
			postfixes = Sort(original, patches2, debugging);
			transpilers = Sort(original, patches3, debugging);
			finalizers = Sort(original, patches4, debugging);
			ilmanipulators = Sort(original, patches5, debugging);
			List<PatchContext> Sort(MethodBase original2, Patch[] patches6, bool debug)
			{
				return (from p in PatchFunctions.GetSortedPatchMethodsAsPatches(original2, patches6, debug)
					select new PatchContext
					{
						method = p.GetMethod(original2),
						wrapTryCatch = p.wrapTryCatch
					}).ToList();
			}
		}

		public static void Manipulate(MethodBase original, PatchInfo patchInfo, ILContext ctx)
		{
			SortPatches(original, patchInfo, out var sortedPrefixes, out var sortedPostfixes, out var sortedTranspilers, out var sortedFinalizers, out var sortedILManipulators);
			bool debugging = patchInfo.Debugging;
			Logger.Log(Logger.LogChannel.Info, delegate
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendLine($"Patching {original.FullDescription()} with {sortedPrefixes.Count} prefixes, {sortedPostfixes.Count} postfixes, {sortedTranspilers.Count} transpilers, {sortedFinalizers.Count} finalizers");
				Print(sortedPrefixes, "prefixes");
				Print(sortedPostfixes, "postfixes");
				Print(sortedTranspilers, "transpilers");
				Print(sortedFinalizers, "finalizers");
				Print(sortedILManipulators, "ilmanipulators");
				return sb.ToString();
				void Print(ICollection<PatchContext> list, string type)
				{
					if (list.Count == 0)
					{
						return;
					}
					sb.AppendLine($"{list.Count} {type}:");
					foreach (PatchContext item in list)
					{
						sb.AppendLine("* " + item.method.FullDescription());
					}
				}
			}, debugging);
			MakePatched(original, ctx, sortedPrefixes, sortedPostfixes, sortedTranspilers, sortedFinalizers, sortedILManipulators, debugging);
		}

		private static void WriteTranspiledMethod(ILContext ctx, MethodBase original, List<PatchContext> transpilers, bool debug)
		{
			if (transpilers.Count == 0)
			{
				return;
			}
			Logger.Log(Logger.LogChannel.Info, () => "Transpiling " + original.FullDescription(), debug);
			ILManipulator iLManipulator = new ILManipulator(ctx.Body, debug);
			foreach (PatchContext transpiler in transpilers)
			{
				iLManipulator.AddTranspiler(transpiler.method);
			}
			iLManipulator.WriteTo(ctx.Body, original);
		}

		private static ILEmitter.Label MakeReturnLabel(ILEmitter il)
		{
			ILEmitter.Label label = il.DeclareLabel();
			label.emitted = false;
			bool flag = false;
			foreach (Instruction item in il.IL.Body.Instructions.Where((Instruction ins) => ins.MatchRet()))
			{
				flag = true;
				item.OpCode = OpCodes.Br;
				item.Operand = label.instruction;
				label.targets.Add(item);
			}
			label.instruction = Instruction.Create(flag ? OpCodes.Ret : OpCodes.Nop);
			il.IL.Append(label.instruction);
			return label;
		}

		private static void WritePostfixes(ILEmitter il, MethodBase original, ILEmitter.Label returnLabel, Dictionary<string, VariableDefinition> variables, ICollection<PatchContext> postfixes, bool debug)
		{
			if (postfixes.Count == 0)
			{
				return;
			}
			Logger.Log(Logger.LogChannel.Info, () => "Writing postfixes", debug);
			il.emitBefore = il.IL.Body.Instructions[il.IL.Body.Instructions.Count - 1];
			il.MarkLabel(returnLabel);
			if (!variables.TryGetValue(RESULT_VAR, out var value))
			{
				Type returnedType = AccessTools.GetReturnedType(original);
				VariableDefinition variableDefinition = (variables[RESULT_VAR] = ((returnedType == typeof(void)) ? null : il.DeclareVariable(returnedType)));
				value = variableDefinition;
			}
			if (value != null)
			{
				il.Emit(OpCodes.Stloc, value);
			}
			if (!variables.ContainsKey(RUN_ORIGINAL_PARAM))
			{
				VariableDefinition variableDefinition = (variables[RUN_ORIGINAL_PARAM] = il.DeclareVariable(typeof(bool)));
				VariableDefinition varDef = variableDefinition;
				il.Emit(OpCodes.Ldc_I4_1);
				il.Emit(OpCodes.Stloc, varDef);
			}
			foreach (PatchContext item in postfixes.Where((PatchContext p) => p.method.ReturnType == typeof(void)))
			{
				MethodInfo method = item.method;
				ILEmitter.Label label = il.DeclareLabel();
				il.MarkLabel(label);
				EmitCallParameter(il, original, method, variables, allowFirsParamPassthrough: true, out var tmpObjectVar, out var tmpBoxVars);
				il.Emit(OpCodes.Call, method);
				EmitResultUnbox(il, original, tmpObjectVar, value);
				EmitArgUnbox(il, tmpBoxVars);
				if (item.wrapTryCatch)
				{
					EmitTryCatchWrapper(il, method, label);
				}
			}
			if (value != null)
			{
				il.Emit(OpCodes.Ldloc, value);
			}
			foreach (PatchContext item2 in postfixes.Where((PatchContext p) => p.method.ReturnType != typeof(void)))
			{
				MethodInfo method2 = item2.method;
				if (item2.wrapTryCatch)
				{
					il.Emit(OpCodes.Stloc, value);
				}
				ILEmitter.Label label2 = il.DeclareLabel();
				il.MarkLabel(label2);
				if (item2.wrapTryCatch)
				{
					il.Emit(OpCodes.Ldloc, value);
				}
				EmitCallParameter(il, original, method2, variables, allowFirsParamPassthrough: true, out var tmpObjectVar2, out var tmpBoxVars2);
				il.Emit(OpCodes.Call, method2);
				EmitResultUnbox(il, original, tmpObjectVar2, value);
				EmitArgUnbox(il, tmpBoxVars2);
				ParameterInfo parameterInfo = method2.GetParameters().FirstOrDefault();
				if (parameterInfo == null || method2.ReturnType != parameterInfo.ParameterType)
				{
					if (parameterInfo != null)
					{
						throw new InvalidHarmonyPatchArgumentException("Return type of pass through postfix " + method2.FullDescription() + " does not match type of its first parameter", original, method2);
					}
					throw new InvalidHarmonyPatchArgumentException("Postfix patch " + method2.FullDescription() + " must have `void` as return type", original, method2);
				}
				if (item2.wrapTryCatch)
				{
					il.Emit(OpCodes.Stloc, value);
					EmitTryCatchWrapper(il, method2, label2);
					il.Emit(OpCodes.Ldloc, value);
				}
			}
		}

		private static bool WritePrefixes(ILEmitter il, MethodBase original, ILEmitter.Label returnLabel, Dictionary<string, VariableDefinition> variables, ICollection<PatchContext> prefixes, bool debug)
		{
			if (prefixes.Count == 0)
			{
				return false;
			}
			Logger.Log(Logger.LogChannel.Info, () => "Writing prefixes", debug);
			il.emitBefore = il.IL.Body.Instructions[0];
			VariableDefinition variableDefinition;
			if (!variables.TryGetValue(RESULT_VAR, out var value))
			{
				Type returnedType = AccessTools.GetReturnedType(original);
				variableDefinition = (variables[RESULT_VAR] = ((returnedType == typeof(void)) ? null : il.DeclareVariable(returnedType)));
				value = variableDefinition;
			}
			bool flag = prefixes.Any((PatchContext p) => p.method.ReturnType == typeof(bool) || p.method.GetParameters().Any((ParameterInfo pp) => pp.Name == RUN_ORIGINAL_PARAM && pp.ParameterType.OpenRefType() == typeof(bool)));
			variableDefinition = (variables[RUN_ORIGINAL_PARAM] = il.DeclareVariable(typeof(bool)));
			VariableDefinition varDef = variableDefinition;
			il.Emit(OpCodes.Ldc_I4_1);
			il.Emit(OpCodes.Stloc, varDef);
			ILEmitter.Label label = ((value != null) ? il.DeclareLabel() : returnLabel);
			foreach (PatchContext prefix in prefixes)
			{
				MethodInfo method = prefix.method;
				ILEmitter.Label label2 = il.DeclareLabel();
				il.MarkLabel(label2);
				EmitCallParameter(il, original, method, variables, allowFirsParamPassthrough: false, out var tmpObjectVar, out var tmpBoxVars);
				il.Emit(OpCodes.Call, method);
				EmitResultUnbox(il, original, tmpObjectVar, value);
				EmitArgUnbox(il, tmpBoxVars);
				if (!AccessTools.IsVoid(method.ReturnType))
				{
					if (method.ReturnType != typeof(bool))
					{
						throw new InvalidHarmonyPatchArgumentException($"Prefix patch {method.FullDescription()} has return type {method.ReturnType}, but only `bool` or `void` are permitted", original, method);
					}
					if (flag)
					{
						il.Emit(OpCodes.Ldloc, varDef);
						il.Emit(OpCodes.And);
						il.Emit(OpCodes.Stloc, varDef);
					}
				}
				if (prefix.wrapTryCatch)
				{
					EmitTryCatchWrapper(il, method, label2);
				}
			}
			if (!flag)
			{
				return false;
			}
			il.Emit(OpCodes.Ldloc, varDef);
			il.Emit(OpCodes.Brfalse, label);
			if (value == null)
			{
				return true;
			}
			il.emitBefore = il.IL.Body.Instructions[il.IL.Body.Instructions.Count - 1];
			il.MarkLabel(label);
			il.Emit(OpCodes.Ldloc, value);
			return true;
		}

		private static bool WriteFinalizers(ILEmitter il, MethodBase original, ILEmitter.Label returnLabel, Dictionary<string, VariableDefinition> variables, ICollection<PatchContext> finalizers, bool debug)
		{
			if (finalizers.Count == 0)
			{
				return false;
			}
			Logger.Log(Logger.LogChannel.Info, () => "Writing finalizers", debug);
			variables[EXCEPTION_VAR] = il.DeclareVariable(typeof(Exception));
			VariableDefinition varDef = il.DeclareVariable(typeof(bool));
			il.emitBefore = il.IL.Body.Instructions[0];
			il.Emit(OpCodes.Ldc_I4_0);
			il.Emit(OpCodes.Stloc, varDef);
			il.emitBefore = il.IL.Body.Instructions[il.IL.Body.Instructions.Count - 1];
			il.MarkLabel(returnLabel);
			if (!variables.TryGetValue(RESULT_VAR, out var returnValueVar))
			{
				Type returnedType = AccessTools.GetReturnedType(original);
				VariableDefinition variableDefinition = (variables[RESULT_VAR] = ((returnedType == typeof(void)) ? null : il.DeclareVariable(returnedType)));
				returnValueVar = variableDefinition;
			}
			ILEmitter.ExceptionBlock block = il.BeginExceptionBlock(il.DeclareLabelFor(il.IL.Body.Instructions[0]));
			if (returnValueVar != null)
			{
				il.Emit(OpCodes.Stloc, returnValueVar);
			}
			WriteFinalizerCalls(suppressExceptions: false);
			il.Emit(OpCodes.Ldc_I4_1);
			il.Emit(OpCodes.Stloc, varDef);
			ILEmitter.Label label = il.DeclareLabel();
			il.Emit(OpCodes.Ldloc, variables[EXCEPTION_VAR]);
			il.Emit(OpCodes.Brfalse, label);
			il.Emit(OpCodes.Ldloc, variables[EXCEPTION_VAR]);
			il.Emit(OpCodes.Throw);
			il.MarkLabel(label);
			il.BeginHandler(block, ExceptionHandlerType.Catch, typeof(Exception));
			il.Emit(OpCodes.Stloc, variables[EXCEPTION_VAR]);
			il.Emit(OpCodes.Ldloc, varDef);
			ILEmitter.Label label2 = il.DeclareLabel();
			il.Emit(OpCodes.Brtrue, label2);
			bool num = WriteFinalizerCalls(suppressExceptions: true);
			il.MarkLabel(label2);
			label = il.DeclareLabel();
			il.Emit(OpCodes.Ldloc, variables[EXCEPTION_VAR]);
			il.Emit(OpCodes.Brfalse, label);
			if (num)
			{
				il.Emit(OpCodes.Rethrow);
			}
			else
			{
				il.Emit(OpCodes.Ldloc, variables[EXCEPTION_VAR]);
				il.Emit(OpCodes.Throw);
			}
			il.MarkLabel(label);
			il.EndExceptionBlock(block);
			if (returnValueVar != null)
			{
				il.Emit(OpCodes.Ldloc, returnValueVar);
			}
			return true;
			bool WriteFinalizerCalls(bool suppressExceptions)
			{
				bool result = true;
				foreach (PatchContext finalizer in finalizers)
				{
					MethodInfo method = finalizer.method;
					ILEmitter.Label label3 = il.DeclareLabel();
					il.MarkLabel(label3);
					EmitCallParameter(il, original, method, variables, allowFirsParamPassthrough: false, out var tmpObjectVar, out var tmpBoxVars);
					il.Emit(OpCodes.Call, method);
					EmitResultUnbox(il, original, tmpObjectVar, returnValueVar);
					EmitArgUnbox(il, tmpBoxVars);
					if (method.ReturnType != typeof(void))
					{
						il.Emit(OpCodes.Stloc, variables[EXCEPTION_VAR]);
						result = false;
					}
					if (suppressExceptions || finalizer.wrapTryCatch)
					{
						EmitTryCatchWrapper(il, method, label3);
					}
				}
				return result;
			}
		}

		internal static void ApplyILManipulators(ILContext ctx, MethodBase original, ICollection<MethodInfo> manipulators, ILEmitter.Label retLabel)
		{
			ILLabel item = ctx.DefineLabel(retLabel?.instruction) ?? ctx.DefineLabel(ctx.Body.Instructions.Last());
			foreach (MethodInfo manipulator in manipulators)
			{
				List<object> list = new List<object>();
				foreach (Type item2 in from p in manipulator.GetParameters()
					select p.ParameterType)
				{
					if (item2.IsAssignableFrom(typeof(ILContext)))
					{
						list.Add(ctx);
					}
					if (item2.IsAssignableFrom(typeof(MethodBase)))
					{
						list.Add(original);
					}
					if (item2.IsAssignableFrom(typeof(ILLabel)))
					{
						list.Add(item);
					}
				}
				manipulator.Invoke(null, list.ToArray());
			}
		}

		private static void MakePatched(MethodBase original, ILContext ctx, List<PatchContext> prefixes, List<PatchContext> postfixes, List<PatchContext> transpilers, List<PatchContext> finalizers, List<PatchContext> ilmanipulators, bool debug)
		{
			try
			{
				if (original == null)
				{
					throw new ArgumentException("original");
				}
				Logger.Log(Logger.LogChannel.Info, () => "Running ILHook manipulator on " + original.FullDescription(), debug);
				WriteTranspiledMethod(ctx, original, transpilers, debug);
				if (prefixes.Count + postfixes.Count + finalizers.Count + ilmanipulators.Count == 0)
				{
					Logger.Log(Logger.LogChannel.IL, () => "Generated patch (" + ctx.Method.FullName + "):\n" + ctx.Body.ToILDasmString(), debug);
					return;
				}
				ILEmitter iLEmitter = new ILEmitter(ctx.IL);
				ILEmitter.Label label = MakeReturnLabel(iLEmitter);
				Dictionary<string, VariableDefinition> dictionary = new Dictionary<string, VariableDefinition>();
				foreach (PatchContext item in prefixes.Union(postfixes).Union(finalizers))
				{
					if (!(item.method.DeclaringType != null) || dictionary.ContainsKey(item.method.DeclaringType.FullName))
					{
						continue;
					}
					foreach (ParameterInfo item2 in from patchParam in item.method.GetParameters()
						where patchParam.Name == STATE_VAR
						select patchParam)
					{
						dictionary[item.method.DeclaringType.FullName] = iLEmitter.DeclareVariable(item2.ParameterType.OpenRefType());
					}
				}
				int num = 0 | (WritePrefixes(iLEmitter, original, label, dictionary, prefixes, debug) ? 1 : 0);
				WritePostfixes(iLEmitter, original, label, dictionary, postfixes, debug);
				int num2 = num | (WriteFinalizers(iLEmitter, original, label, dictionary, finalizers, debug) ? 1 : 0);
				iLEmitter.MarkLabel(label);
				Instruction instruction = iLEmitter.SetOpenLabelsTo(ctx.Instrs[ctx.Instrs.Count - 1]);
				if (num2 != 0)
				{
					instruction.OpCode = OpCodes.Ret;
				}
				ApplyILManipulators(ctx, original, ilmanipulators.Select((PatchContext m) => m.method).ToList(), label);
				Logger.Log(Logger.LogChannel.IL, () => "Generated patch (" + ctx.Method.FullName + "):\n" + ctx.Body.ToILDasmString(), debug);
			}
			catch (Exception ex)
			{
				Exception ex2 = ex;
				Exception e = ex2;
				Logger.Log(Logger.LogChannel.Error, () => $"Failed to patch {original.FullDescription()}: {e}", debug);
				throw HarmonyException.Create(e, ctx.Body);
			}
		}

		private static OpCode GetIndOpcode(Type type)
		{
			if (type.IsEnum)
			{
				return OpCodes.Ldind_I4;
			}
			if (type == typeof(float))
			{
				return OpCodes.Ldind_R4;
			}
			if (type == typeof(double))
			{
				return OpCodes.Ldind_R8;
			}
			if (type == typeof(byte))
			{
				return OpCodes.Ldind_U1;
			}
			if (type == typeof(ushort))
			{
				return OpCodes.Ldind_U2;
			}
			if (type == typeof(uint))
			{
				return OpCodes.Ldind_U4;
			}
			if (type == typeof(ulong))
			{
				return OpCodes.Ldind_I8;
			}
			if (type == typeof(sbyte))
			{
				return OpCodes.Ldind_I1;
			}
			if (type == typeof(short))
			{
				return OpCodes.Ldind_I2;
			}
			if (type == typeof(int))
			{
				return OpCodes.Ldind_I4;
			}
			if (type == typeof(long))
			{
				return OpCodes.Ldind_I8;
			}
			return OpCodes.Ldind_Ref;
		}

		private static void EmitTryCatchWrapper(ILEmitter il, MethodInfo target, ILEmitter.Label start)
		{
			ILEmitter.ExceptionBlock block = il.BeginExceptionBlock(start);
			il.BeginHandler(block, ExceptionHandlerType.Catch, typeof(object));
			il.Emit(OpCodes.Ldstr, target.FullDescription());
			il.Emit(OpCodes.Call, LogPatchExceptionMethod);
			il.EndExceptionBlock(block);
			il.Emit(OpCodes.Nop);
		}

		private static void EmitResultUnbox(ILEmitter il, MethodBase original, VariableDefinition tmp, VariableDefinition result)
		{
			if (tmp != null)
			{
				il.Emit(OpCodes.Ldloc, tmp);
				il.Emit(OpCodes.Unbox_Any, AccessTools.GetReturnedType(original));
				il.Emit(OpCodes.Stloc, result);
			}
		}

		private static void EmitArgUnbox(ILEmitter il, List<ArgumentBoxInfo> boxInfo)
		{
			if (boxInfo == null)
			{
				return;
			}
			foreach (ArgumentBoxInfo item in boxInfo)
			{
				if (item.isByRef)
				{
					il.Emit(OpCodes.Ldarg, item.index);
				}
				il.Emit(OpCodes.Ldloc, item.tmpVar);
				il.Emit(OpCodes.Unbox_Any, item.type);
				if (item.isByRef)
				{
					il.Emit(OpCodes.Stobj, item.type);
				}
				else
				{
					il.Emit(OpCodes.Starg, item.index);
				}
			}
		}

		private static bool EmitOriginalBaseMethod(ILEmitter il, MethodBase original)
		{
			if (original is MethodInfo mInfo)
			{
				il.Emit(OpCodes.Ldtoken, mInfo);
			}
			else
			{
				if (!(original is ConstructorInfo cInfo))
				{
					return false;
				}
				il.Emit(OpCodes.Ldtoken, cInfo);
			}
			Type reflectedType = original.ReflectedType;
			if (reflectedType.IsGenericType)
			{
				il.Emit(OpCodes.Ldtoken, reflectedType);
			}
			il.Emit(OpCodes.Call, reflectedType.IsGenericType ? GetMethodFromHandle2 : GetMethodFromHandle1);
			return true;
		}

		private static void EmitCallParameter(ILEmitter il, MethodBase original, MethodInfo patch, Dictionary<string, VariableDefinition> variables, bool allowFirsParamPassthrough, out VariableDefinition tmpObjectVar, out List<ArgumentBoxInfo> tmpBoxVars)
		{
			tmpObjectVar = null;
			tmpBoxVars = new List<ArgumentBoxInfo>();
			bool flag = !original.IsStatic;
			ParameterInfo[] parameters = original.GetParameters();
			string[] originalParameterNames = parameters.Select((ParameterInfo p) => p.Name).ToArray();
			List<ParameterInfo> list = patch.GetParameters().ToList();
			if (allowFirsParamPassthrough && patch.ReturnType != typeof(void) && list.Count > 0 && list[0].ParameterType == patch.ReturnType)
			{
				list.RemoveRange(0, 1);
			}
			foreach (ParameterInfo item in list)
			{
				if (item.Name == ORIGINAL_METHOD_PARAM)
				{
					if (!EmitOriginalBaseMethod(il, original))
					{
						il.Emit(OpCodes.Ldnull);
					}
					continue;
				}
				if (item.Name == INSTANCE_PARAM)
				{
					if (original.IsStatic)
					{
						il.Emit(OpCodes.Ldnull);
						continue;
					}
					bool num = (object)original.DeclaringType != null && AccessTools.IsStruct(original.DeclaringType);
					bool isByRef = item.ParameterType.IsByRef;
					if (num == isByRef)
					{
						il.Emit(OpCodes.Ldarg_0);
					}
					if (num && !isByRef)
					{
						il.Emit(OpCodes.Ldarg_0);
						il.Emit(OpCodes.Ldobj, original.DeclaringType);
					}
					if (!num && isByRef)
					{
						il.Emit(OpCodes.Ldarga, 0);
					}
					continue;
				}
				if (item.Name.StartsWith(INSTANCE_FIELD_PREFIX, StringComparison.Ordinal))
				{
					string text = item.Name.Substring(INSTANCE_FIELD_PREFIX.Length);
					FieldInfo fieldInfo;
					if (text.All(char.IsDigit))
					{
						fieldInfo = AccessTools.DeclaredField(original.DeclaringType, int.Parse(text));
						if ((object)fieldInfo == null)
						{
							throw new ArgumentException("No field found at given index in class " + original.DeclaringType.FullName, text);
						}
					}
					else
					{
						fieldInfo = AccessTools.Field(original.DeclaringType, text);
						if ((object)fieldInfo == null)
						{
							throw new ArgumentException("No such field defined in class " + original.DeclaringType.FullName, text);
						}
					}
					if (fieldInfo.IsStatic)
					{
						il.Emit(item.ParameterType.IsByRef ? OpCodes.Ldsflda : OpCodes.Ldsfld, fieldInfo);
						continue;
					}
					il.Emit(OpCodes.Ldarg_0);
					il.Emit(item.ParameterType.IsByRef ? OpCodes.Ldflda : OpCodes.Ldfld, fieldInfo);
					continue;
				}
				if (item.Name == STATE_VAR)
				{
					OpCode opcode = (item.ParameterType.IsByRef ? OpCodes.Ldloca : OpCodes.Ldloc);
					if (variables.TryGetValue(patch.DeclaringType.FullName, out var value))
					{
						il.Emit(opcode, value);
					}
					else
					{
						il.Emit(OpCodes.Ldnull);
					}
					continue;
				}
				if (item.Name == RESULT_VAR)
				{
					Type returnedType = AccessTools.GetReturnedType(original);
					if (returnedType == typeof(void))
					{
						throw new Exception("Cannot get result from void method " + original.FullDescription());
					}
					Type type = item.ParameterType;
					if (type.IsByRef && !returnedType.IsByRef)
					{
						type = type.GetElementType();
					}
					if (!type.IsAssignableFrom(returnedType))
					{
						throw new Exception("Cannot assign method return type " + returnedType.FullName + " to " + RESULT_VAR + " type " + type.FullName + " for method " + original.FullDescription());
					}
					OpCode opcode2 = ((item.ParameterType.IsByRef && !returnedType.IsByRef) ? OpCodes.Ldloca : OpCodes.Ldloc);
					if (returnedType.IsValueType && item.ParameterType == typeof(object).MakeByRefType())
					{
						opcode2 = OpCodes.Ldloc;
					}
					il.Emit(opcode2, variables[RESULT_VAR]);
					if (returnedType.IsValueType)
					{
						if (item.ParameterType == typeof(object))
						{
							il.Emit(OpCodes.Box, returnedType);
						}
						else if (item.ParameterType == typeof(object).MakeByRefType())
						{
							il.Emit(OpCodes.Box, returnedType);
							tmpObjectVar = il.DeclareVariable(typeof(object));
							il.Emit(OpCodes.Stloc, tmpObjectVar);
							il.Emit(OpCodes.Ldloca, tmpObjectVar);
						}
					}
					continue;
				}
				if (variables.TryGetValue(item.Name, out var value2))
				{
					OpCode opcode3 = (item.ParameterType.IsByRef ? OpCodes.Ldloca : OpCodes.Ldloc);
					il.Emit(opcode3, value2);
					continue;
				}
				int result;
				if (item.Name.StartsWith(PARAM_INDEX_PREFIX, StringComparison.Ordinal))
				{
					if (!int.TryParse(item.Name.Substring(PARAM_INDEX_PREFIX.Length), out result))
					{
						throw new Exception("Parameter " + item.Name + " does not contain a valid index");
					}
					if (result < 0 || result >= parameters.Length)
					{
						throw new Exception($"No parameter found at index {result}");
					}
				}
				else
				{
					result = patch.GetArgumentIndex(originalParameterNames, item);
					if (result == -1)
					{
						HarmonyMethod mergedFromType = HarmonyMethodExtensions.GetMergedFromType(item.ParameterType);
						MethodType? methodType = mergedFromType.methodType;
						if (!methodType.HasValue)
						{
							mergedFromType.methodType = MethodType.Normal;
						}
						if (mergedFromType.GetOriginalMethod() is MethodInfo methodInfo)
						{
							ConstructorInfo constructor = item.ParameterType.GetConstructor(new Type[2]
							{
								typeof(object),
								typeof(IntPtr)
							});
							if ((object)constructor != null)
							{
								Type declaringType = original.DeclaringType;
								if (methodInfo.IsStatic)
								{
									il.Emit(OpCodes.Ldnull);
								}
								else
								{
									il.Emit(OpCodes.Ldarg_0);
									if (declaringType.IsValueType)
									{
										il.Emit(OpCodes.Ldobj, declaringType);
										il.Emit(OpCodes.Box, declaringType);
									}
								}
								if (!methodInfo.IsStatic && !mergedFromType.nonVirtualDelegate)
								{
									il.Emit(OpCodes.Dup);
									il.Emit(OpCodes.Ldvirtftn, methodInfo);
								}
								else
								{
									il.Emit(OpCodes.Ldftn, methodInfo);
								}
								il.Emit(OpCodes.Newobj, constructor);
								continue;
							}
						}
						throw new Exception("Parameter \"" + item.Name + "\" not found in method " + original.FullDescription());
					}
				}
				Type parameterType = parameters[result].ParameterType;
				Type type2 = (parameterType.IsByRef ? parameterType.GetElementType() : parameterType);
				Type parameterType2 = item.ParameterType;
				Type type3 = (parameterType2.IsByRef ? parameterType2.GetElementType() : parameterType2);
				bool flag2 = !parameters[result].IsOut && !parameterType.IsByRef;
				bool flag3 = !item.IsOut && !parameterType2.IsByRef;
				bool flag4 = type2.IsValueType && !type3.IsValueType;
				int num2 = result + (flag ? 1 : 0);
				if (flag2 == flag3)
				{
					il.Emit(OpCodes.Ldarg, num2);
					if (flag4)
					{
						if (flag3)
						{
							il.Emit(OpCodes.Box, type2);
							continue;
						}
						il.Emit(OpCodes.Ldobj, type2);
						il.Emit(OpCodes.Box, type2);
						VariableDefinition variableDefinition = il.DeclareVariable(type3);
						il.Emit(OpCodes.Stloc, variableDefinition);
						il.Emit(OpCodes.Ldloca_S, variableDefinition);
						tmpBoxVars.Add(new ArgumentBoxInfo
						{
							index = num2,
							type = type2,
							tmpVar = variableDefinition,
							isByRef = true
						});
					}
				}
				else if (flag2 && !flag3)
				{
					if (flag4)
					{
						il.Emit(OpCodes.Ldarg, num2);
						il.Emit(OpCodes.Box, type2);
						VariableDefinition variableDefinition2 = il.DeclareVariable(type3);
						il.Emit(OpCodes.Stloc, variableDefinition2);
						il.Emit(OpCodes.Ldloca_S, variableDefinition2);
						tmpBoxVars.Add(new ArgumentBoxInfo
						{
							index = num2,
							type = type2,
							tmpVar = variableDefinition2,
							isByRef = false
						});
					}
					else
					{
						il.Emit(OpCodes.Ldarga, num2);
					}
				}
				else
				{
					il.Emit(OpCodes.Ldarg, num2);
					if (flag4)
					{
						il.Emit(OpCodes.Ldobj, type2);
						il.Emit(OpCodes.Box, type2);
					}
					else if (type2.IsValueType)
					{
						il.Emit(OpCodes.Ldobj, type2);
					}
					else
					{
						il.Emit(GetIndOpcode(parameters[result].ParameterType));
					}
				}
			}
		}
	}
}
