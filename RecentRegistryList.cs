using System.Collections.ObjectModel;

namespace RegFineViewer
{
    public class RecentRegistryList : ObservableCollection<RecentRegistry>
    {
        public RecentRegistryList()
        {
        }

        // -------------------------------------------------------------------------
        // -------------------------------------------------------------------------
        public void Add(string name)
        {
            if (name != string.Empty)
                if (!this.Contains(name))
                    this.Add(new RecentRegistry(name));
        }

        // -------------------------------------------------------------------------
        // -------------------------------------------------------------------------
        public void Remove(string name)
        {
            int index = this.IndexOf(name);
            if (index >= 0)
                this.RemoveItem(index);
        }

        // -------------------------------------------------------------------------
        // Surcharge
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
        // Surcharge
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
        // 
        // -------------------------------------------------------------------------
        public string GetNameAt(int index)
        {
            if (index < this.Count)
                return this[index].Name;
            else
                return string.Empty;
        }

    }
}
