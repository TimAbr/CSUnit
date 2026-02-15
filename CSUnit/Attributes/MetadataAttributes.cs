using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSUnit.Attributes
{

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class DisplayNameAttribute : Attribute
    {
        public string Name { get; }
        public DisplayNameAttribute(string name) => Name = name;
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class DisabledAttribute : Attribute
    {
        public string Reason { get; }
        public DisabledAttribute(string reason = "None") => Reason = reason;
    }
}
