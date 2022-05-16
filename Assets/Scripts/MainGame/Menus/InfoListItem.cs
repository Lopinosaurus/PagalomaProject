using MainGame.PlayerScripts.Roles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainGame.Menus
{
    public class InfoListItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;

        public void SetUp(string text)
        {
            this.text.text = text;
        }
    }
}
