﻿using Core.CodeAnalysis.Syntax.Expressions;
using Core.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace Core.CodeAnalysis.Syntax
{
    // 1:07:48
    internal sealed class Parser
    {
        private readonly DiagnosticBag diagnostics = new();
        private readonly SourceText text;

        private readonly SyntaxTree syntaxTree;
        private readonly ImmutableArray<SyntaxToken> tokens;
        private int position;
        public DiagnosticBag Diagnostics => diagnostics;
        public Parser(SyntaxTree syntaxTree)
        {
            var tokens = new List<SyntaxToken>();
            var lexer = new Lexer(syntaxTree);
            SyntaxToken token;
            do
            {
                token = lexer.Lex();
                if (token.Kind != SyntaxKind.WhitespaceToken && token.Kind != SyntaxKind.InvalidToken)
                {
                    tokens.Add(token);
                }
            } while (token.Kind != SyntaxKind.EndOfFileToken);

            this.syntaxTree = syntaxTree;
            text = syntaxTree.Text;
            this.tokens = tokens.ToImmutableArray();
            diagnostics.AddRange(lexer.Diagnostics);
        }

        private SyntaxToken Peek(int offset)
        {
            int index = position + offset;
            if (index >= tokens.Length)
            {
                return tokens[^1];
            }

            return tokens[index];
        }

        private SyntaxToken Current => Peek(0);
        private SyntaxToken NextToken()
        {
            var current = Current;
            position++;
            return current;
        }

        private SyntaxToken MatchToken(SyntaxKind kind)
        {
            if (Current.Kind == kind)
            {
                return NextToken();
            }

            diagnostics.ReportUnexpectedToken(Current.Location, Current.Kind, kind);
            return new SyntaxToken(syntaxTree, kind, Current.Position, null!, null!);
        }

        public CompilationUnitSyntax ParseCompilationUnit()
        {
            var members = ParseMembers();
            var endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
            return new CompilationUnitSyntax(syntaxTree, members, endOfFileToken);
        }

        private ImmutableArray<MemberSyntax> ParseMembers()
        {
            var members = ImmutableArray.CreateBuilder<MemberSyntax>();

            while (Current.Kind != SyntaxKind.EndOfFileToken)
            {
                var startToken = Current;

                var member = ParseMember();
                members.Add(member);

                // If ParseMember() did not consume any tokens,
                // we need to skip the current token and continue
                // in order to avoid an infinite loop.
                //
                // We don't need to report an error, because we'll
                // already tried to parse an expression statement
                // and reported one.
                if (Current == startToken)
                {
                    NextToken();
                }
            }

            return members.ToImmutable();
        }

        private MemberSyntax ParseMember()
        {
            if (Current.Kind == SyntaxKind.FunctionKeyword)
            {
                return ParseFunctionDeclaration();
            }

            return ParseGlobalStatement();
        }

        private MemberSyntax ParseFunctionDeclaration()
        {
            var functionKeyword = MatchToken(SyntaxKind.FunctionKeyword);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var openParenthesisToken = MatchToken(SyntaxKind.OpenParenthesisToken);
            var parameters = ParseParameterList();
            var closeParenthesisToken = MatchToken(SyntaxKind.CloseParenthesisToken);
            var type = ParseOptionalTypeClause();
            var body = ParseBlockStatement();
            return new FunctionDeclarationSyntax(syntaxTree, functionKeyword, identifier, openParenthesisToken, parameters, closeParenthesisToken, type, body);
        }

        private SeparatedSyntaxList<ParameterSyntax> ParseParameterList()
        {
            var nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();

            var parseNextParameter = true;

            while (parseNextParameter && Current.Kind != SyntaxKind.CloseParenthesisToken && Current.Kind != SyntaxKind.EndOfFileToken)
            {
                var parameter = ParseParameter();
                nodesAndSeparators.Add(parameter);

                if (Current.Kind == SyntaxKind.CommaToken)
                {
                    var comma = MatchToken(SyntaxKind.CommaToken);
                    nodesAndSeparators.Add(comma);
                }
                else
                {

                    parseNextParameter = false;
                }
            }

            return new SeparatedSyntaxList<ParameterSyntax>(nodesAndSeparators.ToImmutable());
        }

        private ParameterSyntax ParseParameter()
        {
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var type = ParseTypeClause();
            return new ParameterSyntax(syntaxTree, identifier, type);
        }

        private MemberSyntax ParseGlobalStatement()
        {
            var statement = ParseStatement();
            return new GlobalStatementSyntax(syntaxTree, statement);
        }

        private StatementSyntax ParseStatement()
        {
            return Current.Kind switch
            {
                SyntaxKind.OpenBraceToken => ParseBlockStatement(),
                SyntaxKind.LetKeyword => ParseVariableDeclaration(),
                SyntaxKind.VarKeyword => ParseVariableDeclaration(),
                SyntaxKind.IfKeyword => ParseIfStatement(),
                SyntaxKind.WhileKeyword => ParseWhileStatement(),
                SyntaxKind.ForKeyword => ParseForStatement(),
                SyntaxKind.BreakKeyword => ParseBreakStatement(),
                SyntaxKind.ContinueKeyword => ParseContinueStatement(),
                SyntaxKind.ReturnKeyword => ParseReturnStatement(),
                _ => ParseExpressionStatement(),
            };
        }



        private BlockStatementSyntax ParseBlockStatement()
        {
            var statements = ImmutableArray.CreateBuilder<StatementSyntax>();

            var openBraceToken = MatchToken(SyntaxKind.OpenBraceToken);

            while (Current.Kind != SyntaxKind.EndOfFileToken &&
                   Current.Kind != SyntaxKind.CloseBraceToken)
            {
                var startToken = Current;

                var statement = ParseStatement();
                statements.Add(statement);

                if (Current == startToken)
                {
                    NextToken();
                }
                startToken = Current;
            }

            var closeBraceToken = MatchToken(SyntaxKind.CloseBraceToken);

            return new BlockStatementSyntax(syntaxTree, openBraceToken, statements.ToImmutable(), closeBraceToken);
        }

        private StatementSyntax ParseVariableDeclaration()
        {
            var expected = Current.Kind == SyntaxKind.LetKeyword ? SyntaxKind.LetKeyword : SyntaxKind.VarKeyword;
            var keyword = MatchToken(expected);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var typeClause = ParseOptionalTypeClause();
            var equals = MatchToken(SyntaxKind.EqualsToken);
            var initializer = ParseExpression();
            return new VariableDeclarationSyntax(syntaxTree, keyword, identifier, typeClause, equals, initializer);
        }

        private TypeClauseSyntax ParseOptionalTypeClause()
        {
            if (Current.Kind != SyntaxKind.ColonToken)
            {
                return null!;
            }

            return ParseTypeClause();
        }

        private TypeClauseSyntax ParseTypeClause()
        {
            var colonToken = MatchToken(SyntaxKind.ColonToken);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            return new TypeClauseSyntax(syntaxTree, colonToken, identifier);
        }

        private StatementSyntax ParseIfStatement()
        {
            var keyword = MatchToken(SyntaxKind.IfKeyword);
            var condition = ParseExpression();
            var statement = ParseStatement();
            var elseClause = ParseElseClause();
            return new IfStatementSyntax(syntaxTree, keyword, condition, statement, elseClause);
        }

        private ElseClauseSyntax ParseElseClause()
        {
            if (Current.Kind is not SyntaxKind.ElseKeyword)
            {
                return null!;
            }

            var keyword = NextToken();
            var statement = ParseStatement();
            return new ElseClauseSyntax(syntaxTree, keyword, statement);
        }

        private StatementSyntax ParseWhileStatement()
        {
            var keyword = MatchToken(SyntaxKind.WhileKeyword);
            var condition = ParseExpression();
            var body = ParseStatement();
            return new WhileStatementSyntax(syntaxTree, keyword, condition, body);
        }

        private StatementSyntax ParseForStatement()
        {
            var forKeyword = MatchToken(SyntaxKind.ForKeyword);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var inKeyword = MatchToken(SyntaxKind.InKeyword);
            var lowerBound = ParseExpression();
            var dotDotToken = MatchToken(SyntaxKind.DotDotToken);
            var upperBound = ParseExpression();
            var body = ParseStatement();
            return new ForStatementSyntax(syntaxTree, forKeyword, identifier, inKeyword, lowerBound, dotDotToken, upperBound, body);
        }

        private StatementSyntax ParseBreakStatement()
        {
            var keyword = MatchToken(SyntaxKind.BreakKeyword);
            return new BreakStatementSyntax(syntaxTree, keyword);
        }

        private StatementSyntax ParseContinueStatement()
        {
            var keyword = MatchToken(SyntaxKind.ContinueKeyword);
            return new ContinueStatementSyntax(syntaxTree, keyword);
        }

        private StatementSyntax ParseReturnStatement()
        {
            var keyword = MatchToken(SyntaxKind.ReturnKeyword);
            var keywordLine = text.GetLineIndex(keyword.Span.Start);
            var currentLine = text.GetLineIndex(Current.Span.Start);
            var isEof = Current.Kind == SyntaxKind.EndOfFileToken;
            var sameLine = !isEof && keywordLine == currentLine;
            var expression = sameLine ? ParseExpression() : null;
            return new ReturnStatementSyntax(syntaxTree, keyword, expression!);
        }

        private ExpressionStatementSyntax ParseExpressionStatement()
        {
            var expression = ParseExpression();
            return new ExpressionStatementSyntax(syntaxTree, expression);
        }

        private ExpressionSyntax ParseExpression()
        {
            return ParseAssignmentExpression();
        }

        private ExpressionSyntax ParseAssignmentExpression()
        {
            if (Peek(0).Kind == SyntaxKind.IdentifierToken
                && Peek(1).Kind == SyntaxKind.EqualsToken)
            {
                var identifierToken = NextToken();
                var operatorToken = NextToken();
                var right = ParseAssignmentExpression();
                return new AssignmentExpressionSyntax(syntaxTree, identifierToken, operatorToken, right);
            }
            return ParseBinaryExpression();
        }

        private ExpressionSyntax ParseBinaryExpression(int parentPrecedence = 0)
        {
            ExpressionSyntax left;
            var unaryOperatorPrecedence = Current.Kind.GetUnaryOperatorPrecedence();
            if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence)
            {
                var operatorToken = NextToken();
                var operand = ParseBinaryExpression(unaryOperatorPrecedence);
                left = new UnaryExpressionSyntax(syntaxTree, operatorToken, operand);
            }
            else
            {
                left = ParsePrimaryExpression();
            }
            while (true)
            {
                var precedence = Current.Kind.GetBinaryOperatorPrecedence();
                if (precedence == 0 || precedence <= parentPrecedence)
                {
                    break;
                }

                var operatorToken = NextToken();
                var right = ParseBinaryExpression(precedence);
                left = new BinaryExpressionSyntax(syntaxTree, left, operatorToken, right);

            }
            return left;
        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.OpenParenthesisToken:
                    {
                        return ParseParenthesizedExpression();
                    }
                case SyntaxKind.FalseKeyword:
                case SyntaxKind.TrueKeyword:
                    {
                        return ParseBooleanLiteral();
                    }
                case SyntaxKind.NumberToken:
                    {
                        return ParseNumberLiteral();
                    }
                case SyntaxKind.StringToken:
                    {
                        return ParseStringLiteral();
                    }
                case SyntaxKind.IdentifierToken:
                default:
                    {
                        return ParseNameOrCallExpression();
                    }
            }
        }

        private ExpressionSyntax ParseParenthesizedExpression()
        {
            var left = MatchToken(SyntaxKind.OpenParenthesisToken);
            var expression = ParseExpression();
            var right = MatchToken(SyntaxKind.CloseParenthesisToken);
            return new ParenthesizedExpressionSyntax(syntaxTree, left, expression, right);
        }

        private ExpressionSyntax ParseBooleanLiteral()
        {
            var isTrue = Current.Kind is SyntaxKind.TrueKeyword;
            var keywordToken = isTrue ? MatchToken(SyntaxKind.TrueKeyword) : MatchToken(SyntaxKind.FalseKeyword);
            return new LiteralExpressionSyntax(syntaxTree, keywordToken, isTrue);
        }

        private ExpressionSyntax ParseNumberLiteral()
        {
            var numberToken = MatchToken(SyntaxKind.NumberToken);
            return new LiteralExpressionSyntax(syntaxTree, numberToken);
        }

        private ExpressionSyntax ParseStringLiteral()
        {
            var stringToken = MatchToken(SyntaxKind.StringToken);
            return new LiteralExpressionSyntax(syntaxTree, stringToken);
        }

        private ExpressionSyntax ParseNameOrCallExpression()
        {
            if (Peek(0).Kind == SyntaxKind.IdentifierToken && Peek(1).Kind == SyntaxKind.OpenParenthesisToken)
            {
                return ParseCallExpression();
            }
            return ParseNameExpression();
        }

        private ExpressionSyntax ParseCallExpression()
        {
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var openParenthesisToken = MatchToken(SyntaxKind.OpenParenthesisToken);
            var arguments = ParseArguments();
            var closeParenthesisToken = MatchToken(SyntaxKind.CloseParenthesisToken);
            return new CallExpressionSyntax(syntaxTree, identifier, openParenthesisToken, arguments, closeParenthesisToken);
        }

        private SeparatedSyntaxList<ExpressionSyntax> ParseArguments()
        {
            var nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();

            var parseNextArgument = true;

            while (parseNextArgument && Current.Kind != SyntaxKind.CloseParenthesisToken && Current.Kind != SyntaxKind.EndOfFileToken)
            {
                var expression = ParseExpression();
                nodesAndSeparators.Add(expression);

                if (Current.Kind == SyntaxKind.CommaToken)
                {
                    var comma = MatchToken(SyntaxKind.CommaToken);
                    nodesAndSeparators.Add(comma);
                }
                else
                {
                    parseNextArgument = false;
                }
            }

            return new SeparatedSyntaxList<ExpressionSyntax>(nodesAndSeparators.ToImmutable());
        }

        private ExpressionSyntax ParseNameExpression()
        {
            var identifierToken = MatchToken(SyntaxKind.IdentifierToken);
            return new NameExpressionSyntax(syntaxTree, identifierToken);
        }
    }
}
