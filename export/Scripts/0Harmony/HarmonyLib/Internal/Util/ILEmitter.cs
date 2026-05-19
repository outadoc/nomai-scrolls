using System;
using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;

namespace HarmonyLib.Internal.Util
{
	internal class ILEmitter
	{
		public class Label
		{
			public bool emitted;

			public Instruction instruction = Instruction.Create(OpCodes.Nop);

			public List<Instruction> targets = new List<Instruction>();
		}

		public class ExceptionBlock
		{
			public LabelledExceptionHandler prev;

			public LabelledExceptionHandler cur;

			public Label start;

			public Label skip;
		}

		public class LabelledExceptionHandler
		{
			public TypeReference exceptionType;

			public ExceptionHandlerType handlerType;

			public Label tryStart;

			public Label tryEnd;

			public Label filterStart;

			public Label handlerStart;

			public Label handlerEnd;
		}

		public readonly ILProcessor IL;

		private readonly List<LabelledExceptionHandler> pendingExceptionHandlers = new List<LabelledExceptionHandler>();

		private readonly List<Label> pendingLabels = new List<Label>();

		public Instruction emitBefore;

		private Instruction Target => emitBefore ?? IL.Body.Instructions[IL.Body.Instructions.Count - 1];

		public ILEmitter(ILProcessor il)
		{
			IL = il;
			if (IL.Body.Instructions.Count == 0)
			{
				IL.Emit(OpCodes.Nop);
			}
		}

		public ExceptionBlock BeginExceptionBlock(Label start)
		{
			return new ExceptionBlock
			{
				start = start
			};
		}

		public void EndExceptionBlock(ExceptionBlock block)
		{
			EndHandler(block, block.cur);
		}

		public void BeginHandler(ExceptionBlock block, ExceptionHandlerType handlerType, Type exceptionType = null)
		{
			LabelledExceptionHandler labelledExceptionHandler = (block.prev = block.cur);
			if (labelledExceptionHandler != null)
			{
				EndHandler(block, labelledExceptionHandler);
			}
			block.skip = DeclareLabel();
			Emit(OpCodes.Leave, block.skip);
			Label label = DeclareLabel();
			MarkLabel(label);
			block.cur = new LabelledExceptionHandler
			{
				tryStart = block.start,
				tryEnd = label,
				handlerType = handlerType,
				handlerEnd = block.skip,
				exceptionType = ((exceptionType != null) ? IL.Import(exceptionType) : null)
			};
			if (handlerType == ExceptionHandlerType.Filter)
			{
				block.cur.filterStart = label;
			}
			else
			{
				block.cur.handlerStart = label;
			}
		}

		public void EndHandler(ExceptionBlock block, LabelledExceptionHandler handler)
		{
			switch (handler.handlerType)
			{
			case ExceptionHandlerType.Filter:
				Emit(OpCodes.Endfilter);
				break;
			case ExceptionHandlerType.Finally:
				Emit(OpCodes.Endfinally);
				break;
			default:
				Emit(OpCodes.Leave, block.skip);
				break;
			}
			MarkLabel(block.skip);
			pendingExceptionHandlers.Add(block.cur);
		}

		public VariableDefinition DeclareVariable(Type type)
		{
			VariableDefinition variableDefinition = new VariableDefinition(IL.Import(type));
			IL.Body.Variables.Add(variableDefinition);
			return variableDefinition;
		}

		public Label DeclareLabel()
		{
			return new Label();
		}

		public Label DeclareLabelFor(Instruction ins)
		{
			return new Label
			{
				emitted = true,
				instruction = ins
			};
		}

		public void MarkLabel(Label label)
		{
			if (!label.emitted)
			{
				pendingLabels.Add(label);
			}
		}

		public Instruction SetOpenLabelsTo(Instruction ins)
		{
			if (pendingLabels.Count != 0)
			{
				foreach (Label pendingLabel in pendingLabels)
				{
					foreach (Instruction target in pendingLabel.targets)
					{
						if (target.Operand is Instruction)
						{
							target.Operand = ins;
						}
						else
						{
							if (!(target.Operand is Instruction[] array))
							{
								continue;
							}
							for (int i = 0; i < array.Length; i++)
							{
								if (array[i] == pendingLabel.instruction)
								{
									array[i] = ins;
									break;
								}
							}
						}
					}
					pendingLabel.instruction = ins;
					pendingLabel.emitted = true;
				}
				pendingLabels.Clear();
			}
			if (pendingExceptionHandlers.Count != 0)
			{
				foreach (LabelledExceptionHandler pendingExceptionHandler in pendingExceptionHandlers)
				{
					IL.Body.ExceptionHandlers.Add(new ExceptionHandler(pendingExceptionHandler.handlerType)
					{
						TryStart = pendingExceptionHandler.tryStart?.instruction,
						TryEnd = pendingExceptionHandler.tryEnd?.instruction,
						FilterStart = pendingExceptionHandler.filterStart?.instruction,
						HandlerStart = pendingExceptionHandler.handlerStart?.instruction,
						HandlerEnd = pendingExceptionHandler.handlerEnd?.instruction,
						CatchType = pendingExceptionHandler.exceptionType
					});
				}
				pendingExceptionHandlers.Clear();
			}
			return ins;
		}

		public void Emit(OpCode opcode)
		{
			IL.InsertBefore(Target, SetOpenLabelsTo(IL.Create(opcode)));
		}

		public void Emit(OpCode opcode, Label label)
		{
			Instruction instruction = SetOpenLabelsTo(IL.Create(opcode, label.instruction));
			label.targets.Add(instruction);
			IL.InsertBefore(Target, instruction);
		}

		public void Emit(OpCode opcode, ConstructorInfo cInfo)
		{
			IL.InsertBefore(Target, SetOpenLabelsTo(IL.Create(opcode, IL.Import(cInfo))));
		}

		public void Emit(OpCode opcode, MethodInfo mInfo)
		{
			IL.InsertBefore(Target, SetOpenLabelsTo(IL.Create(opcode, IL.Import(mInfo))));
		}

		public void Emit(OpCode opcode, Type cls)
		{
			IL.InsertBefore(Target, SetOpenLabelsTo(IL.Create(opcode, IL.Import(cls))));
		}

		public void Emit(OpCode opcode, int arg)
		{
			IL.InsertBefore(Target, SetOpenLabelsTo(IL.Create(opcode, arg)));
		}

		public void Emit(OpCode opcode, string arg)
		{
			IL.InsertBefore(Target, SetOpenLabelsTo(IL.Create(opcode, arg)));
		}

		public void Emit(OpCode opcode, FieldInfo fInfo)
		{
			IL.InsertBefore(Target, SetOpenLabelsTo(IL.Create(opcode, IL.Import(fInfo))));
		}

		public void Emit(OpCode opcode, VariableDefinition varDef)
		{
			IL.InsertBefore(Target, SetOpenLabelsTo(IL.Create(opcode, varDef)));
		}
	}
}
