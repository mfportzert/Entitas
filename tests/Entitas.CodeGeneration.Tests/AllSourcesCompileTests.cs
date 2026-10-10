using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Entitas.CodeGeneration.Tests;

// The snapshot tests only compare text. This one compiles what the generator produces for every
// sample source in TestSources against the real Entitas assembly, so an Entitas API change
// that breaks the generated code is caught here.
public class AllSourcesCompileTests
{
    // 'Feature' is only generated in the main assembly (visual debugging), so tests declare a stub for it
    const string FeatureStub = @"
public class Feature : Entitas.Systems { }
";

    public static IEnumerable<object[]> SourceNames() => typeof(TestSources)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.IsLiteral && f.FieldType == typeof(string))
        .Select(f => new object[] { f.Name });

    [Theory]
    [MemberData(nameof(SourceNames))]
    public void GeneratedCode_Compiles(string sourceName)
    {
        var source = (string)typeof(TestSources).GetField(sourceName)!.GetRawConstantValue()!;
        var tree = CSharpSyntaxTree.ParseText(source + FeatureStub);
        var compilation = CSharpCompilation.Create("Entitas.CodeGeneration-Tests",
            new[] { tree }, TestHelper.GetCompilationReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new EntitasGenerator());
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var errors = outputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Select(d => $"{d.Id} {d.GetMessage()}")
            .Distinct()
            .ToList();

        Assert.True(errors.Count == 0, sourceName + ":\n" + string.Join("\n", errors));
        Assert.Empty(diagnostics);
    }
}
