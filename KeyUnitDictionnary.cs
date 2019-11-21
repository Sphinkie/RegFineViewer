using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RegFineViewer
{
    class KeyUnitDictionnary
    {
        private List<string> UnitList = new List<string> { "hex", "seconds", "frames", "bool" };
        private Dictionary<string, string> UnitDictionnary = new Dictionary<string, string>();

        // ------------------------------------------------------------------
        // Constructeur
        // ------------------------------------------------------------------
        public KeyUnitDictionnary(string filename)
        {
            try             // On essaye d'ouvrir le fichier de configuration
            {
                XmlReader xmlReader = XmlReader.Create(filename);
                try        // On essaye de lire le fichier de configuration
                {
                    while (xmlReader.Read())
                    {
                        if ((xmlReader.NodeType == XmlNodeType.Element) &&
                            (xmlReader.Name == "Prefered"))
                        {
                            if (xmlReader.HasAttributes)
                            {
                                string unit = xmlReader.GetAttribute("unit");
                                string key = xmlReader.ReadElementContentAsString();
                                UnitDictionnary[key] = unit;
                            }
                        }
                    }
                    xmlReader.Close();
                }
                catch (XmlException)
                { // XML incorrect
                    xmlReader.Close();
                }
            }
            catch (FileNotFoundException)
            {
                // Pas de fichier config: c'est pas grave
            }
        }

        // ------------------------------------------------------------------
        // Retourne la valeur de l'unité associée à la key
        // ------------------------------------------------------------------
        public string GetValue(string key)
        {
            UnitDictionnary.TryGetValue(key, out string unit);
            // S'il n'y a pas d'unité préférée pour cette key, on sort
            if (string.IsNullOrEmpty(unit))
                return string.Empty;
            // on vérifie que l'unité lue fait partie de la liste autorisée
            if (UnitList.Contains(unit))
                return unit;
            else
                return string.Empty;
        }

        // ------------------------------------------------------------------
        // Modifie ou ajoute une nouvelle entrée au dictionnaire
        // ------------------------------------------------------------------
        public void SetValue(string key, string unit)
        {
            // on vérifie que l'unité demandée fait partie de la liste autorisée

            if (UnitList.Contains(unit))
            {
                if (UnitDictionnary.ContainsKey(key))
                    UnitDictionnary[key] = unit;
                else
                    UnitDictionnary.Add(key, unit);
            }
            else    // l'unité demandé ne fait pas partie de la liste autorisée
            {
                if (UnitDictionnary.ContainsKey(key))
                    UnitDictionnary.Remove(key);
            }
        }

        // ------------------------------------------------------------------
        // Retourne l'unité suivante de la liste
        // ------------------------------------------------------------------
        public string GetNextUnit(string unit)
        {
            int Index = UnitList.IndexOf(unit);
            int NewIndex = Index + 1;
            if (NewIndex >= UnitList.Count()) NewIndex = 0;
            return UnitList[NewIndex];
        }
        // ------------------------------------------------------------------
        // Destructeur
        // ------------------------------------------------------------------
        ~KeyUnitDictionnary()
        {
            // On definit le style du fichier de sortie
            var settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;

            // On ecrit dans le fichier de sortie
            XmlWriter xmlWriter = XmlWriter.Create("Config_sample.xml", settings);

            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Root");

            // On enregistre toutes les preferred units du dictionnaire
            xmlWriter.WriteStartElement("Units");
            foreach (KeyValuePair<string, string> entry in UnitDictionnary)
            {
                // do something with entry.Value or entry.Key
                xmlWriter.WriteStartElement("Prefered");
                xmlWriter.WriteAttributeString("unit", entry.Value);
                xmlWriter.WriteString(entry.Key);
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();    // Units

            // On enregistre les elements ouverts les plus recents
            xmlWriter.WriteStartElement("Recents");

            xmlWriter.WriteStartElement("Recent");
            xmlWriter.WriteAttributeString("type", "file");
            xmlWriter.WriteString(@"D:\source\repos\RegFineViewer\bin\Debug\example3.reg");
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("Recent");
            xmlWriter.WriteAttributeString("type", "reg");
            xmlWriter.WriteString(@"[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSRS12.MSSQLSERVER\MSSQLServer]");
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();    // Recents

            xmlWriter.WriteEndElement();    // Root

            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
        }

    }
}
