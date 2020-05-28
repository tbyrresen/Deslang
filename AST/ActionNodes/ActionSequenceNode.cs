using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ActionNodes
{
    public abstract class ActionSequenceNode : AST
    {
        public ActionSequenceNode(SourceCodePosition pos) : base(pos)
        {
        }
    }
}
