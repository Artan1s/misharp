using System.Xml.Linq;

namespace Misharp.XCross
{
    public class XCrossNamespace
    {
        private readonly XNamespace internalObject;

        private static readonly XCrossNamespace noneNamespace = new XCrossNamespace(XNamespace.None);
        private static readonly XCrossNamespace xmlNamespace = new XCrossNamespace(XNamespace.Xml);
        private static readonly XCrossNamespace xmlnsNamespace = new XCrossNamespace(XNamespace.Xmlns);

        internal XCrossNamespace(XNamespace xNamespace)
        {
            internalObject = xNamespace;
        }

        public static XCrossNamespace Get(string namespaceName)
        {
            return new XCrossNamespace(XNamespace.Get(namespaceName));
        }

        public XCrossName GetName(string localName)
        {
            return new XCrossName(internalObject.GetName(localName));
        }

        public string NamespaceName
        {
            get { return internalObject.NamespaceName; }
        }

        public XCrossNamespace None
        {
            get { return noneNamespace; }
        }

        public XCrossNamespace Xml
        {
            get { return xmlNamespace; }
        }

        public XCrossNamespace Xmlns
        {
            get { return xmlnsNamespace; }
        }

        public override string ToString()
        {
            return internalObject.ToString();
        }
    }
}