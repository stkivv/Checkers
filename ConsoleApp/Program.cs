// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using ConsoleApp;
using ConsoleUI;
using DAL;
using DAL_FileSystem;
using DAL.Db;
using Domain;
using GameBrain;
using MenuSystem;
using Microsoft.EntityFrameworkCore;

//=====================================INITIAL SETUP==========================================

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.Title = "CHECKERS";

var dbOptions = new DbContextOptionsBuilder<AppDbContext>().UseSqlite("Data Source=c:/CheckersDatabase/checkers.db").Options;
using var ctx = new AppDbContext(dbOptions);

IGameOptionsRepository optionsRepoFs = new GameOptionsRepository();
IGameOptionsRepository optionsRepoDb = new GameOptionsRepositoryDb(ctx);
IGameOptionsRepository optionsRepo = optionsRepoFs;

IGameRepository gameRepoFs = new GameRepository();
IGameRepository gameRepoDb = new GameRepositoryDb(ctx);
IGameRepository gameRepo = gameRepoFs;

UI ui = new UI();
CheckersBrain brain = new CheckersBrain();
CheckersGame game = new CheckersGame();


//================================MENU SETUP======================================
Menu mainMenu = new Menu("Main Menu");
Submenu optionsMenu = new Submenu("Options", mainMenu, mainMenu);
Submenu loadGameMenu = new Submenu("Load Game", mainMenu, mainMenu);

CheckersOptionsUI optionsUI = new CheckersOptionsUI(brain, mainMenu, optionsRepo, optionsMenu);
CheckersLoadGameUI loadUI = new CheckersLoadGameUI(mainMenu, gameRepo, loadGameMenu);

mainMenu.AddItem("New Game", NewGame);
mainMenu.AddItem("Load Game", loadGameMenu.DrawMenu);
mainMenu.AddItem("View Games", ViewGames);
mainMenu.AddItem("Options", optionsMenu.DrawMenu);
mainMenu.AddItem("Swap to database/filesystem", SwapPersistenceEngine);
mainMenu.AddItem("Exit", mainMenu.CloseMenu);

optionsMenu.AddItem("View options", DisplayOptionsList);
optionsMenu.AddItem("Create options", optionsUI.CreateOptions);
optionsMenu.AddItem("Edit options", optionsUI.EditOptions);
optionsMenu.AddItem("Delete options", optionsUI.DeleteOption);
optionsMenu.AddItem("Back", optionsMenu.GetRootMenu);

loadGameMenu.AddItem("Load by name", ChooseSaveByName);
loadGameMenu.AddItem("Load gamesave from list", ChooseSaveFromList);
loadGameMenu.AddItem("Delete save", loadUI.DeleteSave);
loadGameMenu.AddItem("Back", loadGameMenu.GetRootMenu);

StartScreen.MakeStartScreen();
mainMenu.DrawMenu();


//================================PLAYING==================================

void NewGame()
{
    game = new CheckersGame();
    Console.Clear();
    if (brain.Options == null)
    {
        ChooseOptions();
    }
    game.Options = brain.Options;

    Console.CursorVisible = true;
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("Enter name for Player 1 (white side): ");
    Console.ResetColor();
    game.Player1Name = Console.ReadLine()!;
    
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("Enter name for Player 2 (black side): ");
    Console.ResetColor();
    game.Player2Name = Console.ReadLine()!;
    
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("Should white side be AI (Y/N)?: ");
    Console.ResetColor();
    var whiteSideType = Console.ReadLine();
    game.Player1Side = whiteSideType == "Y" ? EPlayerSide.WhiteSideAI : EPlayerSide.WhiteSide;
    
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("Should black side be AI (Y/N)?: ");
    Console.ResetColor();
    var blackSideType = Console.ReadLine();
    game.Player2Side = blackSideType == "Y" ? EPlayerSide.BlackSideAI : EPlayerSide.BlackSide;
    
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("Enter name for this game: ");
    Console.ResetColor();
    var name = Console.ReadLine();
    game.GameName = name!;
    
    brain.InitializeGame();
    game.StartedAt = DateTime.Now;

    GameState state = new GameState()
    {
        GameStateName = "Console - " + game.Player1Name + "," + game.Player2Name +
                        " - game started at: " + game.StartedAt + " - saved at: --",
        GameBoard = JsonSerializer.Serialize(brain.ConvertBoardToJaggedArray()),
        WhiteTurn = brain.WhiteTurn
    };
    game.States = new List<GameState>();
    game.States.Add(state);
    gameRepo.AddGame(game);
    gameRepo.SaveChanges(game);
    
    ui.DrawGameBoard(brain.GetBoard());
    InputPrompt();
}

