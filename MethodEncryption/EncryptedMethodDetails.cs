using System;
using dnlib.DotNet;

namespace BabelDeobfuscator.Protections.MethodEncryption
{
	public class EncryptedMethodDetails
	{
		public EncryptedMethodDetails(MethodDef origMethod, int EncryptedValue)
		{
			this.method = origMethod;
			this.encryptedValue = EncryptedValue;
		}
		public int encryptedValue;
		public MethodDef method;
	}
}
