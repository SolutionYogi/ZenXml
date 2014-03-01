using System;
using System.Dynamic;
using System.IO;
using System.Xml.Linq;

namespace ZenXml.Core
{
    public class ZenXmlObject : DynamicObject
    {
        private ZenXmlObject()
        {
        }

        public XDocument Document { get; private set; }

        public static ZenXmlObject CreateFromXml(string xml)
        {
            if(string.IsNullOrWhiteSpace(xml))
                throw new ArgumentNullException("xml");

            return CreateFromXDocument(XDocument.Parse(xml));
        }

        public static ZenXmlObject CreateFromFile(string xmlFilePath)
        {
            if(string.IsNullOrWhiteSpace(xmlFilePath))
                throw new ArgumentNullException("xmlFilePath");
            
            if(! File.Exists(xmlFilePath))
                throw new ArgumentException(string.Format("File specified by path {0} does not exist.", xmlFilePath));

            return CreateFromXDocument(XDocument.Load(xmlFilePath));
        }

        private static ZenXmlObject CreateFromXDocument(XDocument document)
        {
            if(document == null)
                throw new ArgumentNullException("document");

            return new ZenXmlObject
                   {
                       Document = document
                   };
        }
    }
}