void ChooseOptions()
{
    Console.Clear();
    Submenu options = new Submenu("Choose options", mainMenu, mainMenu);
    options.AddItem("Choose option from list", optionsUI.ChooseOptionFromList);
    options.AddItem("Load options by name", optionsUI.LoadOption);
    options.AddItem("Back", options.GetRootMenu);
    options.DrawMenu();
}

void SwapPersistenceEngine()
{
    if (optionsRepo == optionsRepoFs)
    {
        optionsRepo = optionsRepoDb;
        optionsUI.OptionsRepo = optionsRepo;
        gameRepo = gameRepoDb;
        loadUI.GameRepo = gameRepo;
        
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Swapped to database. Press any key to return.");
        Console.ResetColor();
        Console.ReadKey();
        mainMenu.DrawMenu();
    }
    else
    {
        optionsRepo = optionsRepoFs;
        optionsUI.OptionsRepo = optionsRepo;
        gameRepo = gameRepoFs;
        loadUI.GameRepo = gameRepo;
        
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Swapped to file system. Press any key to return.");
        Console.ResetColor();
        Console.ReadKey();
        mainMenu.DrawMenu();
    }
}

void DisplayOptionsList()
{
    Console.Clear();
    Submenu optionsListMenu = new Submenu("Choose options for details", optionsMenu, mainMenu);
    var options = optionsRepo.GetGameOptionsList();
    foreach (var o in options)
    {
        Action<CheckersOptions> action = DisplayOptionsInMenus;
        optionsListMenu.AddItem(o, action, optionsRepo.GetGameOptions(o));
    }
    optionsListMenu.AddItem("Back", optionsMenu.DrawMenu);
    optionsListMenu.DrawMenu();
}

//===========================PROMPTER=============================

void InputPrompt(bool errorMessage = false, bool isMultiJump = false)
{
    //resetting brain parameters
    if (isMultiJump == false)
    {
        brain.PieceTakenFlag = false;
        brain.PieceToRemove = null;
    }
    
    var availableMoves = brain.GetAllAvailableMoves();

    //no moves left, end game
    if (availableMoves.Count == 0)
    {
        EndGame();
        gameRepo.SaveChanges(game);
    }
    
    //display game over message
    if (game.GameOverAt != null)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("");
        MakeSpacesToCenterText(36);
        Console.WriteLine("GAME OVER");
        Console.ResetColor();
        Console.WriteLine("");
        MakeSpacesToCenterText(36);
        Console.WriteLine("        Winner is: " + game.GameWonByPlayer);
        MakeSpacesToCenterText(36);
        Console.WriteLine("        press any key to return to main menu");
        Console.ReadKey();
        mainMenu.DrawMenu();
        return;
    }
    
    //AI move
    if ((brain.WhiteTurn == false && game.Player2Side == EPlayerSide.BlackSideAI) ||
        (brain.WhiteTurn && game.Player1Side == EPlayerSide.WhiteSideAI))
    {
        DoTurnAI();
        return;
    }
    
    Console.WriteLine("");
    MakeSpacesToCenterText(36);
    Console.WriteLine("____________________________________");
    
    if (errorMessage)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        MakeSpacesToCenterText(36);
        Console.WriteLine("Invalid move! Try again!");
        Console.ResetColor();
    }

    if (isMultiJump)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        MakeSpacesToCenterText(36);
        Console.WriteLine("Another piece can be captured!");
        Console.ResetColor(); 
    }
    
    Console.CursorVisible = true;
    MakeSpacesToCenterText(36);
    Console.WriteLine("choose H for prompt command list");
    Console.ForegroundColor = ConsoleColor.Yellow;
    MakeSpacesToCenterText(36);
    Console.WriteLine(brain.WhiteTurn ? "White side turn" : "Black side turn");
    Console.ResetColor();
    MakeSpacesToCenterText(36);
    Console.Write("Choose a move:");
    var input = Console.ReadLine();
    
    switch (input)
    {
        case "H":
            DisplayCommandsHelp();
            break;
        case "X":
            ExitGame();
            break;
        case "U":
            Undo();
            break;
        case "END":
            EndGame(true);
            gameRepo.SaveChanges(game);
            ui.DrawGameBoard(brain.GetBoard());
            InputPrompt();
            break;
        case "CANCEL":
            if (isMultiJump == false || game.Options!.CaptureMandatory)
            {
                ui.DrawGameBoard(brain.GetBoard());   
                InputPrompt(false, true);
                break;
            }
            SaveGameStateAndEndTurn();
            ui.DrawGameBoard(brain.GetBoard());   
            InputPrompt();
            break;
        case "O":
            DisplayOptions(game.Options!);
            break;
        default:
            DoTurnPlayer(input!);
            break;
    }
}

