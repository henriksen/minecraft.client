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
    public class Program
    {
        public static void Main(string[] args)
        {

            string codeToCompile = @"
            using System;
            using Decent.Minecraft.Client;
            using Decent.Minecraft.Client.Blocks;
            namespace RoslynCompileSample
            {
                public class Script
                {
                    public void Run(string message)
                    {
                       using (var world = JavaWorld.Connect(""localhost"")) {
                            world.PostToChat(message);
                       }
                    }
                }
            }";
            var assemblyPath = Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location);

            // string smallScript = @"using System;
            //             using Decent.Minecraft.Client;
            //             using Decent.Minecraft.Client.Blocks;
            //             using (var world = JavaWorld.Connect(""localhost"")) {
            //                 Console.WriteLine(message);
            //            }";
            // var assemblyPath = Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location);
                       
            // object result2 = CSharpScript.EvaluateAsync(smallScript, 
                
            //      ScriptOptions.Default.WithReferences(Path.Combine(assemblyPath, "mscorlib.dll"))
            //         .WithReferences(Path.Combine(assemblyPath, "System.Runtime.dll"))
            //         .WithReferences(Path.Combine(assemblyPath, "System.Private.CoreLib.dll"))
            //         .WithReferences(typeof(JavaWorld).GetTypeInfo().Assembly)
            
            //             ).GetAwaiter().GetResult();
            // Console.WriteLine(result2);
            // return;

            Console.WriteLine("Parsing the code into the SyntaxTree");
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(codeToCompile);
            
            string assemblyName = Path.GetRandomFileName();

            Console.WriteLine($"AssemblyPath: {assemblyPath}");
            MetadataReference[] references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Private.CoreLib.dll")),
                MetadataReference.CreateFromFile(typeof(JavaWorld).GetTypeInfo().Assembly.Location)
            };

            Console.WriteLine("Compiling ...");
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    
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
                    var type= assembly.GetType("RoslynCompileSample.Script");
                    var instance = assembly.CreateInstance("RoslynCompileSample.Script");
                    var meth = type.GetMember("Run").First() as MethodInfo;
                    meth.Invoke(instance, new [] {"Hello from Roslyn"});
                }
            }
            return ;

            if (args.Length != 1)
            {

                Console.WriteLine(@"Minecraft.Scratch - a console app using Minecraft.Client

Modify the source if you want to make it do anything useful.

usage:

minecraft.client <raspberry pi ip>");
                return;
            }
            using (var world = JavaWorld.Connect(args[0]))
            {
                world.PostToChat("Hello from C# and .NET Core!");
                var playerPosition = world.Player.GetPosition();
                world.PostToChat($"Player is at {playerPosition}");
                var blockUnderPlayer = world.GetBlock(playerPosition - new Vector3(0, 1, 0));
                world.PostToChat($"Block under player is {blockUnderPlayer.Type}.");
                var wood = new Wood(Wood.Species.Oak, Orientation.UpDown);
                world.SetBlock(wood, playerPosition + new Vector3(0, 0, 1));
            }
        }
    }
}
