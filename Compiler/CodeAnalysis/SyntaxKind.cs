﻿namespace Compiler.CodeAnalysis
{
    public enum SyntaxKind
    {
        // Tokens
        InvalidToken,
        EndOfFileToken,
        WhitespaceToken,
        NumberToken,
        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        OpenParenthesisToken,
        CloseParenthesisToken,

        // Expressions
        LiteralExpression,
        BinaryExpression,
        ParenthesizedExpression
    }
}
