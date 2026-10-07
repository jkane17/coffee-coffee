using System.Text.Json;
using Godot;

/// <summary>Reads and writes the save file as JSON in the player's user data folder (user://).</summary>
public sealed class SaveStore
{
    private const string DefaultPath = "user://save.json";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _path;

    public SaveStore(string path = DefaultPath)
    {
        _path = path;
    }

    /// <summary>Load the save, or null if there isn't a usable one.</summary>
    public SaveData? Load()
    {
        if (!FileAccess.FileExists(_path))
        {
            return null;
        }

        using FileAccess? file = FileAccess.Open(_path, FileAccess.ModeFlags.Read);
        if (file is null)
        {
            GD.PushError($"Couldn't open {_path}: {FileAccess.GetOpenError()}");
            return null;
        }

        SaveData? data;
        try
        {
            data = JsonSerializer.Deserialize<SaveData>(file.GetAsText());
        }
        catch (JsonException e)
        {
            GD.PushError($"Save file {_path} couldn't be read: {e.Message}");
            return null;
        }

        // Version 1 saved the balance in whole dollars; it's in cents now.
        if (data is { Version: 1 })
        {
            data = data with { Version = 2, Money = data.Money * 100 };
        }

        if (data is null || data.Version != SaveData.CurrentVersion)
        {
            GD.PushWarning($"Ignoring save file {_path}: unsupported version {data?.Version}.");
            return null;
        }

        return data;
    }

    public void Save(SaveData data)
    {
        using FileAccess? file = FileAccess.Open(_path, FileAccess.ModeFlags.Write);
        if (file is null)
        {
            GD.PushError($"Couldn't write {_path}: {FileAccess.GetOpenError()}");
            return;
        }

        file.StoreString(JsonSerializer.Serialize(data, JsonOptions));
    }
}
