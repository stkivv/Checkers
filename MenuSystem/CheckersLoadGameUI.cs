using DAL;
using Domain;

namespace MenuSystem;

public class CheckersLoadGameUI
{
    private Menu _mainMenu;
    public IGameRepository GameRepo;
    private Submenu _parentMenu;
    
    public CheckersLoadGameUI(Menu mainMenu, IGameRepository gameRepo, Submenu parentMenu)
    {
        this._mainMenu = mainMenu;
        this.GameRepo = gameRepo;
        this._parentMenu = parentMenu;
    }
    
    public CheckersGame LoadSave()
    {
        Console.Clear();
        Console.CursorVisible = true;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Write the name of the game you want to load: ");
        Console.ResetColor();
        string input = Console.ReadLine()!;

        CheckersGame? save = GameRepo.GetGame(input);
        if (save == null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Save does not exist. Press any key to go back.");
            Console.ResetColor();
            Console.ReadKey();
            _parentMenu.DrawMenu();
        }

        return save!;
    }

    public void DeleteSave()
    {
        Console.Clear();
        Submenu gamesList = new Submenu("choose game to delete", _parentMenu, _mainMenu);
        List<CheckersGame> games = GameRepo.GetAll();

        foreach (CheckersGame gameSave in games)
        {
            Action<CheckersGame> action = DeleteChosenSave;
            gamesList.AddItem(gameSave.GameName, action, gameSave);
        }
        gamesList.AddItem("Back", _parentMenu.DrawMenu);
        gamesList.DrawMenu();
    }

    private void DeleteChosenSave(CheckersGame game)
    {
        GameRepo.DeleteGame(game);
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Save deleted. press any key to go back.");
        Console.ResetColor();
        Console.ReadKey();
        Console.Clear();
        _mainMenu.DrawMenu();
    }

}