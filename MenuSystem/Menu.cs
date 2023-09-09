using Domain;

namespace MenuSystem;

public class Menu
{
    private List<MenuItem> _items = new ();
    public string Title;

    public Menu(string title)
    {
        Title = title;
    }

    public void AddItem(string title, Action actionOnselect)
    {
        MenuItem item = new MenuItem(title, actionOnselect);
        _items.Add(item);
    }
    
    public void AddItem(string title, Action<string> actionOnselect)
    {
        MenuItem item = new MenuItem(title, actionOnselect);
        _items.Add(item);
    }
    
    public void AddItem(string title, Action<CheckersGame> actionOnselect, CheckersGame game)
    {
        MenuItem item = new MenuItem(title, actionOnselect, game);
        _items.Add(item);
    }
    
    public void AddItem(string title, Action<CheckersOptions> actionOnselect, CheckersOptions options)
    {
        MenuItem item = new MenuItem(title, actionOnselect, options);
        _items.Add(item);
    }

    public void ChooseItem(int index)
    {
        _items[index].RunItem();
    }

    public void DrawMenu()
    {
        int index = 0;
        DrawMenuItems(_items, _items[index]);
        
        ConsoleKeyInfo keyinfo;
        while(true)
        {
            keyinfo = Console.ReadKey();

            if (keyinfo.Key == ConsoleKey.DownArrow)
            {
                if (index + 1 < _items.Count)
                {
                    index++;
                    DrawMenuItems(_items, _items[index]);
                }
            }
            if (keyinfo.Key == ConsoleKey.UpArrow)
            {
                if (index - 1 >= 0)
                {
                    index--;
                    DrawMenuItems(_items, _items[index]);
                }
            }

            if (keyinfo.Key == ConsoleKey.Enter){
                _items[index].RunItem();
                index = 0;
            }

        }
        
    }

    private void DrawMenuItems(List<MenuItem> items, MenuItem selectedItem)
    {
        // https://stackoverflow.com/questions/60767909/c-sharp-console-app-how-do-i-make-an-interactive-menu
        Console.Clear();
        Console.CursorVisible = false;
        Console.WriteLine("");
        Console.WriteLine("");
        Console.WriteLine("");
        Console.WriteLine("");


        Console.ForegroundColor = ConsoleColor.Magenta;
        MakeSpacesToCenterText(Title.Length);
        Console.WriteLine(Title);
        Console.ResetColor();
        
        MakeSpacesToCenterText(36);
        Console.WriteLine("____________________________________");
        Console.WriteLine("");

        foreach (MenuItem item in items)
        {
            if (item == selectedItem)
            {
                MakeSpacesToCenterText(30);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(item.ItemTitle + " <");
                Console.ResetColor();
            }
            else
            {
                MakeSpacesToCenterText(30);
                Console.WriteLine(item.ItemTitle);

            }

        }
        MakeSpacesToCenterText(36);
        Console.WriteLine("____________________________________");
        
    }

    private void MakeSpacesToCenterText(int textLength)
    {
        int spacesToMake = Console.WindowWidth / 2 - textLength / 2;
        for (int i = 0; i < spacesToMake; i++)
        {
            Console.Write(" ");
        }
    }

    public void CloseMenu()
    {
        Environment.Exit(0);
    }
    
}