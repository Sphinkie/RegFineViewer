using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;

namespace RegFineViewer
{
    class RegHiveParser : BaseParser
    {
        // ------------------------------------------------------------------
        // Constructeur (on repasse les paramètres à la classe de base)
        // ------------------------------------------------------------------
        public RegHiveParser(ObservableCollection<RegistryItem> registrytree, KeyUnitDictionnary dictionnary)
            : base(registrytree, dictionnary)
        {
        }

        // ------------------------------------------------------------------
        // Parcourt de toute la base de registres
        // Enregistre l'arborescence dans un RegistryTree
        // Enregistre la liste des Nodes dans un Dictionnaire
        // ------------------------------------------------------------------
        public void ParseHive(string rootsubtree)
        {
            // On commence par vider la collection et le dictionnaire
            InitParser();

            // On cree le node Racine
            this.CreateRootNode(rootsubtree);

            // On parcourt le subtree de la base de registres
            this.CreateChildNodes(rootsubtree);
        }

        // ------------------------------------------------------------------
        // Cree un Item dans le RegistryTree pour cette Value
        // ------------------------------------------------------------------
        private RegistryItem CreateRegistryKey(string keyName, string keyKind, string keyValue)
        {
            string keyDType = "";

            if (keyKind.Equals("String", StringComparison.CurrentCultureIgnoreCase))
                keyDType = "SZ";
            else if (keyKind.Equals("MultiString", StringComparison.CurrentCultureIgnoreCase))
                keyDType = "MULTI_SZ";
            else if (keyKind.Equals("DWord", StringComparison.CurrentCultureIgnoreCase))
                keyDType = "DWORD";
            else if (keyKind.Equals("Binary", StringComparison.CurrentCultureIgnoreCase))
                keyValue = "HEX VALUE";
            else
            {
                keyDType = keyKind;
                keyValue = "unrecognized type";
            }
            keyDType = "REG_" + keyDType.ToUpper();
            // On cree la Key
            RegistryItem newKey = new RegistryItem(keyName, keyDType);
            newKey.Value = keyValue;
            // Si cette Key possède une unité préférée, on la prend en compte
            newKey.UserFriendlyUnit = PreferedUnits.GetValue(keyName);
            newKey.UpdateUserFriendyValue();
            // On incrémente nos compteurs internes
            NbKeys++;
            return newKey;
        }

        // ------------------------------------------------------------------
        // Cree le node racine dans le RegistryTree
        // ------------------------------------------------------------------
        private void CreateRootNode(string rootpath)
        {
            // Vérifications
            if (rootpath == string.Empty)
            {
                RegistryItem WrongNode = new RegistryItem("HKLM is not allowed. Try a smaller subtree.", "node");
                RegistryTree.Add(WrongNode);
                return;
            }
            if (rootpath.Equals("SECURITY"))
            {
                RegistryItem WrongNode = new RegistryItem("SECURITY subtree is not accessible (reserved access).", "node");
                RegistryTree.Add(WrongNode);
                return;
            }

            // On crée le Node Racine
            RegistryItem RacineNode = new RegistryItem(rootpath, "node");
            RegistryTree.Add(RacineNode);
            this.AddToNodeTable(RacineNode, RacineNode.Name);
            // On memorise le Level de ce Node
            RacineNodeLevel = rootpath.Split('\\').Length;
            NbNodes = 1;
        }

        // ------------------------------------------------------------------
        // Cree les Nodes SubKeys et les Values du None donné
        // ------------------------------------------------------------------
        private void CreateChildNodes(string ParentPath)
        {
            RegistryKey rk;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey(ParentPath);
            }
            catch (Exception ex)
            {
                RegistryItem WrongNode = new RegistryItem("This registry cannot be read (" + ex.Message + ").", "node");
                RegistryTree.Add(WrongNode);
                return;
            }

            string[] SubKeysArray = rk.GetSubKeyNames();
            string[] ValuesArray = rk.GetValueNames();

            // Traitement des Subkeys: on les ajoute dans l'arborescence
            foreach (string SubKeyName in SubKeysArray)
            {
                // on cree un nouveau node pour chaque SubKey
                string nodePath = ParentPath + @"\" + SubKeyName;
                RegistryItem NewNode = CreateRegistryNode(nodePath);
                // On met le node dans le dictionnaire
                this.AddToNodeTable(NewNode, SubKeyName);
                // On le rattache à son parent
                AttachToParentNode(NewNode, ParentPath);
            }

            // Traitement des Values: on les ajoute dans l'arborescence
            foreach (string ValueName in ValuesArray)
            {
                // On recupère toutes les infos sur cette value
                string ValueKind = rk.GetValueKind(ValueName).ToString();
                string Value = rk.GetValue(ValueName).ToString();
                // On cree une Key
                RegistryItem newKey = CreateRegistryKey(ValueName, ValueKind, Value);
                // On la rattache à son parent
                AttachToParentNode(newKey, ParentPath);
            }
        }


    }
}
