using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Db;

public class GameRepositoryDb : IGameRepository
{
    private readonly AppDbContext _ctx;
    public GameRepositoryDb(AppDbContext dbContext)
    {
        _ctx = dbContext;
    }


    public List<CheckersGame> GetAll()
    {
        return _ctx.CheckersGames
            .Include(g => g.Options)
            .OrderByDescending(o => o.StartedAt)
            .ToList();
    }

    public CheckersGame? GetGame(int? id)
    {
        return _ctx.CheckersGames
            .Include(g => g.Options)
            .Include(g => g.States)
            .FirstOrDefault(g => g.CheckersGameId == id);
    }
    
    public CheckersGame? GetGame(string name)
    {
        return _ctx.CheckersGames
            .Include(g => g.Options)
            .Include(g => g.States)
            .FirstOrDefault(g => g.GameName == name);
    }

    public CheckersGame AddGame(CheckersGame game)
    {
        _ctx.CheckersGames.Add(game);
        _ctx.SaveChanges();

        return game;
    }

    public void SaveChanges(CheckersGame game)
    {
        _ctx.SaveChanges();
    }

    public void DeleteLastState(CheckersGame game)
    {
        GameState state = game.States!.LastOrDefault()!;
        _ctx.Entry(state).State = EntityState.Deleted;
    }

    public void DeleteGame(CheckersGame game)
    {
        int stateCount = game.States!.Count;
        for (int i = 0; i < stateCount; i++)
        {
            DeleteLastState(game);
            SaveChanges(game);
        }
        _ctx.Remove(game);
        SaveChanges(game);
    }

}