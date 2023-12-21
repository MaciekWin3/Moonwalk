namespace Compiler.CodeAnalysis.Binding
{
    internal class BoundConditionalGotoStatement : BoundStatement
    {
        public BoundConditionalGotoStatement(LabelSymbol label, BoundExpression condition, bool jumpIfFalse = false)
        {
            Label = label;
            Condition = condition;
            JumpIfFalse = jumpIfFalse;
        }
        public override BoundNodeKind Kind => BoundNodeKind.ConditionalGotoStatement;

        public LabelSymbol Label { get; }
        public BoundExpression Condition { get; }
        public bool JumpIfFalse { get; }
    }
}
