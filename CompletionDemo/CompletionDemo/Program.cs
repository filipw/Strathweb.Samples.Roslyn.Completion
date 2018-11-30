using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host.Mef;
using System;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;

namespace CompletionDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            // default asseblies are
            //    "Microsoft.CodeAnalysis.Workspaces",
            //    "Microsoft.CodeAnalysis.CSharp.Workspaces",
            //    "Microsoft.CodeAnalysis.VisualBasic.Workspaces",
            //    "Microsoft.CodeAnalysis.Features",
            //    "Microsoft.CodeAnalysis.CSharp.Features",
            //    "Microsoft.CodeAnalysis.VisualBasic.Features"
            // http://source.roslyn.io/#Microsoft.CodeAnalysis.Workspaces/Workspace/Host/Mef/MefHostServices.cs,126
            var partTypes = MefHostServices.DefaultAssemblies
                    .SelectMany(x => x.GetTypes())
                    .ToArray();

            var compositionContext = new ContainerConfiguration()
                .WithParts(partTypes)
                .CreateContainer();

            var host = MefHostServices.Create(MefHostServices.DefaultAssemblies);

            var workspace = new AdhocWorkspace(host);

            Console.WriteLine("Hello World!");
        }
    }
}
