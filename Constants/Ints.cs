using System;
using System.Reflection;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace BabelDeobfuscator.Protections.Constants
{
	internal class Ints
	{
		public static void CleanInts(ModuleDefMD module)
		{
			foreach (TypeDef typeDef in module.GetTypes())
			{
				foreach (MethodDef methodDef in typeDef.Methods)
				{
					bool flag = !methodDef.HasBody;
					bool flag2 = !flag;
					if (flag2)
					{
						for (int i = 0; i < methodDef.Body.Instructions.Count; i++)
						{
							bool flag3 = methodDef.Body.Instructions[i].OpCode == OpCodes.Call && methodDef.Body.Instructions[i].Operand is MethodDef;
							bool flag4 = flag3;
							if (flag4)
							{
								MethodDef methodDef2 = methodDef.Body.Instructions[i].Operand as MethodDef;
								bool flag5 = methodDef2.ReturnType == module.CorLibTypes.Int32 && methodDef2.Parameters.Count == 1 && methodDef2.Parameters[0].Type == module.CorLibTypes.Int32;
								bool flag6 = flag5;
								if (flag6)
								{
									bool flag7 = methodDef.Body.Instructions[i - 1].IsLdcI4();
									bool flag8 = flag7;
									if (flag8)
									{
										MethodBase methodBase = Program.asm.ManifestModule.ResolveMethod(methodDef2.MDToken.ToInt32());
										int ldcI4Value = methodDef.Body.Instructions[i - 1].GetLdcI4Value();
										object obj = methodBase.Invoke(null, new object[]
										{
											ldcI4Value
										});
										bool flag9 = obj == null;
										bool flag10 = flag9;
										if (flag10)
										{
										}
										methodDef.Body.Instructions[i].OpCode = OpCodes.Nop;
										methodDef.Body.Instructions[i - 1].OpCode = OpCodes.Ldc_I4;
										methodDef.Body.Instructions[i - 1].Operand = obj;
									}
								}
							}
						}
					}
				}
			}
		}
		public static void CleanFloats(ModuleDefMD module)
		{
			foreach (TypeDef typeDef in module.GetTypes())
			{
				foreach (MethodDef methodDef in typeDef.Methods)
				{
					bool flag = !methodDef.HasBody;
					bool flag2 = !flag;
					if (flag2)
					{
						for (int i = 0; i < methodDef.Body.Instructions.Count; i++)
						{
							bool flag3 = methodDef.Body.Instructions[i].OpCode == OpCodes.Call && methodDef.Body.Instructions[i].Operand is MethodDef;
							bool flag4 = flag3;
							if (flag4)
							{
								MethodDef methodDef2 = methodDef.Body.Instructions[i].Operand as MethodDef;
								bool flag5 = methodDef2.ReturnType == module.CorLibTypes.Single && methodDef2.Parameters.Count == 1 && methodDef2.Parameters[0].Type == module.CorLibTypes.Int32;
								bool flag6 = flag5;
								if (flag6)
								{
									bool flag7 = methodDef.Body.Instructions[i - 1].IsLdcI4();
									bool flag8 = flag7;
									if (flag8)
									{
										MethodBase methodBase = Program.asm.ManifestModule.ResolveMethod(methodDef2.MDToken.ToInt32());
										int ldcI4Value = methodDef.Body.Instructions[i - 1].GetLdcI4Value();
										object obj = methodBase.Invoke(null, new object[]
										{
											ldcI4Value
										});
										bool flag9 = obj == null;
										bool flag10 = flag9;
										if (flag10)
										{
										}
										methodDef.Body.Instructions[i].OpCode = OpCodes.Nop;
										methodDef.Body.Instructions[i - 1].OpCode = OpCodes.Ldc_R4;
										methodDef.Body.Instructions[i - 1].Operand = obj;
									}
								}
							}
						}
					}
				}
			}
		}
		public static void CleanDouble(ModuleDefMD module)
		{
			foreach (TypeDef typeDef in module.GetTypes())
			{
				foreach (MethodDef methodDef in typeDef.Methods)
				{
					bool flag = !methodDef.HasBody;
					bool flag2 = !flag;
					if (flag2)
					{
						for (int i = 0; i < methodDef.Body.Instructions.Count; i++)
						{
							bool flag3 = methodDef.Body.Instructions[i].OpCode == OpCodes.Call && methodDef.Body.Instructions[i].Operand is MethodDef;
							bool flag4 = flag3;
							if (flag4)
							{
								MethodDef methodDef2 = methodDef.Body.Instructions[i].Operand as MethodDef;
								bool flag5 = methodDef2.ReturnType == module.CorLibTypes.Double && methodDef2.Parameters.Count == 1 && methodDef2.Parameters[0].Type == module.CorLibTypes.Int32;
								bool flag6 = flag5;
								if (flag6)
								{
									bool flag7 = methodDef.Body.Instructions[i - 1].IsLdcI4();
									bool flag8 = flag7;
									if (flag8)
									{
										MethodBase methodBase = Program.asm.ManifestModule.ResolveMethod(methodDef2.MDToken.ToInt32());
										int ldcI4Value = methodDef.Body.Instructions[i - 1].GetLdcI4Value();
										object obj = methodBase.Invoke(null, new object[]
										{
											ldcI4Value
										});
										bool flag9 = obj == null;
										bool flag10 = flag9;
										if (flag10)
										{
										}
										methodDef.Body.Instructions[i].OpCode = OpCodes.Nop;
										methodDef.Body.Instructions[i - 1].OpCode = OpCodes.Ldc_R8;
										methodDef.Body.Instructions[i - 1].Operand = obj;
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
