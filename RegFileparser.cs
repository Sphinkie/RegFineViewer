using System;
using System.IO;
using System.Collections.ObjectModel;

namespace RegFineViewer
{
    class RegFileParser : BaseParser
    {

        // ------------------------------------------------------------------
        // Propriétés Publiques
        // ------------------------------------------------------------------
        private int AverageLabelLength { get; set; }
        public int ModalLabelLength { get; private set; }

        // --------------------------------------------
        // Objets privés
        // --------------------------------------------
        private int[] TableStats = new int[100];                  // Tableau de statistiques


        // ------------------------------------------------------------------
        // Constructeur
        // ------------------------------------------------------------------
        public RegFileParser(ObservableCollection<RegistryItem> registrytree, KeyUnitDictionnary dictionnary)
        {
            // On mémorise le registrytree et le dictionnaire
            RegistryTree = registrytree;
            PreferedUnits = dictionnary;

            // On initialise les variables des statistiques du fichier
            Array.Clear(TableStats, 0, TableStats.Length);
            AverageLabelLength = 0;
            ModalLabelLength = 0;
        }

        // ------------------------------------------------------------------
        // Parse le fichier REG
        // Enregistre l'arborescence dans un RegistryTree
        // Enregistre la liste des Nodes dans un Dictionnaire
        // ------------------------------------------------------------------
        public void ParseFile(string fileName)
        {
            // On commence par vider la collection et le dictionnaire
            InitParser();

            AverageLabelLength = 0;
            ModalLabelLength = 0;
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
                    // on comptabilise sa longueur dans les statistiques
                    TableStats[currentNode.Name.Length] += 1;

                }
                // Lignes du type: "ErrorLogSizeInKb" = dword:000186A0
                // Lignes du type: "Application" = ""
                else if (ligne.StartsWith("\""))
                {
                    // Si on n'a pas de node courant, on passe. Ca ne devrait pas arriver.
                    if (currentNode != null)
                    {
                        // On cree une Key
                        RegistryItem newKey = CreateRegistryKeyFromFileLine(ligne);
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
        // Cree un Item dans le RegistryTree pour les lignes de fichier du type:
        // Lignes du type: "ErrorLogSizeInKb" = dword:000186A0
        // Lignes du type: "Application" = ""
        // ------------------------------------------------------------------
        private RegistryItem CreateRegistryKeyFromFileLine(string keyNameTypeValue)
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
        // STATISTIQUES
        // ------------------------------------------------------------------
        // Calcule la moyenne de toutes les longueurs enregistrées dans la tableau
        // ------------------------------------------------------------------
        public Int32 GetAverageLength()
        {
            int mode = 0;
            int cumul = 0;    // cumul de toutes les longueurs
            int nombre = 0;
            AverageLabelLength = 0;
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
            if (nombre != 0) AverageLabelLength = (cumul / nombre);
            return AverageLabelLength;
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
                Variance = SommeDesCarres - Math.Pow(AverageLabelLength, 2);
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
        // Retourne le nombre de keys dont le label a une longueur plus petite
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

    }
}
