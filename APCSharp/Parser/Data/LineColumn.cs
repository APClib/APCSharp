namespace APCSharp.Parser.Data
{
    internal class LineColumn
    {
        public LineColumn(int line, int column)
        {
            Line = line;
            Column = column;
        }

        public int Line { get; private set; }
        public int Column { get; private set; }

        public void NextColumn() => Column++;
        public void NextLine()
        {
            Column = 0;
            Line++;
        }

        public void Reset()
        {
            Line = 0;
            Column = 0;
        }

        public override string ToString() => $"line {Line}, column {Column}";
    }
}