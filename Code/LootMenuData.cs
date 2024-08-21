using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArchipelagoRandomizer
{
    public class LootMenuData
    {
        public struct LootMenuNode
        {
            public string name;
            public string title;
            public string description;
            public bool useQuantity;
            public string iconPath;
        }

        public struct LootSubMenu
        {
            public string menuName;
            public List<LootMenuNode> nodes;
        }

        public List<LootSubMenu> menus;
    }
}
