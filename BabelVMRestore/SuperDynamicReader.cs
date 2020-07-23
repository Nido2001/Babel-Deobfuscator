using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.MD;
using dnlib.IO;

namespace Babel_Full_Unpacker.BabelVMRestore.Core
{
	public class SuperDynamicReader : MethodBodyReaderBase, ISignatureReaderHelper
	{
		public SuperDynamicReader(ModuleDef module, object obj) : this(module, obj, default(GenericParamContext))
		{
		}
		public SuperDynamicReader(ModuleDef module, object obj, GenericParamContext gpContext)
		{
			this.module = module;
			this.importer = new Importer(module, ImporterOptions.TryToUseDefs, gpContext);
			this.gpContext = gpContext;
			bool flag = obj == null;
			bool flag2 = flag;
			if (flag2)
			{
				throw new ArgumentNullException("obj");
			}
			Delegate @delegate = obj as Delegate;
			bool flag3 = @delegate != null;
			bool flag4 = flag3;
			if (flag4)
			{
				obj = @delegate.Method;
				bool flag5 = obj == null;
				bool flag6 = flag5;
				if (flag6)
				{
					throw new Exception("Delegate.Method == null");
				}
			}
			bool flag7 = obj.GetType().ToString() == "System.Reflection.Emit.DynamicMethod+RTDynamicMethod";
			bool flag8 = flag7;
			if (flag8)
			{
				obj = (SuperDynamicReader.rtdmOwnerFieldInfo.Read(obj) as DynamicMethod);
				bool flag9 = obj == null;
				bool flag10 = flag9;
				if (flag10)
				{
					throw new Exception("RTDynamicMethod.m_owner is null or invalid");
				}
			}
			bool flag11 = obj is DynamicMethod;
			bool flag12 = flag11;
			if (flag12)
			{
				object obj2 = obj;
				obj = SuperDynamicReader.dmResolverFieldInfo.Read(obj);
				bool flag13 = obj == null;
				bool flag14 = flag13;
				if (flag14)
				{
					obj = obj2;
					obj = SuperDynamicReader.methodDynamicInfo.Read(obj);
					bool flag15 = obj == null;
					bool flag16 = flag15;
					if (flag16)
					{
						throw new Exception("No resolver found");
					}
					this.SecondOption(obj);
					return;
				}
			}
			bool flag17 = obj.GetType().ToString() != "System.Reflection.Emit.DynamicResolver";
			bool flag18 = flag17;
			if (flag18)
			{
				throw new Exception("Couldn't find DynamicResolver");
			}
			byte[] array = SuperDynamicReader.rslvCodeFieldInfo.Read(obj) as byte[];
			bool flag19 = array == null;
			bool flag20 = flag19;
			if (flag20)
			{
				throw new Exception("No code");
			}
			this.codeSize = array.Length;
			MethodBase methodBase = SuperDynamicReader.rslvMethodFieldInfo.Read(obj) as MethodBase;
			bool flag21 = methodBase == null;
			bool flag22 = flag21;
			if (flag22)
			{
				throw new Exception("No method");
			}
			this.maxStack = (int)SuperDynamicReader.rslvMaxStackFieldInfo.Read(obj);
			object obj3 = SuperDynamicReader.rslvDynamicScopeFieldInfo.Read(obj);
			bool flag23 = obj3 == null;
			bool flag24 = flag23;
			if (flag24)
			{
				throw new Exception("No scope");
			}
			IList list = SuperDynamicReader.scopeTokensFieldInfo.Read(obj3) as IList;
			bool flag25 = list == null;
			bool flag26 = flag25;
			if (flag26)
			{
				throw new Exception("No tokens");
			}
			this.tokens = new List<object>(list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				this.tokens.Add(list[i]);
			}
			this.ehInfos = (IList<object>)SuperDynamicReader.rslvExceptionsFieldInfo.Read(obj);
			this.ehHeader = (SuperDynamicReader.rslvExceptionHeaderFieldInfo.Read(obj) as byte[]);
			this.UpdateLocals(SuperDynamicReader.rslvLocalsFieldInfo.Read(obj) as byte[]);
			this.reader = MemoryImageStream.Create(array);
			this.method = this.CreateMethodDef(methodBase);
			this.parameters = this.method.Parameters;
		}
		public static T GetFieldValue<T>(object obj, string fieldName)
		{
			bool flag = obj == null;
			bool flag2 = flag;
			if (flag2)
			{
				throw new ArgumentNullException("obj");
			}
			FieldInfo field = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			bool flag3 = field == null;
			bool flag4 = flag3;
			if (flag4)
			{
				throw new ArgumentException("fieldName", "No such field was found.");
			}
			bool flag5 = !typeof(T).IsAssignableFrom(field.FieldType);
			bool flag6 = flag5;
			if (flag6)
			{
				throw new InvalidOperationException("Field type and requested type are not compatible.");
			}
			return (T)((object)field.GetValue(obj));
		}
		private void SecondOption(object obj)
		{
			byte[] fieldValue = SuperDynamicReader.GetFieldValue<byte[]>(obj, "m_code");
			bool flag = fieldValue == null;
			bool flag2 = flag;
			if (flag2)
			{
				throw new Exception("No code");
			}
			this.codeSize = fieldValue.Length;
			MethodBase fieldValue2 = SuperDynamicReader.GetFieldValue<MethodBase>(obj, "m_method");
			bool flag3 = fieldValue2 == null;
			bool flag4 = flag3;
			if (flag4)
			{
				throw new Exception("No method");
			}
			this.maxStack = SuperDynamicReader.GetFieldValue<int>(obj, "m_maxStackSize");
			object fieldValue3 = SuperDynamicReader.GetFieldValue<object>(obj, "m_scope");
			bool flag5 = fieldValue3 == null;
			bool flag6 = flag5;
			if (flag6)
			{
				throw new Exception("No scope");
			}
			IList fieldValue4 = SuperDynamicReader.GetFieldValue<IList>(fieldValue3, "m_tokens");
			bool flag7 = fieldValue4 == null;
			bool flag8 = flag7;
			if (flag8)
			{
				throw new Exception("No tokens");
			}
			this.tokens = new List<object>(fieldValue4.Count);
			for (int i = 0; i < fieldValue4.Count; i++)
			{
				this.tokens.Add(fieldValue4[i]);
			}
			this.ehHeader = SuperDynamicReader.GetFieldValue<byte[]>(obj, "m_exceptions");
			this.UpdateLocals(SuperDynamicReader.GetFieldValue<byte[]>(obj, "m_localSignature"));
			this.reader = MemoryImageStream.Create(fieldValue);
			this.method = this.CreateMethodDef(fieldValue2);
			this.parameters = this.method.Parameters;
		}
		private static List<SuperDynamicReader.ExceptionInfo> CreateExceptionInfos(IList<object> ehInfos)
		{
			bool flag = ehInfos == null;
			bool flag2 = flag;
			List<SuperDynamicReader.ExceptionInfo> result;
			if (flag2)
			{
				result = new List<SuperDynamicReader.ExceptionInfo>();
			}
			else
			{
				List<SuperDynamicReader.ExceptionInfo> list = new List<SuperDynamicReader.ExceptionInfo>(ehInfos.Count);
				foreach (object instance in ehInfos)
				{
					SuperDynamicReader.ExceptionInfo item = new SuperDynamicReader.ExceptionInfo
					{
						CatchAddr = (int[])SuperDynamicReader.ehCatchAddrFieldInfo.Read(instance),
						CatchClass = (Type[])SuperDynamicReader.ehCatchClassFieldInfo.Read(instance),
						CatchEndAddr = (int[])SuperDynamicReader.ehCatchEndAddrFieldInfo.Read(instance),
						CurrentCatch = (int)SuperDynamicReader.ehCurrentCatchFieldInfo.Read(instance),
						Type = (int[])SuperDynamicReader.ehTypeFieldInfo.Read(instance),
						StartAddr = (int)SuperDynamicReader.ehStartAddrFieldInfo.Read(instance),
						EndAddr = (int)SuperDynamicReader.ehEndAddrFieldInfo.Read(instance),
						EndFinally = (int)SuperDynamicReader.ehEndFinallyFieldInfo.Read(instance)
					};
					list.Add(item);
				}
				result = list;
			}
			return result;
		}
		private void UpdateLocals(byte[] localsSig)
		{
			bool flag = localsSig == null || localsSig.Length == 0;
			bool flag2 = !flag;
			if (flag2)
			{
				LocalSig localSig = SignatureReader.ReadSig(this, this.module.CorLibTypes, localsSig, this.gpContext) as LocalSig;
				bool flag3 = localSig == null;
				bool flag4 = !flag3;
				if (flag4)
				{
					foreach (TypeSig typeSig in localSig.Locals)
					{
						this.locals.Add(new Local(typeSig));
					}
				}
			}
		}
		private MethodDef CreateMethodDef(MethodBase delMethod)
		{
			bool flag = true;
			MethodDefUser methodDefUser = new MethodDefUser();
			TypeSig returnType = this.GetReturnType(delMethod);
			List<TypeSig> parameters = this.GetParameters(delMethod);
			bool flag2 = flag;
			bool flag3 = flag2;
			if (flag3)
			{
				methodDefUser.Signature = MethodSig.CreateStatic(returnType, parameters.ToArray());
			}
			else
			{
				methodDefUser.Signature = MethodSig.CreateInstance(returnType, parameters.ToArray());
			}
			methodDefUser.Parameters.UpdateParameterTypes();
			methodDefUser.ImplAttributes = dnlib.DotNet.MethodImplAttributes.IL;
			methodDefUser.Attributes = dnlib.DotNet.MethodAttributes.PrivateScope;
			bool flag4 = flag;
			bool flag5 = flag4;
			if (flag5)
			{
				methodDefUser.Attributes |= dnlib.DotNet.MethodAttributes.Static;
			}
			return this.module.UpdateRowId<MethodDefUser>(methodDefUser);
		}
		private TypeSig GetReturnType(MethodBase mb)
		{
			MethodInfo methodInfo = mb as MethodInfo;
			bool flag = methodInfo != null;
			bool flag2 = flag;
			TypeSig result;
			if (flag2)
			{
				result = this.importer.ImportAsTypeSig(methodInfo.ReturnType);
			}
			else
			{
				result = this.module.CorLibTypes.Void;
			}
			return result;
		}
		private List<TypeSig> GetParameters(MethodBase delMethod)
		{
			List<TypeSig> list = new List<TypeSig>();
			foreach (ParameterInfo parameterInfo in delMethod.GetParameters())
			{
				list.Add(this.importer.ImportAsTypeSig(parameterInfo.ParameterType));
			}
			return list;
		}
		public bool Read()
		{
			base.ReadInstructionsNumBytes((uint)this.codeSize);
			this.CreateExceptionHandlers();
			return true;
		}
		private void CreateExceptionHandlers()
		{
			bool flag = this.ehHeader != null && this.ehHeader.Length != 0;
			bool flag2 = flag;
			if (flag2)
			{
				BinaryReader binaryReader = new BinaryReader(new MemoryStream(this.ehHeader));
				byte b = binaryReader.ReadByte();
				bool flag3 = (b & 64) == 0;
				bool flag4 = flag3;
				if (flag4)
				{
					int num = (int)((ushort)((binaryReader.ReadByte() - 2) / 12));
					binaryReader.ReadInt16();
					for (int i = 0; i < num; i++)
					{
						dnlib.DotNet.Emit.ExceptionHandler exceptionHandler = new dnlib.DotNet.Emit.ExceptionHandler();
						exceptionHandler.HandlerType = (ExceptionHandlerType)binaryReader.ReadInt16();
						int num2 = (int)binaryReader.ReadUInt16();
						exceptionHandler.TryStart = base.GetInstructionThrow((uint)num2);
						exceptionHandler.TryEnd = base.GetInstruction((uint)((int)binaryReader.ReadSByte() + num2));
						num2 = (int)binaryReader.ReadUInt16();
						exceptionHandler.HandlerStart = base.GetInstructionThrow((uint)num2);
						exceptionHandler.HandlerEnd = base.GetInstruction((uint)((int)binaryReader.ReadSByte() + num2));
						bool flag5 = exceptionHandler.HandlerType == ExceptionHandlerType.Catch;
						bool flag6 = flag5;
						if (flag6)
						{
							exceptionHandler.CatchType = (this.ReadToken(binaryReader.ReadUInt32()) as ITypeDefOrRef);
						}
						else
						{
							bool flag7 = exceptionHandler.HandlerType == ExceptionHandlerType.Filter;
							bool flag8 = flag7;
							if (flag8)
							{
								exceptionHandler.FilterStart = base.GetInstruction(binaryReader.ReadUInt32());
							}
							else
							{
								binaryReader.ReadUInt32();
							}
						}
						this.exceptionHandlers.Add(exceptionHandler);
					}
				}
				else
				{
					Stream baseStream = binaryReader.BaseStream;
					long position = baseStream.Position;
					baseStream.Position = position - 1L;
					int num3 = (int)((ushort)(((binaryReader.ReadUInt32() >> 8) - 4U) / 24U));
					for (int j = 0; j < num3; j++)
					{
						dnlib.DotNet.Emit.ExceptionHandler exceptionHandler2 = new dnlib.DotNet.Emit.ExceptionHandler();
						exceptionHandler2.HandlerType = (ExceptionHandlerType)binaryReader.ReadInt32();
						int num4 = binaryReader.ReadInt32();
						exceptionHandler2.TryStart = base.GetInstructionThrow((uint)num4);
						exceptionHandler2.TryEnd = base.GetInstruction((uint)(binaryReader.ReadInt32() + num4));
						num4 = binaryReader.ReadInt32();
						exceptionHandler2.HandlerStart = base.GetInstructionThrow((uint)num4);
						exceptionHandler2.HandlerEnd = base.GetInstruction((uint)(binaryReader.ReadInt32() + num4));
						bool flag9 = exceptionHandler2.HandlerType == ExceptionHandlerType.Catch;
						bool flag10 = flag9;
						if (flag10)
						{
							exceptionHandler2.CatchType = (this.ReadToken(binaryReader.ReadUInt32()) as ITypeDefOrRef);
						}
						else
						{
							bool flag11 = exceptionHandler2.HandlerType == ExceptionHandlerType.Filter;
							bool flag12 = flag11;
							if (flag12)
							{
								exceptionHandler2.FilterStart = base.GetInstruction(binaryReader.ReadUInt32());
							}
							else
							{
								binaryReader.ReadUInt32();
							}
						}
						this.exceptionHandlers.Add(exceptionHandler2);
					}
				}
			}
			else
			{
				bool flag13 = this.ehInfos != null;
				bool flag14 = flag13;
				if (flag14)
				{
					foreach (SuperDynamicReader.ExceptionInfo exceptionInfo in SuperDynamicReader.CreateExceptionInfos(this.ehInfos))
					{
						Instruction instructionThrow = base.GetInstructionThrow((uint)exceptionInfo.StartAddr);
						Instruction instruction = base.GetInstruction((uint)exceptionInfo.EndAddr);
						Instruction instruction2 = (exceptionInfo.EndFinally < 0) ? null : base.GetInstruction((uint)exceptionInfo.EndFinally);
						for (int k = 0; k < exceptionInfo.CurrentCatch; k++)
						{
							dnlib.DotNet.Emit.ExceptionHandler exceptionHandler3 = new dnlib.DotNet.Emit.ExceptionHandler();
							exceptionHandler3.HandlerType = (ExceptionHandlerType)exceptionInfo.Type[k];
							exceptionHandler3.TryStart = instructionThrow;
							exceptionHandler3.TryEnd = ((exceptionHandler3.HandlerType == ExceptionHandlerType.Finally) ? instruction2 : instruction);
							exceptionHandler3.FilterStart = null;
							exceptionHandler3.HandlerStart = base.GetInstructionThrow((uint)exceptionInfo.CatchAddr[k]);
							exceptionHandler3.HandlerEnd = base.GetInstruction((uint)exceptionInfo.CatchEndAddr[k]);
							exceptionHandler3.CatchType = this.importer.Import(exceptionInfo.CatchClass[k]);
							this.exceptionHandlers.Add(exceptionHandler3);
						}
					}
				}
			}
		}
		public MethodDef GetMethod()
		{
			bool initLocals = true;
			CilBody cilBody = new CilBody(initLocals, this.instructions, this.exceptionHandlers, this.locals);
			cilBody.MaxStack = (ushort)Math.Min(this.maxStack, 65535);
			this.instructions = null;
			this.exceptionHandlers = null;
			this.locals = null;
			this.method.Body = cilBody;
			return this.method;
		}
		protected override IField ReadInlineField(Instruction instr)
		{
			return this.ReadToken(this.reader.ReadUInt32()) as IField;
		}
		protected override IMethod ReadInlineMethod(Instruction instr)
		{
			return this.ReadToken(this.reader.ReadUInt32()) as IMethod;
		}
		protected override MethodSig ReadInlineSig(Instruction instr)
		{
			return this.ReadToken(this.reader.ReadUInt32()) as MethodSig;
		}
		protected override string ReadInlineString(Instruction instr)
		{
			return (this.ReadToken(this.reader.ReadUInt32()) as string) ?? string.Empty;
		}
		protected override ITokenOperand ReadInlineTok(Instruction instr)
		{
			return this.ReadToken(this.reader.ReadUInt32()) as ITokenOperand;
		}
		protected override ITypeDefOrRef ReadInlineType(Instruction instr)
		{
			return this.ReadToken(this.reader.ReadUInt32()) as ITypeDefOrRef;
		}
		private object ReadToken(uint token)
		{
			uint num = token & 16777215U;
			uint num2 = token >> 24;
			bool flag = num2 <= 10U;
			if (flag)
			{
				switch (num2)
				{
				case 2U:
					return this.ImportType(num);
				case 3U:
				case 5U:
					goto IL_A6;
				case 4U:
					return this.ImportField(num);
				case 6U:
					break;
				default:
				{
					bool flag2 = num2 != 10U;
					if (flag2)
					{
						goto IL_A6;
					}
					break;
				}
				}
				return this.ImportMethod(num);
			}
			bool flag3 = num2 == 17U;
			if (flag3)
			{
				return this.ImportSignature(num);
			}
			bool flag4 = num2 == 112U;
			if (flag4)
			{
				return this.Resolve(num) as string;
			}
			IL_A6:
			return null;
		}
		private IMethod ImportMethod(uint rid)
		{
			object obj = this.Resolve(rid);
			bool flag = obj == null;
			bool flag2 = flag;
			IMethod result;
			if (flag2)
			{
				result = null;
			}
			else
			{
				bool flag3 = obj is RuntimeMethodHandle;
				bool flag4 = flag3;
				if (flag4)
				{
					result = this.importer.Import(MethodBase.GetMethodFromHandle((RuntimeMethodHandle)obj));
				}
				else
				{
					bool flag5 = obj.GetType().ToString() == "System.Reflection.Emit.GenericMethodInfo";
					bool flag6 = flag5;
					if (flag6)
					{
						RuntimeTypeHandle declaringType = (RuntimeTypeHandle)SuperDynamicReader.gmiContextFieldInfo.Read(obj);
						MethodBase methodFromHandle = MethodBase.GetMethodFromHandle((RuntimeMethodHandle)SuperDynamicReader.gmiMethodHandleFieldInfo.Read(obj), declaringType);
						result = this.importer.Import(methodFromHandle);
					}
					else
					{
						bool flag7 = obj.GetType().ToString() == "System.Reflection.Emit.VarArgMethod";
						bool flag8 = flag7;
						if (flag8)
						{
							MethodInfo varArgMethod = this.GetVarArgMethod(obj);
							bool flag9 = !(varArgMethod is DynamicMethod);
							bool flag10 = flag9;
							if (flag10)
							{
								return this.importer.Import(varArgMethod);
							}
							obj = varArgMethod;
						}
						DynamicMethod left = obj as DynamicMethod;
						bool flag11 = left != null;
						bool flag12 = flag11;
						if (flag12)
						{
							throw new Exception("DynamicMethod calls another DynamicMethod");
						}
						result = null;
					}
				}
			}
			return result;
		}
		private MethodInfo GetVarArgMethod(object obj)
		{
			bool flag = SuperDynamicReader.vamDynamicMethodFieldInfo.Exists(obj);
			bool flag2 = flag;
			MethodInfo result;
			if (flag2)
			{
				MethodInfo methodInfo = SuperDynamicReader.vamMethodFieldInfo.Read(obj) as MethodInfo;
				DynamicMethod dynamicMethod = SuperDynamicReader.vamDynamicMethodFieldInfo.Read(obj) as DynamicMethod;
				result = (dynamicMethod ?? methodInfo);
			}
			else
			{
				result = (SuperDynamicReader.vamMethodFieldInfo.Read(obj) as MethodInfo);
			}
			return result;
		}
		private IField ImportField(uint rid)
		{
			object obj = this.Resolve(rid);
			bool flag = obj == null;
			bool flag2 = flag;
			IField result;
			if (flag2)
			{
				result = null;
			}
			else
			{
				bool flag3 = obj is RuntimeFieldHandle;
				bool flag4 = flag3;
				if (flag4)
				{
					result = this.importer.Import(FieldInfo.GetFieldFromHandle((RuntimeFieldHandle)obj));
				}
				else
				{
					bool flag5 = obj.GetType().ToString() == "System.Reflection.Emit.GenericFieldInfo";
					bool flag6 = flag5;
					if (flag6)
					{
						RuntimeTypeHandle declaringType = (RuntimeTypeHandle)SuperDynamicReader.gfiContextFieldInfo.Read(obj);
						FieldInfo fieldFromHandle = FieldInfo.GetFieldFromHandle((RuntimeFieldHandle)SuperDynamicReader.gfiFieldHandleFieldInfo.Read(obj), declaringType);
						result = this.importer.Import(fieldFromHandle);
					}
					else
					{
						result = null;
					}
				}
			}
			return result;
		}
		private ITypeDefOrRef ImportType(uint rid)
		{
			object obj = this.Resolve(rid);
			bool flag = obj is RuntimeTypeHandle;
			bool flag2 = flag;
			ITypeDefOrRef result;
			if (flag2)
			{
				result = this.importer.Import(Type.GetTypeFromHandle((RuntimeTypeHandle)obj));
			}
			else
			{
				result = null;
			}
			return result;
		}
		private CallingConventionSig ImportSignature(uint rid)
		{
			byte[] array = this.Resolve(rid) as byte[];
			bool flag = array == null;
			bool flag2 = flag;
			CallingConventionSig result;
			if (flag2)
			{
				result = null;
			}
			else
			{
				result = SignatureReader.ReadSig(this, this.module.CorLibTypes, array, this.gpContext);
			}
			return result;
		}
		private object Resolve(uint index)
		{
			bool flag = index >= (uint)this.tokens.Count;
			bool flag2 = flag;
			object result;
			if (flag2)
			{
				result = null;
			}
			else
			{
				result = this.tokens[(int)index];
			}
			return result;
		}
		ITypeDefOrRef ISignatureReaderHelper.ResolveTypeDefOrRef(uint codedToken, GenericParamContext gpContext)
		{
			uint token;
			bool flag = !CodedToken.TypeDefOrRef.Decode(codedToken, out token);
			bool flag2 = flag;
			ITypeDefOrRef result;
			if (flag2)
			{
				result = null;
			}
			else
			{
				uint rid = MDToken.ToRID(token);
				Table table = MDToken.ToTable(token);
				bool flag3 = table - Table.TypeRef > 1 && table != Table.TypeSpec;
				if (flag3)
				{
					result = null;
				}
				else
				{
					result = this.ImportType(rid);
				}
			}
			return result;
		}
		TypeSig ISignatureReaderHelper.ConvertRTInternalAddress(IntPtr address)
		{
			return this.importer.ImportAsTypeSig(MethodTableToTypeConverter.Convert(address));
		}
		private static readonly SuperDynamicReader.ReflectionFieldInfo rtdmOwnerFieldInfo = new SuperDynamicReader.ReflectionFieldInfo("m_owner");
		private static readonly SuperDynamicReader.ReflectionFieldInfo dmResolverFieldInfo = new SuperDynamicReader.ReflectionFieldInfo("m_resolver");
		private static readonly SuperDynamicReader.ReflectionFieldInfo rslvCodeFieldInfo = new SuperDynamicReader.ReflectionFieldInfo("m_code");
		private static readonly SuperDynamicReader.ReflectionFieldInfo rslvDynamicScopeFieldInfo = new SuperDynamicReader.ReflectionFieldInfo("m_scope");
		private static readonly SuperDynamicReader.ReflectionFieldInfo rslvMethodFieldInfo = new SuperDynamicReader.ReflectionFieldInfo("m_method");
		private static readonly SuperDynamicReader.ReflectionFieldInfo rslvLocalsFieldInfo = new SuperDynamicReader.ReflectionFieldInfo("m_localSignature");
		private static readonly SuperDynamicReader.ReflectionFieldInfo rslvMaxStackFieldInfo = new SuperDynamicReader.ReflectionFieldInfo("m_stackSize");
		private static readonly SuperDynamicReader.ReflectionFieldInfo rslvExceptionsFieldInfo = new SuperDynamicReader.ReflectionFieldInfo("m_exceptions");
		private static readonly SuperDynamicReader.ReflectionFieldInfo rslvExceptionHeaderFieldInfo = new SuperDynamicReader.ReflectionFieldInfo("m_exceptionHeader");
		private static readonly SuperDynamicReader.ReflectionFieldInfo scopeTokensFieldInfo = new SuperDynamicReader.ReflectionFieldInfo("m_tokens");
		private static readonly SuperDynamicReader.ReflectionFieldInfo gfiFieldHandleFieldInfo = new SuperDynamicReader.ReflectionFieldInfo("m_field", "m_fieldHandle");
		private static readonly SuperDynamicReader.ReflectionFieldInfo gfiContextFieldInfo = new SuperDynamicReader.ReflectionFieldInfo("m_context");
		private static readonly SuperDynamicReader.ReflectionFieldInfo gmiMethodHandleFieldInfo = new SuperDynamicReader.ReflectionFieldInfo("m_method", "m_methodHandle");
		private static readonly SuperDynamicReader.ReflectionFieldInfo gmiContextFieldInfo = new SuperDynamicReader.ReflectionFieldInfo("m_context");
		private static readonly SuperDynamicReader.ReflectionFieldInfo ehCatchAddrFieldInfo = new SuperDynamicReader.ReflectionFieldInfo("m_catchAddr");
		private static readonly SuperDynamicReader.ReflectionFieldInfo ehCatchClassFieldInfo = new SuperDynamicReader.ReflectionFieldInfo("m_catchClass");
		private static readonly SuperDynamicReader.ReflectionFieldInfo ehCatchEndAddrFieldInfo = new SuperDynamicReader.ReflectionFieldInfo("m_catchEndAddr");
		private static readonly SuperDynamicReader.ReflectionFieldInfo ehCurrentCatchFieldInfo = new SuperDynamicReader.ReflectionFieldInfo("m_currentCatch");
		private static readonly SuperDynamicReader.ReflectionFieldInfo ehTypeFieldInfo = new SuperDynamicReader.ReflectionFieldInfo("m_type");
		private static readonly SuperDynamicReader.ReflectionFieldInfo ehStartAddrFieldInfo = new SuperDynamicReader.ReflectionFieldInfo("m_startAddr");
		private static readonly SuperDynamicReader.ReflectionFieldInfo ehEndAddrFieldInfo = new SuperDynamicReader.ReflectionFieldInfo("m_endAddr");
		private static readonly SuperDynamicReader.ReflectionFieldInfo ehEndFinallyFieldInfo = new SuperDynamicReader.ReflectionFieldInfo("m_endFinally");
		private static readonly SuperDynamicReader.ReflectionFieldInfo vamMethodFieldInfo = new SuperDynamicReader.ReflectionFieldInfo("m_method");
		private static readonly SuperDynamicReader.ReflectionFieldInfo vamDynamicMethodFieldInfo = new SuperDynamicReader.ReflectionFieldInfo("m_dynamicMethod");
		private static readonly SuperDynamicReader.ReflectionFieldInfo methodDynamicInfo = new SuperDynamicReader.ReflectionFieldInfo("m_DynamicILInfo");
		private ModuleDef module;
		private Importer importer;
		private GenericParamContext gpContext;
		private MethodDef method;
		private int codeSize;
		private int maxStack;
		private List<object> tokens;
		private IList<object> ehInfos;
		private byte[] ehHeader;
		private class ReflectionFieldInfo
		{
			public ReflectionFieldInfo(string fieldName)
			{
				this.fieldName1 = fieldName;
			}
			public ReflectionFieldInfo(string fieldName1, string fieldName2)
			{
				this.fieldName1 = fieldName1;
				this.fieldName2 = fieldName2;
			}
			public object Read(object instance)
			{
				bool flag = this.fieldInfo == null;
				bool flag2 = flag;
				if (flag2)
				{
					this.InitializeField(instance.GetType());
				}
				bool flag3 = this.fieldInfo == null;
				bool flag4 = flag3;
				if (flag4)
				{
					throw new Exception(string.Format("Couldn't find field '{0}' or '{1}'", this.fieldName1, this.fieldName2));
				}
				return this.fieldInfo.GetValue(instance);
			}
			public object Read(object instance, Type type)
			{
				bool flag = this.fieldInfo == null;
				bool flag2 = flag;
				if (flag2)
				{
					this.InitializeField(type);
				}
				bool flag3 = this.fieldInfo == null;
				bool flag4 = flag3;
				if (flag4)
				{
					throw new Exception(string.Format("Couldn't find field '{0}' or '{1}'", this.fieldName1, this.fieldName2));
				}
				return this.fieldInfo.GetValue(instance);
			}
			public bool Exists(object instance)
			{
				this.InitializeField(instance.GetType());
				return this.fieldInfo != null;
			}
			private void InitializeField(Type type)
			{
				bool flag = this.fieldInfo != null;
				bool flag2 = !flag;
				if (flag2)
				{
					BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
					this.fieldInfo = type.GetField(this.fieldName1, bindingAttr);
					bool flag3 = this.fieldInfo == null && this.fieldName2 != null;
					bool flag4 = flag3;
					if (flag4)
					{
						this.fieldInfo = type.GetField(this.fieldName2, bindingAttr);
					}
				}
			}
			private FieldInfo fieldInfo;
			private readonly string fieldName1;
			private readonly string fieldName2;
		}
		private class ExceptionInfo
		{
			public int[] CatchAddr;
			public Type[] CatchClass;
			public int[] CatchEndAddr;
			public int CurrentCatch;
			public int[] Type;
			public int StartAddr;
			public int EndAddr;
			public int EndFinally;
		}
	}
}
