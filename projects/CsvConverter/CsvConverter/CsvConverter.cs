namespace CsvConverter
{

    public class CsvConverter
    {
        private const char DefaultSeparator = ';';

        public Stream ConvertToCsv(IEnumerable<User> users)
        {
            var stream = new MemoryStream();
            // Don't dispose the StreamWriter since we need the underlying stream to remain open
            var writer = new StreamWriter(stream, leaveOpen: true);

            writer.WriteLine($"Id{DefaultSeparator}Name{DefaultSeparator}Email");
            foreach (var user in users)
            {
                var values = new[] { user.Id.ToString(), user.Name, user.Email };
                writer.WriteLine(string.Join(DefaultSeparator, values));
            }

            // Ensure all data is written to the underlying stream
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
