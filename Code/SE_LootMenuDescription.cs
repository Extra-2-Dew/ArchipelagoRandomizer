using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ArchipelagoRandomizer
{
    public class SE_LootMenuDescription : GuiSelectEffect
    {
        public string titleText;
        public string descriptionText;
        public bool hasQuantity;
        public LootMenuHandler handler;

        private GameObject blankIcon;
        private GameObject picIcon;
        private GameObject countIcon;
        private TextInterface text;
        private int quantity;

        private void Awake()
        {
            GuiSelectionObject selecter = gameObject.GetComponent<GuiSelectionObject>();
            selecter.selectEffects[2] = this;
            blankIcon = transform.Find("Root/Background").gameObject;
            picIcon = transform.Find("Root/Pic").gameObject;
            countIcon = transform.Find("Button/Count").gameObject;
            text = countIcon.GetComponent<TextInterface>();
        }

        public void SetQuantity(int quantity)
        {
            this.quantity = quantity;
            if (hasQuantity) text.Text = quantity.ToString();
            bool hasItem = quantity > 0;
            blankIcon.SetActive(!hasItem);
            picIcon.SetActive(hasItem);
            countIcon.SetActive(hasItem && hasQuantity);
        }

        public int GetQuantity()
        {
            return quantity;
        }

        public override void DoActivate(bool quick)
        {
            if (quantity > 0)
            {
                handler.SetTitleAndDescription(titleText, descriptionText);
            }
            else
            {
                handler.SetEmptyTitleAndDescription();
            }
        }
    }
}
