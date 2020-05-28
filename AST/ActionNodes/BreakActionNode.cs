using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deslang.AST.ActionNodes
{
    public class BreakActionNode : ActionNode
    {
        public BreakActionNode(SourceCodePosition pos) : base(pos)
        {
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
