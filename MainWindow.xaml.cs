using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;       // ObservableCollections
using System.Windows.Threading;
using System.Configuration;
using Microsoft.Win32;

namespace RegFineViewer
{

    // -------------------------------------------------------------------------
    // Fenêtre principale
    // -------------------------------------------------------------------------
    public partial class MainWindow : Window
    {
        // Chaque RegistryTree est une collection de RegistryItems
        private ObservableCollection<RegistryItem> RegistryTree1 = new ObservableCollection<RegistryItem>();

        private KeyUnitDictionnary UnitDictionnary;
        // Parseur de fichier REG qui remplit un RegistryTree
        private RegFileParser Parser1;
        private RegHiveParser Parser2;
        private RecentRegistry CurrentRegistry;

        // Pour la fonction de recherche
        private string SearchedWord;
        private int SearchDirection = 1;
        private bool SearchedWordIsDirty;
        private int SearchedWordResultsIndex;
        private List<RegistryItem> SearchedWordResults;

        // Gestion des Registry ouvertes récemment.
        private RecentRegistryList RecentsRegs = new RecentRegistryList();

        // -------------------------------------------------------------------------
        // Programme principal
        // -------------------------------------------------------------------------
        public MainWindow()
        {
            // On met les Recent Registry (from Parameter File) dans une liste
            RecentsRegs.Add(Properties.Settings.Default.Recent_1);
            RecentsRegs.Add(Properties.Settings.Default.Recent_2);
            RecentsRegs.Add(Properties.Settings.Default.Recent_3);
            RecentsRegs.Add(Properties.Settings.Default.Recent_4);
            RecentsRegs.Add(Properties.Settings.Default.Recent_5);
            RecentsRegs.Add(Properties.Settings.Default.Recent_6);

            InitializeComponent();
            // Cette instruction permet de rendre les classes visibles depuis le XAML
            DataContext = this;
            // On charge le dictionnaire des unités préférées
            UnitDictionnary = new KeyUnitDictionnary("Config.xml");
            // On initialise le parseur
            Parser1 = new RegFileParser(RegistryTree1, UnitDictionnary);
            Parser2 = new RegHiveParser(RegistryTree1, UnitDictionnary);

            // On binde la stackPanel qui contient la liste des Recent Registry
            RecentRegData.ItemsSource = this.RecentsRegs;
            // On binde la ComboBox qui contient la liste des premiers nodes de la BDR HKLM
            //            cb_SelectHive.ItemsSource = this.HiveNodeArray;
            // On binde les RegistryTree avec les TreeView de l'affichage
            TreeView1.ItemsSource = RegistryTree1;
            // Normalement on devrait pouvoir mettre ceci dans le XAML du TreeView, mais ça marche pas:
            // ... ItemsSource="{Binding Source=RegistryTree1}" ...
            // ... ItemsSource="{Binding Source=StaticResource RegistryTree1}" ...
            Lb_SearchedWordCount.Text = "";
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
            DropZone.Visibility = Visibility.Hidden;
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
                CurrentRegistry = new RecentRegistry(fileName);

                // On renseigne l'InfoChip
                Tree_InfoChip.Content = CurrentRegistry.Name;
                Tree_InfoChipIcon.Kind = CurrentRegistry.Icon;

                // On remplit le RegistryTree à partir du fichier REG
                Parser1.ParseFile(CurrentRegistry.Name);
                Parser1.BuildList();
                // Ajout à la liste des Recent Regs
                RecentsRegs.Add(CurrentRegistry.Name);
                this.SaveRecentRegs();
            }
            // Gestion de l'IHM
            this.ReInitDisplay(showLengthstats: true);
            // on ferme le popup
            Pu_Recent.IsOpen = false;
        }

        // -------------------------------------------------------------------------
        // Bouton CLOSE (de la Chip)
        // -------------------------------------------------------------------------
        private void Bt_CloseFile_Click(object sender, RoutedEventArgs e)
        {
            Tree_InfoChip.Content = "no file loaded";
            RegistryTree1.Clear();
            DropZone.Visibility = Visibility.Visible;
            TreeView1.Visibility = Visibility.Hidden;
            Bt_Search.Content = "Find";
            Tb_SearchedWord.Text = "";
            this.SearchedWordIsDirty = true;
            Lb_SearchedWordCount.Text = "";
        }

        // -------------------------------------------------------------------------
        // Affiche un popup de Statistiques relatives au Tree
        // -------------------------------------------------------------------------
        private void Bt_TreeInfos_Click(object sender, RoutedEventArgs e)
        {
            tbStatLevels.Text = Parser1.NbLevels.ToString();
            tbStatNodes.Text = Parser1.NbNodes.ToString();
            tbStatKeys.Text = Parser1.NbKeys.ToString();
            Pu_TreeInfos.IsOpen = !Pu_TreeInfos.IsOpen;
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
            Int32 Moyenne = parser.GetAverageLength();  // A calculer en premier
            Int32 ModalLength = parser.GetModalLabelLength();
            Int32 EcartType = parser.GetStandardDeviation();
            Int32 Nombre = parser.NbNodes + parser.NbKeys;
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
                    // On diminue la priorité du thread, pour que l'UI se raffraichisse en priorité
                    Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
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
            TreeViewItem tvi = sender as TreeViewItem;
            tvi.BringIntoView();
        }

