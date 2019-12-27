
namespace RegFineViewer
{
    public class RecentRegistry
    {
        public string Name { set; get; }
        public object Icon { private set; get; }

        public RecentRegistry(string name)
        {
            this.Name = name;
            if (Name.StartsWith("[") && Name.EndsWith("]"))
                Icon = MaterialDesignThemes.Wpf.PackIconKind.Cube;
            else if (Name.EndsWith(".reg"))
                Icon = MaterialDesignThemes.Wpf.PackIconKind.File;
            else
                Icon = MaterialDesignThemes.Wpf.PackIconKind.QuestionMarkCircle;
        }

        public void setIcon(string icon)
        {
            if (icon == "File")
                Icon = MaterialDesignThemes.Wpf.PackIconKind.File;
            else if (icon == "Cube")
                Icon = MaterialDesignThemes.Wpf.PackIconKind.Cube;
            else
                Icon = MaterialDesignThemes.Wpf.PackIconKind.About;
        }
    }


}
