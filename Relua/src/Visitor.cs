using System;
using Relua.AST;

namespace Relua {
    public interface IVisitor {
        void Visit(Variable node);
        void Visit(NilLiteral node);
        void Visit(VarargsLiteral node);
        void Visit(BoolLiteral node);
        void Visit(UnaryOp node);
        void Visit(BinaryOp node);
        void Visit(StringLiteral node);
        void Visit(NumberLiteral node);
        void Visit(LuaJITLongLiteral node);
        void Visit(TableAccess node);
        void Visit(FunctionCall node);
        void Visit(TableConstructor node);
        void Visit(TableConstructor.Entry node);
        void Visit(Break node);
        void Visit(Return node);
        void Visit(Block node);
        void Visit(ConditionalBlock node);
        void Visit(If node);
        void Visit(While node);
        void Visit(Repeat node);
        void Visit(FunctionDefinition node);
        void Visit(Assignment node);
        void Visit(NumericFor node);
        void Visit(GenericFor node);
        void Visit(Label node);
        void Visit(GoTo node);
    }

    public abstract class Visitor : IVisitor {
        public virtual void Visit(Variable node)
        {
        }

        public virtual void Visit(NilLiteral node)
        {
        }

        public virtual void Visit(VarargsLiteral node)
        {
        }

        public virtual void Visit(BoolLiteral node)
        {
        }

        public virtual void Visit(UnaryOp node)
        {
            node.Expression?.Accept(this);
        }

        public virtual void Visit(BinaryOp node)
        {
            node.Left?.Accept(this);
            node.Right?.Accept(this);
        }

        public virtual void Visit(StringLiteral node)
        {
        }

        public virtual void Visit(NumberLiteral node)
        {
        }

        public virtual void Visit(LuaJITLongLiteral node)
        {
        }

        public virtual void Visit(TableAccess node)
        {
            node.Table?.Accept(this);
            node.Index?.Accept(this);
        }

        public virtual void Visit(FunctionCall node)
        {
            if (node.Arguments.Count > 0 &&
                node.Function is TableAccess tableAccess &&
                tableAccess.Table == node.Arguments[0] &&
                tableAccess.Index is StringLiteral stringLiteral &&
                stringLiteral.Value.IsIdentifier()
            )
            {
                for (int i = 1; i < node.Arguments.Count; i++)
                {
                    node.Arguments[i].Accept(this);
                }
            }
            else
            {
                foreach (var nodeArgument in node.Arguments)
                {
                    nodeArgument.Accept(this);
                }
            }

            node.Function?.Accept(this);
        }

        public virtual void Visit(TableConstructor node)
        {
            foreach (var nodeEntry in node.Entries)
            {
                nodeEntry.Accept(this);
            }
        }

        public virtual void Visit(TableConstructor.Entry node)
        {
            node.Key?.Accept(this);
            node.Value?.Accept(this);
        }

        public virtual void Visit(Break node)
        {
        }

        public virtual void Visit(Return node)
        {
            foreach (var expression in node.Expressions)
            {
                expression.Accept(this);
            }
        }

        public virtual void Visit(Block node)
        {
            foreach (var statement in node.Statements)
            {
                statement.Accept(this);
            }
        }

        public virtual void Visit(ConditionalBlock node)
        {
            node.Condition?.Accept(this);
            node.Block?.Accept(this);
        }

        public virtual void Visit(If node)
        {
            node.MainIf?.Accept(this);
            foreach (var block in node.ElseIfs)
            {
                block.Accept(this);
            }

            node.Else?.Accept(this);
        }

        public virtual void Visit(While node)
        {
            node.Condition?.Accept(this);
            node.Block?.Accept(this);
        }

        public virtual void Visit(Repeat node)
        {
            node.Condition?.Accept(this);
            node.Block?.Accept(this);
        }

        public virtual void Visit(FunctionDefinition node)
        {
            node.Block?.Accept(this);
        }

        public virtual void Visit(Assignment node)
        {
            foreach (var nodeTarget in node.Targets)
            {
                nodeTarget.Accept(this);
            }

            foreach (var nodeValue in node.Values)
            {
                nodeValue.Accept(this);
            }
        }

        public virtual void Visit(NumericFor node)
        {
            node.StartPoint?.Accept(this);
            node.EndPoint?.Accept(this);
            node.Step?.Accept(this);
            node.Block?.Accept(this);
        }

        public virtual void Visit(GenericFor node)
        {
            foreach (var i in node.Iterator)
            {
                i.Accept(this);
            }
            node.Block?.Accept(this);
        }

        public virtual void Visit(Label node)
        {
        }

        public virtual void Visit(GoTo node)
        {
        }
    }
}
