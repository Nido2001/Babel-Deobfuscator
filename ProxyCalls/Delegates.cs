using System;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace BabelDeobfuscator.Protections.ProxyCalls
{
	internal class Delegates
	{
		public static void CleanDelegates(ModuleDefMD module)
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
								bool isDelegate = methodDef2.DeclaringType.IsDelegate;
								bool flag5 = isDelegate;
								if (flag5)
								{
									bool flag6 = !methodDef2.FullName.Contains("::Invoke");
									bool flag7 = flag6;
									if (flag7)
									{
										MethodDef methodDef3 = methodDef2.DeclaringType.FindConstructors().ToArray<MethodDef>()[1];
										bool flag8 = methodDef3.Body.Instructions.Count == 5;
										bool flag9 = flag8;
										if (flag9)
										{
											MethodDef methodDef4 = methodDef3.Body.Instructions[3].Operand as MethodDef;
											int ldcI4Value = methodDef3.Body.Instructions[0].GetLdcI4Value();
											int ldcI4Value2 = methodDef3.Body.Instructions[1].GetLdcI4Value();
											int ldcI4Value3 = methodDef3.Body.Instructions[2].GetLdcI4Value();
											Program.asm.ManifestModule.ResolveMethod(methodDef4.MDToken.ToInt32()).Invoke(null, new object[]
											{
												ldcI4Value,
												ldcI4Value2,
												ldcI4Value3
											});
											FieldDef fieldDef = methodDef2.DeclaringType.Fields.First<FieldDef>();
											Delegate @delegate = (Delegate)Program.asm.ManifestModule.ResolveField(fieldDef.MDToken.ToInt32()).GetValue(null);
											bool flag10 = @delegate.Method.ReturnTypeCustomAttributes.ToString().Contains("DynamicMethod");
											bool flag11 = flag10;
											if (flag11)
											{
												DynamicMethodBodyReader dynamicMethodBodyReader = new DynamicMethodBodyReader(module, @delegate);
												dynamicMethodBodyReader.Read();
												int count = dynamicMethodBodyReader.Instructions.Count;
												methodDef.Body.Instructions[i].OpCode = dynamicMethodBodyReader.Instructions[count - 2].OpCode;
												methodDef.Body.Instructions[i].Operand = dynamicMethodBodyReader.Instructions[count - 2].Operand;
											}
											else
											{
												IMethod operand = module.Import(@delegate.Method);
												methodDef.Body.Instructions[i].OpCode = OpCodes.Call;
												methodDef.Body.Instructions[i].Operand = operand;
											}
										}
										else
										{
											bool flag12 = methodDef3.Body.Instructions.Count == 6;
											bool flag13 = flag12;
											if (flag13)
											{
												MethodDef methodDef5 = methodDef3.Body.Instructions[4].Operand as MethodDef;
												int ldcI4Value4 = methodDef3.Body.Instructions[0].GetLdcI4Value();
												int ldcI4Value5 = methodDef3.Body.Instructions[1].GetLdcI4Value();
												int ldcI4Value6 = methodDef3.Body.Instructions[2].GetLdcI4Value();
												int ldcI4Value7 = methodDef3.Body.Instructions[3].GetLdcI4Value();
												Program.asm.ManifestModule.ResolveMethod(methodDef5.MDToken.ToInt32()).Invoke(null, new object[]
												{
													ldcI4Value4,
													ldcI4Value5,
													ldcI4Value6,
													ldcI4Value7
												});
												FieldDef fieldDef2 = methodDef2.DeclaringType.Fields.First<FieldDef>();
												Delegate delegate2 = (Delegate)Program.asm.ManifestModule.ResolveField(fieldDef2.MDToken.ToInt32()).GetValue(null);
												bool flag14 = delegate2.Method.ReturnTypeCustomAttributes.ToString().Contains("DynamicMethod");
												bool flag15 = flag14;
												if (flag15)
												{
													DynamicMethodBodyReader dynamicMethodBodyReader2 = new DynamicMethodBodyReader(module, delegate2);
													dynamicMethodBodyReader2.Read();
													int count2 = dynamicMethodBodyReader2.Instructions.Count;
													methodDef.Body.Instructions[i].OpCode = dynamicMethodBodyReader2.Instructions[count2 - 2].OpCode;
													methodDef.Body.Instructions[i].Operand = dynamicMethodBodyReader2.Instructions[count2 - 2].Operand;
												}
												else
												{
													IMethod operand2 = module.Import(delegate2.Method);
													methodDef.Body.Instructions[i].OpCode = OpCodes.Call;
													methodDef.Body.Instructions[i].Operand = operand2;
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
	}
}
