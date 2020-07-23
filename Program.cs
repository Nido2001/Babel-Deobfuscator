using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BabelDeobfuscator.Protections;
using BabelDeobfuscator.Protections.Constants;
using BabelDeobfuscator.Protections.ControlFlow;
using BabelDeobfuscator.Protections.MethodEncryption;
using BabelDeobfuscator.Protections.ProxyCalls;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;

namespace BabelDeobfuscator
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Console.WriteLine("Name of Executable to Unpack : ");
			var path = Console.ReadLine();
			if (path == string.Empty)
				return;
			if (path != null && path.StartsWith("\"") && path[path.Length - 1] == '"')
				path = path.Substring(1, path.Length - 2);

			if (!File.Exists(path))
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("[!] File not found");
				Console.WriteLine("[!] Press key to exit...");
				Console.Read();
				return;
			}
			Console.WriteLine("Babel Unpacker running..");
			ModuleDefMD moduleDefMD = ModuleDefMD.Load(path);
			AssemblyResolver assemblyResolver = new AssemblyResolver();
			ModuleContext moduleContext = new ModuleContext(assemblyResolver);
			assemblyResolver.DefaultModuleContext = moduleContext;
			assemblyResolver.EnableTypeDefCache = true;
			moduleDefMD.Location = path;
			List<AssemblyRef> list = moduleDefMD.GetAssemblyRefs().ToList<AssemblyRef>();
			moduleDefMD.Context = moduleContext;
			foreach (AssemblyRef assemblyRef in list)
			{
				if (assemblyRef != null)
				{
					AssemblyDef assemblyDef = assemblyResolver.Resolve(assemblyRef.FullName, moduleDefMD);
					moduleDefMD.Context.AssemblyResolver.AddToCache(assemblyDef);
				}
			}
			Program.asm = Assembly.LoadFrom(path);
			De4Dot.Cflow(moduleDefMD);
			VMDecryptor.run(moduleDefMD, Program.asm);
			Delegates.CleanDelegates(moduleDefMD);
			Ints.CleanInts(moduleDefMD);
			Ints.CleanFloats(moduleDefMD);
			Ints.CleanDouble(moduleDefMD);
			Strings.CleanStringMethodOne(moduleDefMD);
			Strings.CleanStringMethodTwo(moduleDefMD);
			De4Dot.Cflow(moduleDefMD);
			Ints.CleanInts(moduleDefMD);
			Ints.CleanFloats(moduleDefMD);
			Ints.CleanDouble(moduleDefMD);
			Strings.CleanStringMethodOne(moduleDefMD);
			Strings.CleanStringMethodTwo(moduleDefMD);
			De4Dot.Cflow(moduleDefMD);
			AntiTamper.AntiTamp(moduleDefMD);
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Unpack Complete !");
			moduleDefMD.Write("cleaned_" + path, new ModuleWriterOptions(moduleDefMD));
		}
		public static Assembly asm;
	}
}
