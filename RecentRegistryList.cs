﻿using System.Collections.ObjectModel;

namespace RegFineViewer
{
    public class RecentRegistryList : ObservableCollection<RecentRegistry>
    {
        // -------------------------------------------------------------------------
        // Constructeur
        // -------------------------------------------------------------------------
        public RecentRegistryList() {}

        // -------------------------------------------------------------------------
        // Ajoute un item dans la liste (sauf s'il est vide ou s'il existe déjà)
        // -------------------------------------------------------------------------
        public void Add(string name)
        {
            if (name != string.Empty)
                if (!this.Contains(name))
                    this.Add(new RecentRegistry(name));
        }

        // -------------------------------------------------------------------------
        // Enlève un item de la liste (s'il existe)
        // -------------------------------------------------------------------------
        public void Remove(string name)
        {
            int index = this.IndexOf(name);
            if (index >= 0)
                this.RemoveItem(index);
        }

        // -------------------------------------------------------------------------
        // Surcharge. Renvoie l'index du premier item trouvé, ayant ce nom.
        // -------------------------------------------------------------------------
        public int IndexOf(string name)
        {
            if (name == string.Empty) return -1;

            for (int index = 0; index < this.Count; index++)
            {
                RecentRegistry item = this.Items[index];
                // Dès qu'on a trouvé un item avec ce nom: on sort
                if (item.Name == name)
                    return index;
            }
            // on n'a rien trouvé
            return -1;
        }

        // -------------------------------------------------------------------------
        // Surcharge. Renvoir TRUE si cet item est présent dans la liste.
        // -------------------------------------------------------------------------
        public bool Contains(string name)
        {
            foreach (RecentRegistry item in this)
            {
                // Dès qu'on a trouvé un item avec ce nom: on sort
                if (item.Name == name)
                    return true;
            }
            // on n'a rien trouvé
            return false;
        }

        // -------------------------------------------------------------------------
        // Retourne le nom de l'item ayant cet index.
        // Si l'index est négatif, on part de la fin: -1 étant le dernier.
        // -------------------------------------------------------------------------
        public string GetNameAt(int index)
        {
            // Si la valeur est négative, on convertit l'index: -1 étant le dernier.
            if (index <0)
                index = this.Count + index;
            if (index < 0)
                // Si l'index est encore negatif: pas d'item
                return string.Empty;
            else if (index >= this.Count)
                // Si l'index est superieur au nombre d'items dans la liste: pas d'item
                return string.Empty;
            else
                // Si les controles sont OK, on retourne l'item trouvé
                return this[index].Name;
        }

    }
}
