using Domain;

namespace DAL;

public interface IGameOptionsRepository
{
    List<string> GetGameOptionsList();
    
    // read
    CheckersOptions GetGameOptions(string id);
    
    //create and update
    void SaveGameOptions(string id, CheckersOptions options);
    
    //delete
    void DeleteGameOptions(string id);
    
}