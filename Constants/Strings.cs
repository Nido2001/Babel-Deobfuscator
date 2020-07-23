using System;
using System.Reflection;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace BabelDeobfuscator.Protections.Constants
{
	internal class Strings
	{
		public static void CleanStringMethodOne(ModuleDefMD module)
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
								bool flag5 = methodDef2.ReturnType == module.CorLibTypes.String && methodDef2.Parameters.Count == 1 && methodDef2.Parameters[0].Type == module.CorLibTypes.Int32;
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
										methodDef.Body.Instructions[i - 1].OpCode = OpCodes.Ldstr;
										methodDef.Body.Instructions[i - 1].Operand = obj;
									}
								}
							}
						}
					}
				}
			}
		}
		public static void CleanStringMethodTwo(ModuleDefMD module)
		{
			foreach (TypeDef typeDef in module.GetTypes())
			{
				foreach (MethodDef methodDef in typeDef.Methods)
				{
					bool flag = !methodDef.HasBody;
					bool flag2 = !flag;
					if (flag2)
					{
						bool flag3 = methodDef.MDToken.ToInt32() == 100666352;
						if (flag3)
						{
						}
						for (int i = 0; i < methodDef.Body.Instructions.Count; i++)
						{
							bool flag4 = methodDef.Body.Instructions[i].OpCode == OpCodes.Call && methodDef.Body.Instructions[i].Operand is IMethod;
							bool flag5 = flag4;
							if (flag5)
							{
								IMethod method = methodDef.Body.Instructions[i].Operand as IMethod;
								MethodDef methodDef2 = method.ResolveMethodDef();
								bool flag6 = methodDef2.ReturnType == module.CorLibTypes.String && methodDef2.Parameters.Count == 2 && methodDef2.Parameters[0].Type == module.CorLibTypes.String && methodDef2.Parameters[1].Type == module.CorLibTypes.Int32;
								bool flag7 = flag6;
								if (flag7)
								{
									bool flag8 = methodDef.Body.Instructions[i - 1].IsLdcI4();
									bool flag9 = flag8;
									if (flag9)
									{
										bool flag10 = methodDef.Body.Instructions[i - 2].OpCode == OpCodes.Ldstr;
										if (flag10)
										{
											MethodBase methodBase = Program.asm.ManifestModule.ResolveMethod(methodDef2.MDToken.ToInt32());
											int ldcI4Value = methodDef.Body.Instructions[i - 1].GetLdcI4Value();
											string text = methodDef.Body.Instructions[i - 2].Operand.ToString();
											object obj = methodBase.Invoke(null, new object[]
											{
												text,
												ldcI4Value
											});
											bool flag11 = obj == null;
											bool flag12 = flag11;
											if (flag12)
											{
											}
											methodDef.Body.Instructions[i].OpCode = OpCodes.Nop;
											methodDef.Body.Instructions[i - 1].OpCode = OpCodes.Ldstr;
											methodDef.Body.Instructions[i - 1].Operand = obj;
											methodDef.Body.Instructions[i - 2].OpCode = OpCodes.Nop;
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
