using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RegFineViewer
{
    public class BringSelectedItemIntoViewBehavior
    {
        public static readonly DependencyProperty IsBringSelectedIntoViewProperty = DependencyProperty.RegisterAttached(
            "IsBringSelectedIntoView", typeof(bool), typeof(BringSelectedItemIntoViewBehavior), new PropertyMetadata(default(bool), PropertyChangedCallback));

        public static void SetIsBringSelectedIntoView(DependencyObject element, bool value)
        {
            element.SetValue(IsBringSelectedIntoViewProperty, value);
        }

        public static bool GetIsBringSelectedIntoView(DependencyObject element)
        {
            return (bool)element.GetValue(IsBringSelectedIntoViewProperty);
        }

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