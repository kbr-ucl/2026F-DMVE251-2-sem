using System.Reflection;
using CodeToTest.Entity;
using NetArchTest.Rules;

// eller NUnit/MSTest

namespace TestProject;

public class ArchitectureTests
{
    [Fact]
    public void All_domain_entities_should_have_private_setters_on_public_properties()
    {
        var types = Types
            .InAssembly(typeof(Book).Assembly) //.InCurrentDomain()
            .That()
            .ResideInNamespace("CodeToTest.Entity")
            .GetTypes();

        var failingTypes = types
            .Where(type => !RequirePrivateSettersOnPublicProperties(type))
            .ToList();

        Assert.True(failingTypes.Count == 0,
            "The following types have public setters on public properties: " +
            string.Join(", ", failingTypes.Select(t => t.FullName)));
    }

    private static bool RequirePrivateSettersOnPublicProperties(Type type)
    {
        // Vælg public instance properties, der ikke er indexers
        var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.GetIndexParameters().Length == 0);

        foreach (var p in props)
        {
            // Krav: Property må gerne være read-only (ingen set),
            // ellers skal setter være non-public (typisk private).
            var setMethod = p.SetMethod;
            if (setMethod != null && setMethod.IsPublic) return false; // Fejl: offentlig setter
        }

        return true;
    }
}