using Microsoft.CodeAnalysis;

using System.Collections.Generic;
using System.Linq;

namespace generator
{

    [Generator]
    public class Generator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            System.Diagnostics.Debugger.Launch();
            System.Diagnostics.Debugger.Break();

            // Find all files that end with .txt
            IncrementalValuesProvider<AdditionalText> textFiles
                = context.AdditionalTextsProvider
                //.Where(file => file.Path.EndsWith(".txt"))
                ;

            // Read all files
            IncrementalValuesProvider<(string Name, string Content)> namesAndContents
                = textFiles.Select((text, cancellationToken) =>
                (Name: Path.GetFileNameWithoutExtension(text.Path), Content: text.GetText(cancellationToken)!.ToString()))
                ;

            // Generate the class that holds these values
            context.RegisterSourceOutput(namesAndContents, (spc, nameAndContent) =>
            {
                spc.AddSource($"ConstStrings.{nameAndContent.Name}", $@"
public static partial class ConstStrings
{{
    public const string {nameAndContent.Name} = ""{nameAndContent.Content}"";
}}");
            });
        }

    }
}