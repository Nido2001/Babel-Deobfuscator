using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Babel_Full_Unpacker.BabelVMRestore.Core;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace BabelDeobfuscator.Protections.MethodEncryption
{
	internal class VMDecryptor
	{
		public static void run(ModuleDefMD module, Assembly asm)
		{
			VMDecryptor.FindEncryptedMethods(module);
			bool flag = VMDecryptor.allEncMethods.Count != 0;
			bool flag2 = flag;
			if (flag2)
			{
				DecryptionMethods dec = VMDecryptor.setUpDecryptionRoutine(VMDecryptor.allEncMethods[0].method);
				VMDecryptor.DecryptMethods(dec, asm, module);
			}
		}
		public static void DecryptMethods(DecryptionMethods dec, Assembly asm, ModuleDefMD module)
		{
			Module manifestModule = asm.ManifestModule;
			foreach (EncryptedMethodDetails encryptedMethodDetails in VMDecryptor.allEncMethods)
			{
				try
				{
					int encryptedValue = encryptedMethodDetails.encryptedValue;
					MethodBase methodBase = manifestModule.ResolveMethod(dec.thirdMethod.MDToken.ToInt32());
					object obj = Activator.CreateInstance(methodBase.DeclaringType);
					object obj2 = methodBase.Invoke(obj, new object[]
					{
						encryptedValue
					});
					Type type = manifestModule.ResolveType(dec.initalField.FieldType.ToTypeDefOrRef().MDToken.ToInt32());
					object obj3 = Activator.CreateInstance(type);
					FieldInfo fieldInfo = obj3.GetType().GetFields()[0];
					BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
					FieldInfo field = obj3.GetType().GetField(fieldInfo.Name, bindingAttr);
					object value = field.GetValue(obj3);
					MethodBase methodBase2 = manifestModule.ResolveMethod(dec.fifthMethod.MDToken.ToInt32());
					TypeDef typeDef = dec.initalType as TypeDef;
					MethodDef methodDef = dec.fifthMethod.DeclaringType.FindConstructors().ToArray<MethodDef>()[0];
					ConstructorInfo constructorInfo = (ConstructorInfo)manifestModule.ResolveMethod(methodDef.MDToken.ToInt32());
					object obj4 = constructorInfo.Invoke(new object[]
					{
						value
					});
					object obj5 = methodBase2.Invoke(obj4, new object[]
					{
						obj2
					});
					SuperDynamicReader superDynamicReader = new SuperDynamicReader(module, obj5);
					superDynamicReader.Read();
					superDynamicReader.RestoreMethod(encryptedMethodDetails.method);
				}
				catch
				{
					Console.WriteLine(encryptedMethodDetails.method.FullName + " Failed to decrypt");
				}
			}
		}
		public static DecryptionMethods setUpDecryptionRoutine(MethodDef methods)
		{
			DecryptionMethods decryptionMethods = new DecryptionMethods();
			MethodDef methodDef = methods.Body.Instructions[methods.Body.Instructions.Count - 3].Operand as MethodDef;
			for (int i = 0; i < methodDef.Body.Instructions.Count; i++)
			{
				bool flag = methodDef.Body.Instructions[i].OpCode == OpCodes.Callvirt;
				bool flag2 = flag;
				if (flag2)
				{
					decryptionMethods.initalmethod = (methodDef.Body.Instructions[i].Operand as MethodDef);
					break;
				}
			}
			for (int j = 0; j < decryptionMethods.initalmethod.Body.Instructions.Count; j++)
			{
				bool flag3 = decryptionMethods.initalmethod.Body.Instructions[j].OpCode == OpCodes.Call && decryptionMethods.initalmethod.Body.Instructions[j].Operand is MethodDef;
				bool flag4 = flag3;
				if (flag4)
				{
					MethodDef methodDef2 = decryptionMethods.initalmethod.Body.Instructions[j].Operand as MethodDef;
					bool flag5 = methodDef2.Parameters.Count == 2 && methodDef2.HasReturnType;
					bool flag6 = flag5;
					if (flag6)
					{
						decryptionMethods.secondMethod = methodDef2;
					}
					break;
				}
			}
			for (int k = 0; k < decryptionMethods.secondMethod.Body.Instructions.Count; k++)
			{
				bool flag7 = decryptionMethods.secondMethod.Body.Instructions[k].OpCode == OpCodes.Callvirt && decryptionMethods.secondMethod.Body.Instructions[k].Operand is MethodDef;
				bool flag8 = flag7;
				if (flag8)
				{
					MethodDef methodDef3 = decryptionMethods.secondMethod.Body.Instructions[k].Operand as MethodDef;
					bool flag9 = methodDef3.Parameters.Count == 2 && methodDef3.HasReturnType;
					bool flag10 = flag9;
					if (flag10)
					{
						decryptionMethods.thirdMethod = methodDef3;
					}
				}
				bool flag11 = decryptionMethods.secondMethod.Body.Instructions[k].OpCode == OpCodes.Ldfld && decryptionMethods.secondMethod.Body.Instructions[k].Operand is FieldDef && decryptionMethods.secondMethod.Body.Instructions[k + 2].OpCode == OpCodes.Call && decryptionMethods.secondMethod.Body.Instructions[k + 2].Operand is MethodDef;
				bool flag12 = flag11;
				if (flag12)
				{
					MethodDef methodDef4 = decryptionMethods.secondMethod.Body.Instructions[k + 2].Operand as MethodDef;
					bool flag13 = methodDef4.Parameters.Count == 2 && methodDef4.HasReturnType;
					bool flag14 = flag13;
					if (flag14)
					{
						decryptionMethods.fourthMethod = methodDef4;
					}
					decryptionMethods.initalField = (FieldDef)decryptionMethods.secondMethod.Body.Instructions[k].Operand;
					break;
				}
			}
			decryptionMethods.initalType = (methods.Module.ResolveToken(33554456) as ITypeDefOrRef);
			for (int l = 0; l < decryptionMethods.fourthMethod.Body.Instructions.Count; l++)
			{
				bool flag15 = decryptionMethods.fourthMethod.Body.Instructions[l].IsLdloc() && decryptionMethods.fourthMethod.Body.Instructions[l + 1].IsLdarg() && decryptionMethods.fourthMethod.Body.Instructions[l + 2].OpCode == OpCodes.Callvirt && decryptionMethods.fourthMethod.Body.Instructions[l + 3].IsStloc();
				bool flag16 = flag15;
				if (flag16)
				{
					MethodDef methodDef5 = decryptionMethods.fourthMethod.Body.Instructions[l + 2].Operand as MethodDef;
					bool flag17 = methodDef5.Parameters.Count == 2 && methodDef5.HasReturnType;
					bool flag18 = flag17;
					if (flag18)
					{
						decryptionMethods.fifthMethod = methodDef5;
					}
					break;
				}
			}
			return decryptionMethods;
		}
		public static ITypeDefOrRef finder(LocalList locals)
		{
			foreach (Local local in locals)
			{
				bool flag = local.Type.ElementType == ElementType.Class;
				bool flag2 = flag;
				if (flag2)
				{
					return local.Type.ToTypeDefOrRef();
				}
			}
			return null;
		}
		public static void FindEncryptedMethods(ModuleDefMD module)
		{
			foreach (TypeDef typeDef in module.GetTypes())
			{
				foreach (MethodDef methodDef in typeDef.Methods)
				{
					bool flag = methodDef.MDToken.ToInt32() == 100663440;
					bool flag2 = flag;
					if (flag2)
					{
					}
					bool flag3 = !methodDef.HasBody;
					bool flag4 = !flag3;
					if (flag4)
					{
						bool flag5 = methodDef.Body.Instructions.Count > 70;
						bool flag6 = !flag5;
						if (flag6)
						{
							bool flag7 = !methodDef.Body.Instructions[0].IsLdcI4();
							bool flag8 = !flag7;
							if (flag8)
							{
								bool flag9 = methodDef.Body.Instructions.Count < 3;
								bool flag10 = !flag9;
								if (flag10)
								{
									bool flag11 = methodDef.Body.Instructions[methodDef.Body.Instructions.Count - 3].OpCode != OpCodes.Call;
									bool flag12 = !flag11;
									if (flag12)
									{
										bool flag13 = !VMDecryptor.checkMethod(methodDef);
										bool flag14 = !flag13;
										if (flag14)
										{
											int ldcI4Value = methodDef.Body.Instructions[0].GetLdcI4Value();
											EncryptedMethodDetails item = new EncryptedMethodDetails(methodDef, ldcI4Value);
											VMDecryptor.allEncMethods.Add(item);
										}
									}
								}
							}
						}
					}
				}
			}
		}
		private static bool checkMethod(MethodDef methods)
		{
			for (int i = 0; i < methods.Body.Instructions.Count; i++)
			{
				bool flag = methods.Body.Instructions[i].OpCode == OpCodes.Call && methods.Body.Instructions[i].Operand is MethodDef;
				bool flag2 = flag;
				if (flag2)
				{
					MethodDef methodDef = (MethodDef)methods.Body.Instructions[i].Operand;
					bool flag3 = methodDef.ReturnType.FullName.Contains("Int32");
					bool result;
					if (flag3)
					{
						result = false;
					}
					else
					{
						bool flag4 = methodDef.Body.Instructions.Count == 41 || methodDef.Body.Instructions.Count == 40;
						bool flag5 = flag4;
						if (!flag5)
						{
							goto IL_C7;
						}
						result = true;
					}
					return result;
				}
				IL_C7:;
			}
			return false;
		}
		public static List<EncryptedMethodDetails> allEncMethods = new List<EncryptedMethodDetails>();
	}
}
