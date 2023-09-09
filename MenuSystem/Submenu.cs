namespace MenuSystem;

public class Submenu : Menu
{
    public Menu ParentMenu;
    public Menu RootMenu;

    public Submenu(string title, Menu parentMenu, Menu rootMenu) : base(title)
    {
        ParentMenu = parentMenu;
        RootMenu = rootMenu;
    }

    public void GetPreviousMenu()
    {
        ParentMenu.DrawMenu();
    }

    public void GetRootMenu()
    {
        RootMenu.DrawMenu();
    }

}