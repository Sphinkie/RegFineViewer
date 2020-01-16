using System;
using System.Collections.ObjectModel;

namespace RegFineViewer
{
    class RegHiveParser : BaseParser
    {
        // ------------------------------------------------------------------
        // Constructeur
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
        public void ParseHive(string subtree)
        {
            // On commence par vider la collection et le dictionnaire
            InitParser();

            // Vérification
            if (subtree.Equals(@"HKLM\"))
            {
                RegistryItem WrongNode = new RegistryItem("HKLM is not allowed. Try a smaller subtree", "node");
                RegistryTree.Add(WrongNode);
                return;
            }


            string[] lignes = { };

            // Vérification
            if (lignes.Length > 50000)
            {
                RegistryItem WrongNode = new RegistryItem("The file contains too many lines (max 10.000).", "node");
                RegistryTree.Add(WrongNode);
                return;
            }

            // On crée un Node Racine
            RegistryItem currentNode = new RegistryItem("root", "node");
            RegistryTree.Add(currentNode);
            bool firstNode = true;
            NbNodes = 1;

            // On parcourt le tableau des lignes du fichier
            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i];
                // On ignore la ligne d'entete
                if (ligne.StartsWith("Windows Registry Editor"))
                { }
                // On ignore les lignes vides
                else if (ligne.Equals(""))
                { }
                // Lignes de type [path\to\node]
                else if (ligne.StartsWith("[") && ligne.EndsWith("]"))
                {
                    string nodePath = ligne.Trim('[', ']');
                    string parentPath = GetParentPath(nodePath);
                    // ----------------------------------------------------------------------
                    // S'il s'agit du premier node
                    // ----------------------------------------------------------------------
                    // Le node racine du treeView est le parent du premier node du fichier REG
                    if (firstNode)
                    {
                        if (parentPath != "")
                            // Pour ce node racine, son nom est le chemin complet du parent
                            currentNode.Name = parentPath;
                        else
                            // sauf si le nom du parent est vide...
                            currentNode.Name = nodePath;
                        // On met le node Racine dans le dictionnaire
                        AddToNodeTable(currentNode, currentNode.Name);
                        // On memorise le Level de ce Node
                        RacineNodeLevel = nodePath.Split('\\').Length;
                        firstNode = false;
                    }
                    // on cree un nouveau node
                    currentNode = CreateRegistryNode(nodePath);
                    // On le rattache à son parent
                    AttachToParentNode(currentNode, parentPath);
                }
                // Lignes du type: "ErrorLogSizeInKb" = dword:000186A0
                // Lignes du type: "Application" = ""
                else if (ligne.StartsWith("\""))
                {
                    // Si on n'a pas de node courant, on passe. Ca ne devrait pas arriver.
                    if (currentNode != null)
                    {
                        // On cree une Key
                        RegistryItem newKey = CreateRegistryKey(ligne);
                        // On la rattache au Node courant
                        currentNode.AddSubItem(newKey);
                    }
                }
                // Autres cas
                else
                { }
            }
        }

        // ------------------------------------------------------------------
        // Cree un Item dans le RegistryTree pour les lignes du type:
        // Lignes du type: "ErrorLogSizeInKb" = dword:000186A0
        // Lignes du type: "Application" = ""
        // ------------------------------------------------------------------
        private RegistryItem CreateRegistryKey(string keyNameTypeValue)
        {
            string[] separator = { "\"" }; // tableau des séparateur. on n'a besoin que d'un seul.
            string[] splittedString = keyNameTypeValue.Split(separator, 2, StringSplitOptions.RemoveEmptyEntries);
            string keyName = splittedString[0];
            string keyDType = string.Empty;
            string keyValue = string.Empty;
            // le reste est alors soit ="string", soit  =type:valeur
            string reste = splittedString[1].Trim('=');
            if (reste.StartsWith("\"") && reste.EndsWith("\""))
            {
                // Type String. Ex: "valeur"
                keyDType = "SZ";
                keyValue = reste.Trim('"'); ;
            }
            else
            {
                int typeSeparatorPos = reste.IndexOf(':');
                if (typeSeparatorPos > 0)
                {
                    keyDType = reste.Substring(0, typeSeparatorPos);
                    string keyRawValue = reste.Substring(typeSeparatorPos + 1);
                    if (keyDType.Equals("dword"))
                    {
                        // Ex: dword:000186A0
                        string hexValue = "0x" + keyRawValue;             // hexa 
                        UInt32 intValue = Convert.ToUInt32(hexValue, 16); // on convertit en decimal
                        keyValue = Convert.ToString(intValue);            // on convertit en string
                    }
                    else if (keyDType.StartsWith("hex"))
                        // Ex: hex:0f,be,5a,8d,69,cc,21,0b,67,38,d5,88,27,61,47,9a,24,bc,72,e9,75
                        keyValue = "HEX VALUE";
                    else
                        keyValue = "unrecognized type";
                }
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

    }
}
