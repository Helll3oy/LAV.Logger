using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;

namespace LAV.LoggerTemplateGenerator
{
    [Generator]
    public sealed partial class LogTemplateGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // Register a syntax receiver to find classes with [LogTemplate]
            context.RegisterForSyntaxNotifications(() => new LogTemplateSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not LogTemplateSyntaxReceiver receiver)
                return;

            foreach (var classSymbol in receiver.TargetClasses)
            {
                // Get the message from the [LogTemplate] attribute
                var attributeData = classSymbol.GetAttributes()[0];
                var message = attributeData.ConstructorArguments[0].Value?.ToString();

                // Generate the source code
                var source = $@"
namespace {classSymbol.ContainingNamespace}
{{
    public static partial class {classSymbol.Name}
    {{
        public const string LogTemplate = ""{message}"";
    }}
}}
";
                context.AddSource($"{classSymbol.Name}_LogTemplates.g.cs", SourceText.From(source, Encoding.UTF8));
            }
        }
    }

    // Syntax receiver to find classes decorated with [LogTemplate]
    public class LogTemplateSyntaxReceiver : ISyntaxContextReceiver
    {
        public List<INamedTypeSymbol> TargetClasses { get; } = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax classDecl &&
                classDecl.AttributeLists.Count > 0)
            {
                var symbol = context.SemanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
                if (symbol?.GetAttributes().Any(attr => attr.AttributeClass?.Name == nameof(LogTemplateAttribute)) == true)
                {
                    TargetClasses.Add(symbol);
                }
            }
        }
    }
}
