namespace Compiler.CodeAnalysis.Syntax
{
    public enum SyntaxKind
    {
        // Tokens
        InvalidToken,
        EndOfFileToken,
        WhitespaceToken,
        NumberToken,
        StringToken,
        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        EqualsToken,
        OpenParenthesisToken,
        CloseParenthesisToken,
        OpenBraceToken,
        CloseBraceToken,
        BangToken,
        EqualsEqualsToken,
        BangEqualsToken,
        LessToken,
        LessOrEqualsToken,
        GreaterToken,
        GreaterOrEqualsToken,
        AmpersandAmpersandToken,
        PipePipeToken,
        DotToken,
        DotDotToken,
        TildeToken,
        AmpersandToken,
        PipeToken,
        HatToken,
        ColonToken,
        CommaToken,
        IdentifierToken,

        // Keywords
        FalseKeyword,
        LetKeyword,
        TrueKeyword,
        VarKeyword,
        IfKeyword,
        ElseKeyword,
        WhileKeyword,
        ForKeyword,
        FunctionKeyword,
        InKeyword,

        //Nodes
        CompilationUnit,
        FunctionDeclaration,
        GlobalStatement,
        Parameter,
        ElseClause,
        TypeClause,

        // Statements
        BlockStatement,
        VariableDeclaration,
        ExpressionStatement,
        IfStatement,
        WhileStatement,
        ForStatement,

        // Expressions
        LiteralExpression,
        NameExpression,
        BinaryExpression,
        ParenthesizedExpression,
        UnaryExpression,
        AssignmentExpression,
        CallExpression,
    }
}
