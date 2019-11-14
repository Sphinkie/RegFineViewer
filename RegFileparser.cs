using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;

namespace RegFineViewer
{
    class RegFileParser
    {
        // Chaque RegistryTree est une collection de RegistryItems
        private ObservableCollection<RegistryItem> RegistryTree;
        // Dictionaire des nodes
        private Dictionary<string, RegistryItem> nodepathTable = new Dictionary<string, RegistryItem>();

        // ------------------------------------------------------------------
        // Constructeur
        // ------------------------------------------------------------------
        public RegFileParser(ObservableCollection<RegistryItem> registrytree)
        {
            // On mémorise le registrytree
            RegistryTree = registrytree;
            NbKeys = 0;
            NbLevels = 0;
            NbNodes = 0;
        }

        // ------------------------------------------------------------------
        // Parse le fichier REG
        // Enregistre l'arborescence dans un RegistryTree
        // Enregistre la liste des Nodes dans un Dictionnaire
        // ------------------------------------------------------------------
        public void ParseFile(string fileName)
        {
            // On commence par vider la collection et le dictionnaire
            RegistryTree.Clear();
            nodepathTable.Clear();
            NbKeys = 0;
            NbLevels = 0;
            NbNodes = 0;

            // Vérification
            if (!fileName.EndsWith(".reg"))
            {
                RegistryItem WrongNode = new RegistryItem("Not a REG file", "node");
                RegistryTree.Add(WrongNode);
                return;
            }
            // On lit le fichier et on met tout dans une très longue string.
            // fileName = "E:\\source\\repos\\RegFineViewer\\_example1.reg";
            StreamReader streamFile = new StreamReader(File.Open(fileName, FileMode.OpenOrCreate));
            string fichier = streamFile.ReadToEnd();
            streamFile.Close();

            // On decoupe le fichier en un tableau de lignes
            string[] lignes = fichier.Split('\r', '\n');

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
            NbLevels = 1;
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
                    // S'il s'agit du premier node
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
                        RegistryItem newKey = CreateRegistryKey(ligne);
                        currentNode.AddSubItem(newKey);
                    }
                }
                // Autres cas
                else
                { }
            }
        }

        // ------------------------------------------------------------------
        // Attache le node fourni à son parent. On retrouve la parent grace au dictionnaire.
        // ------------------------------------------------------------------
        private void AttachToParentNode(RegistryItem node, string parentpath)
        {
            if (parentpath == string.Empty) return;
            // On cherche le Node Parent dans la table
            RegistryItem parentNode = GetFromNodeTable(parentpath);
            // Si on le trouve: on lui attache le node
            if (parentNode != null)
            {
                parentNode.AddSubItem(node);
            }
            // Si on ne le trouve pas: on le crée et on le rattache à son propre parent
            else
            {
                // On crée un nouveau Node pour le Parent
                string parentName = GetNodeNameFromPath(parentpath);
                parentNode = new RegistryItem(parentName, "node");
                // On le met dans le dictionnaire
                AddToNodeTable(parentNode, parentpath);
                // On le rattache à son propre parent (le parent du parent)
                string greatParentPath = GetParentPath(parentpath);
                AttachToParentNode(parentNode, greatParentPath);
            }
            if (parentNode.SubItem.Count == 1) NbLevels++;
        }

        // ------------------------------------------------------------------
        // Ajoute le node au dictionnaire
        // ------------------------------------------------------------------
        private void AddToNodeTable(RegistryItem node, string nodepath)
        {
            nodepath = nodepath.ToUpper();
            if (!nodepathTable.ContainsKey(nodepath))
                nodepathTable[nodepath] = node;
        }

        // ------------------------------------------------------------------
        // Cherche un node dans le dictionnaire
        // ------------------------------------------------------------------
        private RegistryItem GetFromNodeTable(string nodepath)
        {
            nodepath = nodepath.ToUpper();
            if (nodepathTable.ContainsKey(nodepath))
                return nodepathTable[nodepath];
            else
                return null;
        }

        // ------------------------------------------------------------------
        // Renvoie le nom du node
        // ------------------------------------------------------------------
        private string GetNodeNameFromPath(string nodepath)
        {
            int lastSep = nodepath.LastIndexOf("\\");
            string nodeName = nodepath.Substring(lastSep + 1, nodepath.Length - lastSep - 1);
            return nodeName;
        }

        // ------------------------------------------------------------------
        // Renvoie le chemin du parent
        // ------------------------------------------------------------------
        private string GetParentPath(string nodepath)
        {
            string parentPath = string.Empty;
            int lastSep = nodepath.LastIndexOf("\\");
            if (lastSep != -1)
                parentPath = nodepath.Substring(0, lastSep);
            return parentPath;
        }

        // ------------------------------------------------------------------
        // Cree un nouveau node et l'ajoute au dictionnaire.
        // ------------------------------------------------------------------
        private RegistryItem CreateRegistryNode(string nodepath)
        {
            string nodeName = GetNodeNameFromPath(nodepath);
            RegistryItem NewNode = new RegistryItem(nodeName, "node");
            AddToNodeTable(NewNode, nodepath);
            NbNodes++;
            return NewNode;
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
                // Ex: "valeur"
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
                        string hexValue = "0x" + keyRawValue;            // hexa 
                        UInt32 intValue = Convert.ToUInt32(hexValue, 16); // converti en decimal
                        keyValue = Convert.ToString(intValue);           // converti en string
                    }
                    else if (keyDType.StartsWith("hex"))
                        keyValue = "HEXA VALUE";
                    else
                        keyValue = "unrecognized type";
                }
            }
            // keyNameTypeValue    "\"Passwords\"=hex:0f,be,5a,8d,69,cc,21,0b,67,38,d5,88,27,61,47,9a,24,bc,72,e9,75,\\"   string

            keyDType = "REG_" + keyDType.ToUpper();
            RegistryItem newKey = new RegistryItem(keyName, keyDType);
            newKey.Value = keyValue;
            NbKeys++;
            return newKey;
        }

        public int NbKeys   { get; private set; }
        public int NbNodes  { get; private set; }
        public int NbLevels { get; private set; }
    }
}
