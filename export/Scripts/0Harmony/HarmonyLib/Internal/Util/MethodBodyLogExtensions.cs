using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using MonoMod.Cil;
using MonoMod.Utils;

namespace HarmonyLib.Internal.Util
{
	internal static class MethodBodyLogExtensions
	{
		public static string ToILDasmString(this MethodBody mb)
		{
			StringBuilder sb = new StringBuilder();
			Collection<Instruction> instructions = mb.Instructions;
			Instruction instruction = instructions.First();
			instruction.Offset = 0;
			foreach (Instruction item in instructions.Skip(1))
			{
				object operand = instruction.Operand;
				if (operand is ILLabel iLLabel)
				{
					instruction.Operand = iLLabel.Target;
				}
				else if (operand is ILLabel[] source)
				{
					instruction.Operand = source.Select((ILLabel l) => l.Target).ToArray();
				}
				item.Offset = instruction.Offset + instruction.GetSize();
				instruction.Operand = operand;
				instruction = item;
			}
			Dictionary<Instruction, List<ExceptionBlock>> exBlocks = new Dictionary<Instruction, List<ExceptionBlock>>();
			foreach (ExceptionHandler exceptionHandler in mb.ExceptionHandlers)
			{
				AddBlock(exceptionHandler.TryStart, ExceptionBlockType.BeginExceptionBlock);
				AddBlock(exceptionHandler.TryEnd, ExceptionBlockType.EndExceptionBlock);
				AddBlock(exceptionHandler.HandlerEnd, ExceptionBlockType.EndExceptionBlock);
				switch (exceptionHandler.HandlerType)
				{
				case ExceptionHandlerType.Catch:
					AddBlock(exceptionHandler.HandlerStart, ExceptionBlockType.BeginCatchBlock).catchType = exceptionHandler.CatchType.ResolveReflection();
					break;
				case ExceptionHandlerType.Filter:
					AddBlock(exceptionHandler.FilterStart, ExceptionBlockType.BeginExceptFilterBlock);
					break;
				case ExceptionHandlerType.Finally:
					AddBlock(exceptionHandler.HandlerStart, ExceptionBlockType.BeginFinallyBlock);
					break;
				case ExceptionHandlerType.Fault:
					AddBlock(exceptionHandler.HandlerStart, ExceptionBlockType.BeginFaultBlock);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			int indent = 0;
			WriteLine(".locals init (");
			indent += 4;
			for (int num = 0; num < mb.Variables.Count; num++)
			{
				VariableDefinition variableDefinition = mb.Variables[num];
				WriteLine($"{variableDefinition.VariableType.FullName} V_{num}");
			}
			indent -= 4;
			WriteLine(")");
			Stack<string> stack = new Stack<string>();
			foreach (Instruction item2 in instructions)
			{
				List<ExceptionBlock> obj = exBlocks.GetValueSafe(item2) ?? new List<ExceptionBlock>();
				obj.Sort((ExceptionBlock a, ExceptionBlock b) => (a.blockType == ExceptionBlockType.EndExceptionBlock) ? (-1) : 0);
				foreach (ExceptionBlock item3 in obj)
				{
					switch (item3.blockType)
					{
					case ExceptionBlockType.BeginExceptionBlock:
						WriteLine(".try");
						WriteLine("{");
						indent += 2;
						stack.Push(".try");
						break;
					case ExceptionBlockType.BeginCatchBlock:
						WriteLine("catch " + item3.catchType.FullName);
						WriteLine("{");
						indent += 2;
						stack.Push("handler (catch)");
						break;
					case ExceptionBlockType.BeginExceptFilterBlock:
						WriteLine("filter");
						WriteLine("{");
						indent += 2;
						stack.Push("handler (filter)");
						break;
					case ExceptionBlockType.BeginFaultBlock:
						WriteLine("fault");
						WriteLine("{");
						indent += 2;
						stack.Push("handler (fault)");
						break;
					case ExceptionBlockType.BeginFinallyBlock:
						WriteLine("finally");
						WriteLine("{");
						indent += 2;
						stack.Push("handler (finally)");
						break;
					case ExceptionBlockType.EndExceptionBlock:
						indent -= 2;
						WriteLine("} // end " + stack.Pop());
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				object operand2 = item2.Operand;
				if (operand2 is ILLabel iLLabel2)
				{
					item2.Operand = iLLabel2.Target;
				}
				else if (operand2 is ILLabel[] source2)
				{
					item2.Operand = source2.Select((ILLabel l) => l.Target).ToArray();
				}
				WriteLine(item2.ToString());
				item2.Operand = operand2;
			}
			return sb.ToString();
			ExceptionBlock AddBlock(Instruction ins, ExceptionBlockType t)
			{
				if (ins == null)
				{
					return new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock);
				}
				if (!exBlocks.TryGetValue(ins, out var value))
				{
					value = (exBlocks[ins] = new List<ExceptionBlock>());
				}
				ExceptionBlock exceptionBlock = new ExceptionBlock(t);
				value.Add(exceptionBlock);
				return exceptionBlock;
			}
			void WriteLine(string text)
			{
				sb.Append(new string(' ', indent)).AppendLine(text);
			}
		}
	}
}
