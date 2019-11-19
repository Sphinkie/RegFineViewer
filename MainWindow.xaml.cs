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

    public class RegistryItem
    {
        // constructeur
        public RegistryItem(string name, string type)
        {
            // Initialisations
            Name = name;
            DType = type;
            Value = string.Empty;
            UserFriendlyUnit = string.Empty;
            UserFriendlyValue = string.Empty;
            SubItem = new ObservableCollection<RegistryItem>();
        }
        public void SetUnitToHex()
        {
            UserFriendlyUnit = "hex";
            Int32 intValue = Convert.ToInt32(Value);
            UserFriendlyValue = "0x" + intValue.ToString("X");
        }
        public void SetUnitToSec()
        {
            UserFriendlyUnit = "seconds";
            double intValue = Convert.ToInt32(Value);
            TimeSpan time = TimeSpan.FromSeconds(intValue);
            UserFriendlyValue = time.ToString(); //  default format is: [-][d.]hh:mm:ss[.fffffff]
        }
        public void SetUnitToFrames()
        {
            UserFriendlyUnit = "frames";
            double intValue = Convert.ToInt32(Value);
            intValue = intValue * 0.040;
            TimeSpan time = TimeSpan.FromSeconds(intValue);
            // Backslash is just to tell that : is not the part of format, but a character that we want in output.
            string hh_mm_ss = time.ToString(@"hh\:mm\:ss");
            string millisec = time.ToString("fff");
            int frames = Convert.ToInt32(millisec) / 40;
            UserFriendlyValue = hh_mm_ss + ":" + frames.ToString("D2");
        }
        public void SetUnitToNone()
        {
            UserFriendlyUnit = string.Empty;
            UserFriendlyValue = string.Empty;
        }
        // Ajout d'un sous-item (key ou Node)
        public void AddSubItem(RegistryItem subnode) { SubItem.Add(subnode); }
        // variables
        public string Name { get; set; }
        public string DType { get; }
        public string Value { get; set; }
        public ObservableCollection<RegistryItem> SubItem { get; }
        public string UserFriendlyUnit { get; set; }
        public string UserFriendlyValue { get; set; }
    }

    //
    public partial class MainWindow : Window
    {
        // Chaque RegistryTree est une collection de RegistryItems
        private ObservableCollection<RegistryItem> RegistryTree1 = new ObservableCollection<RegistryItem>();
        private ObservableCollection<RegistryItem> RegistryTree2 = new ObservableCollection<RegistryItem>();
        private RegFileParser Parser1;
        private RegFileParser Parser2;

        public MainWindow()
        {
            InitializeComponent();
            // Cette instruction permet de rendre les classes visibles depuis le XAML
            DataContext = this;
            // On charge le dictionnaire des unités préférées
            KeyUnitDictionnary UnitDictionnary = new KeyUnitDictionnary("Config.xml");
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
            RegistryTree2.Add(N1);

            RegistryItem K3 = new RegistryItem("clef 3", "dword");
            RegistryItem N2 = new RegistryItem("Node 2", "node");
            K3.Value = "0003";
            N2.AddSubItem(K3);
            RegistryTree2.Add(N2);

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

            if ((null == droppedFiles) || (!droppedFiles.Any())) { return; }
            // S'il y a un seul fichier droppé, on l'ouvre dans la TreeView courante
            if (droppedFiles.Length == 1)
            {
                string fileName = droppedFiles[0];
                Tree1_InfoChip.Content = fileName;
                // On remplit le RegistryTree à partir du fichier
                Parser1.ParseFile(fileName);
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
            if ((null == droppedFiles) || (!droppedFiles.Any())) { return; }

            // S'il y a un seul fichier droppé, on l'ouvre dans la TreeView courante
            if (droppedFiles.Length == 1)
            {
                string fileName = droppedFiles[0];
                Tree2_InfoChip.Content = fileName;
                // On remplit le RegistryTree à partir du fichier
                Parser2.ParseFile(fileName);
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
            }
        }

        // -------------------------------------------------------------------------
        // Bouton CLOSE (de la chip)
        // -------------------------------------------------------------------------
        private void Tree1_CloseFile_bt(object sender, RoutedEventArgs e)
        {
            Tree1_InfoChip.Content = "no file loaded";
            RegistryTree1.Clear();
        }
        private void Tree2_CloseFile_bt(object sender, RoutedEventArgs e)
        {
            Tree2_InfoChip.Content = "no file loaded";
            RegistryTree2.Clear();
        }

        // -------------------------------------------------------------------------
        // Selection
        // -------------------------------------------------------------------------
        private void TreeView1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }
        private void Tree2_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }

        // -------------------------------------------------------------------------
        // Bouton du Tray
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

        private void CardlengthStats_Close(object sender, RoutedEventArgs e)
        {
            CardlengthStats.IsOpen = false;
        }
        private void CardTreeInfo_Close(object sender, RoutedEventArgs e)
        {
            CardTreeInfo.IsOpen = false;
        }

    }
}
