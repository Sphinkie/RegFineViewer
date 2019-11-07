using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RegFineViewer
{
    class RegFileParser
    {
        // Chaque RegistryTree est une collection de RegistryItems
        private ObservableCollection<RegistryItem> RegistryTree;
        // Dictionaire des nodes
        private Dictionary<string, RegistryItem> nodeTable = new Dictionary<string, RegistryItem>();

        // ------------------------------------------------------------------
        // Constructeur
        // ------------------------------------------------------------------
        public RegFileParser(ObservableCollection<RegistryItem> registrytree)
        {
            // On mémorise le registrytree
            RegistryTree = registrytree;
            // On commence par vider la collection
            RegistryTree.Clear();
        }

        // ------------------------------------------------------------------
        // Parse le fichier REG
        // Enregistre l'arborescence dans un RegistryTree
        // Enregistre la liste des Nodes dans un Dictionnaire
        // ------------------------------------------------------------------
        public void ParseFile(string fileName)
        {
            if (!fileName.EndsWith(".reg"))
            {
                RegistryItem WrongNode = new RegistryItem("Not a REG file", "node");
                RegistryTree.Add(WrongNode);
                return;
            }
            // On lit le fichier et on met tout dans une très longue string
            // fileName = "E:\\source\\repos\\RegFineViewer\\_example1.reg";
            StreamReader streamFile = new StreamReader(File.Open(fileName, FileMode.OpenOrCreate));
            string fichier = streamFile.ReadToEnd();
            streamFile.Close();

            // On decoupe le fichier en un tableau de lignes
            string[] lignes = fichier.Split('\r', '\n');

            // On crée un Node Racine
            RegistryItem currentNode = new RegistryItem("root", "node");
            RegistryTree.Add(currentNode);
            bool firstNode = true;

            // On parcourt le tableau des lignes du fichier
            for (int i = 0; i < lignes.Length; i++)
            {
                string ligne = lignes[i];
                // On ignore la ligne d'entete
                if (ligne.StartsWith("Windows Registry Editor"))
                { }
                // On ignore les lignes vides
                else if (ligne == "")
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
                        // Pour ce node racine, son nom est le chemin complet du parent
                        currentNode.Name = parentPath;
                        // On met le node Racine dans le dictionnaire
                        AddToNodeTable(currentNode, parentPath);
                        firstNode = false;
                    }
                    // on cree un nouveau node
                    currentNode = CreateRegistryNode(nodePath);
                    // On le rattache à son parent
                    AttachToParentNode(currentNode, parentPath);
                }
                // Lignes du type: "ErrorLogSizeInKb" = dword:000186A0
                // Lignes du type: "Application" = ""
                else if (currentNode != null)
                {
                    RegistryItem newKey = CreateRegistryKey(ligne);
                    currentNode.AddSubItem(newKey);
                }
                // si CurrentNode est null
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
        }

        // ------------------------------------------------------------------
        // Ajoute le node au dictionnaire
        // ------------------------------------------------------------------
        private void AddToNodeTable(RegistryItem node, string nodepath)
        {
            nodepath = nodepath.ToUpper();
            if (!nodeTable.ContainsKey(nodepath))
                nodeTable[nodepath] = node;
        }

        // ------------------------------------------------------------------
        // Cherche un node dans le dictionnaire
        // ------------------------------------------------------------------
        private RegistryItem GetFromNodeTable(string nodepath)
        {
            nodepath = nodepath.ToUpper();
            if (nodeTable.ContainsKey(nodepath))
                return nodeTable[nodepath];
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
            return NewNode;
        }

        // ------------------------------------------------------------------
        // Cree un Item dans le RegistryTree pour les lignes du type:
        // Lignes du type: "ErrorLogSizeInKb" = dword:000186A0
        // Lignes du type: "Application" = ""
        // ------------------------------------------------------------------
        private RegistryItem CreateRegistryKey(string keyNameTypeValue)
        {
            string[] keyNameValue = keyNameTypeValue.Split('=');
            string[] keyTypeValue = keyNameValue[1].Split(':');
            string keyName = keyNameValue[0].Trim('"');
            string keyDType = string.Empty;
            string keyValue = string.Empty;
            if (keyTypeValue.Length == 1)
            {
                // Ex: "valeur"
                keyDType = "SZ";
                keyValue = keyTypeValue[0];
            }
            else
            {
                // Ex: dword:000186A0
                keyDType = keyTypeValue[0];
                keyValue = keyTypeValue[1];
            }
            keyDType = "REG_" + keyDType.ToUpper();
            RegistryItem newKey = new RegistryItem(keyName, keyDType);
            newKey.Value = keyValue;
            return newKey;
        }


    }
}
