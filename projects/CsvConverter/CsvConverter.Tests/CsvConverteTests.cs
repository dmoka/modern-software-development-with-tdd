namespace CsvConverter.Tests
{
    public class Tests
    {
        [Test]
        public async Task VerifyCsvSnapshot()
        {
            // Arrange
            var users = new List<User>
            {
                new User {
                    Id = 12345,
                    Name = "John Doe",
                    Email = "john.doe@gmail.com" },
                new User {
                    Id = 67890,
                    Name = "Jane Smith",
                    Email = "jane.smith@gmail.com" }
            };
            // Act
            using var csvStream = new CsvConverter().ConvertToCsv(users);

            // Assert
            await Verifier.Verify(csvStream);
        }
    }
}