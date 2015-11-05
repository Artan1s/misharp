using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Misharp.XCross
{
    public class XCrossElement
    {
        private readonly XElement internalObject;
        private XCrossName xCrossName;

        internal XCrossElement(XElement xElement)
        {
            internalObject = xElement;
            xCrossName = new XCrossName(internalObject.Name);
        }

        public XCrossElement(XCrossElement other)
        {
            internalObject = new XElement(other.internalObject);
        }

        public XCrossElement(XCrossName xCrossName)
        {
            internalObject = new XElement(xCrossName.InternalObject);
        }

        public XCrossName Name
        {
            get { return xCrossName; }
            set
            {
                internalObject.Name = value.InternalObject;
                xCrossName = new XCrossName(internalObject.Name);
            }
        }

        internal XElement InternalObject
        {
            get { return internalObject; }
        }

        public override string ToString()
        {
            return internalObject.ToString();
        }
    }

    public class XCrossAttribute
    {
        private XAttribute internalObject;

        internal XCrossAttribute(XAttribute xAttribute)
        {
            internalObject = xAttribute;
            
        }

        public XCrossAttribute(XCrossAttribute other)
        {
            internalObject = new XAttribute(other.internalObject);
        }



        internal XAttribute InternalObject
        {
            get { return internalObject; }
        }

        public override string ToString()
        {
            return internalObject.ToString();
        }
    }
}
