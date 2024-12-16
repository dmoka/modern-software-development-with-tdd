using System.Text;
using Newtonsoft.Json;

namespace MutationTestingTDD.Tests
{
    public class JsonPayloadBuilder
    {
        public static StringContent Build(dynamic payloadAsDynamicObject)
        {
            var jsonString = JsonConvert.SerializeObject(payloadAsDynamicObject);

            return CreateJsonPayloadContent(jsonString);
        }

        private static StringContent CreateJsonPayloadContent(dynamic jsonContent)
        {
            return new StringContent(
                jsonContent,
                Encoding.UTF8,
                "application/json");
        }
    }
}
