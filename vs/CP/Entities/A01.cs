using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace By.Besmart.PaymentSystemCore.BuisnessEntities.ServiceInfo
{
    public class BsResponseEntity
    {
        public bool IsCanceled { get; set; }

        public BsResponseErrorEntity Error { get; set; }

        public bool HasError
        {
            get { return Error != null; }
        }
    }

    public class BsResponseErrorEntity
    {
        public BsResponseErrorEntity()
        {
            ErrorLines = new List<string>();
        }

        public List<string> ErrorLines { get; set; }

        public ErrorClass ErrorClass { get; set; }

        public string FullText
        {
            get { return GetErrorText(ErrorLines); }
        }

        private static string GetErrorText(List<string> errorLines)
        {
            if (errorLines == null)
            {
                return "";
            }
            var errorTextBuilder = new StringBuilder();
            foreach (var errorLine in errorLines)
            {
                errorTextBuilder.Append(errorLine);
            }
            return errorTextBuilder.ToString();
        }
    }

    public class ServiceInfoBsResponseEntity : BsResponseEntity
    {
        public ServiceInfoBsResponseEntity()
        {
            Parameters = new List<ParameterEntity>();
            ExtraInfo = new List<string>();
        }

        public string ServiceId { get; set; }

        public string ServiceInfoId { get; set; }

        public List<ParameterEntity> Parameters { get; set; }

        public List<string> ExtraInfo { get; set; }

        public AmountEntity Amount { get; set; }

        public bool IsLastServiceInfo { get; set; }
    }

    public class ParameterEntity
    {
        public ParameterEntity()
        {
            Lookup = new LookupEntity();
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public bool IsEditable { get; set; }

        public DataTypeEntity DataType { get; set; }

        // for DataType == DataTypeEntity.DateType only
        public string DataFormat { get; set; }

        public bool IsRequired { get; set; }

        public int MinLength { get; set; }

        public int MaxLength { get; set; }

        public bool IsPassword { get; set; }

        public string Hint { get; set; }

        public string Value { get; set; }

        public bool IsSearchAccountAvailable { get; set; }

        public bool HasLookup { get; set; }

        public LookupEntity Lookup { get; set; }

        public ParameterEntity Clone()
        {
            return (ParameterEntity) this.MemberwiseClone();
        }
    }

    public enum ErrorClass
    {
        Default,
        InvalidSession,
        InvalidAdditionalAuthParams
    }

    public enum DataTypeEntity
    {
        StringType, NumericType, RealType, DateType, BoolType
    }

    public class LookupEntity
    {
        public LookupEntity()
        {
            LookupItems = new List<LookupItemEntity>();
        }

        public bool IsUserInputAllowed { get; set; }

        public List<LookupItemEntity> LookupItems { get; set; }
    }

    public class LookupItemEntity
    {
        public string Value { get; set; }

        public string Name { get; set; }
    }

    public class AmountEntity
    {
        public bool IsVisible { get; set; }

        public bool IsEditable { get; set; }

        public double Min { get; set; }

        public double Max { get; set; }

        public double Precision { get; set; }

        public string Currency { get; set; }

        public string Value { get; set; }
    }
}