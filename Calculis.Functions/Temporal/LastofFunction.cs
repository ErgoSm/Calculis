using System;
using System.Collections.Generic;
using Calculis.Core;

namespace Calculis.Functions
{
    [ArgumentsNumber(3)]
    [ArgumentsType(1, typeof(ConstantItem))]
    [ArgumentsType(2, typeof(ConstantItem), 1)]
    internal sealed class LastofFunction : TemporalFunction
    {
        private readonly int _interval;
        private readonly int _number;

        private double _value;
        private DateTime _nextUpdate;


        public LastofFunction(IList<IValueItem> args) : base(args)
        {
            _interval = (int)args[1].Value;
            _number = (int)args[2].Value + 1;

            Function = () =>
            {
                return _isInitialized ? _value : 0;
            };

            InitializeCash(_number);
        }

        protected override void Initialize()
        {
            InitializeCash(_number);
        }

        //_cash[0] - is the oldest value
        public override void Update(DateTime dateTime)
        {
            if (dateTime > _nextUpdate)
            {
                base.Update(dateTime);

                _value = _cash[0].Value;
                _nextUpdate = dateTime.Date.AddSeconds(((int)(dateTime - dateTime.Date).TotalSeconds / _interval + 1) * _interval);    
            }

            _cash[_cash.Length - 1].Value = _args[0].Value;
            _cash[_cash.Length - 1].Timestamp = dateTime;
        }
    }
}
