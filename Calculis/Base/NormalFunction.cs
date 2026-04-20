using System.Collections.Generic;

namespace Calculis.Core
{
    public abstract class NormalFunction : FunctionBase
    {
        protected NormalFunction(IList<IValueItem> args) : base(args) { }
    }
}