void MakeSpacesToCenterText(int length)
{
    Console.BackgroundColor = ConsoleColor.Black;
    int spacesToMake = Console.WindowWidth / 2 - length / 2;
    for (int i = 0; i < spacesToMake; i++)
    {
        Console.Write(" ");
    }
}

void DoTurnPlayer(string input)
{
    List<int> coords;
    try
    { 
        coords = CoordinateTranslator(input);
    }
    catch
    {
        ui.DrawGameBoard(brain.GetBoard());
        InputPrompt(true);
        return;
    }
    if (!brain.CoordinatesAreWithinBounds(coords[0], coords[1]) ||
        !brain.CoordinatesAreWithinBounds(coords[2], coords[3]))
    {
        ui.DrawGameBoard(brain.GetBoard());
        InputPrompt(true);
        return;
    }
    try
    {
        brain.MakeAMove(coords[0], coords[1], coords[2], coords[3]);
    }
    catch (ArgumentException)
    {
        ui.DrawGameBoard(brain.GetBoard());
        InputPrompt(true);
        return;
    }
        
    //multi-jump aka capturing several pieces in one move
    if (brain.CanCaptureAgain(coords[2], coords[3]))
    { 
        //SaveGameStateAndEndTurn();
        ui.DrawGameBoard(brain.GetBoard());   
        InputPrompt(false, true);
        return;
    }
        
    SaveGameStateAndEndTurn();
    ui.DrawGameBoard(brain.GetBoard());
    InputPrompt();
}

void SaveGameStateAndEndTurn()
{
    GameState newState = new GameState()
    {
        GameStateName = "Console - " + game.Player1Name + "," + game.Player2Name +
                        " - game started at: " + game.StartedAt + " - saved at: " + DateTime.Now,
        GameBoard = JsonSerializer.Serialize(brain.ConvertBoardToJaggedArray())
    };

    game.States!.Add(newState);
    brain.ChangeTurn();
    newState.WhiteTurn = brain.WhiteTurn;
    gameRepo.SaveChanges(game);
}

void DoTurnAI()
{
    Tuple<int, int> landingSquare = brain.MakeAMoveAI();
    GameState newState = new GameState()
    {
        GameStateName = "Console - " + game.Player1Name + "," + game.Player2Name +
                        " - game started at: " + game.StartedAt + " - saved at: " + DateTime.Now,
        GameBoard = JsonSerializer.Serialize(brain.ConvertBoardToJaggedArray()),
    };

    game.States!.Add(newState);
    
    //multi-jump move
    while (brain.CanCaptureAgain(landingSquare.Item1, landingSquare.Item2))
    {
        landingSquare = brain.MakeAMoveAI();
        newState.GameBoard = JsonSerializer.Serialize(brain.ConvertBoardToJaggedArray());
    }
    
    brain.ChangeTurn();
    newState.WhiteTurn = brain.WhiteTurn;
    gameRepo.SaveChanges(game);
    
    ui.DrawGameBoard(brain.GetBoard());
    InputPrompt();
}

