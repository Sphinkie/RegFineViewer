
namespace RegFineViewer
{
    public class RecentRegistry
    {
        // --------------------------------------------------------------------------
        // Variables publiques
        // --------------------------------------------------------------------------
        public enum Genre { file, hive, other };
        // --------------------------------------------------------------------------
        public string Name { private set; get; }
        public object Icon { private set; get; }
        // --------------------------------------------------------------------------
        private Genre RegGenre;

        // --------------------------------------------------------------------------
        // Constructeur: fournir le Nom
        // --------------------------------------------------------------------------
        public RecentRegistry(string name)
        {
            this.Name = name;
            if (Name.StartsWith("[") && Name.EndsWith("]"))
            {
                this.RegGenre = Genre.hive;
                this.Icon = MaterialDesignThemes.Wpf.PackIconKind.Cube;
            }
            else if (Name.EndsWith(".reg"))
            {
                this.RegGenre = Genre.file;
                this.Icon = MaterialDesignThemes.Wpf.PackIconKind.File;
            }
            else
            {
                this.RegGenre = Genre.other;
                this.Icon = MaterialDesignThemes.Wpf.PackIconKind.QuestionMarkCircle;
            }
        }

        // --------------------------------------------------------------------------
        // Positionne le type, l'icone (et le type de Parser?).
        // --------------------------------------------------------------------------
        public void SetGenre(Genre genre)
        {
            this.RegGenre = genre;
            if (genre == Genre.file)
                Icon = MaterialDesignThemes.Wpf.PackIconKind.File;
            else if (genre == Genre.hive)
                Icon = MaterialDesignThemes.Wpf.PackIconKind.Cube;
            else
                Icon = MaterialDesignThemes.Wpf.PackIconKind.About;
        }
        // --------------------------------------------------------------------------
        public Genre GetGenre()
        {
            return RegGenre;
        }
    }

    }
