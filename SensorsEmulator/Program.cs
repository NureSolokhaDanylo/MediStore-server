using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

const string ApiKeyHeader = "X-Sensor-Api-Key";
const string ApiBaseUrlEnv = "API_BASE_URL";
const string SensorApiKeyEnv = "SENSOR_API_KEY";
const string SensorTypeEnv = "SENSOR_TYPE";
const string MinValueEnv = "MIN_VALUE";
const string MaxValueEnv = "MAX_VALUE";
const string IntervalMsEnv = "INTERVAL_MS";
const int DefaultIntervalMs = 1000;

var sensors = LoadSensors();

if (sensors.Count == 0)
{
    throw new InvalidOperationException(
        "No sensors configured. Set API_BASE_URL, SENSOR_API_KEY, SENSOR_TYPE, MIN_VALUE, MAX_VALUE and INTERVAL_MS, or provide appsettings.json.");
}

using var http = new HttpClient();

Console.WriteLine($"Sensor emulator started with {sensors.Count} sensor(s).");

while (true)
{
    foreach (var sensor in sensors)
    {
        var value = GenerateValue(sensor);
        var payload = new ReadingDto
        {
            TimeStamp = DateTime.UtcNow,
            Value = value
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, sensor.ApiBaseUrl);
        request.Headers.Add(ApiKeyHeader, sensor.ApiKey);
        request.Content = JsonContent.Create(payload);

        try
        {
            using var response = await http.SendAsync(request);
            Console.WriteLine(
                $"{DateTime.Now:HH:mm:ss} [{sensor.Type}] Sent {value:0.0} to {sensor.ApiBaseUrl}, Status: {(int)response.StatusCode} {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR [{sensor.Type}] sending data to {sensor.ApiBaseUrl}: {ex.Message}");
        }
    }

    var delayMs = sensors.Min(static x => x.IntervalMs ?? DefaultIntervalMs);
    await Task.Delay(delayMs);
}

static List<SensorConfig> LoadSensors()
{
    var envSensor = TryLoadFromEnvironment();
    if (envSensor is not null)
    {
        return [envSensor];
    }

    if (!File.Exists("appsettings.json"))
    {
        return [];
    }

    var sensors = JsonSerializer.Deserialize<List<SensorConfig>>(File.ReadAllText("appsettings.json")) ?? [];
    foreach (var sensor in sensors)
    {
        NormalizeSensor(sensor);
    }

    return sensors.Where(IsValidSensor).ToList();
}

static SensorConfig? TryLoadFromEnvironment()
{
    var apiBaseUrl = Environment.GetEnvironmentVariable(ApiBaseUrlEnv);
    var apiKey = Environment.GetEnvironmentVariable(SensorApiKeyEnv);
    var sensorTypeRaw = Environment.GetEnvironmentVariable(SensorTypeEnv);

    if (string.IsNullOrWhiteSpace(apiBaseUrl)
        && string.IsNullOrWhiteSpace(apiKey)
        && string.IsNullOrWhiteSpace(sensorTypeRaw))
    {
        return null;
    }

    if (string.IsNullOrWhiteSpace(apiBaseUrl)
        || string.IsNullOrWhiteSpace(apiKey)
        || string.IsNullOrWhiteSpace(sensorTypeRaw))
    {
        throw new InvalidOperationException(
            "Incomplete sensor env configuration. Expected API_BASE_URL, SENSOR_API_KEY and SENSOR_TYPE together.");
    }

    if (!Enum.TryParse<SensorType>(sensorTypeRaw, ignoreCase: true, out var sensorType))
    {
        throw new InvalidOperationException($"Invalid SENSOR_TYPE value '{sensorTypeRaw}'. Expected Temperature or Humidity.");
    }

    var sensor = new SensorConfig
    {
        ApiBaseUrl = apiBaseUrl,
        ApiKey = apiKey,
        Type = sensorType,
        MinValue = ParseNullableDouble(Environment.GetEnvironmentVariable(MinValueEnv), MinValueEnv),
        MaxValue = ParseNullableDouble(Environment.GetEnvironmentVariable(MaxValueEnv), MaxValueEnv),
        IntervalMs = ParseNullableInt(Environment.GetEnvironmentVariable(IntervalMsEnv), IntervalMsEnv)
    };

    NormalizeSensor(sensor);

    if (!IsValidSensor(sensor))
    {
        throw new InvalidOperationException("Environment sensor configuration is invalid after normalization.");
    }

    return sensor;
}

static void NormalizeSensor(SensorConfig sensor)
{
    sensor.ApiBaseUrl = string.IsNullOrWhiteSpace(sensor.ApiBaseUrl)
        ? "http://localhost:14000/api/v1/readings"
        : sensor.ApiBaseUrl.Trim();

    sensor.IntervalMs ??= DefaultIntervalMs;

    var (defaultMin, defaultMax) = sensor.Type switch
    {
        SensorType.Temperature => (15d, 30d),
        SensorType.Humidity => (35d, 65d),
        _ => (0d, 100d)
    };

    sensor.MinValue ??= defaultMin;
    sensor.MaxValue ??= defaultMax;

    if (sensor.MaxValue < sensor.MinValue)
    {
        (sensor.MinValue, sensor.MaxValue) = (sensor.MaxValue, sensor.MinValue);
    }
}

static bool IsValidSensor(SensorConfig sensor)
{
    return !string.IsNullOrWhiteSpace(sensor.ApiBaseUrl)
        && !string.IsNullOrWhiteSpace(sensor.ApiKey)
        && sensor.IntervalMs is > 0
        && sensor.MinValue is not null
        && sensor.MaxValue is not null;
}

static double GenerateValue(SensorConfig sensor)
{
    var min = sensor.MinValue ?? 0;
    var max = sensor.MaxValue ?? min;

    if (max <= min)
    {
        return min;
    }

    return min + Random.Shared.NextDouble() * (max - min);
}

static double? ParseNullableDouble(string? value, string envName)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return null;
    }

    if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
    {
        return parsed;
    }

    throw new InvalidOperationException($"Invalid {envName} value '{value}'.");
}

static int? ParseNullableInt(string? value, string envName)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return null;
    }

    if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
    {
        return parsed;
    }

    throw new InvalidOperationException($"Invalid {envName} value '{value}'.");
}
