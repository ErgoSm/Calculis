using System;
using System.Collections.Generic;
using System.Linq;

namespace Calculis.Core
{
    public class CashItem : IValue
    {
        public double Value { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }

    public abstract class TemporalFunction : FunctionBase
    {
        protected bool _isInitialized = false;
        protected CashItem[] _cash { get; private set; }
        protected DateTimeOffset Timestamp { get; private set; }
        protected double _previousValue { get; private set; }

        protected TemporalFunction(IList<IValueItem> args) : base(args)
        {
            Initialize();
        }

        protected virtual void Initialize()
        {
            InitializeCash((int)_args[1].Value);
        }

        protected void InitializeCash(int cashSize)
        {
            _cash = new CashItem[cashSize];

            for (var i = 0; i < cashSize; i++)
                _cash[i] = new CashItem();
        }

        public void Initialize(IEnumerable<IValue> values)
        {
            var valuesList = values.ToList();
            if (valuesList.Count != _cash.Length)
                throw new ArgumentException("Number of values don't correspond to cash size!");

            bool hasTimestamp = false;
            if (valuesList[0] is CashItem)
                hasTimestamp = true;

            for(int i = 0; i < valuesList.Count; i++)
            {
                _cash[i].Value = valuesList[i].Value;

                if(hasTimestamp)
                    _cash[i].Timestamp = ((CashItem)valuesList[i]).Timestamp;
            }
        }

        //_cash[0] - is the oldest value
        public override void Update(DateTime dateTime)
        {
            for (int i = 1; i < _cash.Length; i++)
            {
                _cash[i - 1].Value = _cash[i].Value;
                _cash[i - 1].Timestamp = _cash[i].Timestamp;
            }

            _previousValue = this.Function();
            _cash[_cash.Length - 1].Value = _args[0].Value;
            _cash[_cash.Length - 1].Timestamp = dateTime;

            Timestamp = dateTime;
            _isInitialized = true;
        }
    }
}
