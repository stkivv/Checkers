using System.ComponentModel.DataAnnotations;

namespace Domain;

public class CheckersGame
{
    public int? CheckersGameId { get; set; }
    
    [MaxLength(128)] 
    public string GameName { get; set; } = default!;

    public DateTime StartedAt { get; set; } = DateTime.Now;
    public DateTime? GameOverAt { get; set; }
    public string? GameWonByPlayer { get; set; }

    [MaxLength(128)] 
    public string Player1Name { get; set; } = default!;

    public EPlayerSide Player1Side { get; set; }
    
    [MaxLength(128)] 
    public string Player2Name { get; set; } = default!;
    
    public EPlayerSide Player2Side { get; set; }
    
    public int CheckersOptionsId { get; set; }
    public CheckersOptions? Options { get; set; }

    public ICollection<GameState>? States { get; set; }
}