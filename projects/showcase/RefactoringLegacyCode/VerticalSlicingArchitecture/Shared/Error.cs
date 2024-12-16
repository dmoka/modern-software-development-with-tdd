namespace VerticalSlicingArchitecture.Shared
{
    public sealed record Error(string Code, string Description)
    {
        public static readonly Error Empty = new(string.Empty, string.Empty);
    }
}