//coordinates are returned in [sourceX, sourceY, targetX, targetY] format
List<int> CoordinateTranslator(string coords)
{ 
    List<int> result = new List<int>();
    List<char> alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToList();

    string[] parts = coords.Split(" ");
    string source = parts[0];
    string target = parts[1];

    string sourceYString = source.Substring(0,1);
    int? sourceY = null;
    int sourceX = Int32.Parse(source.Substring(1)) - 1;
    
    string targetYString = target.Substring(0,1);
    int? targetY = null;
    int targetX = Int32.Parse(target.Substring(1)) - 1;

    for (int i = 0; i < alphabet.Count; i++)
    {
        if (alphabet[i].ToString() == sourceYString)
        {
            sourceY = i;
        }
        if (alphabet[i].ToString() == targetYString)
        {
            targetY = i;
        }
    }

    if (sourceY == null || targetY == null)
    {
        throw new Exception("invalid coordinate!");
    }
    
    result.AddRange(new []{sourceX, sourceY.Value, targetX, targetY.Value});
    
    return result;
}

void DisplayCommandsHelp()
{
    Console.Clear();
    Console.WriteLine("These are the commands you can use");
    Console.WriteLine("____________________________________");
    Console.WriteLine("H - help");
    Console.WriteLine("U - undo");
    Console.WriteLine("X - exit game");
    Console.WriteLine("O - display options");
    Console.WriteLine("END - end the game");
    Console.WriteLine("CANCEL - cancel a multi-jump (only usable if capture is not mandatory)");
    Console.WriteLine("");
    Console.WriteLine("Moves can be made in format 'B2 C3' where");
    Console.WriteLine("the first coordinate is the piece you want to move");
    Console.WriteLine("and the second coordinate is where you want to move it");
    Console.WriteLine("____________________________________");
    Console.WriteLine("Press any key to go back.");
    Console.ReadKey();
    Console.Clear();
    ui.DrawGameBoard(brain.GetBoard());
    InputPrompt();
}

void ExitGame()
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("You are about to close the game.");
    Console.WriteLine("Progress has been saved automatically.");
    Console.WriteLine("Do you wish to continue (Y/N)?");
    Console.ResetColor();
    var input = Console.ReadLine();
    switch (input)
    {
        case "Y":
            brain = new CheckersBrain();
            optionsUI = new CheckersOptionsUI(brain, mainMenu, optionsRepo, optionsMenu);
            mainMenu.DrawMenu();
            break;
        case "N":
            ui.DrawGameBoard(brain.GetBoard());
            InputPrompt();
            break;
        default:
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Invalid input. Press any key to try again.");
            Console.ReadKey();
            ExitGame();
            break;
    }
}

void Undo()
{
    if (game.States!.Count > 1)
    {
        gameRepo.DeleteLastState(game);
    }
    gameRepo.SaveChanges(game);

    GameState? previousState = game.States!.LastOrDefault();
    if (previousState != null && game.States!.Count > 1)
    {
        //if AI, undo their move too
        if ((previousState.WhiteTurn == false && game.Player2Side == EPlayerSide.BlackSideAI) ||
            (previousState.WhiteTurn && game.Player1Side == EPlayerSide.WhiteSideAI))
        {
            gameRepo.DeleteLastState(game);
            gameRepo.SaveChanges(game);
        }
        OpenGame(game);
    }
    else
    {
        brain.InitializeGame();
        GameState newState = new GameState()
        {
            GameStateName = "Console - " + game.Player1Name + "," + game.Player2Name +
                            " - game started at: " + game.StartedAt + " - saved at: " + DateTime.Now,
            GameBoard = JsonSerializer.Serialize(brain.ConvertBoardToJaggedArray()),
            WhiteTurn = brain.WhiteTurn
        };

        game.States!.Add(newState);
        gameRepo.SaveChanges(game);
        ui.DrawGameBoard(brain.GetBoard());
        InputPrompt();
    }
}

