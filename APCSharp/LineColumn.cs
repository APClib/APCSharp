namespace APCSharp
{
    internal class LineColumn
    {
        private int line;
        private int column;
        public LineColumn(int line, int column)
        {
            this.line = line;
            this.column = column;
        }

        public int Line { get => line; }
        public int Column { get => column; }

        public void NextColumn() => column++;
        public void NextLine()
        {
            column = 0;
            line++;
        }

        public override string ToString() => $"line {Line}, column {Column}";
    }
}