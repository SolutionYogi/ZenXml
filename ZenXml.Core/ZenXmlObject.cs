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

        private const string AsEnumerableMethodName = "AsEnumerable";

        private const string AsMethodName = "As";

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

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if(binder.Type == typeof(string))
            {
                result = Container.ToString();
                return true;
            }

            return base.TryConvert(binder, out result);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if(binder.Name.Equals(AsEnumerableMethodName, StringComparison.OrdinalIgnoreCase))
            {
                result = _container.Elements().Select(x => new ZenXmlObject(x, _comparison));
                return true;
            }

            if(binder.Name.Equals(AsMethodName, StringComparison.OrdinalIgnoreCase))
            {
                var typeList = binder.GetGenericTypeArguments();
                if(typeList.Count == 0)
                    throw new InvalidOperationException("The As method must be called with a single type parameter.");

                if(typeList.Count > 1)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "The As method must be called with a single type parameter. Current method call passed multiple type parameter. Type parameter list: [{0}]",
                            string.Join(", ", typeList.Select(x => x.Name))));
                }

                if(args.Length != 0)
                    throw new InvalidOperationException(string.Format("As method must be called without parameters. Current parameter count: {0}", args.Length));

                result = binder.GetGenericTypeArguments().Single().Name;
                return true;
            }

            result = null;
            return false;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            Logger.Trace(string.Format("Binder.Name: {0}", binder.Name));

            if(binder.Name.Equals(RootPropertyName, StringComparison.OrdinalIgnoreCase) && IsRoot)
            {
                var root = (XDocument) _container;
                Logger.Trace("Returning Root.");
                result = new ZenXmlObject(root.Root, _comparison);
                return true;
            }

            return TryGetMemberXContainer(binder, out result);
        }

        private bool TryGetMemberXContainer(GetMemberBinder binder, out object result)
        {
            var attribute = GetAttribute(binder.Name);

            if(attribute != null)
            {
                result = attribute.Value;
                return true;
            }

            var matchingChilds = Container.Elements().Where(x => x.Name.LocalName.Equals(binder.Name, _comparison)).ToList();

            if(matchingChilds.Count == 0)
            {
                result = null;
                return false;
            }

            if(matchingChilds.Count == 1)
            {
                var child = matchingChilds.Single();

                if(! child.HasElements && ! child.HasAttributes)
                {
                    result = child.Value;
                    return true;
                }

                result = new ZenXmlObject(child, _comparison);
                return true;
            }

            result = matchingChilds.Select(x => new ZenXmlObject(x, _comparison));
            return true;
        }

        private XAttribute GetAttribute(string name)
        {
            var element = Container as XElement;
            return element == null ? null : element.Attributes().SingleOrDefault(x => x.Name.LocalName.Equals(name, _comparison));
        }

        public override string ToString()
        {
            return Container.ToString();
        }
    }
}