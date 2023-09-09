using System.ComponentModel.DataAnnotations;

namespace Domain;

public class GameState
{
    public int GameStateId { get; set; }
    
    [MaxLength(128)]
    public string GameStateName { get; set; } = default!;
    
    public string GameBoard { get; set; } = default!;
    
    public bool WhiteTurn { get; set; }
}