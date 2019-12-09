using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;

namespace RegFineViewer
{
    class RegFileParser
    {
        // --------------------------------------------
        // Objets privés
        // --------------------------------------------
        // Chaque RegistryTree est une collection de RegistryItems
        private ObservableCollection<RegistryItem> RegistryTree;
        // Dictionaire des nodes
        private Dictionary<string, RegistryItem> NodepathTable = new Dictionary<string, RegistryItem>();
        // Dictionnaire des Prefered units
        private KeyUnitDictionnary PreferedUnits;
        // Tableau de statistiques
        private int[] TableStats = new int[100];

        // ------------------------------------------------------------------
        // Constructeur
        // ------------------------------------------------------------------
        public RegFileParser(ObservableCollection<RegistryItem> registrytree, KeyUnitDictionnary dictionnary)
        {
            // On crée un objet NodeList
            NodeList = new List<RegistryItem>();
            // On mémorise le registrytree et le dictionnaire
            RegistryTree = registrytree;
            PreferedUnits = dictionnary;
            // On initialise les variables
            Array.Clear(TableStats, 0, TableStats.Length);
            AverageLabelLengh = 0;
            ModalLabelLength = 0;
            NbNodes = 0;
            NbKeys = 0;
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
            NodepathTable.Clear();
            AverageLabelLengh = 0;
            ModalLabelLength = 0;
            HighestNodeLevel = 0;
            RacineNodeLevel = 0;
            NbNodes = 0;
            NbKeys = 0;
            Array.Clear(TableStats, 0, TableStats.Length);

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
            // On l'ouvre
//            parentNode.ExpandAll(parentNode, true);
        }

        // ------------------------------------------------------------------
        // Ajoute le node au dictionnaire
        // ------------------------------------------------------------------
        private void AddToNodeTable(RegistryItem node, string nodepath)
        {
            nodepath = nodepath.ToUpper();
            if (!NodepathTable.ContainsKey(nodepath))
                NodepathTable[nodepath] = node;
        }

        // ------------------------------------------------------------------
        // Cherche un node dans le dictionnaire
        // ------------------------------------------------------------------
        private RegistryItem GetFromNodeTable(string nodepath)
        {
            nodepath = nodepath.ToUpper();
            if (NodepathTable.ContainsKey(nodepath))
                return NodepathTable[nodepath];
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
            TableStats[nodeName.Length] += 1;
            // On determine le Level de ce Node
            int NodeLevel = nodepath.Split('\\').Length;
            if (NodeLevel > this.HighestNodeLevel) this.HighestNodeLevel = NodeLevel;
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
            TableStats[keyName.Length] += 1;
            return newKey;
        }

        // ------------------------------------------------------------------
        // Contruit la liste (à plat) de tous les nodes du registryTree
        // C'est pratique pour y faire des recherches
        // ------------------------------------------------------------------
        public void BuildList()
        {
            NodeList.Clear();
            if (RegistryTree.Count > 0)
                NodeList = BuildNodeList(RegistryTree[0]);
        }

        // ------------------------------------------------------------------
        // Fonction recursive: on lui donne un Node, elle retourne la liste de ses nodes Children
        // ------------------------------------------------------------------
        private List<RegistryItem> BuildNodeList(RegistryItem item)
        {
            List<RegistryItem> liste = new List<RegistryItem>();
            foreach (RegistryItem child in item.SubItem)
            {
                // ajoute les enfants du node courant à liste
                liste.AddRange(child.SubItem);
                // Ajoute à la liste les enfants de chaque Child
                liste.AddRange(this.BuildNodeList(child));
            }
            return liste;
            // La liste est détruite chaque fois que l'on sort de la méthode,
            // mais après qu'elle ait été retournée à la methode appelante (ré-entrance)
        }

        // ------------------------------------------------------------------
        // STATISTIQUES
        // ------------------------------------------------------------------
        // Calcule la moyenne de toutes les longueurs enregistrées dans la tableau
        // ------------------------------------------------------------------
        public Int32 GetAverageLength()
        {
            int mode = 0;
            int cumul = 0;    // cumul de toutes les longueurs
            int nombre = 0;
            AverageLabelLengh = 0;
            ModalLabelLength = 0;

            for (int lg = 0; lg < TableStats.Length; lg++)
            {
                int nbLg = TableStats[lg];
                // Cumul des tailles (pour calcul de moyenne)
                cumul += lg * nbLg;
                // Décompte des éléments
                nombre += nbLg;
                // Détermination du max (=mode)
                if ((nbLg > mode) && (lg > 1))
                {
                    // On a trouvé un nouveau Mode
                    mode = nbLg;
                    ModalLabelLength = lg;
                }
            }
            if (nombre != 0) AverageLabelLengh = (cumul / nombre);
            return AverageLabelLengh;
        }

        // ------------------------------------------------------------------
        // STATISTIQUES
        // ------------------------------------------------------------------
        // Calcule l'ecart type: sqr(variance)
        // variance = 1/n * (somme (x²) - moy²)
        // ------------------------------------------------------------------
        public Int32 GetStandardDeviation()
        {
            double Variance = 0;
            double EcartType = 0;
            int nombre = 0;
            // somme des carrés 
            double SommeDesCarres = 0;
            for (int lg = 0; lg < TableStats.Length; lg++)
            {
                SommeDesCarres += Math.Pow(lg, 2) * TableStats[lg];
                nombre += TableStats[lg];
            }
            if (nombre != 0)
            {
                Variance = SommeDesCarres - Math.Pow(AverageLabelLengh, 2);
                Variance = Variance / nombre;
                EcartType = Math.Sqrt(Variance);
            }
            return Convert.ToInt32(EcartType);
        }

        // ------------------------------------------------------------------
        // STATISTIQUES
        // ------------------------------------------------------------------
        // Retourne le nombre de keys dont le label a cette longueur
        // ------------------------------------------------------------------
        public Int32 GetNbOfItemsLengthEqualsTo(int length)
        {
            if (length < TableStats.Length)
                return TableStats[length];
            else
                return 0;
        }

        // ------------------------------------------------------------------
        // STATISTIQUES
        // ------------------------------------------------------------------
        // Retourne le nombre de keys dont le label a un longueur plus petite
        // ------------------------------------------------------------------
        public Int32 GetNbOfItemsLengthLowerThan(int length)
        {
            Int32 Nombre = 0;
            if (length < TableStats.Length)
            {
                for (int i = 0; i < length; i++)
                {
                    Nombre += TableStats[i];
                }
            }
            return Nombre;
        }

        // ------------------------------------------------------------------
        // Propriétés
        // ------------------------------------------------------------------
        public int AverageLabelLengh { get; private set; }
        public int ModalLabelLength { get; private set; }
        public int NbKeys { get; private set; }
        public int NbNodes { get; private set; }
        public int NbLevels { get { return HighestNodeLevel - RacineNodeLevel +1; } }
        // Pour les recherches, on construit une liste plate des Nodes, plus facile à parcourir
        public List<RegistryItem> NodeList { get; private set; }
        private int RacineNodeLevel;
        private int HighestNodeLevel;
    }
}
