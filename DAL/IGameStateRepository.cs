using Domain;

namespace DAL;

public interface IGameStateRepository
{
    List<string> GetGameSavesList();
    
    // read
    GameState GetGameState(string id);
    
    //create and update
    void SaveGameState(string id, GameState state);
    
    //delete
    void DeleteGameState(string id);
}