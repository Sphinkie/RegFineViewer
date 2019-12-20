﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Collections.ObjectModel;       // ObservableCollections
using System.Windows.Threading;

namespace RegFineViewer
{

    // -------------------------------------------------------------------------
    // Programme principal
    // -------------------------------------------------------------------------
    public partial class MainWindow : Window
    {
        // Chaque RegistryTree est une collection de RegistryItems
        private ObservableCollection<RegistryItem> RegistryTree1 = new ObservableCollection<RegistryItem>();
        private KeyUnitDictionnary UnitDictionnary;
        // Parseur de fichier REG qui remplit un RegistryTree
        private RegFileParser Parser1;

        // Pour la fonction de recherche
        private string SearchedWord;
        private int SearchDirection = 1;
        private bool SearchedWordIsDirty;
        private int SearchedWordResultsIndex;
        private List<RegistryItem> SearchedWordResults;

        public MainWindow()
        {
            InitializeComponent();
            // Cette instruction permet de rendre les classes visibles depuis le XAML
            DataContext = this;
            // On charge le dictionnaire des unités préférées
            UnitDictionnary = new KeyUnitDictionnary("Config.xml");
            // On initialise le parseur
            Parser1 = new RegFileParser(RegistryTree1, UnitDictionnary);
            // On binde les RegistryTree avec les TreeView de l'affichage
            TreeView1.ItemsSource = RegistryTree1;
<<<<<<< HEAD
//            TreeView2.ItemsSource = RegistryTree2;
            // Normalement on devrait pouvoir mettre ceci dans le XAML du TreeView, mais ça marche pas:
            // ... ItemsSource="{Binding Source=RegistryTree1}" ...
            // ItemsSource = "{Binding Source={StaticResource myEmployeeData}
=======
            // Normalement on devrait pouvoir mettre ceci dans le XAML du TreeView, mais ça marche pas:
            // ... ItemsSource="{Binding Source=RegistryTree1}" ...
            // ... ItemsSource="{Binding Source=StaticResource RegistryTree1}" ...
            Lb_SearchedWordCount.Text = "";
>>>>>>> version-1.3
        }

        // -------------------------------------------------------------------------
        // Pour les tests, ce bouton remplit le RegistryTree
        // -------------------------------------------------------------------------
        private void FillRegistryTree(object sender, RoutedEventArgs e)
        {
            RegistryItem N1 = new RegistryItem("Node 1", "node");
            RegistryItem K1 = new RegistryItem("clef 1", "dword") { Value = "0001" };
            RegistryItem K2 = new RegistryItem("clef 2", "dword") { Value = "0002" };
            N1.AddSubItem(K1);
            N1.AddSubItem(K2);
            RegistryTree1.Add(N1);

            RegistryItem N2 = new RegistryItem("Node 2", "node");
            RegistryItem K3 = new RegistryItem("clef 3", "dword") { Value = "0003" };
            N2.AddSubItem(K3);
            RegistryTree1.Add(N2);

            RegistryItem N3 = new RegistryItem("SubNode 3", "node");
            RegistryItem K4 = new RegistryItem("clef 4", "dword") { Value = "0004" };
            N3.AddSubItem(K4);
            N2.AddSubItem(N3);
            RegistryItem K5 = new RegistryItem("clef 5", "dword") { Value = "0005" };
            RegistryItem K6 = new RegistryItem("clef 6", "dword") { Value = "0006" };
            RegistryItem N4 = new RegistryItem("SubNode 4", "node");
            N4.AddSubItem(K5);
            N4.AddSubItem(K6);
            N2.AddSubItem(N4);
            DropZone1.Visibility = Visibility.Hidden;
            TreeView1.Visibility = Visibility.Visible;
        }

        // -------------------------------------------------------------------------
        // Drop d'un (ou plusieurs) fichier(s) dans une TreeView
        // -------------------------------------------------------------------------
        private void Tree1_drop(object sender, DragEventArgs e)
        {
            string[] droppedFiles = null;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                droppedFiles = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            }

            if ((droppedFiles == null) || (!droppedFiles.Any())) { return; }

