using System;
using System.Collections.Generic;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;

namespace RegFineViewer
{
    // ----------------------------------------------------------
    // Cette classe de base provient du tutoriel WPF-TUTORIAL;COM
    // Les objets qui heritent de cette classe de base bénéficie de deux propriétés:
    // isSelected et isExpanded
    // qui permettent de d'accéder à ces attributs de la TreeView depuis le code-behind
    // (Ce que le WPF ne permet pas de façon native)
    // ----------------------------------------------------------
    public class TreeViewItemBase : INotifyPropertyChanged
    {
        // ----------------------------------------------------------
        // Propriété isSelected et IsSelected
        // ----------------------------------------------------------
        private bool isSelected;
        public bool IsSelected
        {
            // ------------------------------------------------------
            // Get: on stocke dana la variable privée
            // ------------------------------------------------------
            get
            { 
                return this.isSelected; 
            }
            // ------------------------------------------------------
            // Set: si changement, on notifie le TreeViewItem
            // ------------------------------------------------------
            set
            {
                if (value != this.isSelected)
                {
                    this.isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }
        }

        // ----------------------------------------------------------
        // Propriété isExpanded et IsExpanded
        // ----------------------------------------------------------
        private bool isExpanded;
        public bool IsExpanded
        {
            // ------------------------------------------------------
            // Get: on stocke dana la variable privée
            // ------------------------------------------------------
            get { return this.isExpanded; }
            // ------------------------------------------------------
            // Set: si changement, on notifie le TreeViewItem
            // ------------------------------------------------------
            set
            {
                if (value != this.isExpanded)
                {
                    this.isExpanded = value;
                    NotifyPropertyChanged("IsExpanded");
                }
            }
        }

        // ----------------------------------------------------------
        // fonction héritée de INotifyPropertyChanged
        // ----------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;

        // ----------------------------------------------------------
        // Notification
        // ----------------------------------------------------------
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

}

// -------------------------------------------------------------
// Necessite aussi la section suivante dans le XAML  <TreeView>
// -------------------------------------------------------------
// <TreeView.ItemContainerStyle>
//    <Style TargetType = "TreeViewItem" >
//        <Setter Property= "IsSelected" Value="{Binding IsSelected}" />
//        <Setter Property= "IsExpanded" Value="{Binding IsExpanded}" />
//    </Style>
// </TreeView.ItemContainerStyle>
// -------------------------------------------------------------
// Malheureusement non-compatible avec la library MaterialDesign
// -------------------------------------------------------------

