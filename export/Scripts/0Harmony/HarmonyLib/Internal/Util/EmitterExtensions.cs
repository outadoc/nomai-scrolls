using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using MonoMod.Utils.Cil;

namespace HarmonyLib.Internal.Util
{
	internal static class EmitterExtensions
	{
		private static DynamicMethodDefinition emitDMD;

		private static MethodInfo emitDMDMethod;

		private static Action<CecilILGenerator, System.Reflection.Emit.OpCode, object> emitCodeDelegate;

		private static readonly ConstructorInfo c_LocalBuilder;

		private static readonly FieldInfo f_LocalBuilder_position;

		private static readonly FieldInfo f_LocalBuilder_is_pinned;

		private static int c_LocalBuilder_params;

		[MethodImpl(MethodImplOptions.Synchronized)]
		static EmitterExtensions()
		{
			c_LocalBuilder = (from c in typeof(LocalBuilder).GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				orderby c.GetParameters().Length descending
				select c).First();
			f_LocalBuilder_position = typeof(LocalBuilder).GetField("position", BindingFlags.Instance | BindingFlags.NonPublic);
			f_LocalBuilder_is_pinned = typeof(LocalBuilder).GetField("is_pinned", BindingFlags.Instance | BindingFlags.NonPublic);
			c_LocalBuilder_params = c_LocalBuilder.GetParameters().Length;
			if (emitDMD == null)
			{
				InitEmitterHelperDMD();
			}
		}

		public static Type OpenRefType(this Type t)
		{
			if (t.IsByRef)
			{
				return t.GetElementType();
			}
			return t;
		}

		private static void InitEmitterHelperDMD()
		{
			emitDMD = new DynamicMethodDefinition("EmitOpcodeWithOperand", typeof(void), new Type[3]
			{
				typeof(CecilILGenerator),
				typeof(System.Reflection.Emit.OpCode),
				typeof(object)
			});
			ILGenerator iLGenerator = emitDMD.GetILGenerator();
			Label label = iLGenerator.DefineLabel();
			iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldarg_2);
			iLGenerator.Emit(System.Reflection.Emit.OpCodes.Brtrue, label);
			iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldstr, "Provided operand is null!");
			iLGenerator.Emit(System.Reflection.Emit.OpCodes.Newobj, typeof(Exception).GetConstructor(new Type[1] { typeof(string) }));
			iLGenerator.Emit(System.Reflection.Emit.OpCodes.Throw);
			foreach (MethodInfo item in from m in typeof(CecilILGenerator).GetMethods()
				where m.Name == "Emit"
				select m)
			{
				ParameterInfo[] parameters = item.GetParameters();
				if (parameters.Length != 2)
				{
					continue;
				}
				Type[] array = parameters.Select((ParameterInfo p) => p.ParameterType).ToArray();
				if (!(array[0] != typeof(System.Reflection.Emit.OpCode)))
				{
					Type type = array[1];
					iLGenerator.MarkLabel(label);
					label = iLGenerator.DefineLabel();
					iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldarg_2);
					iLGenerator.Emit(System.Reflection.Emit.OpCodes.Isinst, type);
					iLGenerator.Emit(System.Reflection.Emit.OpCodes.Brfalse, label);
					iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldarg_2);
					if (type.IsValueType)
					{
						iLGenerator.Emit(System.Reflection.Emit.OpCodes.Unbox_Any, type);
					}
					LocalBuilder local = iLGenerator.DeclareLocal(type);
					iLGenerator.Emit(System.Reflection.Emit.OpCodes.Stloc, local);
					iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
					iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
					iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldloc, local);
					iLGenerator.Emit(System.Reflection.Emit.OpCodes.Callvirt, item);
					iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ret);
				}
			}
			iLGenerator.MarkLabel(label);
			iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ldstr, "The operand is none of the supported types!");
			iLGenerator.Emit(System.Reflection.Emit.OpCodes.Newobj, typeof(Exception).GetConstructor(new Type[1] { typeof(string) }));
			iLGenerator.Emit(System.Reflection.Emit.OpCodes.Throw);
			iLGenerator.Emit(System.Reflection.Emit.OpCodes.Ret);
			emitDMDMethod = emitDMD.Generate();
			emitCodeDelegate = emitDMDMethod.CreateDelegate<Action<CecilILGenerator, System.Reflection.Emit.OpCode, object>>();
		}

		public static void Emit(this CecilILGenerator il, System.Reflection.Emit.OpCode opcode, object operand)
		{
			emitCodeDelegate(il, opcode, operand);
		}

		public static void MarkBlockBefore(this CecilILGenerator il, ExceptionBlock block)
		{
			switch (block.blockType)
			{
			case ExceptionBlockType.BeginExceptionBlock:
				il.BeginExceptionBlock();
				break;
			case ExceptionBlockType.BeginCatchBlock:
				il.BeginCatchBlock(block.catchType);
				break;
			case ExceptionBlockType.BeginExceptFilterBlock:
				il.BeginExceptFilterBlock();
				break;
			case ExceptionBlockType.BeginFaultBlock:
				il.BeginFaultBlock();
				break;
			case ExceptionBlockType.BeginFinallyBlock:
				il.BeginFinallyBlock();
				break;
			case ExceptionBlockType.EndExceptionBlock:
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		public static void MarkBlockAfter(this CecilILGenerator il, ExceptionBlock block)
		{
			if (block.blockType == ExceptionBlockType.EndExceptionBlock)
			{
				il.EndExceptionBlock();
			}
		}

		public static LocalBuilder GetLocal(this CecilILGenerator il, VariableDefinition varDef)
		{
			Dictionary<LocalBuilder, VariableDefinition> dictionary = (Dictionary<LocalBuilder, VariableDefinition>)AccessTools.Field(typeof(CecilILGenerator), "_Variables").GetValue(il);
			LocalBuilder key = dictionary.FirstOrDefault((KeyValuePair<LocalBuilder, VariableDefinition> kv) => kv.Value == varDef).Key;
			if (key != null)
			{
				return key;
			}
			Type type = varDef.VariableType.ResolveReflection();
			bool isPinned = varDef.VariableType.IsPinned;
			int index = varDef.Index;
			object obj;
			if (c_LocalBuilder_params != 4)
			{
				if (c_LocalBuilder_params != 3)
				{
					if (c_LocalBuilder_params != 2)
					{
						if (c_LocalBuilder_params != 0)
						{
							throw new NotSupportedException();
						}
						obj = c_LocalBuilder.Invoke(new object[0]);
					}
					else
					{
						obj = c_LocalBuilder.Invoke(new object[2] { type, null });
					}
				}
				else
				{
					obj = c_LocalBuilder.Invoke(new object[3] { index, type, null });
				}
			}
			else
			{
				obj = c_LocalBuilder.Invoke(new object[4] { index, type, null, isPinned });
			}
			key = (LocalBuilder)obj;
			f_LocalBuilder_position?.SetValue(key, (ushort)index);
			f_LocalBuilder_is_pinned?.SetValue(key, isPinned);
			dictionary[key] = varDef;
			return key;
		}
	}
}
