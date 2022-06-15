using TMPro;
using UnityEngine;

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
