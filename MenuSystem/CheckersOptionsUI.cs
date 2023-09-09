using DAL;
using Domain;
using GameBrain;

namespace MenuSystem;

public class CheckersOptionsUI
{
    private readonly CheckersBrain _brain;
    private readonly Menu _mainMenu;
    public IGameOptionsRepository OptionsRepo;
    private readonly Submenu _parentMenu;
    
    public CheckersOptionsUI(CheckersBrain brain, Menu mainMenu, IGameOptionsRepository optionsRepo, Submenu parentMenu)
    {
        _brain = brain;
        _mainMenu = mainMenu;
        OptionsRepo = optionsRepo;
        _parentMenu = parentMenu;
    }

    private void PostAlert(string message, bool sameLine = false)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        if (sameLine)
        {
            Console.Write(message);
        }
        else
        {
            Console.WriteLine(message);
        }
        Console.ResetColor();
    }
    
    public void LoadOption()
    {
        Console.Clear();
        Console.CursorVisible = true;
        PostAlert("Write the name of the desired options: ", true);
        string? id = Console.ReadLine();
        LoadOptionWhenNameIsKnown(id!);
    }
    
    private void LoadOptionWhenNameIsKnown(string id)
    {
        try
        {
            _brain.Options = OptionsRepo.GetGameOptions(id);
        }
        catch (FileNotFoundException)
        {
            PostAlert("Options not found! press any key to try again.");
            Console.ReadKey();
        }
        _mainMenu.ChooseItem(0);
    }

    public void DeleteOption()
    {
        Console.Clear();
        Console.CursorVisible = true;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Enter the name of the option set to delete: ");
        Console.ResetColor();
        string? id = Console.ReadLine();

        try
        {
            OptionsRepo.DeleteGameOptions(id!);
        }
        catch(FileNotFoundException)
        {
            HandleIllegalArgument("File does not exist.", DeleteOption);
        }
        
        PostAlert("Options deleted. Press any key to return to the main menu.");
        Console.ReadKey();
        Console.CursorVisible = false;
        _mainMenu.DrawMenu();
    }

    
    public void ChooseOptionFromList()
    {
        Submenu optionsList = new Submenu("options list", _parentMenu, _mainMenu);
        List<string> options = OptionsRepo.GetGameOptionsList();

        foreach (string option in options)
        {
            Action<string> action = LoadOptionWhenNameIsKnown;
            optionsList.AddItem(option, action);
        }
        optionsList.AddItem("Back", _parentMenu.DrawMenu);
        
        optionsList.DrawMenu();
    }

    
    public void CreateOptions()
    {
        Console.Clear();
        Console.CursorVisible = true;
        PostAlert("Please choose the following settings");
        
        Console.Write("Board width: ");
        int width = Convert.ToInt32(Console.ReadLine());
        Console.Write("Board height: ");
        int height = Convert.ToInt32(Console.ReadLine());

        if (height < 3 || width < 3)
        {
            HandleIllegalArgument("Board size too small!", CreateOptions);
        }
        
        if (width > 26)
        {
            HandleIllegalArgument("Board can not be wider than 26 squares, as there aren't enough characters in the alphabet", CreateOptions);
        }
        
        Console.Write("White side starts (Y/N): ");
        string? input = Console.ReadLine();
        bool whiteStarts = HandleYesNoStatements(input, CreateOptions);

        Console.Write("Must capture pieces when possible (Y/N): ");
        string? input2 = Console.ReadLine();
        bool captureMandatory = HandleYesNoStatements(input2, CreateOptions);

        Console.Write("Can capture pieces backwards (Y/N): ");
        string? input3 = Console.ReadLine();
        bool canCaptureBackwards = HandleYesNoStatements(input3, CreateOptions);
        
        Console.Write("King pieces can fly (Y/N): ");
        string? input4 = Console.ReadLine();
        bool kingsCanFly = HandleYesNoStatements(input4, CreateOptions);
        
        Console.Write("Number of rows on each side to be filled with pieces: ");
        int pieceFilledRows = Convert.ToInt32(Console.ReadLine());
        if ((height - (pieceFilledRows * 2)) < 1)
        {
            HandleIllegalArgument("Too many rows of pieces for the given height. Playing is not possible.", CreateOptions);
        }
        
        Console.Write("Enter the desired name for these options: ");
        string? name = Console.ReadLine();
        if (name == null)
        {
            HandleIllegalArgument("Options must have a name!", CreateOptions);
        }

        CheckersOptions options = new CheckersOptions
        {
            OptionsName = name!,
            Height = height,
            Width = width,
            CaptureMandatory = captureMandatory,
            WhiteStarts = whiteStarts,
            CaptureBackwardsAllowed = canCaptureBackwards,
            KingCanFly = kingsCanFly,
            PieceFilledRowsPerSide = pieceFilledRows
        };
        OptionsRepo.SaveGameOptions(name!, options);
        

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Options have been saved. Press any key to return to the previous menu.");
        Console.ResetColor();
        Console.ReadKey();
        Console.CursorVisible = false;
        _parentMenu.DrawMenu();
    }

    
    public void EditOptions()
    {
        Console.Clear();
        Console.CursorVisible = true;
        PostAlert("Please choose the options to edit: ", true);
        string? id = Console.ReadLine();
        if (id == null)
        {
            HandleIllegalArgument("No such options!", EditOptions);
        }
        CheckersOptions currentOptions = OptionsRepo.GetGameOptions(id!);
        
        Console.WriteLine("Current board width is: " + currentOptions.Width);
        Console.Write("New board width: ");
        int width = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine("");
        
        Console.WriteLine("Current board height is: " + currentOptions.Height);
        Console.Write("New board height: ");
        int height = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine("");
        
        if (height < 3 || width < 3)
        {
            HandleIllegalArgument("Board size too small!", EditOptions);
        }
        
        if (width > 26)
        {
            HandleIllegalArgument("Board can not be wider than 26 squares, as there aren't enough characters in the alphabet", EditOptions);
        }

        Console.WriteLine("Current setting is: " + currentOptions.WhiteStarts);
        Console.Write("White side starts (Y/N): ");
        string? input = Console.ReadLine();
        bool whiteStarts = HandleYesNoStatements(input, EditOptions);
        Console.WriteLine("");

        Console.WriteLine("Current setting is: " + currentOptions.CaptureMandatory);
        Console.Write("Must capture pieces when possible (Y/N): ");
        string? input2 = Console.ReadLine();
        bool captureMandatory = HandleYesNoStatements(input2, EditOptions);
        Console.WriteLine("");

        Console.WriteLine("Current setting is: " + currentOptions.CaptureBackwardsAllowed);
        Console.Write("Can capture pieces backwards (Y/N): ");
        string? input3 = Console.ReadLine();
        bool canCaptureBackwards = HandleYesNoStatements(input3, EditOptions);
        Console.WriteLine("");

        Console.WriteLine("Current setting is: " + currentOptions.KingCanFly);
        Console.Write("King pieces can fly (Y/N): ");
        string? input4 = Console.ReadLine();
        bool kingsCanFly = HandleYesNoStatements(input4, EditOptions);
        Console.WriteLine("");

        Console.WriteLine("Number of rows currently filled with pieces per each side: " + currentOptions.PieceFilledRowsPerSide);
        Console.Write("Number of rows on each side to be filled with pieces: ");
        int pieceFilledRows = Convert.ToInt32(Console.ReadLine());
        if ((height - (pieceFilledRows * 2)) < 1)
        {
            HandleIllegalArgument("Too many rows of pieces for the given height. Playing is not possible.", EditOptions);
        }
        Console.WriteLine("");

        CheckersOptions options = new CheckersOptions
        {
            OptionsName = id!,
            Height = height,
            Width = width,
            CaptureMandatory = captureMandatory,
            WhiteStarts = whiteStarts,
            CaptureBackwardsAllowed = canCaptureBackwards,
            KingCanFly = kingsCanFly,
            PieceFilledRowsPerSide = pieceFilledRows
        };
        OptionsRepo.SaveGameOptions(id!, options);
        

        PostAlert("Options have been edited. Press any key to return to the previous menu.");
        Console.ReadKey();
        Console.CursorVisible = false;
        _parentMenu.DrawMenu();
    }

    private void HandleIllegalArgument(string message, Action method)
    {
        Console.WriteLine("");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.WriteLine("Do you want to try again (Y/N)?");
        Console.ResetColor();
        Console.Write("choose: ");
        var input = Console.ReadLine();
        if (input == "Y")
        {
            method.Invoke();
        }
        else
        {
            _parentMenu.DrawMenu();
        }
    }

    private bool HandleYesNoStatements(string? input, Action methodToRunOnIllegalArgument)
    {
        bool result = true;
        switch (input)
        {
            case "Y":
                result = true;
                break;
            case "N":
                result = false;
                break;
            default: HandleIllegalArgument("Invalid parameter given!", methodToRunOnIllegalArgument);
                break;
        }

        return result;
    }
}