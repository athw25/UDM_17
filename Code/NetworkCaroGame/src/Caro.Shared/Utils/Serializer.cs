
using System.Text.Json;

namespace Caro.Shared.Utils
{
    public static class Serializer
    {
        public static string Serialize(object obj) => JsonSerializer.Serialize(obj);
        public static T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json);
    }
}
