using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Composition.Hosting;
using System.Linq;
using System.Threading.Tasks;

namespace CompletionDemo
{
    class Program
    {
        async static Task Main(string[] args)
        {
            // default assemblies are
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

            var code = @"using System;

            public class MyClass
            {
                public static void MyMethod(int value)
                {
                    Guid.
                }
            }";

            var projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "MyProject", "MyProject", LanguageNames.CSharp).
                WithMetadataReferences(new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) });
            var project = workspace.AddProject(projectInfo);
            var document = workspace.AddDocument(project.Id, "MyFile.cs", SourceText.From(code));

            await PrintCompletionResults(document, code.LastIndexOf("Guid.") + 5);

            Console.WriteLine();
            Console.WriteLine("*****");
            Console.WriteLine();
            
            // SCRIPT VERSION

            var scriptCode = "Guid.N";

            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                usings: new[] { "System" });
            var parseOptions = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Parse, SourceCodeKind.Script);
            var scriptProjectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Script","Script", LanguageNames.CSharp,
                    compilationOptions: compilationOptions, parseOptions: parseOptions, isSubmission: true)
                .WithMetadataReferences(new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) });

            var scriptProject = workspace.AddProject(scriptProjectInfo);
            var scriptDocumentInfo = DocumentInfo.Create(
                DocumentId.CreateNewId(scriptProject.Id), Guid.NewGuid() + ".csx",
                sourceCodeKind: SourceCodeKind.Script,
                loader: TextLoader.From(TextAndVersion.Create(SourceText.From(scriptCode), VersionStamp.Create())));
            var scriptDocument = workspace.AddDocument(scriptDocumentInfo);

            await PrintCompletionResults(scriptDocument, scriptCode.Length - 1);

            Console.ReadLine();
        }

        private static async Task PrintCompletionResults(Document document, int position)
        {
            var completionService = CompletionService.GetService(document);
            var results = await completionService.GetCompletionsAsync(document, position);

            foreach (var i in results.Items.Where(x => !x.Tags.Contains("Keyword")))
            {
                Console.WriteLine(i.DisplayText);

                foreach (var prop in i.Properties)
                {
                    Console.Write($"{prop.Key}:{prop.Value}  ");
                }

                Console.WriteLine();
                foreach (var tag in i.Tags)
                {
                    Console.Write($"{tag}  ");
                }

                Console.WriteLine();
                Console.WriteLine();
            }
        }
    }
}
