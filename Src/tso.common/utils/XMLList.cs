using System;
using System.Collections.Generic;
using System.Xml;

namespace FSO.Common.Utils
{
    public class XMLList<T> : List<T>, IXMLEntity where T : IXMLEntity
    {
        private string NodeName;

        public XMLList(string nodeName)
        {
            NodeName = nodeName;
        }

        public XMLList()
        {
            NodeName = "Unknown";
        }

        #region IXMLPrinter Members

        public XmlElement Serialize(XmlDocument doc)
        {
            var element = doc.CreateElement(NodeName);
            foreach (var child in this)
            {
                element.AppendChild(child.Serialize(doc));
            }
            return element;
        }

        public void Parse(XmlElement element)
        {
            var type = typeof(T);

            foreach (XmlElement child in element.ChildNodes)
            {
                var instance = (T)Activator.CreateInstance(type);
                instance.Parse(child);
                Add(instance);
            }
        }

        #endregion
    }
}
