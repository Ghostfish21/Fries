using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fries.EvtsysSrcgen {
    public class EvtListenerReceiver : ISyntaxReceiver {
        public List<MethodDeclarationSyntax> candidateMethods { get; } = new List<MethodDeclarationSyntax>();
        
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode) {
            if (!(syntaxNode is MethodDeclarationSyntax method)) return;
            if (method.AttributeLists.Count <= 0) return;
            
            foreach (var attribute in method.AttributeLists.SelectMany(a => a.Attributes)) {
                if (!attribute.Name.ToString().Contains("EvtListener")) continue;
                candidateMethods.Add(method);
                break;
            }
        }
    }
}