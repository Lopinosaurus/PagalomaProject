using System.Collections.Generic;
using UnityEngine;

namespace MainGame.Helpers
{
    public class ItemManager : MonoBehaviour
    {
        private static ItemManager Instance;
        
        private int _itemCount;
        private List<Item> _items;

        private void Awake()
        {
            if (Instance)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }
        
        
    }
}