            // S'il y a un (ou plus) fichier(s) droppé(s), on ouvre le premier dans la TreeView 
            if (droppedFiles.Length > 0)
            {
                string fileName = droppedFiles[0];
                Tree1_InfoChip.Content = fileName;
                // On remplit le RegistryTree à partir du fichier REG
                Parser1.ParseFile(fileName);
                Parser1.BuildList();
            }
<<<<<<< HEAD

            // S'il y a plusieurs fichiers droppés, on ouvre les deux premiers dans chaque TreeView
            else
            {
                string fileName1 = droppedFiles[0];
                string fileName2 = droppedFiles[1];
                Tree1_InfoChip.Content = fileName1;
//                Tree2_InfoChip.Content = fileName2;
                // On remplit le RegistryTree à partir du fichier
                Parser1.ParseFile(fileName1);
                Parser2.ParseFile(fileName2);
                Parser1.BuildList();
                Parser2.BuildList();
            }
=======
            DropZone1.Visibility = Visibility.Hidden;
            TreeView1.Visibility = Visibility.Visible;
            // Initialisation de la recherche
            SearchedWordResultsIndex = 0;
            SearchedWordIsDirty = false;
            Lb_SearchedWordCount.Text = "";
>>>>>>> version-1.3
        }
        //private void Tree2_drop(object sender, DragEventArgs e)
        //{
        //    string[] droppedFiles = null;
        //    // on remplit la liste avec les fichiers qui ont été droppés dans la fenetre
        //    if (e.Data.GetDataPresent(DataFormats.FileDrop))
        //    {
        //        droppedFiles = e.Data.GetData(DataFormats.FileDrop, true) as string[];
        //    }
        //    // En cas de liste vide, on sort.
        //    if ((droppedFiles == null) || (!droppedFiles.Any())) { return; }

        //    DropZone2.Visibility = Visibility.Hidden;
        //    TreeView2.Visibility = Visibility.Visible;

        //    // S'il y a un seul fichier droppé, on l'ouvre dans la TreeView courante
        //    if (droppedFiles.Length == 1)
        //    {
        //        string fileName = droppedFiles[0];
        //        Tree2_InfoChip.Content = fileName;
        //        // On remplit le RegistryTree à partir du fichier
        //        Parser2.ParseFile(fileName);
        //        Parser2.BuildList();
        //    }
        //    // S'il y a plusieurs fichiers droppés, on ouvre les deux premiers dans chaque TreeView
        //    else
        //    {
        //        string fileName1 = droppedFiles[0];
        //        string fileName2 = droppedFiles[1];
        //        Tree1_InfoChip.Content = fileName1;
        //        Tree2_InfoChip.Content = fileName2;
        //        // On remplit le RegistryTree à partir du fichier
        //        Parser1.ParseFile(fileName1);
        //        Parser2.ParseFile(fileName2);
        //        Parser1.BuildList();
        //        Parser2.BuildList();
        //    }
        //}

        // -------------------------------------------------------------------------
        // Bouton CLOSE (de la Chip)
        // -------------------------------------------------------------------------
        private void Bt_CloseFile_Click(object sender, RoutedEventArgs e)
        {
            Tree1_InfoChip.Content = "no file loaded";
            RegistryTree1.Clear();
            DropZone1.Visibility = Visibility.Visible;
            TreeView1.Visibility = Visibility.Hidden;
<<<<<<< HEAD

        }
        //private void Tree2_CloseFile_bt(object sender, RoutedEventArgs e)
        //{
        //    Tree2_InfoChip.Content = "no file loaded";
        //    RegistryTree2.Clear();
        //    DropZone2.Visibility = Visibility.Visible;
        //    TreeView2.Visibility = Visibility.Hidden;
        //}
=======
            Bt_Search.Content = "Find";
            Tb_SearchedWord.Text = "";
            this.SearchedWordIsDirty = true;
            Lb_SearchedWordCount.Text = "";
        }
>>>>>>> version-1.3

        // -------------------------------------------------------------------------
        // Affiche un popup de Statistiques relatives au Tree
        // -------------------------------------------------------------------------
