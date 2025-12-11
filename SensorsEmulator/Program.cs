using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

const string serverUrl = "https://localhost:7003/api/v1/readings";
const string apiKeyHeader = "X-Sensor-Api-Key";

var sensors = JsonSerializer.Deserialize<List<SensorConfig>>(File.ReadAllText("appsettings.json"));

if (sensors is null || sensors.Count == 0) throw new Exception("No sensors configured in appsettings.json");

var http = new HttpClient();

Console.WriteLine("Sensor emulator started.");

while (true)
{
    foreach (var sensor in sensors)
    {
        // генерируем показание
        double value = sensor.Type switch
        {
            SensorType.Temperature => GenerateTemperature(),
            SensorType.Humidity => GenerateHumidity(),
            _ => 0
        };

        var payload = new ReadingDto
        {
            Timestamp = DateTime.UtcNow,
            Value = value
        };

        var request = new HttpRequestMessage(HttpMethod.Post, serverUrl);
        request.Headers.Add(apiKeyHeader, sensor.ApiKey);
        request.Content = JsonContent.Create(payload);

        try
        {
            var response = await http.SendAsync(request);
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} [{sensor.Type}] Sent {value:0.0}, Status: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR sending data: {ex.Message}");
        }
    }

    await Task.Delay(1000);
}

// генераторы показаний
double GenerateTemperature()
{
    // около 20–30 градусов
    return 20 + Random.Shared.NextDouble() * 10;
}

double GenerateHumidity()
{
    // около 40–60%
    return 40 + Random.Shared.NextDouble() * 20;
}
