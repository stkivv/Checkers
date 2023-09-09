using System.Text.Json;
using DAL;
using Domain;

namespace DAL_FileSystem;

public class GameRepository : IGameRepository
{
    private const string FileExtension = "json";
    private readonly string _savesDirectory = "." + Path.DirectorySeparatorChar + "games";
    private readonly string _idFile = "." + Path.DirectorySeparatorChar + "idNumber" + Path.DirectorySeparatorChar + "id.txt.txt";

    public List<CheckersGame> GetAll()
    {
        CheckOrCreateDirectory();
        
        var result = new List<CheckersGame>();
        
        foreach (var name in Directory.GetFileSystemEntries(_savesDirectory, "*." + FileExtension))
        {
            int id = Int32.Parse(Path.GetFileNameWithoutExtension(name));
            CheckersGame game = GetGame(id);
            result.Add(game);
        }
        return result;
    }

    public CheckersGame GetGame(int? id)
    {
        var fileContent = File.ReadAllText(GetFileName(id.ToString()!));

        var game = JsonSerializer.Deserialize<CheckersGame>(fileContent);

        if (game == null)
        {
            throw new NullReferenceException($"Could not deserialize: {fileContent}");
        }

        return game;
    }

    public CheckersGame GetGame(string name)
    {
        List<CheckersGame> games = GetAll();
        foreach (var game in games)
        {
            if (game.GameName == name)
            {
                return game;
            }
        }
        throw new Exception("Game not found!");
    }

    public CheckersGame AddGame(CheckersGame game)
    {
        if (game.CheckersGameId == null)
        {
            var contents = File.ReadAllText(_idFile);
            int id = Int32.Parse(contents);
            int idNew = id + 1;
            File.WriteAllText(_idFile, idNew.ToString());
            game.CheckersGameId = id;
        }

        CheckOrCreateDirectory();
        var fileContent = JsonSerializer.Serialize(game);
        
        File.WriteAllText(GetFileName(game.CheckersGameId.ToString()!), fileContent);
        return game;
    }

    //deletes old file for game and makes a new one
    public void SaveChanges(CheckersGame game)
    {
        CheckersGame gameToDelete = GetGame(game.CheckersGameId);
        if (gameToDelete == null)
        {
            throw new FileNotFoundException("File does not exist!");
        }
        DeleteGame(gameToDelete);
        AddGame(game);
    }

    public void DeleteLastState(CheckersGame game)
    {
        GameState? state = game.States!.LastOrDefault();
        if (state == null) return;
        game.States!.Remove(state);
        SaveChanges(game);
    }

    public void DeleteGame(CheckersGame game)
    {
        string name = game.CheckersGameId.ToString()!;
        if (!File.Exists(GetFileName(name)))
        {
            throw new FileNotFoundException("File does not exist!");
        }
        File.Delete(GetFileName(name));
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