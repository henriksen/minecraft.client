using Decent.Minecraft.Client;
using Decent.Minecraft.Client.Blocks;
using System;
using System.Numerics;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Minecraft.REPL
{
    public class ScriptRunner
    {
        MetadataReference[] _references;
        CSharpCompilationOptions _compilationOptions =
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

        public void Run(string script)
        {
            var syntaxTree = CreateSyntaxTree(script);

            if (_references == null)
            {
                _references = GenerateReferences();
            }

            Console.WriteLine("Compiling ...");
            CSharpCompilation compilation = CSharpCompilation.Create(
                Path.GetRandomFileName(),
                syntaxTrees: new[] { syntaxTree },
                references: _references,
                options: _compilationOptions);

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    Console.WriteLine("Compilation failed!");
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        Console.Error.WriteLine("\t{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }
                }
                else
                {
                    Console.WriteLine("Compilation successful! Now instantiating and executing the code ...");
                    ms.Seek(0, SeekOrigin.Begin);

                    Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
                    var type = assembly.GetType("Minecraft.REPL.Runner.Script");
                    var instance = assembly.CreateInstance("Minecraft.REPL.Runner.Script");
                    var meth = type.GetMember("Run").First() as MethodInfo;
                    meth.Invoke(instance, new[] { "localhost" });
                }
            }
        }

        private MetadataReference[] GenerateReferences()
        {
            var assemblyPath = Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location);
            Console.WriteLine($"AssemblyPath: {assemblyPath}");
            MetadataReference[] references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Private.CoreLib.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Numerics.Vectors.dll")),
                MetadataReference.CreateFromFile(typeof(JavaWorld).GetTypeInfo().Assembly.Location),
            };
            return references;
        }

        private SyntaxTree CreateSyntaxTree(string script)
        {
            var codeToCompile = GetCodeToCompile(script);

            Console.WriteLine("Parsing the code into the SyntaxTree");
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(codeToCompile);
            return syntaxTree;
        }

        private string GetCodeToCompile(string script)
        {
            string codeScaffoldTop = @"
                using System;
                using Decent.Minecraft.Client;
                using Decent.Minecraft.Client.Blocks;
                using System.Numerics;

                namespace Minecraft.REPL.Runner
                {
                    public class Script
                    {
                        public void Run(string serverAddress)
                        {
                        using (var mc = JavaWorld.Connect(serverAddress)) {
                            ";
            // world.PostToChat(message);
            string codeScaffoldBottom = @"
                        }
                        }
                    }
                }";

            string codeToCompile = codeScaffoldTop + script + codeScaffoldBottom;
            return codeToCompile;
        }
    }
}