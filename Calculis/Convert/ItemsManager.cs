using Calculis.Core.Auxilliary;
using Calculis.Core.Calculation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;


namespace Calculis.Core.Convert
{
    internal sealed class ItemsManager
    {
        internal IDictionary<string, string> ExpressionAlias { get; private set; } = new Dictionary<string, string>();
        internal event EventHandler<UpdateArgs> Updating;

        private IDictionary<string, IValueItem> _items;
        private IDictionary<string, string> _itemsNames;
        private IDictionary<string, ItemInfo> _aliasFunctions = new Dictionary<string, ItemInfo>();

        private int _count;
        private int _fncCount;

        private CultureInfo _culture;
        private ExpressionParser _expressionParser;

        internal ItemsManager(IEnumerable<IValueItem> items)
        {
            _itemsNames = items.ToDictionary(x => x.Name, x => $"item{++_count}");
            _items = items.ToDictionary(x => _itemsNames[x.Name]);
        }

        internal CalculatingItem Create(string name, string expression, CultureInfo culture)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (_itemsNames.ContainsKey(name)) throw new ArgumentException($"The name {name} has already used!");

            _culture = culture;
            _expressionParser = new ExpressionParser(this, culture);

            _itemsNames.Add(name, $"item{++_count}");

            foreach (var itemName in _itemsNames.OrderByDescending(x => x.Key.Length))
                expression = expression.Replace(itemName.Key, itemName.Value);

            var expressions = _expressionParser.Parse(expression);

            FunctionDescription functionDescription;
            IValueItem item = null;
            foreach (var functionExpression in expressions)
            {
                functionDescription = new FunctionDescription(functionExpression);
                item = _aliasFunctions[ExpressionAlias[functionExpression]].Item = CreateItem(FunctionManager.Create(functionDescription.Name, ExtractArgs(functionExpression)));
            }

            _items.Add(_itemsNames[name], item);

            return (CalculatingItem)item;
        }

        internal IValueItem GetItem(string name)
        {
            return _items.TryGetValue(_itemsNames[name], out var item) ? item : throw new KeyNotFoundException($"Item {name} does not exist!");
        }

        internal void Update(DateTime timestamp)
        {
            Updating?.Invoke(this, new UpdateArgs { Timestamp = timestamp });
        }

        private CalculatingItem CreateItem(FunctionBase function)
        {
            var item = new CalculatingItem(function);
            Updating += item.Update;

            return item;
        }

        private IList<IValueItem> ExtractArgs(string expression)
        {
            var functionDescription = new FunctionDescription(expression);
            var args = new List<IValueItem>();
            foreach (var argString in functionDescription.Args)
            {
                var arg = GetArg(argString, _items.ContainsKey, (key) => _items[key]) ??
                          GetArg(argString, _aliasFunctions.ContainsKey, (key) => _aliasFunctions[key].Item) ??
                          GetArg(argString, IsDouble, (expr) => new ConstantItem(double.Parse(expr, NumberStyles.Float, _culture.NumberFormat)));

                args.Add(arg ?? throw new ArgumentOutOfRangeException(argString));
            }

            return args;
        }

        private bool IsDouble(string expression)
        {
            return double.TryParse(expression, NumberStyles.Float, _culture.NumberFormat, out double result);
        }

        private IValueItem GetArg(string argString, Func<string, bool> checkingFunction, Func<string, IValueItem> creationFunction)
        {
            IValueItem newItem = null;

            var isDenominator = false;
            var isNegative = false;

            if (argString.Substring(0, 1) == "/")
            {
                argString = argString.Replace("/", "");
                isDenominator = true;
            }
            if (argString.Substring(0, 1) == "-")
            {
                argString = argString.Replace("-", "");
                isNegative = true;
            }
            if (checkingFunction(argString))
            {
                newItem = creationFunction(argString);
                if (isNegative) newItem = new CalculatingItem(FunctionManager.Create("MUL", new List<IValueItem> { new ConstantItem(-1), newItem }));
                if (isDenominator) newItem = new CalculatingItem(FunctionManager.Create("DIV", new List<IValueItem> { new ConstantItem(1), newItem }));
            }

            return newItem;
        }

        internal string GetExpression(string alias)
        {
            return _aliasFunctions[alias].ReplacedExpression;
        }

        internal string AddAlias(string expression, string original)
        {
            string alias = $"__fnc{++_fncCount}";
            ExpressionAlias.Add(expression, alias);
            _aliasFunctions.Add(alias, new ItemInfo { Alias = alias, OriginalExpression = original, ReplacedExpression = expression });

            return alias;
        }

        internal ICollection<string> GetHint(string expression, int position)
        {
            var hintCollection = new List<string>();
            
            var typingStart = -1;
            var typingEnd = expression.Length;

            for (int i = position; i >= 0; i--)
                if ("(;+-/* ".Contains(expression[i]))
                {
                    typingStart = i;
                    break;
                }
                    

            for (int i = position; i < expression.Length; i++)
                if (");+-/* ".Contains(expression[i]))
                {
                    typingEnd = i;
                    break;
                }
              

            var typingElement = expression.Substring(typingStart + 1, typingEnd - typingStart - 1);

            //elements
            foreach (var item in _itemsNames.Keys)
                if(item.Contains(typingElement))
                    hintCollection.Add(item);

            //functions
            foreach (var funcName in FunctionManager.Functions.Keys)
                if (funcName.Contains(typingElement))
                    hintCollection.Add(funcName);

            return hintCollection;
        } 
    }

    struct FunctionDescription
    {
        internal string Name;
        internal string[] Args;

        internal FunctionDescription(string expression)
        {
            var bracketIndex = expression.IndexOf('(');
            Name = expression.Substring(0, bracketIndex);
            Args = expression.Substring(bracketIndex).Trim('(', ')').Split(';');
        }
    }
}
