using System.Text.Json;
using ConsoleUI;
using DAL;
using Domain;
using GameBrain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages_CheckersGame;

public class Play : PageModel
{
    private readonly IGameRepository _repo;

    public Play(IGameRepository repo)
    {
        _repo = repo;
    }

    public CheckersBrain Brain { get; set; } = default!;
    public CheckersGame? Game { get; set; } 
    public int? SourceX { get; set; }
    public int? SourceY { get; set; }
    public bool? MoveInProgress { get; set; }
    public bool? GameIsOver { get; set; }
    public EPlayerSide? PlayerSide;

    public async Task<ActionResult> OnGet(int id, EPlayerSide? playerSide, int? sourceY, int? sourceX, int? targetY,
        int? targetX, bool? moveInProgress, bool? changeTurnFlag, bool? undo, bool? gameOverFlag)
    {
        Game = _repo.GetGame(id);
        MoveInProgress = moveInProgress;

        if (Game == null || Game.Options == null || Game.States == null)
        {
            return NotFound();
        }

        PlayerSide = playerSide;
        
        Brain = new CheckersBrain
        {
            Options = Game.Options
        };
        Brain.LoadWidthAndHeight();
        GameState? state = Game.States.LastOrDefault();

        //undo
        if (state != null && undo == true)
        {
            Undo();
            _repo.SaveChanges(Game);
            return RedirectToPage("/CheckersGame/Play",new { id = Game.CheckersGameId, playerSide = PlayerSide });
        }
        
        //new game, no state yet
        if (state == null)
        {
            Brain.InitializeGame();
        }
        else //state exists, assign to current brain
        {
            Brain.GameBoard = Brain.ConvertJaggedArrayToBoard(JsonSerializer.Deserialize<ESquareState[][]>(state.GameBoard)!);
            Brain.WhiteTurn = state.WhiteTurn;
        }

        //change turn (used when canceling a multi-jump)
        if (changeTurnFlag == true)
        {
            Brain.ChangeTurn();
            state!.WhiteTurn = Brain.WhiteTurn;
            _repo.SaveChanges(Game);
            return RedirectToPage("/CheckersGame/Play",new { id = Game.CheckersGameId, playerSide = PlayerSide });
        }
        
        var availableMoves = Brain.GetAllAvailableMoves();

        //no moves left, end game
        if (availableMoves.Count == 0)
        {
            EndGame();
        }
        
        //game has ended
        if (gameOverFlag == true || Game.GameOverAt != null)
        {
            GameIsOver = gameOverFlag;
            if (Game.GameOverAt == null)
            {
                Game.GameOverAt = DateTime.Now;
            }
            _repo.SaveChanges(Game);
            if (gameOverFlag != true)
            {
                return RedirectToPage("/CheckersGame/Play",new { id = Game.CheckersGameId, gameOverFlag=true });
            }
            return Page();
        }

        //AI moves
        if ((!Brain.WhiteTurn && Game.Player2Side == EPlayerSide.BlackSideAI) || 
            (Brain.WhiteTurn && Game.Player1Side == EPlayerSide.WhiteSideAI))
        {
            DoTurnAI();
            _repo.SaveChanges(Game);
            return RedirectToPage("/CheckersGame/Play",new { id = Game.CheckersGameId, playerSide = PlayerSide });
        }

        //Player moves
        if (sourceX != null && sourceY != null) //piece selected
        {
            SourceX = sourceX;
            SourceY = sourceY;
            if (targetX != null && targetY != null) //target selected, making a move
            {
                try
                {
                    Brain.MakeAMove(sourceX.Value, sourceY.Value, targetX.Value, targetY.Value);
                }
                catch (ArgumentException)
                {
                    return RedirectToPage("/CheckersGame/Play",new { id = Game.CheckersGameId, playerSide = PlayerSide });
                }
                
                GameState newState = new GameState()
                {
                    GameStateName = "Web - " + Game.Player1Name + "," + Game.Player2Name +
                                    " - game started at: " + Game.StartedAt + " - saved at: " + DateTime.Now,
                    GameBoard = JsonSerializer.Serialize(Brain.ConvertBoardToJaggedArray())
                };

                Game.States.Add(newState);
                newState.WhiteTurn = Brain.WhiteTurn;

                if (Brain.CanCaptureAgain(targetX.Value, targetY.Value)) //multiple jumps in one move
                {
                    _repo.SaveChanges(Game);
                    return RedirectToPage("/CheckersGame/Play",
                        new {id=Game.CheckersGameId, playerSide = PlayerSide, sourceY=targetY, sourceX=targetX, moveInProgress=true});
                }

                Brain.ChangeTurn();
                newState.WhiteTurn = Brain.WhiteTurn;
                _repo.SaveChanges(Game);
                return RedirectToPage("/CheckersGame/Play", new {id=Game.CheckersGameId, playerSide = PlayerSide});
            }
        }
        return Page();
    }

    private void DoTurnAI()
    {
        //make the move, save landing square to tuple
        Tuple<int, int> landingSquare = Brain.MakeAMoveAI();

        //make a new game state
        GameState newState = new GameState()
        {
            GameStateName = "Web - " + Game!.Player1Name + "," + Game.Player2Name +
                            " - game started at: " + Game.StartedAt + " - saved at: " + DateTime.Now,
            GameBoard = JsonSerializer.Serialize(Brain.ConvertBoardToJaggedArray()),
        };

        Game.States!.Add(newState);
        newState.WhiteTurn = Brain.WhiteTurn;

        //multi-jump move
        while (Brain.CanCaptureAgain(landingSquare.Item1, landingSquare.Item2))
        {
            landingSquare = Brain.MakeAMoveAI();
            newState.GameBoard = JsonSerializer.Serialize(Brain.ConvertBoardToJaggedArray());
        }

        Brain.ChangeTurn();
        newState.WhiteTurn = Brain.WhiteTurn;
    }

    private void EndGame()
    {
        string whiteSidePlayer = Game!.Player1Name;
        string blackSidePlayer = Game.Player2Name;

        var winner = Brain.WhiteTurn ? blackSidePlayer : whiteSidePlayer;

        Game.GameOverAt = DateTime.Now;
        Game.GameWonByPlayer = winner;
    }

    private void Undo()
    {
        if (Game!.States!.Count > 1)
        {
            _repo.DeleteLastState(Game!);
        }
        _repo.SaveChanges(Game!);

        GameState? newState = Game!.States!.LastOrDefault();
        if (newState != null)
        {
            //if AI, undo their move too
            if ((!newState.WhiteTurn &&
                 Game.Player2Side == EPlayerSide.BlackSideAI) ||
                (newState.WhiteTurn &&
                 Game.Player1Side == EPlayerSide.WhiteSideAI))
            {
                _repo.DeleteLastState(Game);
            }
        }
    }
}