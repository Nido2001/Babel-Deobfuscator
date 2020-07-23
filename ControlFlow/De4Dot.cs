using System;
using System.Collections.Generic;
using de4dot.blocks;
using de4dot.blocks.cflow;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace BabelDeobfuscator.Protections.ControlFlow
{
	internal class De4Dot
	{
		public static void Cflow(ModuleDefMD asm)
		{
			foreach (TypeDef typeDef in asm.GetTypes())
			{
				foreach (MethodDef methodDef in typeDef.Methods)
				{
					bool flag = !methodDef.HasBody;
					bool flag2 = !flag;
					if (flag2)
					{
						try
						{
							De4Dot.DeobfuscateCflow2(methodDef);
						}
						catch
						{
						}
					}
				}
			}
		}
		public static void CflowE(ModuleDefMD asm)
		{
			foreach (TypeDef typeDef in asm.GetTypes())
			{
				foreach (MethodDef methodDef in typeDef.Methods)
				{
					bool flag = !methodDef.HasBody;
					bool flag2 = !flag;
					if (flag2)
					{
						bool flag3 = methodDef != asm.EntryPoint;
						bool flag4 = !flag3;
						if (flag4)
						{
							De4Dot.Expermient(methodDef);
						}
					}
				}
			}
		}
		public static void DeobfuscateCflow2(MethodDef meth)
		{
			for (int i = 0; i < 1; i++)
			{
				Blocks blocks = new Blocks(meth);
				De4Dot.CfDeob.Initialize(blocks);
				De4Dot.CfDeob.Deobfuscate();
				blocks.RepartitionBlocks();
				IList<Instruction> instructions;
				IList<ExceptionHandler> exceptionHandlers;
				blocks.GetCode(out instructions, out exceptionHandlers);
				DotNetUtils.RestoreBody(meth, instructions, exceptionHandlers);
			}
		}
		public static void Expermient(MethodDef meth)
		{
			for (int i = 0; i < 1; i++)
			{
				Blocks blocks = new Blocks(meth);
				List<Block> allBlocks = blocks.MethodBlocks.GetAllBlocks();
				De4Dot.CfDeob.Initialize(blocks);
				De4Dot.CfDeob.Deobfuscate();
				blocks.RepartitionBlocks();
				IList<Instruction> instructions;
				IList<ExceptionHandler> exceptionHandlers;
				blocks.GetCode(out instructions, out exceptionHandlers);
				DotNetUtils.RestoreBody(meth, instructions, exceptionHandlers);
			}
		}
		private static readonly BlocksCflowDeobfuscator CfDeob = new BlocksCflowDeobfuscator();
	}
}
