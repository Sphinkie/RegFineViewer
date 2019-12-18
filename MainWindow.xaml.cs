using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Collections.ObjectModel;       // ObservableCollections


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
        private RegFileParser Parser1;
        string SearchedWord;
        bool SearchedWordIsDirty;
        List<RegistryItem> SearchedWordResults;
        int SearchedWordResultsIndex;
        int SearchDirection = 1;

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
            // Normalement on devrait pouvoir mettre ceci dans le XAML du TreeView, mais ça marche pas:
            // ... ItemsSource="{Binding Source=RegistryTree1}" ...
            // ... ItemsSource="{Binding Source=StaticResource RegistryTree1}" ...
            SearchedWordCount.Text = "";
        }

        // -------------------------------------------------------------------------
        // Pour les tests, ce bouton remplit le RegistryTree
        // -------------------------------------------------------------------------
        private void FillRegistryTree(object sender, RoutedEventArgs e)
        {
            RegistryItem K1 = new RegistryItem("clef 1", "dword");
            RegistryItem K2 = new RegistryItem("clef 2", "dword");
            RegistryItem N1 = new RegistryItem("Node 1", "node");
            K1.Value = "0001";
            K2.Value = "0002";
            N1.AddSubItem(K1);
            N1.AddSubItem(K2);
            RegistryTree1.Add(N1);

            RegistryItem K3 = new RegistryItem("clef 3", "dword");
            RegistryItem N2 = new RegistryItem("Node 2", "node");
            K3.Value = "0003";
            N2.AddSubItem(K3);
            RegistryTree1.Add(N2);

            RegistryItem K4 = new RegistryItem("clef 4", "dword");
            RegistryItem N3 = new RegistryItem("SubNode 3", "node");
            K4.Value = "0004";
            N3.AddSubItem(K4);
            N2.AddSubItem(N3);
            RegistryItem K5 = new RegistryItem("clef 5", "dword");
            RegistryItem K6 = new RegistryItem("clef 6", "dword");
            RegistryItem N4 = new RegistryItem("SubNode 4", "node");
            K5.Value = "0005";
            K6.Value = "0006";
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
            DropZone1.Visibility = Visibility.Hidden;
            TreeView1.Visibility = Visibility.Visible;
            // Initialisation de la recherche
            SearchedWordResultsIndex = 0;
            SearchedWordIsDirty = false;
            SearchedWordCount.Text = "";
        }

        // -------------------------------------------------------------------------
        // Bouton CLOSE (de la Chip)
        // -------------------------------------------------------------------------
        private void Tree1_CloseFile_bt(object sender, RoutedEventArgs e)
        {
            Tree1_InfoChip.Content = "no file loaded";
            RegistryTree1.Clear();
            DropZone1.Visibility = Visibility.Visible;
            TreeView1.Visibility = Visibility.Hidden;
        }

        // -------------------------------------------------------------------------
        // Bouton FIND (barre de recherche)
        // -------------------------------------------------------------------------
        private void Tree1_Search_bt(object sender, RoutedEventArgs e)
        {
            // On deselectionne les TreeItems pouvant être deja selectionnés
            if ((SearchedWordResults != null) && (SearchedWordResults.Count > 0))
            {
                if (SearchedWordResults[SearchedWordResultsIndex] is RegistryItem)
                    SearchedWordResults[SearchedWordResultsIndex].IsSelected = false;
            }

            if (SearchedWordIsDirty)
            // Si on vient de cliquer sur FIND, *après* avoir modifié le SearchedWord
            {
                // On lance la recherche
                working.IsOpen = true;  // Popup sablier
                this.SearchedWord = SearchedWord1.Text.ToUpper();
                // On recupère la liste des RegistryItems correspondant à la recherche
                SearchedWordResults = Parser1.NodeList.FindAll(Predicat);
                // On change quelques textes
                switch (SearchedWordResults.Count)
                {
                    case 0:
                        SearchedWordCount.Text = "no item found";
                        btFind.Content = "Find";
                        break;
                    case 1:
                        SearchedWordCount.Text = "1 item found";
                        btFind.Content = "Find";
                        break;
                    default:
                        SearchedWordCount.Text = "1/" + SearchedWordResults.Count.ToString();
                        btFind.Content = "Next";
                        break;
                }
                // On sélectionne le premier RegistryItem de la liste
                SearchedWordResultsIndex = 0;
                SearchDirection = 1;
                if (SearchedWordResults.Count>0)
                    if (SearchedWordResults[0] is RegistryItem) 
                        SearchedWordResults[0].IsSelected = true;
            }

            else if ((SearchedWordResults != null) && (SearchedWordResults.Count>0))
            // Si on vient de cliquer sur FIND, *sans* avoir modifié le SearchedWord
            {
                // on déselectionne l'item précédent
                if (SearchedWordResults[SearchedWordResultsIndex] is RegistryItem)
                    SearchedWordResults[SearchedWordResultsIndex].IsSelected = false;
                // On incrémente l'index
                SearchedWordResultsIndex += SearchDirection;
                // Vérification des bornes
                if (SearchedWordResultsIndex < 0) SearchedWordResultsIndex = SearchedWordResults.Count-1;
                if (SearchedWordResultsIndex >= SearchedWordResults.Count) SearchedWordResultsIndex = 0;
                // on selectionne l'item suivant
                if (SearchedWordResults[SearchedWordResultsIndex] is RegistryItem)
                    SearchedWordResults[SearchedWordResultsIndex].IsSelected = true;
                // On met à jour le No de l'item
                SearchedWordCount.Text = (SearchedWordResultsIndex+1).ToString() + "/" + SearchedWordResults.Count.ToString();
                working.IsOpen = false;  // Popup sablier
            }

            //  GetTreeViewItem(TreeView1, Result);
            SearchedWordIsDirty = false;
            // Affiche les boutons UP/DOWN
            SearchDirButton.IsPopupOpen = true;
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
        // Selection du TreeView: (inutile)
        // -------------------------------------------------------------------------
        private void TreeView1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
        }


        // -------------------------------------------------------------------------
        // Boutons du Tray
        // -------------------------------------------------------------------------
        private void bTray1Button1_Click(object sender, RoutedEventArgs e)
        {
            tbStatLevels.Text = Parser1.NbLevels.ToString();
            tbStatNodes.Text = Parser1.NbNodes.ToString();
            tbStatKeys.Text = Parser1.NbKeys.ToString();
            CardTreeInfo.IsOpen = !CardTreeInfo.IsOpen;
        }
        private void bTray1Button2_Click(object sender, RoutedEventArgs e)
        {
            RefreshLengthStats(Parser1);
            // Affiche ou masque le popup
            CardlengthStats.IsOpen = !CardlengthStats.IsOpen;
        }

        // -------------------------------------------------------------------------
        // Calculs des valeurs statistiques, et refresh de l'UI
        // -------------------------------------------------------------------------
        private void RefreshLengthStats(RegFileParser Parser)
        {
            Int32 Moyenne = Parser.GetAverageLength();  // A calculer en premier
            Int32 ModalLength = Parser.ModalLabelLength;
            Int32 EcartType = Parser.GetStandardDeviation();
            Int32 Nombre = Parser.NbNodes + Parser.NbKeys;
            // Les stats disent que 84% de la population se trouve entre 0 et Moy + EcType
            Int32 SD84 = Moyenne + EcartType;
            // Les stats disent que 98% de la population se trouve entre 0 et Moy + 2 x EcType
            Int32 SD98 = Moyenne + 2 * EcartType;
            // On met à jour les textes affichés dans l'UI
            nbItems.Text = Nombre.ToString();
            tbAvLength.Text = Moyenne.ToString() + " chars";
            nbAvLength.Text = Parser.GetNbOfItemsLengthEqualsTo(Moyenne).ToString();
            tbModelength.Text = ModalLength.ToString() + " chars";
            nbModelength.Text = Parser.GetNbOfItemsLengthEqualsTo(ModalLength).ToString();
            tbSD.Text = EcartType.ToString() + " chars";
            tbSD84.Text = SD84.ToString() + " chars";
            nbSD84.Text = Parser.GetNbOfItemsLengthLowerThan(SD84).ToString();
            tbSD98.Text = SD98.ToString() + " chars";
            nbSD98.Text = Parser.GetNbOfItemsLengthLowerThan(SD98).ToString();
        }

        // -------------------------------------------------------------------------
        // Ferme le Popup (Card)
        // -------------------------------------------------------------------------
        private void CardlengthStats_Close(object sender, RoutedEventArgs e)
        {
            CardlengthStats.IsOpen = false;
        }
        private void CardTreeInfo_Close(object sender, RoutedEventArgs e)
        {
            CardTreeInfo.IsOpen = false;
        }

        // -------------------------------------------------------------------------
        // Click sur le bouton ChangeUnit d'un Registry Key du Registry Tree
        // -------------------------------------------------------------------------
        private void TreeView_ChangeUnit_Click(object sender, RoutedEventArgs e)
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
        // Selection du TreeViewItem
        // On ne passe ici que si l'item est visible (expanded):
        // il faut donc ouvrir tout l'arbre avant de faire la recherche.
        // -------------------------------------------------------------------------
        private void TreeViewItem_OnItemSelected(object sender, RoutedEventArgs e)
        {
            TreeViewItem S = e.Source as TreeViewItem;
            S.BringIntoView();

            // l'un des deux ne sert à rien ...

            TreeViewItem X = sender as TreeViewItem;
            X.BringIntoView();
        }

        // -------------------------------------------------------------------------
        // Boutons EXPAND et COLLAPSE
        // -------------------------------------------------------------------------
        private void TreeView1_CollapseAll(object sender, RoutedEventArgs e)
        {
            TreeView_CollapseAll();
        }
        private void TreeView1_ExpandAll(object sender, RoutedEventArgs e)
        {
            TreeView_ExpandAll();
        }

        // -------------------------------------------------------------------------
        // N'agit que sur les nodes qui sont dejà associés à un IUElement (cad visibles).
        // Donc il ne traite qu'un level à la fois.
        // -------------------------------------------------------------------------
        private void TreeView_ExpandAll()
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
        }

        // -------------------------------------------------------------------------
        // Agit sur tous les nodes qui sont dejà associés à un IUElement (cad visibles).
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
                    item.IsExpanded = expand;  // true
            }
        }


        // -------------------------------------------------------------------------
        // Essai pour expandre tout l'arbre d'un coup.
        // Comportement bizarre: c'est long (10 secs), et fait une exception
        // -------------------------------------------------------------------------
        private void test1(object sender, RoutedEventArgs e)
        {
            object NodeRacine = TreeView1.Items[0];
            TreeViewItem treeItem = this.TreeView1.ItemContainerGenerator.ContainerFromItem(NodeRacine) as TreeViewItem;
            if (treeItem != null)
            {
                treeItem.ExpandSubtree();
                //treeItem.IsExpanded = true;
            }
        }


        // -------------------------------------------------------------------------
        // exemple Microsoft
        // Ne marche pas: selected item est toujours à null
        // -------------------------------------------------------------------------
        private void expandSelected_Click(object sender, RoutedEventArgs e)
        {
            if (TreeView1.SelectedItem == null)
            {
                return;
            }

            TreeViewItem tvi = GetTreeViewItem(TreeView1, TreeView1.SelectedItem);

            if (tvi != null)
            {
                tvi.ExpandSubtree();
            }
        }

        // Traverse the TreeView to find the TreeViewItem that corresponds to the given item.
        // Ne marche pas: ne trouve rien 
        private TreeViewItem GetTreeViewItem(ItemsControl parent, object item)
        {
            // Check whether the selected item is a direct child of 
            // the parent ItemsControl.
            TreeViewItem tvi = parent.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

            if (tvi == null)
            {
                // The selected item is not a child of parent, so check
                // the child items of parent.
                foreach (object child in parent.Items)
                {
                    TreeViewItem childItem = parent.ItemContainerGenerator.ContainerFromItem(child) as TreeViewItem;
                    if (childItem != null)
                    {
                        // Check the next level for the appropriate item.
                        tvi = GetTreeViewItem(childItem, item);
                    }
                }
            }
            return tvi;
        }

        // -------------------------------------------------------------------------
        // Appelé chaque fois que le mot à chercher change.
        // -------------------------------------------------------------------------
        private void SearchedWord_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchedWordIsDirty = true;
            btFind.Content = "Find";
        }

        // -------------------------------------------------------------------------
        // Changement de la direction de la recherche
        // -------------------------------------------------------------------------
        private void Button_SearchDown_Click(object sender, RoutedEventArgs e)
        {
            SearchDirection = 1;
            if ((string)btFind.Content == "Prev") btFind.Content = "Next";
        }
        private void Button_SearchUp_Click(object sender, RoutedEventArgs e)
        {
            SearchDirection = -1;
            if ((string)btFind.Content == "Next") btFind.Content = "Prev";
        }

    }
}