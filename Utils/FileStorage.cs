using System.Text.Json;

namespace infonetica_task.Utils;

public static class FileStorage {
    private static readonly JsonSerializerOptions _jsonOptions = new() {
        WriteIndented = true
    };
    public static List<T> LoadList<T>(string filePath) {
        if (!File.Exists(filePath))
            return [];

        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<List<T>>(json) ?? [];
    }

    public static void SaveList<T>(string filePath, List<T> data) {
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        File.WriteAllText(filePath, json);
    }
}
