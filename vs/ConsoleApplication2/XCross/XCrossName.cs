using System.Xml.Linq;

namespace Misharp.XCross
{
    public class XCrossName
    {
        private readonly XName internalObject;

        private readonly XCrossNamespace xCrossNamespace;

        internal XCrossName(XName xName)
        {
            internalObject = xName;
            xCrossNamespace = new XCrossNamespace(internalObject.Namespace);
        }

        internal XName InternalObject
        {
            get { return internalObject; }
        }

        public static XCrossName Get(string expandedName)
        {
            return new XCrossName(XName.Get(expandedName));
        }

        public static XCrossName Get(string localName, string namespaceName)
        {
            return new XCrossName(XName.Get(localName, namespaceName));
        }

        public string LocalName 
        {
            get { return internalObject.LocalName; }
        }

        public XCrossNamespace Namespace
        {
            get { return xCrossNamespace; }
        }

        public string NamespaceName
        {
            get { return xCrossNamespace.NamespaceName; }
        }

        public override string ToString()
        {
            return internalObject.ToString();
        }
    }
}