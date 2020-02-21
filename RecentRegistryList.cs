using System.Collections.ObjectModel;

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
        // Si la valeur est négative, on part de la fin: -1 étant le dernier.
        // -------------------------------------------------------------------------
        public string GetNameAt(int index)
        {
            if (index <0)
                index = this.Count + index;

            if (index < this.Count)
                return this[index].Name;
            else
                return string.Empty;
        }

    }
}
