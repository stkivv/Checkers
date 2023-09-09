using Domain;

namespace MenuSystem;

public class MenuItem
{
    public string ItemTitle { get; set; }
    private Action? ActionOnSelect { get; set; }
    private Action<string>? ActionOnSelectWithString { get; set; }
    private Action<CheckersGame>? ActionOnSelectWithGame { get; set; }
    private readonly CheckersGame? _game;
    private Action<CheckersOptions>? ActionOnSelectWithOptions { get; set; }
    private readonly CheckersOptions? _options;

    public MenuItem(string title, Action actionOnSelect)
    {
        ItemTitle = title;
        ActionOnSelect = actionOnSelect;
    }
    public MenuItem(string title, Action<string> actionOnSelect)
    {
        ItemTitle = title;
        ActionOnSelectWithString = actionOnSelect;
    }
    
    public MenuItem(string title, Action<CheckersGame> actionOnSelect, CheckersGame game)
    {
        ItemTitle = title;
        ActionOnSelectWithGame = actionOnSelect;
        _game = game;
    }
    
    public MenuItem(string title, Action<CheckersOptions> actionOnSelect, CheckersOptions options)
    {
        ItemTitle = title;
        ActionOnSelectWithOptions = actionOnSelect;
        _options = options;
    }
    
    public override string ToString()
    {
        return ItemTitle;
    }

    public void RunItem()
    {
        if (ActionOnSelect != null)
        {
            ActionOnSelect.Invoke();
        }
        else if (ActionOnSelectWithString != null)
        {
            ActionOnSelectWithString.Invoke(ItemTitle);
        }
        else if (ActionOnSelectWithGame != null)
        {
            ActionOnSelectWithGame.Invoke(_game!);
        }
        else if (ActionOnSelectWithOptions != null)
        {
            ActionOnSelectWithOptions.Invoke(_options!);
        }
    }
}