<<<<<<< HEAD
        private void Tree1_Search_bt(object sender, RoutedEventArgs e)
        {
            // On deselectionne les TreeItems pouvant être deja selectionnés
            //RegistryItem SI = TreeView1.SelectedItem as RegistryItem;
            //while (SI is RegistryItem)
            //{
            //    SI.IsSelected = false; 
            //}
            // On lance la recherche
            this.SearchedWord = SearchedWord1.Text.ToUpper();
            RegistryItem Result = Parser1.NodeList.Find(Predicat);
            // On se positionne sur cet item
            Result.IsSelected = false;
            Result.IsSelected = true;
<<<<<<< Updated upstream
            // On expand le node parent
            TreeViewItem item = TreeView1.SelectedItem as TreeViewItem;
            if (item is TreeViewItem)
                item.BringIntoView();
            RegistryItem racine = TreeView1.Items[0] as RegistryItem;
            racine.IsExpanded = true;
=======
            // test:
            TreeViewItem selectedTVI = (TreeViewItem)TreeView1.SelectedItem;
>>>>>>> Stashed changes
        }

        //private void Tree2_Search_bt(object sender, RoutedEventArgs e)
        //{
        //    this.SearchedWord = SearchedWord2.Text.ToUpper();
        //    RegistryItem Result = Parser1.NodeList.Find(Predicat);
        //}

        // --------------------------------------------
        // Retourne TRUE si le nom ou la valeur de l'item contient le mot recherché (ne tient pas compte de la casse)
        // --------------------------------------------
        private bool Predicat(RegistryItem item)
        {
            string UpperName = item.Name.ToUpper();
            string UpperValue = item.Value.ToUpper();
            if (UpperName.Contains(this.SearchedWord) || UpperValue.Contains(this.SearchedWord))
                return true;
            else
                return false;
        }

        // -------------------------------------------------------------------------
        // Selection du TreeView: (inutile)
        // -------------------------------------------------------------------------
        private void TreeView1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var X = sender;     // Treeview1
                                //Perform actions when SelectedItem changes
            MessageBox.Show(((TreeViewItem)e.NewValue).Header.ToString());
