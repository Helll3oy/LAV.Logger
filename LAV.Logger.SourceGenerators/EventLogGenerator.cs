using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace System.Runtime.CompilerServices
{
    internal interface IsExternalInit { }
}

namespace LAV.Logger.SourceGenerators
{
    [Generator]
    public class EventLogGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var enumDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (s, _) => s is EnumDeclarationSyntax,
                    transform: GetSemanticModelForEnum)
                .Where(enumInfo => enumInfo?.HasAttribute == true);

            //context.RegisterSourceOutput(enumDeclarations, GenerateEventLogFiles!);

            context.RegisterPostInitializationOutput(ctx =>
            {
                // Generate the .resx content
                string resxContent = GenerateResxContent();

                // Add the .resx file to the compilation
                ctx.AddSource("MyGeneratedResources.resx", SourceText.From(resxContent, Encoding.UTF8));
            });
        }

        private string GenerateResxContent()
        {
            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <data name=""Greeting"" xml:space=""preserve"">
    <value>Hello, World!</value>
  </data>
</root>";
        }

        private static void GenerateEventLogFiles(
            SourceProductionContext context,
            EnumDeclarationInfo enumInfo)
        {
            GenerateCategoriesMcFile(context, enumInfo);
            //GenerateRegistrationCode(context, enumInfo);
            //GenerateBuildTargets(context, enumInfo);
            //GenerateImportProps(context, enumInfo);
        }

        private static void GenerateRegistrationCode(SourceProductionContext context, EnumDeclarationInfo enumInfo)
        {
            var registrationCode = $@"
public static class EventLogInitializer
{{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterEventSource()
    {{
        if (!System.Diagnostics.EventLog.SourceExists(""{enumInfo.SourceName}""))
        {{
            var sourceData = new System.Diagnostics.EventSourceCreationData(
                ""{enumInfo.SourceName}"",
                ""Application"")
            {{
                CategoryResourceFile = ""{enumInfo.SourceName}Categories.dll"",
                CategoryCount = {enumInfo.Members.Count},
                MessageResourceFile = ""{enumInfo.SourceName}Messages.dll""
            }};
            System.Diagnostics.EventLog.CreateEventSource(sourceData);
        }}
    }}
}}";

            context.AddSource($"{enumInfo.SourceName}Registration.g.cs", registrationCode);
        }

        private static void GenerateCategoriesMcFile(SourceProductionContext context, EnumDeclarationInfo enumInfo)
        {
            var mcBuilder = new StringBuilder(
@"
MessageIdTypedef=WORD
LanguageNames=(English=0x409:MSG00409)
"
            );

            foreach (var member in enumInfo.Members)
            {
                mcBuilder.AppendLine(
$@"
MessageId=0x{member.Id:X4}
SymbolicName={enumInfo.SymbolicName}_{member.Name}
Language=English
{member.DisplayName}
."
                );
            }

            mcBuilder.AppendLine("");

            context.AddSource($"{enumInfo.SourceName}Categories.mc", SourceText.From(mcBuilder.ToString(), Encoding.UTF8));
        }

        private static string FindWindowsSdkTool(string toolName)
        {
            // Implementation from previous examples
            return $@"$(WindowsSdkDir)bin\{WindowsSdkLocator.GetLatestWindowsSdkVersion()}\{GetMachineArchitecture()}\{toolName}";
        }

        private static string FindVCTool(string toolName)
        {
            // Implementation from previous examples
            return $@"$(VCToolsInstallDir)bin\Host{GetMachineArchitecture()}\{GetMachineArchitecture()}\{toolName}";
        }

        private static string GetMachineArchitecture()
        {
            return RuntimeInformation.OSArchitecture == Architecture.X64 ? "x64" : "x86";
        }

        private static void GenerateBuildTargets(SourceProductionContext context, EnumDeclarationInfo enumInfo)
        {
            string targetName = $"Compile{enumInfo.SourceName}EventMessages";

            string buildTarget = 
$"""
<Target Name="{targetName}" AfterTargets="CoreCompile">
    <PropertyGroup>
        <McToolPath>{FindWindowsSdkTool("mc.exe")}</McToolPath>
        <RcToolPath>{FindWindowsSdkTool("rc.exe")}</RcToolPath>
        <LinkToolPath>{FindVCTool("link.exe")}</LinkToolPath>
    </PropertyGroup>
          
    <Exec Command="&quot;$(McToolPath)&quot; -r $(IntermediateOutputPath) -h $(IntermediateOutputPath) $(IntermediateOutputPath){enumInfo.SourceName}Categories.mc" />
    <Exec Command="&quot;$(RcToolPath)&quot; /fo $(IntermediateOutputPath){enumInfo.SourceName}Categories.res $(IntermediateOutputPath){enumInfo.SourceName}Categories.rc" />
    <Exec Command="&quot;$(LinkToolPath)&quot; /DLL /NOENTRY /MACHINE:{GetMachineArchitecture()} /OUT:$(OutputPath){enumInfo.SourceName}Categories.dll $(IntermediateOutputPath){enumInfo.SourceName}Categories.res" />
          
    <ItemGroup>
        <Content Include="$(OutputPath){enumInfo.SourceName}Categories.dll">
            <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
        </Content>
    </ItemGroup>
</Target>
""";

            var extension = "targets";
            string fileName = $"{enumInfo.SourceName}Build.{extension}";
            context.AddSource(
                hintName: fileName, 
                sourceText: SourceText.From(buildTarget, Encoding.UTF8));

            // Generate a .targets file (no .cs extension)
            context.AddSource(
                hintName: "MyGeneratorBuild.targets",
                sourceText: SourceText.From("""<Target Name="CustomTarget"/>""", Encoding.UTF8)
            );
        }

        private static void GenerateImportProps(SourceProductionContext context, EnumDeclarationInfo enumInfo)
        {
            var props = $"""
<Project>
    <Import Project="$(MSBuildThisFileDirectory){enumInfo.SourceName}Build.targets" 
            Condition="Exists('$(MSBuildThisFileDirectory){enumInfo.SourceName}Build.targets')" />
</Project>
""";

            context.AddSource(
                hintName: $"{enumInfo.SourceName}Import.props",
                sourceText: SourceText.From(props, Encoding.UTF8));
        }

        private static EnumDeclarationInfo? GetSemanticModelForEnum(
            GeneratorSyntaxContext context,
            CancellationToken ct)
        {
            var enumDeclaration = (EnumDeclarationSyntax)context.Node;
            var enumSymbol = context.SemanticModel.GetDeclaredSymbol(enumDeclaration, ct);

            if (enumSymbol == null) return default;

            var sourceAttr = enumSymbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == nameof(EventLogSourceAttribute));

            if (sourceAttr == null) return default;

            var members = new List<CategoryMember>();

            foreach (var member in enumDeclaration.Members)
            {
                var memberSymbol = context.SemanticModel.GetDeclaredSymbol(member, ct) as IFieldSymbol;

                if (memberSymbol == null)
                    continue;

                var categoryAttr = memberSymbol.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.Name == nameof(EventCategoryAttribute));

                if (categoryAttr != null)
                {
                    //members.Add(new CategoryMember(
                    //    member.Identifier.ValueText,
                    //    (int)categoryAttr.ConstructorArguments[0].Value!,
                    //    (string)categoryAttr.ConstructorArguments[1].Value!
                    //));

                    members.Add(new CategoryMember(
                        memberSymbol.Name,
                        (int)categoryAttr.ConstructorArguments[0].Value!,
                        (string)categoryAttr.ConstructorArguments[1].Value!
                    ));
                }
            }

            //return new EnumDeclarationInfo(
            //    SourceName: (string)sourceAttr.ConstructorArguments[0].Value!,
            //    SymbolicName: enumDeclaration.Identifier.ValueText,
            //    Members: members
            //);

            return new EnumDeclarationInfo(
                SourceName: (string)sourceAttr.ConstructorArguments[0].Value!,
                SymbolicName: enumSymbol.Name,
                Members: members
            );
        }

        private sealed record EnumDeclarationInfo(
            string SourceName,
            string SymbolicName,
            IList<CategoryMember> Members)
        {
            public bool HasAttribute => Members.Count > 0;
        };

        private sealed record CategoryMember(
            string Name,
            int Id,
            string DisplayName
        );
    }
}
