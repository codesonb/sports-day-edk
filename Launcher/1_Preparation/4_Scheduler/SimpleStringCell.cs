namespace Launcher
{
    class SimpleStringCell
    {
        public SimpleStringCell(int row, int column, int span, string name)
        {
            this.Row = row;
            this.Column = column;
            this.Span = span;
            this.Name = name;
        }
        public int Row { get; }
        public int Column { get; }
        public int Span { get; }
        public string Name { get; }
    }
}
