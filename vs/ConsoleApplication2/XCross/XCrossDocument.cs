using System.Xml.Linq;

namespace Misharp.XCross
{
    public class XCrossDocument
    {
        private readonly XDocument internalObject;
        private readonly XCrossElement root;

        internal XCrossDocument(XDocument xDocument)
        {
            internalObject = xDocument;
            root = new XCrossElement(internalObject.Root);
        }

        public static XCrossDocument Parse(string text)
        {
            return new XCrossDocument(XDocument.Parse(text));
        }

        public XCrossElement Root
        {
            get { return root; }
        }

        internal XDocument InternalObject
        {
            get { return internalObject; }
        }

        public override string ToString()
        {
            return internalObject.ToString();
        }
    }
}