﻿using System;
using System.Collections.Generic;
//using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
//using System.Linq;

namespace RegFineViewer
{
    class RegistryItem  : TreeViewItemBase 
    {
        // --------------------------------------------
        // constructeur
        // --------------------------------------------
        public RegistryItem(string name, string type)
        {
            // InitialisationsB
            Name = name;
            DType = type;
            Value = string.Empty;
            UserFriendlyUnit = string.Empty;
            UserFriendlyValue = string.Empty;
            SubItem = new ObservableCollection<RegistryItem>();
        }
        // --------------------------------------------
        // Recalcul de la UF Value en fonction de l'unité préférée active
        // --------------------------------------------
        private void UpdateUserFriendlyValueToHex()
        {
            Int32 intValue = Convert.ToInt32(Value);
            UserFriendlyValue = "0x" + intValue.ToString("X4");
        }
        private void UpdateUserFriendlyValueToBoolean()
        {
            if (Value == "0")
                UserFriendlyValue = "false";
            else
                UserFriendlyValue = "true";
        }
        private void UpdateUserFriendlyValueToSec()
        {
            double intValue = Convert.ToInt32(Value);
            TimeSpan time = TimeSpan.FromSeconds(intValue);
            UserFriendlyValue = time.ToString(); //  default format is: [-][d.]hh:mm:ss[.fffffff]
        }
        private void UpdateUserFriendlyValueToMilliSec()
        {
            double intValue = Convert.ToInt32(Value);
            TimeSpan time = TimeSpan.FromMilliseconds(intValue);
            string hh_mm_ss = time.ToString(@"hh\:mm\:ss");
            string millisec = time.ToString("fff");
            int frames = Convert.ToInt32(millisec) / 40;
            UserFriendlyValue = hh_mm_ss + ":" + frames.ToString("D2");
        }
        private void UpdateUserFriendlyValueToFrames()
        {
            double intValue = Convert.ToInt32(Value);
            intValue = intValue * 0.040;
            TimeSpan time = TimeSpan.FromSeconds(intValue);
            // Backslash is just to tell that : is not the part of format, but a character that we want in output.
            string hh_mm_ss = time.ToString(@"hh\:mm\:ss");
            string millisec = time.ToString("fff");
            int frames = Convert.ToInt32(millisec) / 40;
            UserFriendlyValue = hh_mm_ss + ":" + frames.ToString("D2");
        }
        private void UpdateUserFriendlyValueToNone()
        {
            UserFriendlyValue = string.Empty;
        }
        // --------------------------------------------
        // Recalcule la valeur de UserFriendyValue
        // --------------------------------------------
        public void UpdateUserFriendyValue()
        {
            switch (this.UserFriendlyUnit)
            {
                case "seconds":
                    this.UpdateUserFriendlyValueToSec();
                    break;
                case "frames":
                    this.UpdateUserFriendlyValueToFrames();
                    break;
                case "msecs":
                    this.UpdateUserFriendlyValueToMilliSec();
                    break;
                case "bool":
                    this.UpdateUserFriendlyValueToBoolean();
                    break;
                case "hex":
                    this.UpdateUserFriendlyValueToHex();
                    break;
                default:
                    UserFriendlyUnit = "  ";
                    this.UpdateUserFriendlyValueToNone();
                    break;
            }
        }
        // --------------------------------------------
        // Passe à l'unité suivante dans la liste
        // --------------------------------------------
        public void ChangeToNextUnit(KeyUnitDictionnary unitDictionnary)
        {
            // On récupère l'unité suivante
            this.UserFriendlyUnit = unitDictionnary.GetNextUnit(this.UserFriendlyUnit);
            // On recalcule la UserFriendyValue
            this.UpdateUserFriendyValue();
            // On ajoute la nouvelle correspondance dans le dictionnaire
            unitDictionnary.SetValue(Name, this.UserFriendlyUnit);
        }
        // --------------------------------------------
        // Ajout d'un sous-item (key ou Node)
        // --------------------------------------------
        public void AddSubItem(RegistryItem subnode) { SubItem.Add(subnode); }

        // --------------------------------------------
        // Propriétés publiques
        // --------------------------------------------
        public string Name { get; set; }
        public string DType { get; }
        public string Value { get; set; }
        public ObservableCollection<RegistryItem> SubItem { get; }
        public string UserFriendlyUnit { get; set; }
        public string UserFriendlyValue { get; set; }

    }
}
