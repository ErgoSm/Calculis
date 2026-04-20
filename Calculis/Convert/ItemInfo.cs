namespace Calculis.Core.Convert
{
    internal class ItemInfo
    {
        internal IValueItem Item { get; set; }
        internal string Alias { get; set; }
        internal string OriginalExpression { get; set; }
        internal string ReplacedExpression { get; set; }
    }
}