=======
        private void Bt_TreeInfos_Click(object sender, RoutedEventArgs e)
        {
            tbStatLevels.Text = Parser1.NbLevels.ToString();
            tbStatNodes.Text  = Parser1.NbNodes.ToString();
            tbStatKeys.Text   = Parser1.NbKeys.ToString();
            Pu_TreeInfos.IsOpen = !Pu_TreeInfos.IsOpen;
>>>>>>> version-1.3
        }
        private void Bt_TreeInfos_Close(object sender, RoutedEventArgs e)
        {
            Pu_TreeInfos.IsOpen = false;
        }

        // -------------------------------------------------------------------------
        // Calculs des statistiques sur la longueur des textes affichés
        // -------------------------------------------------------------------------
        private void Bt_LengthStats_Click(object sender, RoutedEventArgs e)
        {
            RefreshLengthStats(Parser1);
            // Affiche ou masque le popup
            CardlengthStats.IsOpen = !CardlengthStats.IsOpen;
        }
        private void RefreshLengthStats(RegFileParser parser)
        {
            Int32 Moyenne     = parser.GetAverageLength();  // A calculer en premier
            Int32 ModalLength = parser.ModalLabelLength;
            Int32 EcartType   = parser.GetStandardDeviation();
            Int32 Nombre      = parser.NbNodes + parser.NbKeys;
            // Les stats disent que 84% de la population se trouve entre 0 et Moy + EcType
            Int32 SD84 = Moyenne + EcartType;
            // Les stats disent que 98% de la population se trouve entre 0 et Moy + 2 x EcType
            Int32 SD98 = Moyenne + 2 * EcartType;
            // On met à jour les textes affichés dans l'UI
            nbItems.Text = Nombre.ToString();
            tbAvLength.Text = Moyenne.ToString() + " chars";
            nbAvLength.Text = parser.GetNbOfItemsLengthEqualsTo(Moyenne).ToString();
            tbModelength.Text = ModalLength.ToString() + " chars";
            nbModelength.Text = parser.GetNbOfItemsLengthEqualsTo(ModalLength).ToString();
            tbSD.Text = EcartType.ToString() + " chars";
            tbSD84.Text = SD84.ToString() + " chars";
            nbSD84.Text = parser.GetNbOfItemsLengthLowerThan(SD84).ToString();
            tbSD98.Text = SD98.ToString() + " chars";
            nbSD98.Text = parser.GetNbOfItemsLengthLowerThan(SD98).ToString();
        }
        private void Bt_LengthStats_Close(object sender, RoutedEventArgs e)
        {
            CardlengthStats.IsOpen = false;
        }

        // -------------------------------------------------------------------------
        // Click sur le bouton ChangeUnit d'un Registry Key du Registry Tree
        // -------------------------------------------------------------------------
        private void Bt_ChangeUnit_Click(object sender, RoutedEventArgs e)
        {
            // On retrouve l'item en cours du TreeView
            var UnitButton = sender as Button;
            // DataContext renvoie le DataSource du Control
            RegistryItem Item = UnitButton.DataContext as RegistryItem;

            // 3 méthodes possibles pour faire un refresh du binding
            // ((TextBox)sender).GetBindingExpression(ComboBox.TextProperty).UpdateSource();         // Update le DataSource en fonction du Control
            // ((ComboBox)sender).GetBindingExpression(ComboBox.ItemsSourceProperty).UpdateTarget(); // Update le Control en fonction du DataSource
            // OnPropertyChanged("Property");
            // Style PlainStyle = Application.Current.Resources["PlainStyle"] as Style;
            // Style OutlinedStyle = Application.Current.Resources["OutlinedStyle"] as Style;

            // On modifie son unité préférée
            Item.ChangeToNextUnit(UnitDictionnary);
            // On raffraichit le texte du bouton "btUfUnit"
            UnitButton.GetBindingExpression(Button.ContentProperty).UpdateTarget();
            // On raffraichit le texte de son frère TextBlock "lbUfValue"
            var StackP = UnitButton.Parent as StackPanel;
            var TextB = StackP.Children[3] as TextBlock;
            TextB.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
        }

        // -------------------------------------------------------------------------
        // Bouton COLLAPSE
        // -------------------------------------------------------------------------
        private void Bt_Collapse_Click(object sender, RoutedEventArgs e)
        {
            TreeView_CollapseAll();
            Pu_Working.IsOpen = false;      // Popup Sablier
        }
        // -------------------------------------------------------------------------
        // Agit sur tous les nodes qui sont associés à un IUElement (cad visibles).
        // Donc referme tous les levels à la fois.
        // -------------------------------------------------------------------------
        private void TreeView_CollapseAll()
        {
            foreach (object item in this.TreeView1.Items)
            {
                TreeViewItem treeItem = this.TreeView1.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (treeItem != null)
                {
                    ExpandAllChilds(treeItem, false);
                    treeItem.IsExpanded = false;
                }
            }
        }

        // -------------------------------------------------------------------------
        // Bouton EXPAND
        // -------------------------------------------------------------------------
        private void Bt_Expand_Click(object sender, RoutedEventArgs e)
        {
            Pu_Working.IsOpen = true;      // Popup Sablier ON
            TreeView_ExpandLevel();
            Pu_Working.IsOpen = false;      // Popup Sablier OFF
        }
        // -------------------------------------------------------------------------
        // N'agit que sur les nodes qui sont dejà associés à un IUElement (cad visibles).
        // Donc il ne traite qu'un level à la fois.
        // -------------------------------------------------------------------------
        private void TreeView_ExpandLevel()
        {
            foreach (object item in this.TreeView1.Items)
            {
                TreeViewItem treeItem = this.TreeView1.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (treeItem != null)
                {
                    ExpandAllChilds(treeItem, true);
                    treeItem.IsExpanded = true;
                }
            }
            // Le rendu visuel est asynchrone: 
            // donc il n'est pas fini quand on sort de la fonction...
        }
        // -------------------------------------------------------------------------
        // Expand ou referme tous les Childs d'un treeViewItem visible
        // -------------------------------------------------------------------------
        private void ExpandAllChilds(ItemsControl items, bool expand)
        {
            foreach (object obj in items.Items)
            {
                ItemsControl childControl = items.ItemContainerGenerator.ContainerFromItem(obj) as ItemsControl;
                if (childControl != null)
                {
                    ExpandAllChilds(childControl, expand);
                }
                TreeViewItem item = childControl as TreeViewItem;
                if (item != null)
                    item.IsExpanded = expand;
            }
        }

        // -------------------------------------------------------------------------
        // Example: Traverse le TreeView pour trouver le TreeViewItem qui correspond à l'item donné.
        // Pour le premier appel, utiliser: parent = TreeView1
        // Exemple d'utilisation: on peut appliquer ExpandSubtree() sur le tvi retourné.
        // -----------------------------------------------------------------------------------
        private TreeViewItem TestFunction(ItemsControl parent, object item)
        {
            // Check whether the selected item is a direct child of the parent ItemsControl.
            TreeViewItem tvi = parent.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

            if (tvi == null)
            {
                // Si tvi est nul, c'est que l'item donné n'est pas un child direct du parent.
                // Donc on verifie pour s'il est un child de l'un des Items enfants du parent.
                foreach (object child in parent.Items)
                {
                    // pour chaque enfant du Parent
                    // si ce n'est pas un node: on passe
                    RegistryItem CurrentChild = child as RegistryItem;
                    if (CurrentChild.DType != "node") continue;
                    // Si c'est un node: on récupère son TVI
                    TreeViewItem childItem = parent.ItemContainerGenerator.ContainerFromItem(child) as TreeViewItem;
                    if (childItem != null)
                    {
                        // Check the next level for the appropriate item.
                        tvi = TestFunction(childItem, item);
                    }
                }
            }
            else
            {
                // le tvi est un child direct de ce parent, donc ont peut expandre le parent
                TreeViewItem t = parent as TreeViewItem;
                if (t is TreeViewItem)
                    t.IsExpanded = true;
            }
            return tvi;
        }

        // -------------------------------------------------------------------------
        // Bouton FIND (barre de recherche)
        // -------------------------------------------------------------------------
        private void Bt_Search_Click(object sender, RoutedEventArgs e)
        {
            // On deselectionne les TreeItems pouvant être déjà sélectionnés
            if ((SearchedWordResults != null) && (SearchedWordResults.Count > 0))
            {
                if (SearchedWordResults[SearchedWordResultsIndex] is RegistryItem)
                    SearchedWordResults[SearchedWordResultsIndex].IsSelected = false;
            }

            if (SearchedWordIsDirty)
            // Si on vient de cliquer sur FIND, *après* avoir modifié le SearchedWord
            {
                // On lance la recherche
                Pu_Working.IsOpen = true;  // Popup sablier
                this.SearchedWord = Tb_SearchedWord.Text.ToUpper();
                // On recupère la liste des RegistryItems correspondant à la recherche
                this.SearchedWordResults = Parser1.NodeList.FindAll(Predicat);
                // On change quelques textes
                switch (this.SearchedWordResults.Count)
                {
                    case 0:
                        Lb_SearchedWordCount.Text = "no item found";
                        Bt_Search.Content = "Find";
                        break;
                    case 1:
                        Lb_SearchedWordCount.Text = "1 item found";
                        Bt_Search.Content = "Find";
                        break;
                    default:
                        Lb_SearchedWordCount.Text = "1/" + this.SearchedWordResults.Count.ToString();
                        Bt_Search.Content = "Next";
                        break;
                }
                // On sélectionne le premier RegistryItem de la liste
                this.SearchedWordResultsIndex = 0;
                this.SearchDirection = 1;
            }

            else if ((SearchedWordResults != null) && (SearchedWordResults.Count > 0))
            // Si on vient de cliquer sur FIND, *sans* avoir modifié le SearchedWord
            {
                // on déselectionne l'item précédent
                if (SearchedWordResults[SearchedWordResultsIndex] is RegistryItem)
                    SearchedWordResults[SearchedWordResultsIndex].IsSelected = false;
                // On incrémente l'index
                SearchedWordResultsIndex += SearchDirection;
                // Vérification des bornes
                if (SearchedWordResultsIndex < 0) SearchedWordResultsIndex = SearchedWordResults.Count - 1;
                if (SearchedWordResultsIndex >= SearchedWordResults.Count) SearchedWordResultsIndex = 0;
                // On met à jour le No de l'item affiché dans le compteur
                Lb_SearchedWordCount.Text = (SearchedWordResultsIndex + 1).ToString() + "/" + SearchedWordResults.Count.ToString();
            }

            if (SearchedWordResultsIndex < SearchedWordResults.Count)
            {
                RegistryItem Item = SearchedWordResults[SearchedWordResultsIndex];
                // On deploie le TreeView jusquà l'item
                List<RegistryItem> PathToNode = new List<RegistryItem> { };
                PathToNode = this.BuildPathToNode(Item);
                this.ExpandPath(PathToNode);
                PathToNode.Clear();

                // On surligne l'item
                if (Item is RegistryItem)
                    Item.IsSelected = true;
            }

            Pu_Working.IsOpen = false;                  // Popup sablier OFF
            this.SearchedWordIsDirty = false;
            Bt_SearchDirection.IsPopupOpen = true;      // Affiche les boutons UP/DOWN
        }
        // --------------------------------------------
        // Retourne TRUE si le nom ou la valeur de l'item contient le mot recherché (sans tenir compte de la casse)
        // --------------------------------------------
        private bool Predicat(RegistryItem item)
        {
            string UpperName = item.Name.ToUpper();
            string UpperValue = item.Value.ToUpper();
            if (UpperName.Contains(this.SearchedWord) || UpperValue.Contains(this.SearchedWord))
                return true;
            else
                return false;
        }

        // -------------------------------------------------------------------------
        // Construit une liste des Nodes conduisant à l'Item donné
        // -------------------------------------------------------------------------
        private List<RegistryItem> BuildPathToNode(RegistryItem item)
        {
            List<RegistryItem> PathToItem = new List<RegistryItem> { };
            if (item != null)
            {
                RegistryItem Curseur = item;
                while (Curseur.Parent is RegistryItem)
                {
                    PathToItem.Add(Curseur.Parent);
                    Curseur = Curseur.Parent;
                }
                PathToItem.Reverse();
            }
            return PathToItem;
        }
        // -------------------------------------------------------------------------
        // Expand les nodes donnés dans la liste passée en paramètre. 
        // (Liste ordonnée en partant de la racine).
        // -------------------------------------------------------------------------
        private void ExpandPath(List<RegistryItem> path)
        {
            ItemsControl ParentTvi = TreeView1;
            foreach (var item in path)
            {
                // Pour expandre un Node, il faut son Control (tvi).
                // Pour l'obtenir, il faut le Control du parent (ParentTvi) et l'Object du Node (item).
                TreeViewItem tvi = ParentTvi.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                // S'il n'est pas nul, on l'expand
                if (tvi is TreeViewItem) tvi.IsExpanded = true;
                // pour le node suivant: le node courant est le parent du node suivant de la liste
                ParentTvi = tvi;
                // On attend que l'UI ait fini l'expansion
                Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
                // Si on tomber sur un tvi null (non trouvé): on sort
                if (ParentTvi == null) return;
            }
        }
        // -------------------------------------------------------------------------
        // Affiche la partie du TreeView qui contient l'item sélectionné. 
        // On ne passe ici que si l'item fait partie de l'UI: il faut donc expandre l'arbre avant.
        // -------------------------------------------------------------------------
        private void TreeViewItem_OnItemSelected(object sender, RoutedEventArgs e)
        {
<<<<<<< HEAD
            // On affiche l'item sélectionné
            TreeViewItem item = sender as TreeViewItem;
            if (item is TreeViewItem)
                item.BringIntoView();
=======
            TreeViewItem tvi = sender as TreeViewItem;
            tvi.BringIntoView();
        }

        // -------------------------------------------------------------------------
        // Appelé chaque fois que le mot à chercher change.
        // -------------------------------------------------------------------------
        private void Tb_SearchedWord_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.SearchedWordIsDirty = true;
            Bt_Search.Content = "Find";
            Lb_SearchedWordCount.Text = "";
        }

        // -------------------------------------------------------------------------
        // Changement de la direction de la recherche
        // -------------------------------------------------------------------------
        private void Bt_SearchDown_Click(object sender, RoutedEventArgs e)
        {
            SearchDirection = 1;
            if ((string)Bt_Search.Content == "Prev") Bt_Search.Content = "Next";
>>>>>>> version-1.3
        }
        private void Bt_SearchUp_Click(object sender, RoutedEventArgs e)
        {
            SearchDirection = -1;
            if ((string)Bt_Search.Content == "Next") Bt_Search.Content = "Prev";
        }

    }
}