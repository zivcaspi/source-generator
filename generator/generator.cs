using System.Diagnostics;

using Microsoft.CodeAnalysis;


namespace generator
{
    /// <summary>
    /// Trivial incremental source generator to test how source generation could be done.
    /// This particular generator looks for files added to the AdditionalFiles ItemGroup
    /// of the project that references this generator assembly, and adds the contents of
    /// each such file as a ConstStrings.NAME constant value.
    /// </summary>
    /// <remarks>
    /// See the following links on how to create incremental source generators:
    /// <list type="bullet">
    /// <item>https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md</item>
    /// <item>https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md</item>
    /// </list>
    /// </remarks>
    [Generator]
    public class Generator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // TODO: What I really want is a way to debug the code by paying attention to the generated code.

            // Find all files that end with .txt
            IncrementalValuesProvider<AdditionalText> textFiles
                = context.AdditionalTextsProvider
                .Where(file => IsTextFile(file))
                ;

            // Read all files
            IncrementalValuesProvider<(string Name, string Content)> namesAndContents
                = textFiles.Select((file, cancellationToken) => ReadTextFile(file, cancellationToken));

            // Generate the class that holds these values
            context.RegisterSourceOutput(namesAndContents, ProduceOutput);

            context.RegisterPostInitializationOutput(context1 =>
            {
                // TODO: We can add arbitrary files here
            });
        }

        private static void ProduceOutput(SourceProductionContext context, (string Name, string Content) nameAndContent)
        {
            // An example for generating a diagnostic message during build
            //var dd = new DiagnosticDescriptor("0000", "title", "bla", "category", DiagnosticSeverity.Info, isEnabledByDefault: true);
            //context.ReportDiagnostic(Diagnostic.Create(dd, location: null, messageArgs: null));

            context.AddSource($"ConstStrings.{nameAndContent.Name}.cs", $@"
public static partial class ConstStrings
{{
    public const string {nameAndContent.Name} = ""{nameAndContent.Content}"";
}}");

        }

        private static bool IsTextFile(AdditionalText file)
        {
            return file.Path.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);
        }

        private static (string Name, string Content) ReadTextFile(AdditionalText text, CancellationToken cancellationToken)
        {
            return (Name: Path.GetFileNameWithoutExtension(text.Path), Content: text.GetText(cancellationToken)!.ToString());
        }
    }
}