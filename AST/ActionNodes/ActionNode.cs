using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ActionNodes
{
    public abstract class ActionNode : AST 
    {
        public ActionNode(SourceCodePosition pos) : base(pos)
        {
        }
    }
}
