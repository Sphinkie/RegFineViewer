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
        private List<string> UnitList = new List<string> {"hex", "seconds", "frames" };
        private Dictionary<string, string> UnitDictionnary = new Dictionary<string, string>();

        // ------------------------------------------------------------------
        // Constructeur
        // ------------------------------------------------------------------
        public KeyUnitDictionnary(string filename)
        {
            
            try
            {
                XmlReader xmlReader = XmlReader.Create(filename);
                try
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
                catch (XmlException e)
                { // XML incorrect
                    xmlReader.Close(); 
                }
            }
            catch (FileNotFoundException e)
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
        // Modifie ou Ajoute une nouvelle entrée au dictionnaire
        // ------------------------------------------------------------------
        public void SetValue(string key, string value)
        {
            UnitDictionnary[key] = value;
        }

        // ------------------------------------------------------------------
        // Destructeur
        // ------------------------------------------------------------------
        ~KeyUnitDictionnary()
        {
            XmlWriter xmlWriter = XmlWriter.Create("Config_sample.xml");

            xmlWriter.WriteStartDocument();

            xmlWriter.WriteStartElement("Root");
            xmlWriter.WriteStartElement("Units");

            xmlWriter.WriteStartElement("Prefered");
            xmlWriter.WriteAttributeString("unit", "seconds");
            xmlWriter.WriteString("Verification");
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("Prefered");
            xmlWriter.WriteAttributeString("unit", "frames");
            xmlWriter.WriteString("Preroll stop");
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();    // Units

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
