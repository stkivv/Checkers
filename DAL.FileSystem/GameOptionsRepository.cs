using System.Text.Json;
using DAL;
using Domain;

namespace DAL_FileSystem;

public class GameOptionsRepository : IGameOptionsRepository
{
    private const string FileExtension = "json";
    private readonly string _optionsDirectory = "." + Path.DirectorySeparatorChar + "options";
    
    public List<string> GetGameOptionsList()
    {
        CheckOrCreateDirectory();
        
        var result = new List<string>();
        
        
        foreach (var fileName in Directory.GetFileSystemEntries(_optionsDirectory, "*." + FileExtension))
        {
            result.Add(Path.GetFileNameWithoutExtension(fileName));
        }

        return result;
    }

    public CheckersOptions GetGameOptions(string id)
    {
        var fileContent = File.ReadAllText(GetFileName(id));

        var options = JsonSerializer.Deserialize<CheckersOptions>(fileContent);

        if (options == null)
        {
            throw new NullReferenceException($"Could not deserialize: {fileContent}");
        }
        
        return options;
    }

    public void SaveGameOptions(string id, CheckersOptions options)
    {
        CheckOrCreateDirectory();

        var fileContent = JsonSerializer.Serialize(options);
        
        File.WriteAllText(GetFileName(id), fileContent);
    }

    public void DeleteGameOptions(string id)
    {
        if (!File.Exists(GetFileName(id)))
        {
            throw new ArgumentException("File does not exist!");
        }
        File.Delete(GetFileName(id));
    }

    private string GetFileName(string id)
    {
        return _optionsDirectory + Path.DirectorySeparatorChar + id + "." + FileExtension;
    }

    private void CheckOrCreateDirectory()
    {
        if (!Directory.Exists(_optionsDirectory))
        {
            Directory.CreateDirectory(_optionsDirectory);
        }
    }

}