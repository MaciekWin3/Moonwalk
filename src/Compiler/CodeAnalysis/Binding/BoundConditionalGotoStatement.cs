﻿namespace Compiler.CodeAnalysis.Binding
{
    internal class BoundConditionalGotoStatement : BoundStatement
    {
        public BoundConditionalGotoStatement(LabelSymbol label, BoundExpression condition, bool jumpIfTrue = true)
        {
            Label = label;
            Condition = condition;
            JumpIfTrue = jumpIfTrue;
        }
        public override BoundNodeKind Kind => BoundNodeKind.ConditionalGotoStatement;

        public LabelSymbol Label { get; }
        public BoundExpression Condition { get; }
        public bool JumpIfTrue { get; }
    }
}
