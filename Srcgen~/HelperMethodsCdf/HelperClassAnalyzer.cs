using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Fries.HelperClassCdf {
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class HelperClassAnalyzer : DiagnosticAnalyzer {
        private const string id = "HelperMethodCodeFix";
        private static readonly DiagnosticDescriptor RuleSucceed = new DiagnosticDescriptor(
            id, "Helper method can be extracted",
            "Method '{0}' is marked with [ToHelperMethod]",
            "Refactoring",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true
        );
        
        private static readonly DiagnosticDescriptor RuleFailed = new DiagnosticDescriptor(
            id, "Helper method cannot be extracted",
            "Method with keyword virtual, new, override, and abstract can not be extracted!",
            "Refactoring",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );
        
        
        
        public override void Initialize(AnalysisContext context) {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(methodAnalyzer, SyntaxKind.MethodDeclaration);
        }

        private static void methodAnalyzer(SyntaxNodeAnalysisContext context) {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;
            if (methodDeclaration.AttributeLists.Count == 0) return;
            
            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration);
            if (methodSymbol == null) return;

            var compilation = context.Compilation;
            object targetAttrSymbol = compilation.GetTypeByMetadataName("Fries.HelperClass.ToHelperMethod");
            if (targetAttrSymbol == null) return;

            bool hasAttribute = false;
            foreach (var attribute in methodSymbol.GetAttributes()) {
                if (attribute.AttributeClass == null) continue;
                if (!attribute.AttributeClass.Equals(targetAttrSymbol)) continue;
                hasAttribute = true;
                break;
            }
            if (!hasAttribute) return;

            context.ReportDiagnostic(Diagnostic.Create(RuleSucceed, methodDeclaration.Identifier.GetLocation(), methodSymbol.Name));
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleSucceed);
    }
}