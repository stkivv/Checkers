using System.ComponentModel.DataAnnotations;

namespace Domain;

public class CheckersOptions
{
    public int CheckersOptionsId { get; set; }
    
    [MaxLength(128)] 
    public string OptionsName { get; set; } = default!;
    
    public int Width { get; set; } = 8;
    
    public int Height { get; set; } = 8;
    
    public bool WhiteStarts { get; set; }= true;
    
    public bool CaptureMandatory { get; set; } = true;
    
    public bool CaptureBackwardsAllowed { get; set; }= true;
    
    public bool KingCanFly { get; set; } = true;
    
    public int PieceFilledRowsPerSide { get; set; } = 3;


    public override string ToString()
    {
        return $"Board: {Width} x {Height}, " +
               $"white starts: {WhiteStarts}, " +
               $"must capture: {CaptureMandatory}, " +
               $"can capture backwards: {CaptureBackwardsAllowed}, " +
               $"kings can fly: {KingCanFly}, " +
               $"nr of rows to be filled with pieces on each side: {PieceFilledRowsPerSide}";
    }
}