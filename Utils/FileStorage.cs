using System.Text.Json;

namespace infonetica_task.Utils;

public static class FileStorage {
    private static readonly JsonSerializerOptions _jsonOptions = new() {
        WriteIndented = true
    };

    // Used generics to allow for any type of object to be saved or loaded.

    // Load a list of items from a JSON file. Returns an empty list if file doesn't exist or fails to load.
    public static async Task<List<T>> LoadList<T>(string filePath) {
        try {
            if (!File.Exists(filePath))
                return [];

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<List<T>>(json) ?? [];
        }
        catch (Exception ex) {
            Console.WriteLine($"Error reading file '{filePath}': {ex.Message}");
            return [];
        }
    }

    // Save a list of items to a JSON file
    public static async Task SaveList<T>(string filePath, List<T> data) {
        try {
            if (data == null) {
                Console.WriteLine("Data to save is null, skipping write operation.");
                return;
            }
            // Ensure the directory exists before writing the file
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) {
                // Create the directory if it doesn't exist
                Directory.CreateDirectory(directory);
            }
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex) {
            Console.WriteLine($"Error writing file '{filePath}': {ex.Message}");
        }
    }
}