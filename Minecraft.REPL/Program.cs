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

                var script = @"
                world.PostToChat(""Hello from C# and .NET Core via Roslyn!"");
                var playerPosition = world.Player.GetPosition();
                world.PostToChat($""Player is at {playerPosition}"");
                var blockUnderPlayer = world.GetBlock(playerPosition - new Vector3(0, 1, 0));
                world.PostToChat($""Block under player is {blockUnderPlayer.Type}."");
                var wood = new Wood(Wood.Species.Oak, Orientation.UpDown);
                world.SetBlock(wood, playerPosition + new Vector3(0, 0, 1));
                ";

                var runner = new ScriptRunner();
                runner.Run(script);
        }
    }
}
