using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArchipelagoRandomizer
{
    public class SE_LootMenuDescription : GuiSelectEffect
    {
        public string titleText;
        public string descriptionText;
        public LootMenuHandler handler;

        private void Awake()
        {
            GuiSelectionObject selecter = gameObject.GetComponent<GuiSelectionObject>();
            List<GuiSelectEffect> selectEffects = new(selecter.selectEffects);
            selectEffects.Add(this);
            selecter.selectEffects = selectEffects.ToArray();
        }

        public override void DoActivate(bool quick)
        {
            handler.SetTitleAndDescription(titleText, descriptionText);
        }
    }
}
