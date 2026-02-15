using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSUnit.Attributes
{

    [AttributeUsage(AttributeTargets.Method)]
    public class TestAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class BeforeEachAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class AfterEachAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class BeforeAllAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class AfterAllAttribute : Attribute { }


}
