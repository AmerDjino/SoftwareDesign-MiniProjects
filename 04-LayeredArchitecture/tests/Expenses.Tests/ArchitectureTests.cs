using System.Reflection;
using Expenses.Business;
using Expenses.DataAccess;
using Expenses.Presentation;

namespace Expenses.Tests;

public sealed class ArchitectureTests
{
    private static readonly Dictionary<string, Assembly> Layers = new()
    {
        ["Presentation"] = typeof(ExpenseEndpoints).Assembly,
        ["Business"] = typeof(ExpenseService).Assembly,
        ["DataAccess"] = typeof(IExpenseRepository).Assembly,
    };

    [Fact]
    public void Presentation_DoesNotReferenceDataAccess()
    {
        var references = ReferencedLayers("Presentation");

        Assert.Contains("Business", references);
        Assert.DoesNotContain("DataAccess", references);
    }

    [Theory]
    [InlineData("Business", "Presentation")]
    [InlineData("DataAccess", "Business")]
    [InlineData("DataAccess", "Presentation")]
    public void Layers_DoNotReferenceLayersAboveThem(string layer, string layerAbove)
    {
        var references = ReferencedLayers(layer);

        Assert.DoesNotContain(layerAbove, references);
    }

    private static List<string> ReferencedLayers(string layer)
    {
        var referencedNames = Layers[layer].GetReferencedAssemblies().Select(reference => reference.Name).ToList();
        return [.. Layers.Keys.Where(other => referencedNames.Contains(Layers[other].GetName().Name))];
    }
}
