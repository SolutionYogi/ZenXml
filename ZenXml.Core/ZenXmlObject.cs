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
        private const string RootPropertyName = "Root";

        private const string InnerTextPropertyName = "InnerText";

        private const string AsEnumerableMethodName = "AsEnumerable";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly XContainer _container;

        private readonly StringComparison _comparison;

        public XContainer Container
        {
            get { return _container; }
        }

        private bool IsRoot
        {
            get { return _container is XDocument; }
        }

        protected ZenXmlObject(XContainer container, StringComparison comparison)
        {
            if(container == null)
                throw new ArgumentNullException("container");

            _container = container;
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

            return CreateFromXContainer(XDocument.Parse(xml), comparison);
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

            return CreateFromXContainer(XDocument.Load(xmlFilePath), comparison);
        }

        public static dynamic CreateFromXContainer(XContainer document)
        {
            return CreateFromXContainer(document, StringComparison.OrdinalIgnoreCase);
        }

        public static dynamic CreateFromXContainer(XContainer document, StringComparison comparison)
        {
            if(document == null)
                throw new ArgumentNullException("document");

            return new ZenXmlObject(document, comparison);
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            Logger.Info("Method called " + binder.CallInfo.ArgumentCount);
            return base.TryInvoke(binder, args, out result);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if(binder.Name.Equals(AsEnumerableMethodName))
            {
                result = _container.Elements().Select(x => new ZenXmlObject(x, _comparison));
                return true;
            }

            result = null;
            return false;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            Logger.Trace(string.Format("Binder.Name: {0}", binder.Name));

            if(binder.Name.Equals(RootPropertyName) && IsRoot)
            {
                var root = (XDocument) _container;
                Logger.Trace("Returning Root.");
                result = new ZenXmlObject(root.Root, _comparison);
                return true;
            }

            if(binder.Name.Equals(InnerTextPropertyName))
            {
                var el = Container as XElement;
                if(el == null)
                    throw new InvalidOperationException("Can not use InnerText property for a container which is not an XElement.");

                result = el.Value;
                return true;
            }

            var asElement = Container as XElement;

            if(asElement != null)
            {
                var attribute = asElement.Attributes().SingleOrDefault(x => x.Name.LocalName.Equals(binder.Name, _comparison));
                if(attribute != null)
                {
                    result = attribute.Value;
                    return true;
                }
            }

            var elements = Container.Elements().Where(x => x.Name.LocalName.Equals(binder.Name, _comparison)).ToList();

            if(elements.Count == 0)
            {
                result = null;
                return false;
            }

            if(elements.Count == 1)
            {
                var element = elements.Single();

                if(!element.HasElements && !element.HasAttributes)
                {
                    result = element.Value;
                    return true;
                }

                if(element.HasElements)
                {
                    result = new ZenXmlObject(element, _comparison);
                    return true;
                }

                result = new ZenXmlObject(element, _comparison);
                return true;
            }

            result = elements.Select(x => new ZenXmlObject(x, _comparison));
            return true;
        }
    }
}