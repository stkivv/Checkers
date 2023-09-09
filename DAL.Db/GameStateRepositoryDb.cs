using Domain;

namespace DAL.Db;

public class GameStateRepositoryDb : IGameStateRepository
{
    private readonly AppDbContext _dbContext;

    public GameStateRepositoryDb(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<string> GetGameSavesList()
    {
        List<string> result = _dbContext.GameStates.Select(s => s.GameStateName).ToList();
        result.Sort();
        return result;
    }

    public GameState GetGameState(string id)
    {
        return _dbContext.GameStates.First(o => o.GameStateName == id);

    }

    public void SaveGameState(string id, GameState state)
    {
        var savesFromDb = _dbContext.GameStates.FirstOrDefault(o => o.GameStateName == id);

        if (savesFromDb == null)
        {
            _dbContext.GameStates.Add(state);
            _dbContext.SaveChanges();
            return;
        }

        savesFromDb.GameStateName = state.GameStateName;
        savesFromDb.GameBoard = state.GameBoard;
        savesFromDb.WhiteTurn = state.WhiteTurn;
        _dbContext.SaveChanges();
    }

    public void DeleteGameState(string id)
    {
        var savesFromDb = GetGameState(id);
        _dbContext.GameStates.Remove(savesFromDb);
        _dbContext.SaveChanges();
    }
}