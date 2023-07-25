namespace Compiler.CodeAnalysis
{
    public struct TextSpan
    {
        public int Start { get; }
        public int Length { get; }
        public TextSpan(int start, int length)
        {
            Start = start;
            Length = length;
        }
        public int End => Start + Length;

        internal static TextSpan FromBounds(int start, int end)
        {
            throw new NotImplementedException();
        }
    }
}
