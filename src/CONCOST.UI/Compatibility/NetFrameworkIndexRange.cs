#if NET48
namespace System
{
    /// <summary>Compiler support type for index-from-end and range syntax on .NET Framework 4.8.</summary>
    public readonly struct Index
    {
        private readonly int _value;

        public Index(int value, bool fromEnd = false)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            _value = fromEnd ? ~value : value;
        }

        public int Value => _value < 0 ? ~_value : _value;
        public bool IsFromEnd => _value < 0;

        public int GetOffset(int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            var offset = IsFromEnd ? length + _value : _value;
            if ((uint)offset > (uint)length) throw new ArgumentOutOfRangeException(nameof(length));
            return offset;
        }

        public static Index Start => new(0);
        public static Index End => new(0, fromEnd: true);
        public static Index FromStart(int value) => new(value);
        public static Index FromEnd(int value) => new(value, fromEnd: true);
        public static implicit operator Index(int value) => FromStart(value);
    }

    /// <summary>Compiler support type for range syntax on .NET Framework 4.8.</summary>
    public readonly struct Range
    {
        public Range(Index start, Index end)
        {
            Start = start;
            End = end;
        }

        public Index Start { get; }
        public Index End { get; }

        public (int Offset, int Length) GetOffsetAndLength(int length)
        {
            var start = Start.GetOffset(length);
            var end = End.GetOffset(length);
            if (end < start) throw new ArgumentOutOfRangeException(nameof(length));
            return (start, end - start);
        }

        public static Range StartAt(Index start) => new(start, Index.End);
        public static Range EndAt(Index end) => new(Index.Start, end);
        public static Range All => new(Index.Start, Index.End);
    }
}
#endif
