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
            var runner = new ScriptRunner();

            if (string.IsNullOrEmpty(args[0])) {
                Console.WriteLine("You need a script file as an argument");
                return;
            }

            string script;
            try
            {
                script = File.ReadAllText(args[0]);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to read file. {e.Message}");
                return;
            }

            runner.Run(script);

        }
    }
}
