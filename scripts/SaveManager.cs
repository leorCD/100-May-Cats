using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class SaveManager : Node
{
    private class SaveData // purely a helper class, no other class needs to access this
    {
        public List<string> CollectedCats { get; set; }
        // List<int> because JsonSerializer works cleanly with it
        // JSON arrays naturally map to lists
    }

    public static SaveManager Instance {get; private set;} 

    private readonly string _savePath = OS.HasFeature("editor")
        ? ProjectSettings.GlobalizePath("res://save.json")
        : OS.GetExecutablePath().GetBaseDir() + "/save.json";
    private HashSet<string> _collectedCats = new();

    public override void _Ready()
    {
        Instance = this;
        LoadSave();
    }


    private void LoadSave()
    {
        if (!FileAccess.FileExists(_savePath))
        {
            GD.Print("No save file");
            return;
        } // exit if no save file

        GD.Print("Save file found");
        using var file = FileAccess.Open(_savePath, FileAccess.ModeFlags.Read); // writes to the file, then closes it automatically
        var data = JsonSerializer.Deserialize<SaveData>(file.GetAsText()); // deserialized data in the form of an object we can read from
        if (data != null && data.CollectedCats != null) // if dezerialized with no problems, and collectedCats exists
            _collectedCats = new HashSet<string>(data.CollectedCats);
            // we're saving it as a HashSet<int> instead because HashSet has faster lookup functions
    }

    private void WriteSave()
    {
        using var file = FileAccess.Open(_savePath, FileAccess.ModeFlags.Write); // creates a file if this path is empty

        var catList = new List<string>(_collectedCats); // list of collected cats
        var saveData = new SaveData { CollectedCats = catList }; // create new SaveData with list of collected cats
        var json = JsonSerializer.Serialize(saveData); // turn saveData into a JSON string

        // we cant write a C# object directly to a file, so we have to serialize it to write it on text
        // we deserialize it so that WE can actually use it

        file.StoreString(json);
    }

    public void CollectCat(string name)
    {
        if (_collectedCats.Add(name)) WriteSave();
    }

    public bool IsCollected(string name) => _collectedCats.Contains(name);
    // {
    //     return _collectedCats.Contains(index)
    // }

}
