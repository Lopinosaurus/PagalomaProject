    using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;
    [SerializeField] private Menu[] menus;

    private void Awake()
    {
        Instance = this;
    }

    public void OpenMenu(string menuName)
    {
        foreach (Menu t in menus)
        {
            if (t.menuName == menuName) t.Open();
            else if (t.open) CloseMenu(t);
        }
    }

    public void OpenMenu(Menu menu)
    {
        foreach (Menu t in menus) if (t.open) CloseMenu(t);

        menu.Open();
    }

    private void CloseMenu(Menu menu) => menu.Close();
}
