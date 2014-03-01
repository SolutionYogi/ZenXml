using System;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NLog;

namespace ZenXml.Core
{
    public class ZenXmlObject : DynamicObject
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly XDocument _document;

        private readonly ZenXmlObject _parent;

        private readonly StringComparison? _comparison;

        private ZenXmlObject Parent
        {
            get { return _parent; }
        }

        public XDocument Document
        {
            get
            {
                if(_document != null)
                    return _document;

                var currentParent = Parent;
                var document = currentParent._document;
                while(document == null)
                {
                    currentParent = currentParent.Parent;
                    document =  currentParent._document;
                }

                return document;
            }
        }

        public StringComparison Comparison
        {
            get
            {
                if(_comparison.HasValue)
                    return _comparison.Value;

                var currentParent = Parent;
                var comparison = currentParent._comparison;
                while(!comparison.HasValue)
                {
                    currentParent = currentParent.Parent;
                    comparison = currentParent._comparison;
                }

                return comparison.Value;
            }
        }

        private bool IsRoot
        {
            get { return _document != null; }
        }

        private ZenXmlObject(ZenXmlObject parent)
        {
            _parent = parent;
        }

        protected ZenXmlObject(XDocument document, StringComparison comparison)
        {
            if(document == null)
                throw new ArgumentNullException("document");

            _document = document;
            _comparison = comparison;
        
        }

        public static dynamic CreateFromXml(string xml)
        {
            return CreateFromXml(xml, StringComparison.OrdinalIgnoreCase);
        }

        public static dynamic CreateFromXml(string xml, StringComparison comparison)
        {
            if(string.IsNullOrWhiteSpace(xml))
                throw new ArgumentNullException("xml");

            return CreateFromXDocument(XDocument.Parse(xml), comparison);
        }

        public static dynamic CreateFromFile(string xmlFilePath)
        {
            return CreateFromFile(xmlFilePath, StringComparison.OrdinalIgnoreCase);
        }
        public static dynamic CreateFromFile(string xmlFilePath, StringComparison comparison)
        {
            if(string.IsNullOrWhiteSpace(xmlFilePath))
                throw new ArgumentNullException("xmlFilePath");

            if(! File.Exists(xmlFilePath))
                throw new ArgumentException(string.Format("File specified by path {0} does not exist.", xmlFilePath));

            return CreateFromXDocument(XDocument.Load(xmlFilePath), comparison);
        }

        public static dynamic CreateFromXDocument(XDocument document)
        {
            return CreateFromXDocument(document, StringComparison.OrdinalIgnoreCase);
        }

        public static dynamic CreateFromXDocument(XDocument document, StringComparison comparison)
        {
            if(document == null)
                throw new ArgumentNullException("document");

            return new ZenXmlObject(document, comparison);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            Logger.Trace(string.Format("Binder.Name: {0}", binder.Name));

            if(binder.Name.Equals("Root"))
            {
                Logger.Trace("Returning Root.");
                result = new ZenXmlObject(this);
                return true;
            }

            var element = Document.Elements().SingleOrDefault(x => x.Name.LocalName.Equals(binder.Name, Comparison));

            if(element != null)
            {
                if(!element.HasElements && !element.HasAttributes)
                {
                    result = element.Value;
                    return true;
                }
            }

            result = null;
            return true;
        }
    }
}