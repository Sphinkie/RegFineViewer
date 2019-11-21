using System;
//using System.Collections.Generic;
using System.Collections.ObjectModel;
//using System.Linq;

namespace RegFineViewer
{
    class RegistryItem
    {
        // --------------------------------------------
        // constructeur
        // --------------------------------------------
        public RegistryItem(string name, string type)
        {
            // Initialisations
            Name = name;
            DType = type;
            Value = string.Empty;
            UserFriendlyUnit = string.Empty;
            UserFriendlyValue = string.Empty;
            SubItem = new ObservableCollection<RegistryItem>();
        }
        // --------------------------------------------
        // --------------------------------------------
        public void SetUnitToHex()
        {
            UserFriendlyUnit = "hex";
            Int32 intValue = Convert.ToInt32(Value);
            UserFriendlyValue = "0x" + intValue.ToString("X4");
        }
        // --------------------------------------------
        // --------------------------------------------
        public void SetUnitToBoolean()
        {
            UserFriendlyUnit = "bool";
            if (Value == "0")
                UserFriendlyValue = "false";
            else
                UserFriendlyValue = "true";
        }
        // --------------------------------------------
        // --------------------------------------------
        public void SetUnitToSec()
        {
            UserFriendlyUnit = "seconds";
            double intValue = Convert.ToInt32(Value);
            TimeSpan time = TimeSpan.FromSeconds(intValue);
            UserFriendlyValue = time.ToString(); //  default format is: [-][d.]hh:mm:ss[.fffffff]
        }
        // --------------------------------------------
        // --------------------------------------------
        public void SetUnitToFrames()
        {
            UserFriendlyUnit = "frames";
            double intValue = Convert.ToInt32(Value);
            intValue = intValue * 0.040;
            TimeSpan time = TimeSpan.FromSeconds(intValue);
            // Backslash is just to tell that : is not the part of format, but a character that we want in output.
            string hh_mm_ss = time.ToString(@"hh\:mm\:ss");
            string millisec = time.ToString("fff");
            int frames = Convert.ToInt32(millisec) / 40;
            UserFriendlyValue = hh_mm_ss + ":" + frames.ToString("D2");
        }
        // --------------------------------------------
        // --------------------------------------------
        public void SetUnitToNone()
        {
            UserFriendlyUnit = "no unit";
            UserFriendlyValue = string.Empty;
        }
        // --------------------------------------------
        // Passe à la unité suivante dans la liste
        // --------------------------------------------
        public void ChangeToNextUnit(KeyUnitDictionnary unitDictionnary)
        {
            switch (UserFriendlyUnit)
            {
                case "hex":
                    this.SetUnitToSec();
                    break;
                case "seconds":
                    this.SetUnitToFrames();
                    break;
                case "frames":
                    this.SetUnitToBoolean();
                    break;
                case "bool":
                    this.SetUnitToNone();
                    break;
                default:
                    this.SetUnitToHex();
                    break;
            }
            unitDictionnary.SetValue(Name, UserFriendlyUnit);
        }
        // --------------------------------------------
        // Ajout d'un sous-item (key ou Node)
        // --------------------------------------------
        public void AddSubItem(RegistryItem subnode) { SubItem.Add(subnode); }
        // --------------------------------------------
        // Variables
        // --------------------------------------------
        public string Name { get; set; }
        public string DType { get; }
        public string Value { get; set; }
        public ObservableCollection<RegistryItem> SubItem { get; }
        public string UserFriendlyUnit { get; set; }
        public string UserFriendlyValue { get; set; }
    }
}
