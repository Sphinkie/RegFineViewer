using System;
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
            XmlReader xmlReader = XmlReader.Create(filename);
            while (xmlReader.Read())
            {
                if ((xmlReader.NodeType == XmlNodeType.Element) &&
                    (xmlReader.Name == "Prefered"))
                {
                    string key = xmlReader.ReadElementContentAsString();
                    if (xmlReader.HasAttributes)
                    {
                        string unit = xmlReader.GetAttribute("unit");
                        UnitDictionnary[key] = unit;
                    }
                }
            }
            xmlReader.Close();
        }
        // ------------------------------------------------------------------
        // Retourne la valeur de l'unité associée à la key
        // ------------------------------------------------------------------
        public string GetValue(string key)
        {
            string unit = UnitDictionnary[key];
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
            XmlWriter xmlWriter = XmlWriter.Create("test.xml");

            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Units");

            xmlWriter.WriteStartElement("Prefered");
            xmlWriter.WriteAttributeString("unit", "seconds");
            xmlWriter.WriteString("Verification");
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("Prefered");
            xmlWriter.WriteAttributeString("unit", "frames");
            xmlWriter.WriteString("Preroll stop");

            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
        }

    }
}
