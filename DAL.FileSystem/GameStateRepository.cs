using System.Text.Json;
using DAL;
using Domain;

namespace DAL_FileSystem;

public class GameStateRepository : IGameStateRepository
{
    private const string FileExtension = "json";
    private readonly string _savesDirectory = "." + Path.DirectorySeparatorChar + "gamesaves";


    public List<string> GetGameSavesList()
    {
        CheckOrCreateDirectory();
        
        var result = new List<string>();
        
        
        foreach (var fileName in Directory.GetFileSystemEntries(_savesDirectory, "*." + FileExtension))
        {
            result.Add(Path.GetFileNameWithoutExtension(fileName));
        }

        return result;
    }
    public GameState GetGameState(string id)
    {
        var fileContent = File.ReadAllText(GetFileName(id));

        var state = JsonSerializer.Deserialize<GameState>(fileContent);

        if (state == null)
        {
            throw new NullReferenceException($"Could not deserialize: {fileContent}");
        }
        
        return state;
    }

    public void SaveGameState(string id, GameState state)
    {
        CheckOrCreateDirectory();

        var fileContent = JsonSerializer.Serialize(state);
        
        File.WriteAllText(GetFileName(id), fileContent);
    }

    public void DeleteGameState(string id)
    {
        if (!File.Exists(GetFileName(id)))
        {
            throw new ArgumentException("File does not exist!");
        }
        File.Delete(GetFileName(id));
    }
    
    private string GetFileName(string id)
    {
        return _savesDirectory + Path.DirectorySeparatorChar + id + "." + FileExtension;
    }

    private void CheckOrCreateDirectory()
    {
        if (!Directory.Exists(_savesDirectory))
        {
            Directory.CreateDirectory(_savesDirectory);
        }
    }
}