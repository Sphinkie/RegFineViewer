using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
            Name = name;
            DType = type;
            Value = string.Empty;
            SubItem = new ObservableCollection<RegistryItem>();
        }
        // Ajout d'un sous-item (key ou Node).
        public void AddSubItem(RegistryItem subnode) { SubItem.Add(subnode); }
        // variables
        public string Name { get; set; }
        public string DType { get; }
        public string Value { get; set; }
        public ObservableCollection<RegistryItem> SubItem { get; }
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
            // On initialise les parseurs
            Parser1 = new RegFileParser(RegistryTree1);
            Parser2 = new RegFileParser(RegistryTree2);
            // On binde les RegistryTree avec les TreeView de l'affichage
            // TreeView1.ItemsSource = RegistryTree1;
            TreeView2.ItemsSource = RegistryTree2;
            // Normalement on devrait pouvoir mettre ceci dans le XAML du TreeView, mais ça marche pas 
            // ... ItemsSource="{Binding Source=RegistryTree2}" ...
        }

        // -------------------------------------------------------------------------
        // Pour les tests ce bouton remplit RegistryTree
        // -------------------------------------------------------------------------
        private void FillRegistryTree()
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

        private void Tree2_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

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
            string fileName = droppedFiles[0];
            Tree1_InfoChip.Content = fileName;

            //listFiles.Items.Clear();
            //listFiles.Items.Add("Load in Tree 1:");
            //foreach (string s in droppedFiles)
            //{
            //    listFiles.Items.Add(s);
            //}
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
        // Bouton EXPAND (de la toolbar)
        // -------------------------------------------------------------------------
        private void Tree1_Expand_bt(object sender, RoutedEventArgs e)
        {
            {
                object selectedNode = TreeView1.SelectedItem;
                if (selectedNode != null)
                {
                    // string tt = selectedNode.ToString(); // selectedItem / SelectedValue :	renvoient	tt="RegFineViewer.RegistryNode"
                    TreeViewItem tvi = TreeView1.ItemContainerGenerator.ContainerFromItem(selectedNode) as TreeViewItem;
                    tvi.ExpandSubtree();
                    // tvi.IsExpanded = true;   // fonctionne aussi (1 niveau ?)
                }
            }

        }
        private void Tree2_Expand_bt(object sender, RoutedEventArgs e)
        {
            object selectedNode = TreeView2.SelectedItem;
            if (selectedNode != null)
            {
                // string tt = selectedNode.ToString(); // selectedItem / SelectedValue :	renvoient	tt="RegFineViewer.RegistryNode"
                TreeViewItem tvi = TreeView2.ItemContainerGenerator.ContainerFromItem(selectedNode) as TreeViewItem;
                tvi.ExpandSubtree();
                // tvi.IsExpanded = true;   // fonctionne aussi (1 niveau seulement ?)
            }
        }

        // -------------------------------------------------------------------------
        // Bouton COLLAPSE (de la toolbar)
        // -------------------------------------------------------------------------
        private void Tree1_Collapse_bt(object sender, RoutedEventArgs e)
        {
            FillRegistryTree();
        }
        private void Tree2_Collapse_bt(object sender, RoutedEventArgs e)
        {
            object selectedNode = TreeView2.SelectedItem;
            if (selectedNode != null)
            {
                TreeViewItem tvi = TreeView2.ItemContainerGenerator.ContainerFromItem(selectedNode) as TreeViewItem;
                tvi.IsExpanded = false;
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

    }
}
