namespace AutoRepairShop.UI.Menus;

/// <summary>Пункт меню: клавіша, підпис і дія.</summary>
public sealed class MenuItem
{
    public MenuItem(string key, string caption, Action action)
    {
        Key = key;
        Caption = caption;
        Action = action;
    }

    public string Key { get; }
    public string Caption { get; }
    public Action Action { get; }
}
