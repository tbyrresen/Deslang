using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.Errors
{
    public class ErrorSummary
    {
        public List<SyntaxError> SyntaxErrors { get; }
        public List<SemanticError> SemanticErrors { get; }

        public ErrorSummary()
        {
            SyntaxErrors = new List<SyntaxError>();
            SemanticErrors = new List<SemanticError>();
        }

        public void AddError(SyntaxError error)
        {
            SyntaxErrors.Add(error);
        }

        public void AddError(SemanticError error)
        {
            SemanticErrors.Add(error);
        }
    }
}
