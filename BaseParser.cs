using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RegFineViewer
{
    class BaseParser
    {

        // ------------------------------------------------------------------
        // Propriétés Publiques
        // ------------------------------------------------------------------
        public int NbKeys { get; private set; }
        public int NbNodes { get; private set; }
        public int NbLevels { get { return HighestNodeLevel - RacineNodeLevel + 1; } }
        // Pour les recherches, on construit une liste plate des Nodes, plus facile à parcourir
        public List<RegistryItem> NodeList { get; private set; }


        // --------------------------------------------
        // Protected Objects 
        // L’accès est limité à la classe conteneur ou aux dérivées de la classe conteneur.
        // --------------------------------------------
        protected ObservableCollection<RegistryItem> RegistryTree;  // Chaque RegistryTree est une collection de RegistryItems
        protected KeyUnitDictionnary PreferedUnits;                 // Dictionnaire des Prefered units
        protected Dictionary<string, RegistryItem> NodepathTable = new Dictionary<string, RegistryItem>();         // Dictionaire des nodes

        protected int RacineNodeLevel;
        private int HighestNodeLevel;


        // ------------------------------------------------------------------
        // CONSTRUCTEUR
        // Le constructeur de la sous-classe appelle toujours celui de la classe de base, implicitement ou explicitement.
        // Si rien n'est spécifié, le compilateur génère un appel implicite au constructeur de la classe de base ne comportant aucun paramètre. 
        // ------------------------------------------------------------------
        public BaseParser(ObservableCollection<RegistryItem> registrytree, KeyUnitDictionnary dictionnary)
        {
            // On mémorise le registrytree et le dictionnaire
            RegistryTree = registrytree;
            PreferedUnits = dictionnary;

            // On crée un objet NodeList
            NodeList = new List<RegistryItem>();
            this.InitParser();
        }

        // ------------------------------------------------------------------
        // Fonction d'init à appeler avant chaque nouveau parsing
        // ------------------------------------------------------------------
        public void InitParser()
        {
            RegistryTree.Clear();
            NodepathTable.Clear();
            HighestNodeLevel = 0;
            RacineNodeLevel = 0;
            NbNodes = 1;    // On a toujours au moins un node racine
            NbKeys = 0;
        }

        // ------------------------------------------------------------------
        // Attache le node fourni à son parent. On retrouve le parent grace au dictionnaire.
        // ------------------------------------------------------------------
        protected void AttachToParentNode(RegistryItem node, string parentpath)
        {
            if (parentpath == string.Empty) return;
            // On cherche le Node Parent dans la table
            RegistryItem parentNode = this.GetFromNodeTable(parentpath);
            // Si on le trouve: on lui attache le node
            if (parentNode != null)
            {
                parentNode.AddSubItem(node);
            }
            // Si on ne trouve pas le Parent: on en crée un, et on le rattache à son propre parent (grand-parent)
            else
            {
                // On crée un nouveau Node pour le Parent
                string parentName = this.GetNodeNameFromPath(parentpath);
                parentNode = new RegistryItem(parentName, "node");
                // On le met dans le dictionnaire
                this.AddToNodeTable(parentNode, parentpath);
                // On le rattache à son propre parent (le parent du parent)
                string greatParentPath = GetParentPath(parentpath);
                this.AttachToParentNode(parentNode, greatParentPath);
            }
        }

        // ------------------------------------------------------------------
        // Attache le node fourni à son parent. Fonction rapide, si on connait deja le Parent.
        // ------------------------------------------------------------------
        protected void AttachToParentNode(RegistryItem node, RegistryItem parentnode)
        {
            if (parentnode is RegistryItem)
                parentnode.AddSubItem(node);
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
        // Ajoute le node au dictionnaire
        // ------------------------------------------------------------------
        protected void AddToNodeTable(RegistryItem node, string nodepath)
        {
            nodepath = nodepath.ToUpper();
            if (!NodepathTable.ContainsKey(nodepath))
                NodepathTable[nodepath] = node;
        }

        // ------------------------------------------------------------------
        // Renvoie le chemin du parent
        // ------------------------------------------------------------------
        protected string GetParentPath(string nodepath)
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
        protected RegistryItem CreateRegistryNode(string nodepath)
        {
            string nodeName = this.GetNodeNameFromPath(nodepath);
            RegistryItem NewNode = new RegistryItem(nodeName, "node");
            AddToNodeTable(NewNode, nodepath);
            NbNodes++;
            // On determine le Level de ce Node
            int NodeLevel = nodepath.Split('\\').Length;
            if (NodeLevel > this.HighestNodeLevel) this.HighestNodeLevel = NodeLevel;
            return NewNode;
        }

        // ------------------------------------------------------------------
        // Cree une nouvelle key
        // ------------------------------------------------------------------
        protected RegistryItem CreateRegistryKey(string keyName, string keyDType, string keyValue)
        {
            RegistryItem newKey = new RegistryItem(keyName, keyDType.ToUpper());
            if (keyValue.Length > 50) keyValue = keyValue.Substring(0, 50);   // On tronque à 50 chars
            newKey.Value = keyValue;
            // Si cette Key possède une unité préférée, on la prend en compte
            newKey.UserFriendlyUnit = PreferedUnits.GetValue(keyName);
            newKey.UpdateUserFriendyValue();
            // On incrémente nos compteurs internes
            NbKeys++;
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





    }
}
