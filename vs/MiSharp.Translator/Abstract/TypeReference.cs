using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiSharp.Translator.Abstract
{
    public class TypeReference
    {
        public bool IsPredefined { get; set; }

        public string Text { get; set; }

        public bool IsGeneric { get; set; }

        public bool IsReferenceType { get; set; }

        protected bool Equals(TypeReference other)
        {
            return IsPredefined.Equals(other.IsPredefined) && string.Equals(Text, other.Text) && IsGeneric.Equals(other.IsGeneric) && IsReferenceType.Equals(other.IsReferenceType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TypeReference)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = IsPredefined.GetHashCode();
                hashCode = (hashCode * 397) ^ (Text != null ? Text.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsGeneric.GetHashCode();
                hashCode = (hashCode * 397) ^ IsReferenceType.GetHashCode();
                return hashCode;
            }
        }
    }

    

    public class Property
    {
        public TypeReference TypeReference { get; set; }

        public string Text { get; set; }
    }

    public class Method
    {
        public List<TypeReference> TypeReferences { get; set; }

        public string Text { get; set; }
    }
}
