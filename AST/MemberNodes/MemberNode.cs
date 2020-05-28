using Deslang.AST.TypeNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.MemberNodes
{
    public abstract class MemberNode : AST
    {
        public TypeNode Type { get; set; }   // used when decorating the AST during contextual analysis phase 
        public bool IsVariable { get; set; }   // used by the type checker to denote if the member is a variable and thus assignable
        public bool IsInitialized { get; set; }   // used by the type checker to check for use of unassigned variables

        public MemberNode(SourceCodePosition pos) : base(pos)
        {
        }
    }
}
