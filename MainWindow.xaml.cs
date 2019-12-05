using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;       // ObservableCollections
using System.Collections;                   // Hashtables


namespace RegFineViewer
{

    // -------------------------------------------------------------------------
    // Programme principal
    // -------------------------------------------------------------------------
    public partial class MainWindow : Window
    {
        // Chaque RegistryTree est une collection de RegistryItems
        private ObservableCollection<RegistryItem> RegistryTree1 = new ObservableCollection<RegistryItem>();
        private ObservableCollection<RegistryItem> RegistryTree2 = new ObservableCollection<RegistryItem>();
        private KeyUnitDictionnary UnitDictionnary;
        private RegFileParser Parser1;
        private RegFileParser Parser2;
        // Pour les recherches
        string SearchedWord;

        public MainWindow()
        {
            InitializeComponent();
            // Cette instruction permet de rendre les classes visibles depuis le XAML
            DataContext = this;
            // On charge le dictionnaire des unités préférées
            UnitDictionnary = new KeyUnitDictionnary("Config.xml");
            // On initialise les parseurs
            Parser1 = new RegFileParser(RegistryTree1, UnitDictionnary);
            Parser2 = new RegFileParser(RegistryTree2, UnitDictionnary);
            // On binde les RegistryTree avec les TreeView de l'affichage
            TreeView1.ItemsSource = RegistryTree1;
            TreeView2.ItemsSource = RegistryTree2;
            // Normalement on devrait pouvoir mettre ceci dans le XAML du TreeView, mais ça marche pas:
            // ... ItemsSource="{Binding Source=RegistryTree1}" ...
        }

        // -------------------------------------------------------------------------
        // Pour les tests, ce bouton remplit le RegistryTree2
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

            DropZone1.Visibility = Visibility.Hidden;
            TreeView1.Visibility = Visibility.Visible;

            // S'il y a un seul fichier droppé, on l'ouvre dans la TreeView courante
            if (droppedFiles.Length == 1)
            {
                string fileName = droppedFiles[0];
                Tree1_InfoChip.Content = fileName;
                // On remplit le RegistryTree à partir du fichier
                Parser1.ParseFile(fileName);
                Parser1.BuildList();
            }

            // S'il y a plusieurs fichiers droppés, on ouvre les deux premiers dans chaque TreeView
            else
            {
                string fileName1 = droppedFiles[0];
                string fileName2 = droppedFiles[1];
                Tree1_InfoChip.Content = fileName1;
                Tree2_InfoChip.Content = fileName2;
                // On remplit le RegistryTree à partir du fichier
                Parser1.ParseFile(fileName1);
                Parser2.ParseFile(fileName2);
                Parser1.BuildList();
                Parser2.BuildList();
            }
        }
        private void Tree2_drop(object sender, DragEventArgs e)
        {
            string[] droppedFiles = null;
            // on remplit la liste avec les fichiers qui ont été droppés dans la fenetre
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                droppedFiles = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            }
            // En cas de liste vide, on sort.
            if ((droppedFiles == null) || (!droppedFiles.Any())) { return; }

            DropZone2.Visibility = Visibility.Hidden;
            TreeView2.Visibility = Visibility.Visible;

            // S'il y a un seul fichier droppé, on l'ouvre dans la TreeView courante
            if (droppedFiles.Length == 1)
            {
                string fileName = droppedFiles[0];
                Tree2_InfoChip.Content = fileName;
                // On remplit le RegistryTree à partir du fichier
                Parser2.ParseFile(fileName);
                Parser2.BuildList();
            }
            // S'il y a plusieurs fichiers droppés, on ouvre les deux premiers dans chaque TreeView
            else
            {
                string fileName1 = droppedFiles[0];
                string fileName2 = droppedFiles[1];
                Tree1_InfoChip.Content = fileName1;
                Tree2_InfoChip.Content = fileName2;
                // On remplit le RegistryTree à partir du fichier
                Parser1.ParseFile(fileName1);
                Parser2.ParseFile(fileName2);
                Parser1.BuildList();
                Parser2.BuildList();
            }
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
        private void Tree2_CloseFile_bt(object sender, RoutedEventArgs e)
        {
            Tree2_InfoChip.Content = "no file loaded";
            RegistryTree2.Clear();
            DropZone2.Visibility = Visibility.Visible;
            TreeView2.Visibility = Visibility.Hidden;

        }

        // -------------------------------------------------------------------------
        // Bouton FIND (barre de recherche)
        // -------------------------------------------------------------------------
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
            Result.IsSelected = true;
            // On expand le node parent
            TreeViewItem item = TreeView1.SelectedItem as TreeViewItem;
            if (item is TreeViewItem)
                item.BringIntoView();
            RegistryItem racine = TreeView1.Items[0] as RegistryItem;
            racine.IsExpanded = true;
        }

        private void Tree2_Search_bt(object sender, RoutedEventArgs e)
        {
            this.SearchedWord = SearchedWord2.Text.ToUpper();
            RegistryItem Result = Parser1.NodeList.Find(Predicat);
        }

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
        }
        private void Tree2_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }

        // -------------------------------------------------------------------------
        // Boutons du Tray
        // -------------------------------------------------------------------------
        private void bTray2Button1_Click(object sender, RoutedEventArgs e)
        {
            tbStatLevels.Text = Parser2.NbLevels.ToString();
            tbStatNodes.Text = Parser2.NbNodes.ToString();
            tbStatKeys.Text = Parser2.NbKeys.ToString();
            CardTreeInfo.IsOpen = !CardTreeInfo.IsOpen;
        }
        private void bTray1Button1_Click(object sender, RoutedEventArgs e)
        {
            tbStatLevels.Text = Parser1.NbLevels.ToString();
            tbStatNodes.Text = Parser1.NbNodes.ToString();
            tbStatKeys.Text = Parser1.NbKeys.ToString();
            CardTreeInfo.IsOpen = !CardTreeInfo.IsOpen;
        }

        private void bTray2Button2_Click(object sender, RoutedEventArgs e)
        {
            RefreshLengthStats(Parser2);
            // Affiche ou masque le popup
            CardlengthStats.IsOpen = !CardlengthStats.IsOpen;
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
        // Ferme le Popup
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
        // -------------------------------------------------------------------------
        private void TreeViewItem_OnItemSelected(object sender, RoutedEventArgs e)
        {
            var X = sender as TreeViewItem;
            var E = e;
            var S = e.Source;

            //le pb est qu'il y a (au bout d'un moment) plusieurs items sélectionnées et on passe ici N fois
            X.BringIntoView();
            TreeViewItem item = TreeView1.SelectedItem as TreeViewItem;

        }
    }
}