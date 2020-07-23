using System;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace BabelDeobfuscator.Protections
{
	internal class AntiTamper
	{
		public static void AntiTamp(ModuleDefMD module)
		{
			foreach (TypeDef typeDef in module.GetTypes())
			{
				foreach (MethodDef methodDef in typeDef.Methods)
				{
					bool flag = !methodDef.HasBody;
					bool flag2 = !flag;
					if (flag2)
					{
						bool flag3 = methodDef.Body.Instructions.Count != 3;
						bool flag4 = !flag3;
						if (flag4)
						{
							bool flag5 = methodDef.Body.Instructions[0].OpCode != OpCodes.Ldnull;
							bool flag6 = !flag5;
							if (flag6)
							{
								bool flag7 = methodDef.Body.Instructions[1].OpCode != OpCodes.Call;
								bool flag8 = !flag7;
								if (flag8)
								{
									bool flag9 = methodDef.Body.Instructions[2].OpCode != OpCodes.Ret;
									bool flag10 = !flag9;
									if (flag10)
									{
										bool flag11 = !methodDef.Body.Instructions[1].Operand.ToString().Contains("FailFast");
										bool flag12 = !flag11;
										if (flag12)
										{
											methodDef.Body.Instructions[0].OpCode = OpCodes.Nop;
											methodDef.Body.Instructions[1].OpCode = OpCodes.Nop;
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}
}
