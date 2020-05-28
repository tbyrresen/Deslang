using Deslang.AST.ActionNodes;
using Deslang.AST.ParameterNodes;
using Deslang.AST.TerminalNodes;
using Deslang.AST.TypeNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.DeclaringNodes
{
    public class MethodDeclaringNode : DeclaringNode
    {
        public IdentifierNode Identifier { get; }
        public TypeNode Type { get; }
        public ParameterSequenceNode Parameters { get; }
        public DeclaringSequenceNode VarDeclarings { get; }
        public ActionSequenceNode Actions { get; }
        public ReturnActionNode Return { get; }

        public MethodDeclaringNode(IdentifierNode ident, TypeNode type, ParameterSequenceNode parameters,
            DeclaringSequenceNode vars, ActionSequenceNode actions, ReturnActionNode ret, SourceCodePosition pos)
            : base(pos)
        {
            Identifier = ident;
            Type = type;
            Parameters = parameters;
            VarDeclarings = vars;
            Actions = actions;
            Return = ret;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
