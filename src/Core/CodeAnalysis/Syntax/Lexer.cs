using Core.CodeAnalysis;
using Core.CodeAnalysis.Symbols;
using Core.CodeAnalysis.Text;
using System.Text;

namespace Core.CodeAnalysis.Syntax
{
    internal class Lexer
    {
        private readonly SourceText text;
        private readonly DiagnosticBag diagnostics = new();
        private int position;
        private int start;
        private SyntaxKind kind;
        private object? value;
        public DiagnosticBag Diagnostics => diagnostics;

        public Lexer(SourceText text)
        {
            this.text = text;
        }

        private char Current => Peek(0);
        private char Lookahead => Peek(1);
        private char Peek(int offset)
        {
            var index = position + offset;
            if (index >= text.Length)
            {
                return '\0';
            }
            return text[index];
        }

        private void Next()
        {
            position++;
        }

        public SyntaxToken Lex()
        {
            start = position;
            kind = SyntaxKind.InvalidToken;
            value = null!;

            switch (Current)
            {
                case '\0':
                    kind = SyntaxKind.EndOfFileToken;
                    break;
                case '+':
                    kind = SyntaxKind.PlusToken;
                    position++;
                    break;
                case '-':
                    kind = SyntaxKind.MinusToken;
                    position++;
                    break;
                case '*':
                    kind = SyntaxKind.StarToken;
                    position++;
                    break;
                case '/':
                    kind = SyntaxKind.SlashToken;
                    position++;
                    break;
                case '(':
                    kind = SyntaxKind.OpenParenthesisToken;
                    position++;
                    break;
                case ')':
                    kind = SyntaxKind.CloseParenthesisToken;
                    position++;
                    break;
                case '{':
                    kind = SyntaxKind.OpenBraceToken;
                    position++;
                    break;
                case '}':
                    kind = SyntaxKind.CloseBraceToken;
                    position++;
                    break;
                case ':':
                    kind = SyntaxKind.ColonToken;
                    position++;
                    break;
                case ',':
                    kind = SyntaxKind.CommaToken;
                    position++;
                    break;
                case '~':
                    kind = SyntaxKind.TildeToken;
                    position++;
                    break;
                case '^':
                    kind = SyntaxKind.HatToken;
                    position++;
                    break;
                case '&':
                    if (Lookahead == '&')
                    {
                        kind = SyntaxKind.AmpersandAmpersandToken;
                        position += 2;
                    }
                    else
                    {
                        kind = SyntaxKind.AmpersandToken;
                        position++;
                    }
                    break;
                case '|':
                    if (Lookahead == '|')
                    {
                        kind = SyntaxKind.PipePipeToken;
                        position += 2;
                        break;
                    }
                    else
                    {
                        kind = SyntaxKind.PipeToken;
                        position++;
                    }
                    break;
                case '=':
                    if (Lookahead != '=')
                    {
                        kind = SyntaxKind.EqualsToken;
                        position++;
                    }
                    else
                    {
                        position += 2;
                        kind = SyntaxKind.EqualsEqualsToken;
                    }
                    break;
                case '!':
                    if (Lookahead != '=')
                    {
                        kind = SyntaxKind.BangToken;
                        position++;
                    }
                    else
                    {
                        kind = SyntaxKind.BangEqualsToken;
                        position += 2;
                    }
                    break;
                case '<':
                    if (Lookahead != '=')
                    {
                        kind = SyntaxKind.LessToken;
                        position++;
                    }
                    else
                    {
                        kind = SyntaxKind.LessOrEqualsToken;
                        position += 2;
                    }
                    break;
                case '>':
                    if (Lookahead != '=')
                    {
                        kind = SyntaxKind.GreaterToken;
                        position++;
                    }
                    else
                    {
                        kind = SyntaxKind.GreaterOrEqualsToken;
                        position += 2;
                    }
                    break;
                case '.':
                    if (Lookahead != '.')
                    {
                        kind = SyntaxKind.DotToken;
                        position++;
                    }
                    else
                    {
                        kind = SyntaxKind.DotDotToken;
                        position += 2;
                    }
                    break;
                case '\"':
                    ReadString();
                    break;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    ReadNumberToken();
                    break;
                case ' ':
                case '\t':
                case '\n':
                case '\r':
                    ReadWhitespace();
                    break;
                default:
                    if (char.IsLetter(Current))
                    {
                        ReadIdentifierOrKeyword();
                    }
                    else if (char.IsWhiteSpace(Current))
                    {
                        ReadWhitespace();
                    }
                    else
                    {
                        diagnostics.ReportBadCharacter(position, Current);
                        position++;
                    }
                    break;
            }

            var length = position - start;
            var text = SyntaxFacts.GetText(kind);
            if (text is null)
            {
                text = this.text.ToString(start, length);
            }

            return new SyntaxToken(kind, start, text, value);
        }

        private void ReadString()
        {
            // Skip the current quote
            position++;

            var sb = new StringBuilder();
            var done = false;

            while (!done)
            {
                switch (Current)
                {
                    case '\0':
                    case '\r':
                    case '\n':
                        var span = new TextSpan(start, 1);
                        diagnostics.ReportUnterminatedString(span);
                        done = true;
                        break;
                    case '"':
                        if (Lookahead == '"')
                        {
                            sb.Append(Current);
                            position += 2;
                        }
                        else
                        {
                            position++;
                            done = true;
                        }
                        break;
                    default:
                        sb.Append(Current);
                        position++;
                        break;
                }
            }

            kind = SyntaxKind.StringToken;
            value = sb.ToString();
        }

        private void ReadNumberToken()
        {
            while (char.IsDigit(Current))
            {
                Next();
            }
            int length = position - start;
            string tokenText = text.ToString(start, length);
            if (!int.TryParse(tokenText, out int value))
            {
                diagnostics.ReportInvalidNumber(new TextSpan(start, length), tokenText, TypeSymbol.Int);
            }

            this.value = value;
            kind = SyntaxKind.NumberToken;
        }

        private void ReadWhitespace()
        {
            while (char.IsWhiteSpace(Current))
            {
                Next();
            }
            kind = SyntaxKind.WhitespaceToken;
        }

        private void ReadIdentifierOrKeyword()
        {
            while (char.IsLetter(Current))
            {
                Next();
            }
            int length = position - start;
            string tokenText = text.ToString(start, length);
            kind = SyntaxFacts.GetKeywordKind(tokenText);
        }
    }
}
