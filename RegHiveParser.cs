﻿using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Threading;

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
        public void ParseHive(string rootPath)
        {
            // Si le chemein commence par HKLM, on l'enlève (pour les appels aux fonctions Microsoft)
            if (rootPath.StartsWith(@"HKLM\")) rootPath = rootPath.Substring(5, rootPath.Length - 5);

            // On commence par vider la collection et le dictionnaire
            InitParser();
            // On cree le node Racine
            RegistryItem RootNode = this.CreateRootNode(rootPath);

            // On parcourt le subtree de la base de registres, en commençant par le Node Racine
            this.CreateChildNodes(RootNode, rootPath);
        }

        // ------------------------------------------------------------------
        // Cree un Item dans le RegistryTree pour cette Value
        // ------------------------------------------------------------------
        private RegistryItem CreateRegistryKeyFromHive(string keyName, string keyKind, string keyValue)
        {
            string KeyDType;

            if (keyKind.Equals("String", StringComparison.CurrentCultureIgnoreCase))
                KeyDType = "SZ";
            else if (keyKind.Equals("MultiString", StringComparison.CurrentCultureIgnoreCase))
                KeyDType = "MULTI_SZ";
            else if (keyKind.Equals("DWord", StringComparison.CurrentCultureIgnoreCase))
                KeyDType = "DWORD";
            else if (keyKind.Equals("Binary", StringComparison.CurrentCultureIgnoreCase))
            {
                KeyDType = "HEX";
                keyValue = "HEX VALUE";
            }
            else
            {
                KeyDType = keyKind;
                keyValue = "unrecognized type";
            }
            KeyDType = "REG_" + KeyDType;
            // On cree la Key
            RegistryItem newKey = base.CreateRegistryKey(keyName, KeyDType, keyValue);
            return newKey;
        }

        // ------------------------------------------------------------------
        // Cree le node racine dans le RegistryTree.
        // Retourne Null si la racine est un WrongNode (sans enfants)
        // ------------------------------------------------------------------
        private RegistryItem CreateRootNode(string rootpath)
        {
            if (rootpath == string.Empty)
            {
                // Vérification : HKLM
                RegistryItem WrongNode = new RegistryItem("HKLM is not allowed. Try a smaller subtree.", "node");
                RegistryTree.Add(WrongNode);
                return null;
            }
            else if (rootpath.Equals("SECURITY"))
            {
                // Vérification : HKLM\SECURITY
                RegistryItem WrongNode = new RegistryItem("SECURITY subtree is not accessible (reserved access).", "node");
                RegistryTree.Add(WrongNode);
                return null;
            }
            else
            {
                // Les vérifications sont OK: On crée le node Racine
                RegistryItem RacineNode = new RegistryItem(rootpath, "node");
                RegistryTree.Add(RacineNode);
                this.AddToNodeTable(RacineNode, RacineNode.Name);
                // On memorise le Level de ce Node
                base.RacineNodeLevel = rootpath.Split('\\').Length;
                return RacineNode;
            }
        }

        // ------------------------------------------------------------------
        // Cree les Nodes SubKeys et les Values du Node donné
        // C'est un peut redondant de donner le node et le path, mais ça évite de le recalculer à chaque fois.
        // ------------------------------------------------------------------
        private void CreateChildNodes(RegistryItem ParentNode, string ParentPath)
        {
            RegistryKey rk;
            if (ParentNode == null) return;

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

            if (rk == null)
            {

                RegistryItem WrongNode = new RegistryItem("This registry was found null (" + ParentPath + ").", "node");
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
                RegistryItem NewNode = base.CreateRegistryNode(nodePath);
                // On met le node dans le dictionnaire
                base.AddToNodeTable(NewNode, SubKeyName);
                // On le rattache à son parent
                base.AttachToParentNode(NewNode, ParentNode);
                // On traite ses enfants (recursivité)
                this.CreateChildNodes(NewNode, nodePath);
            }

            // Traitement des Values: on les ajoute dans l'arborescence
            foreach (string ValueName in ValuesArray)
            {
                // On recupère toutes les infos sur cette value
                string ValueKind = string.Empty;
                string Value = string.Empty;
                try
                {
                    ValueKind = rk.GetValueKind(ValueName).ToString();
                }
                catch (NullReferenceException)
                {
                    ValueKind = string.Empty;
                }
                try
                {
                    Value = rk.GetValue(ValueName).ToString();
                }
                catch (NullReferenceException)
                {
                    Value = string.Empty;
                }
                // On cree une Key
                RegistryItem newKey = this.CreateRegistryKeyFromHive(ValueName, ValueKind, Value);
                // On la rattache à son parent
                base.AttachToParentNode(newKey, ParentNode);
            }
        }
    }
}