void EndGame(bool noWinner = false)
{
    if (noWinner == false)
    {
        string whiteSidePlayer = game.Player1Name;
        string blackSidePlayer = game.Player2Name;

        var winner = brain.WhiteTurn ? blackSidePlayer : whiteSidePlayer;
        game.GameWonByPlayer = winner;
    }
    game.GameOverAt = DateTime.Now;
}

void DisplayOptions(CheckersOptions options)
{
    DisplayOptionsInformation(options);
    ui.DrawGameBoard(brain.GetBoard());
    InputPrompt();
}

void DisplayOptionsInMenus(CheckersOptions options)
{
    DisplayOptionsInformation(options);
    optionsMenu.DrawMenu();
}

void DisplayOptionsInformation(CheckersOptions options)
{
    Console.Clear();
    Console.WriteLine("");
    Console.WriteLine(options.OptionsName);
    Console.WriteLine("___________________________");
    Console.WriteLine("");
    Console.WriteLine("board width:" + options.Width);
    Console.WriteLine("board height:" + options.Height);
    Console.WriteLine("capture is mandatory: " + options.CaptureMandatory);
    Console.WriteLine("can capture backwards: " + options.CaptureBackwardsAllowed);
    Console.WriteLine("white side starts: " + options.WhiteStarts);
    Console.WriteLine("kings can fly: " + options.KingCanFly);
    Console.WriteLine("___________________________");
    Console.WriteLine("press any key to return");
    Console.ReadKey();
}

//==================================LOADING================================
void ChooseSaveByName()
{
    CheckersGame gameSave = loadUI.LoadSave();
    game = gameSave;
    GameState state = gameSave.States!.LastOrDefault()!;
    brain.Options = game.Options;
    brain.GameBoard = brain.ConvertJaggedArrayToBoard(JsonSerializer.Deserialize<ESquareState[][]>(state.GameBoard)!);
    brain.WhiteTurn = state.WhiteTurn;
    brain.LoadWidthAndHeight();
    
    ui.DrawGameBoard(brain.GetBoard());
    InputPrompt();
}

void ChooseSaveFromList()
{
    Submenu gamesList = new Submenu("Game saves", mainMenu, mainMenu);
    List<CheckersGame> games = gameRepo.GetAll();

    foreach (CheckersGame gameSave in games)
    {
        Action<CheckersGame> action = OpenGame;
        gamesList.AddItem(gameSave.GameName, action, gameSave);
    }
    gamesList.AddItem("Back", loadGameMenu.DrawMenu);
    gamesList.DrawMenu();
}

void OpenGame(CheckersGame gameSave)
{
    game = gameRepo.GetGame(gameSave.CheckersGameId)!;
    GameState state = gameSave.States!.LastOrDefault()!;
    brain.Options = game.Options;
    brain.GameBoard = brain.ConvertJaggedArrayToBoard(JsonSerializer.Deserialize<ESquareState[][]>(state.GameBoard)!);
    brain.WhiteTurn = state.WhiteTurn;
    brain.LoadWidthAndHeight();
    
    ui.DrawGameBoard(brain.GetBoard());
    InputPrompt();
}


void ViewGames()
{
    Console.Clear();
    Submenu gameList = new Submenu("Game list. Choose to see states.", mainMenu, mainMenu);
    var games = gameRepo.GetAll();
    foreach (var g in games)
    {
        Action<CheckersGame> action = ViewGameStates;
        gameList.AddItem(g.GameName, action, g);
    }
    gameList.AddItem("Back", mainMenu.DrawMenu);
    gameList.DrawMenu();
}

void ViewGameStates(CheckersGame gameToView)
{
    Console.Clear();
    Console.WriteLine($"States for {gameToView.GameName}. Press any key to return.");
    Console.WriteLine("____________________________________");
    Console.WriteLine("");
    var states = gameToView.States!.ToList();
    foreach (var state in Enumerable.Reverse(states))
    {
        Console.WriteLine(state.GameStateName);
    }
    Console.WriteLine("____________________________________");
    Console.ReadKey();
    mainMenu.DrawMenu();
}
