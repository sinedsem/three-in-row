public struct Cell
{
    public int Row { get; }
    public int Col { get; }

    public Cell(int row, int col)
    {
        this.Row = row;
        this.Col = col;
    }

    public override string ToString()
    {
        return "(" + Row + ";" + Col + ")";
    }

    public bool Equals(Cell other)
    {
        return Row == other.Row && Col == other.Col;
    }

    public override bool Equals(object obj)
    {
        return obj is Cell other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Row * 397) ^ Col;
        }
    }
}