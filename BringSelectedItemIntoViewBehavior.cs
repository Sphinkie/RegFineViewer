using System.Windows;
using System.Windows.Controls;

// -----------------------------------------------------------------------------------------
// [WPF][MVVM] TreeView: Scroll To Selected Item
// -----------------------------------------------------------------------------------------
// 2015-09-020 
// Here is the MVVM way to bring selected TreeViewItem into a view.
// First we need an attached behavior.We cannot use regular behavior because we will attach this property through Style.
// And now we can attach this behavior through property in a ItemContainerStyle
// Done! Just set IsSelected to true from code.
// Here is full project(C# 6.0) on GitHub:
// https://github.com/drussilla/WpfExampleBringTreeViewItemIntoViewMVVM
// -----------------------------------------------------------------------------------------

namespace RegFineViewer
{
    public class BringSelectedItemIntoViewBehavior
    {
        public static readonly DependencyProperty IsBringSelectedIntoViewProperty = DependencyProperty.RegisterAttached(
            "IsBringSelectedIntoView", typeof(bool), typeof(BringSelectedItemIntoViewBehavior), new PropertyMetadata(default(bool), PropertyChangedCallback));

        // Set
        public static void SetIsBringSelectedIntoView(DependencyObject element, bool value)
        {
            element.SetValue(IsBringSelectedIntoViewProperty, value);
        }

        // Get
        public static bool GetIsBringSelectedIntoView(DependencyObject element)
        {
            return (bool)element.GetValue(IsBringSelectedIntoViewProperty);
        }

        // Callback
        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var treeViewItem = dependencyObject as TreeViewItem;
            if (treeViewItem == null)
            {
                return;
            }

            if (!((bool)dependencyPropertyChangedEventArgs.OldValue) &&
                ((bool)dependencyPropertyChangedEventArgs.NewValue))
            {
                treeViewItem.Unloaded += TreeViewItemOnUnloaded;
                treeViewItem.Selected += TreeViewItemOnSelected;
            }
        }

        private static void TreeViewItemOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var treeViewItem = sender as TreeViewItem;
            if (treeViewItem == null)
            {
                return;
            }
            treeViewItem.Unloaded -= TreeViewItemOnUnloaded;
            treeViewItem.Selected -= TreeViewItemOnSelected;
        }

        private static void TreeViewItemOnSelected(object sender, RoutedEventArgs routedEventArgs)
        {
            var treeViewItem = sender as TreeViewItem;
            treeViewItem?.BringIntoView();
        }
    }
}