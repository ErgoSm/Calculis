using System;
using System.Collections.Generic;

namespace Calculis.Core
{
    public abstract class FunctionBase
    {
        protected readonly IList<IValueItem> _args;

        public FunctionBase(IList<IValueItem> args)
        {
            _args = args;
            Name = GetType().Name.Replace("Function", "").ToUpper();

            ValidateArgs();
        }

        public virtual void ValidateArgs()
        {
            object[] numberAttributes = GetType().GetCustomAttributes(typeof(ArgumentsNumberAttribute), true);
            object[] typeAttributes = GetType().GetCustomAttributes(typeof(ArgumentsTypeAttribute), false);

            if(numberAttributes.Length > 0)
            {
                var numberAttribute = numberAttributes[0] as ArgumentsNumberAttribute;
                if (numberAttribute.Number > 0 && numberAttribute.Number != _args.Count)
                {
                    throw new ArgumentException("Number of arguments is not correspond to specification!");
                }
                else if(_args.Count < numberAttribute.MinNumber || _args.Count > numberAttribute.MaxNumber)
                {
                    throw new ArgumentException("Number of arguments is out of range!");
                }
                
                if (numberAttribute.Number > 0 && typeAttributes.Length > numberAttribute.Number ||
                    typeAttributes.Length > numberAttribute.MaxNumber)
                    throw new ArithmeticException("Number of attributes exeeds the specified number of arguments!");
            }

            foreach (var attribute in typeAttributes)
            {
                if((attribute as ArgumentsTypeAttribute).ArgNumber >= _args.Count)
                    throw new ArithmeticException("Number of attributes exeeds the number of arguments!");

                if (attribute is ArgumentsTypeAttribute attr2)
                    if (!Equals(_args[attr2.ArgNumber].GetType(), attr2.Type))
                        throw new ArgumentException("Arguments does not correspond to specified type!");
            }
                
        }

        public string Name { get; set; }
        public string Description { get; }
        public virtual Func<double> Function { get; protected set; }

        public virtual void Update(DateTime dateTime)
        {
            return;
        }
    }
}
