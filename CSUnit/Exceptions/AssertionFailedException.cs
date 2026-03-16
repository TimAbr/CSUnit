using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSUnit.Exceptions;

public class AssertionFailedException(string message) : Exception(message);
