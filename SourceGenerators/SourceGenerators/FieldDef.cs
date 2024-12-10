using Microsoft.CodeAnalysis;

namespace SourceGenerators;

public struct FieldDef {
    public IFieldSymbol     Symbol;
    public INamedTypeSymbol ContainingClassSymbol;

    public FieldDef(IFieldSymbol symbol, INamedTypeSymbol containingClassSymbol) {
        Symbol                = symbol;
        ContainingClassSymbol = containingClassSymbol;
    }
}