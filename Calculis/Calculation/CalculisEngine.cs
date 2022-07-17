using Calculis.Core.Convert;
using System.Collections.Generic;
using Calculis.Core.Auxilliary;
using System.Globalization;
using System;

namespace Calculis.Core.Calculation
{
    public sealed class CalculisEngine
    {
        private readonly ItemsManager _itemsManager;
        private readonly TimeProvider _timeProvider;

        ///<summary>
        ///Initializes a new instance of calculation engine that carry out real-time calculations based on items values
        ///</summary>
        ///<param name="items">Collection of value contained objects inherited of IValueItem</param>
        ///<param name="timeProvider">Provider of time for control of iteration in temporal functions;\nBy default is used standard provider based on System.DateTime</param>
        public CalculisEngine(IEnumerable<IValueItem> items, TimeProvider timeProvider = null)
        {
            _timeProvider = timeProvider ?? new DefaultTimeProvider();
            _itemsManager = new ItemsManager(items);
        }

        ///<summary>
        ///Adds new IValueItem object with formulary expression
        ///</summary>
        ///<param name="name">Name of the new object</param>
        ///<param name="expression">Formulary expression that will be used for calculation the value</param>
        ///<param name="culture">Object CultureInfo specifies the culture for Expression</param>
        ///<returns>CalculatingItem object with the result of calculation in Value field</returns>
        ///<exception>ArgumentNullException</exception>
        public CalculatingItem Add(string name, string expression, CultureInfo culture = null)
        {
            return _itemsManager.Create(name, expression, culture ?? CultureInfo.CurrentCulture);
        }

        ///<summary>
        ///Initializes cash of item based on temporal function
        ///</summary>
        ///<param name="name">Name of the initialized object</param>
        ///<param name="cashValues">Content for initialization of cash</param>
        ///<exception>ArgumentException</exception>
        ///<exception>InvalidOperationException</exception>
        public void Initialize(string name, IEnumerable<IValue> cashValues)
        {
            var item = _itemsManager.GetItem(name);

            var calc = item as CalculatingItem;

            if (calc?.IsTemporal != true)
            {
                throw new InvalidOperationException("Non temporal function cannot be initialized!");
            }

            calc.Initialize(cashValues);
        }

        ///<summary>
        ///Returns IValueItem contained in Calculis instance
        ///</summary>
        ///<param name="name">Name of the object</param>
        ///<returns>The object implemented IValueItem</returns>
        ///<exception>ArgumentNullException</exception>
        ///<exception>ArgumentException</exception>
        ///<exception>InvalidOperationException</exception>
        public IValueItem GetItem(string name)
        {
            return _itemsManager.GetItem(name); ;
        }

        ///<summary>
        ///Pluggs-in an assembly containing additional functions 
        ///</summary>
        ///<param name="assemblyName">Name of assembly</param>
        public void Register(string assemblyName)
        {
            FunctionManager.Register(assemblyName);
        }

        ///<summary>
        ///Signals the expiration of a discrete period of time, initiates the updating of the cache of temporal functions
        ///</summary>
        public void Iterate()
        {
            _timeProvider?.Update();
            _itemsManager.Update(_timeProvider.Now);
        }

        ///<summary>
        ///Returns hint about allowed arguments depending on Expression context 
        ///</summary>
        ///<param name="Expression">Typed string of symbols for futher elemnts' expression</param>
        ///<param name="Position">Cursor position in Expression</param>
        public ICollection<string> GetHint(string Expression, int? Position = null)
        {
            if (Expression == null)
            {
                throw new ArgumentNullException(Expression);
            }

            var position = Position ?? Expression.Length - 1;
            return _itemsManager.GetHint(Expression, position);
        }
    }
}
