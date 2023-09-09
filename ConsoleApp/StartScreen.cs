namespace ConsoleApp;

public static class StartScreen
{
    public static void MakeStartScreen()
    {
        Console.CursorVisible = false;
        for (int i = 0; i < 15; i++)
        {
            Console.WriteLine("");
        }

        MakeLineForStartScreen("░█████╗░██╗░░██╗███████╗░█████╗░██╗░░██╗███████╗██████╗░░██████╗", ConsoleColor.Red);
        MakeLineForStartScreen("██╔══██╗██║░░██║██╔════╝██╔══██╗██║░██╔╝██╔════╝██╔══██╗██╔════╝", ConsoleColor.Yellow);
        MakeLineForStartScreen("██║░░╚═╝███████║█████╗░░██║░░╚═╝█████═╝░█████╗░░██████╔╝╚█████╗░", ConsoleColor.Green);
        MakeLineForStartScreen("██║░░██╗██╔══██║██╔══╝░░██║░░██╗██╔═██╗░██╔══╝░░██╔══██╗░╚═══██╗", ConsoleColor.Cyan);
        MakeLineForStartScreen("╚█████╔╝██║░░██║███████╗╚█████╔╝██║░╚██╗███████╗██║░░██║██████╔╝", ConsoleColor.Blue);
        MakeLineForStartScreen("░╚════╝░╚═╝░░╚═╝╚══════╝░╚════╝░╚═╝░░╚═╝╚══════╝╚═╝░░╚═╝╚═════╝░", ConsoleColor.Magenta);
        Console.WriteLine("");
        MakeLineForStartScreen("                   Press any key to continue", ConsoleColor.White);
        Console.ReadKey();
    }

    private static void MakeLineForStartScreen(string text, ConsoleColor col)
    {
        Console.ForegroundColor = col;
        int spacesToMake = Console.WindowWidth / 2 - 32;
        for (int i = 0; i < spacesToMake; i++)
        {
            Console.Write(" ");
        }
        Console.WriteLine(text);
    }
}