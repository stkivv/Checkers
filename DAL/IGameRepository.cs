using Domain;

namespace DAL;

public interface IGameRepository
{
    List<CheckersGame> GetAll();
    
    CheckersGame? GetGame(int? id);

    CheckersGame? GetGame(string playerNames);
    
    CheckersGame AddGame(CheckersGame game);

    void SaveChanges(CheckersGame game);

    void DeleteLastState(CheckersGame game);

    void DeleteGame(CheckersGame game);
}