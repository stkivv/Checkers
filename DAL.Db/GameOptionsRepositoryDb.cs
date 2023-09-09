using Domain;

namespace DAL.Db;

public class GameOptionsRepositoryDb : IGameOptionsRepository
{

    private readonly AppDbContext _dbContext;
    
    public GameOptionsRepositoryDb(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public List<string> GetGameOptionsList()
    {
        List<string> result = _dbContext.GameOptions.Select(o => o.OptionsName).ToList();
        result.Sort();
        return result;
    }

    public CheckersOptions GetGameOptions(string id)
    {
        return _dbContext.GameOptions.First(o => o.OptionsName == id);
    }

    public void SaveGameOptions(string id, CheckersOptions options)
    {
        var optionsFromDb = _dbContext.GameOptions.FirstOrDefault(o => o.OptionsName == id);

        if (optionsFromDb == null)
        {
            _dbContext.GameOptions.Add(options);
            _dbContext.SaveChanges();
            return;
        }
        optionsFromDb.OptionsName = options.OptionsName;
        optionsFromDb.Height = options.Height;
        optionsFromDb.Width = options.Width;
        optionsFromDb.WhiteStarts = options.WhiteStarts;
        optionsFromDb.CaptureMandatory = options.CaptureMandatory;
        optionsFromDb.CaptureBackwardsAllowed = options.CaptureBackwardsAllowed;
        optionsFromDb.PieceFilledRowsPerSide = options.PieceFilledRowsPerSide;
        optionsFromDb.KingCanFly = options.KingCanFly;
        _dbContext.SaveChanges();
    }

    public void DeleteGameOptions(string id)
    {
        var optionsFromDb = GetGameOptions(id);
        _dbContext.GameOptions.Remove(optionsFromDb);
        _dbContext.SaveChanges();
    }
}