using System;
using CP.Core;

namespace CPClient
{
    public class SomeService
    {
        public string GetFullName(string firstName, string secondName)
        {
            return StringUtils.Concat(StringUtils.Substring(firstName, 1), 
                StringUtils.Concat(" ", secondName));
        }
    }
}