        // -------------------------------------------------------------------------
        // Event appelé chaque fois que le TextBox du "mot à chercher" change.
        // -------------------------------------------------------------------------
        private void Tb_SearchedWord_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.SearchedWordIsDirty = true;
            Bt_Search.Content = "Find";
            Lb_SearchedWordCount.Text = "";
        }
        // -------------------------------------------------------------------------
        // Boutons de changement de la direction de la recherche
        // -------------------------------------------------------------------------
        private void Bt_SearchDown_Click(object sender, RoutedEventArgs e)
        {
            SearchDirection = 1;
            if ((string)Bt_Search.Content == "Prev") Bt_Search.Content = "Next";
        }
        private void Bt_SearchUp_Click(object sender, RoutedEventArgs e)
        {
            SearchDirection = -1;
            if ((string)Bt_Search.Content == "Next") Bt_Search.Content = "Prev";
        }

        // -------------------------------------------------------------------------
        // MENU: Ouverture de la base de registre du poste
        // -------------------------------------------------------------------------
        private void Bt_SelectHive_Click(object sender, RoutedEventArgs e)
        {
            if (Pu_SelectHive.IsOpen)
                // Si le popup est ouvert: on le ferme.
                Pu_SelectHive.IsOpen = false;
            else
            {
                // Si le popup est fermé:
                // on initialise le chemin en registry
                Tb_HivePath.Text = "";
                // on remplit la listbox, avec les subKey du node HKLM
                this.FillHiveComboBox(Registry.LocalMachine);
                // on ouvre le popup de sélection du subtree de la base de registres
                Pu_SelectHive.IsOpen = true;
            }
            // On masque le bouton Back
            Bt_SelectHiveBack.Visibility = Visibility.Hidden;
        }
        // -------------------------------------------------------------------------
        // Remplit la combobox avec les subkey du node passé en paramètre
        // -------------------------------------------------------------------------
        private void FillHiveComboBox(RegistryKey rk)
        {
            string[] HiveNodeArray = rk.GetSubKeyNames();
            Cb_SelectHive.Items.Clear();
            foreach (string item in HiveNodeArray)
            {
                Cb_SelectHive.Items.Add(item);
            }
        }
        // -------------------------------------------------------------------------
        // L'utilisateur a choisit un SubKey dans la ComboBox
        // -------------------------------------------------------------------------
        private void Cb_SelectHive_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Cb_SelectHive.SelectedItem != null)
            {
                string SelectedNodeName = Cb_SelectHive.SelectedItem.ToString();
                if (Tb_HivePath.Text.Length > 0) Tb_HivePath.Text += @"\";
                Tb_HivePath.Text += SelectedNodeName;
                try
                {
                    RegistryKey rk = Registry.LocalMachine.OpenSubKey(Tb_HivePath.Text);
                    this.FillHiveComboBox(rk);
                    Bt_SelectHiveBack.Visibility = Visibility.Visible;
                }
                catch (System.Security.SecurityException)
                {
                    // On vide la comboBox
                    Cb_SelectHive.Items.Clear();
                    Bt_SelectHiveBack.Visibility = Visibility.Visible;
                }
            }
        }
        // -------------------------------------------------------------------------
        // Ferme le popup SelectHive
        // -------------------------------------------------------------------------
        private void Bt_SelectHive_Close(object sender, RoutedEventArgs e)
        {
            Pu_SelectHive.IsOpen = false;
            Bt_SelectHiveBack.Visibility = Visibility.Hidden;
        }
        // -------------------------------------------------------------------------
        // On remonte d'un cran dans la Registry (bouton Back)
        // -------------------------------------------------------------------------
        private void Bt_SelectHive_Back(object sender, RoutedEventArgs e)
        {
            int Pos = Tb_HivePath.Text.LastIndexOf('\\');
            if (Pos > 0)
            {
                Bt_SelectHiveBack.Visibility = Visibility.Visible;
                // on enlève un cran au TextBox
                string NewHivepath = Tb_HivePath.Text.Substring(0, Pos);
                Tb_HivePath.Text = NewHivepath;
                // on refresh la ComboBox
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(NewHivepath);
                this.FillHiveComboBox(rk);
            }
            else
            {
                Bt_SelectHiveBack.Visibility = Visibility.Hidden;
                // on vide la TextBox
                Tb_HivePath.Text = "";
                // on refresh la ComboBox
                this.FillHiveComboBox(Registry.LocalMachine);
            }
        }
        // -------------------------------------------------------------------------
        // On importe le subtree sélectionné
        // -------------------------------------------------------------------------
        private void Bt_SelectHive_Import(object sender, RoutedEventArgs e)
        {
            // Ferme le popup
            Pu_SelectHive.IsOpen = false;

            // On remplit le Chip donnant le titre du TreeView
            string HivePath = @"HKLM\" + Tb_HivePath.Text;

            CurrentRegistry = new RecentRegistry(HivePath);

            // On renseigne l'InfoChip
            Tree_InfoChip.Content  = CurrentRegistry.Name;
            Tree_InfoChipIcon.Kind = CurrentRegistry.Icon;

            // On remplit le RegistryTree à partir du subtree de la Registry (=Hive)
            Parser2.ParseHive(Tb_HivePath.Text);
            Parser2.BuildList();

            // Ajout à la liste des Recent Regs
            RecentsRegs.Add(HivePath);
            this.SaveRecentRegs();
            // Gestion de l'affichage IHM
            this.ReInitDisplay();
        }


        // -------------------------------------------------------------------------
        // Ouverture/Fermeture du popup "Recent Registries"
        // -------------------------------------------------------------------------
        private void Bt_OpenRecent_Click(object sender, RoutedEventArgs e)
        {
            // Ouvre/Ferme le popup "Recent trees"
            Pu_Recent.IsOpen = !Pu_Recent.IsOpen;
        }

        // -------------------------------------------------------------------------
        // Enleve un Recent Reg de la liste
        // -------------------------------------------------------------------------
        private void Bt_RecentChip_Close(object sender, RoutedEventArgs e)
        {
            MaterialDesignThemes.Wpf.Chip SenderChip = sender as MaterialDesignThemes.Wpf.Chip;
            this.RecentsRegs.Remove(SenderChip.Content.ToString());
            this.SaveRecentRegs();
        }

        // -------------------------------------------------------------------------
        // Fermeture du popup "Recent Registries"
        // -------------------------------------------------------------------------
        private void Pu_OpenRecent_Close(object sender, RoutedEventArgs e)
        {
            Pu_Recent.IsOpen = false;
        }

        // -------------------------------------------------------------------------
        // Sauve dans les UserSettings le contenu de la liste RecentRegs
        // -------------------------------------------------------------------------
        private void SaveRecentRegs()
        {
            Properties.Settings.Default.Recent_1 = RecentsRegs.GetNameAt(0);
            Properties.Settings.Default.Recent_2 = RecentsRegs.GetNameAt(1);
            Properties.Settings.Default.Recent_3 = RecentsRegs.GetNameAt(2);
            Properties.Settings.Default.Recent_4 = RecentsRegs.GetNameAt(3);
            Properties.Settings.Default.Recent_5 = RecentsRegs.GetNameAt(4);
            Properties.Settings.Default.Recent_6 = RecentsRegs.GetNameAt(5);
            Properties.Settings.Default.Save();
        }

        // -------------------------------------------------------------------------
        // On clique sur une RecentChip: on charge le fichier ou la ruche (Hive)
        // -------------------------------------------------------------------------
        private void Bt_RecentChip_Click(object sender, RoutedEventArgs e)
        {
            // On recupère la Chip qui a été cliquée
            MaterialDesignThemes.Wpf.Chip SenderChip = sender as MaterialDesignThemes.Wpf.Chip;
            // On energistre son nom
            string RecentRegToLoadName = SenderChip.Content.ToString();
            CurrentRegistry = new RecentRegistry(RecentRegToLoadName);

            // On renseigne l'InfoChip
            Tree_InfoChip.Content = CurrentRegistry.Name;
            Tree_InfoChipIcon.Kind = CurrentRegistry.Icon;

            // Si le RecentReg est un fichier
            if (CurrentRegistry.GetGenre() == RecentRegistry.Genre.file)
            {
                // On remplit le RegistryTree à partir du fichier REG
                Parser1.ParseFile(CurrentRegistry.Name);
                Parser1.BuildList();
                // Gestion de l'affichage
                this.ReInitDisplay();
            }
            // Si le RecentReg est un subtree de la Registry
            else if (CurrentRegistry.GetGenre() == RecentRegistry.Genre.hive)
            {
                // On remplit le RegistryTree à partir du subtree de Registre
                Parser2.ParseHive(CurrentRegistry.Name);
                Parser2.BuildList();
                // Gestion de l'affichage
                this.ReInitDisplay();
            }
            // Autres cas...
            else
            {
                // Gestion de l'affichage
                this.ReInitDisplay();
            }

            // On ferme le popup
            Pu_Recent.IsOpen = false;
        }

        // -------------------------------------------------------------------------
        // Masque le message "Drop your file here" et 
        // Réinitialisation de la Recherche
        // -------------------------------------------------------------------------
        private void ReInitDisplay(bool showLengthstats = false)
        {
            DropZone.Visibility = Visibility.Hidden;
            TreeView1.Visibility = Visibility.Visible;
            Bt_LengthStats.Visibility = showLengthstats ? Visibility.Visible : Visibility.Hidden;
            // Initialisation de la recherche
            SearchedWordResultsIndex = 0;
            SearchedWordIsDirty = false;
            Lb_SearchedWordCount.Text = "";
        }


    }
}