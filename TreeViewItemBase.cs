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
    // qui permettent de d'accéder à ces attributs de la TreeView depuis le code-bihind
    // (Ce que le WPF ne permet pas de façon native)
    // ----------------------------------------------------------
    public class TreeViewItemBase : INotifyPropertyChanged
    {
        private bool isSelected;
        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                if (value != this.isSelected)
                {
                    this.isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }
        }

        private bool isExpanded;
        public bool IsExpanded
        {
            get { return this.isExpanded; }
            set
            {
                if (value != this.isExpanded)
                {
                    this.isExpanded = value;
                    NotifyPropertyChanged("IsExpanded");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
//        < Setter Property="IsSelected" Value="{Binding IsSelected}" />
//        <Setter Property = "IsExpanded" Value="{Binding IsExpanded}" />
//    </Style>
// </TreeView.ItemContainerStyle>
// -------------------------------------------------------------
// Malheureusement non compatible avec la library MaterialDesign
// -------------------------------------------------------------

