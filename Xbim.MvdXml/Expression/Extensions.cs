using System;

namespace Xbim.MvdXml.Expression
{
    internal  static class Extensions
    {
        //internal static bool Satisfies(this CachedPropertyValue property, Tokens condition, string value)
        //{
        //    if (property.Prop.StoresNumber)
        //    {
        //        if (condition == Tokens.OP_LIKE)
        //            throw new QueryException(
        //                $"Invalid condition {condition} on numeric property '{property.Prop.Name}'");

        //        var v1 = Convert.ToDouble(property.Value);
        //        var v2 = Convert.ToDouble(value);
        //        return IsSatisfied(v1, condition, v2);
        //    }
        //    else
        //    {
        //        if (!(condition == Tokens.OP_EQ || condition == Tokens.OP_NEQ || condition == Tokens.OP_LIKE))
        //        {
        //            throw new QueryException(
        //                $"Invalid condition {condition} on non-numeric property '{property.Prop.Name}'");
        //        }

        //        var v1 = property.Value.ToString();
        //        return IsSatisfied(v1, condition, value);
        //    }
        //}

        private static bool IsSatisfied(double v1, Tokens condition, double v2)
        {
            switch (condition)
            {
                case Tokens.OP_EQ:
                    return (Math.Abs(v1 - v2) < double.Epsilon);
                case Tokens.OP_GT:
                    return v1 > v2;
                case Tokens.OP_GTE:
                    return v1 >= v2;
                case Tokens.OP_LT:
                    return v1 < v2;
                case Tokens.OP_LTQ:
                    return v1 <= v2;
                case Tokens.OP_NEQ:
                    return Math.Abs(v1 - v2) > double.Epsilon;
                default:
                    throw new ArgumentOutOfRangeException("condition", condition, null);
            }
        }

        private static bool IsSatisfied(string v1, Tokens condition, string v2)
        {
            switch (condition)
            {
                case Tokens.OP_EQ:
                    return v1 == v2;
                case Tokens.OP_NEQ:
                    return v1 != v2;
                case Tokens.OP_LIKE:
                    return v1.IndexOf(v2,StringComparison.CurrentCultureIgnoreCase) >= 0;
                default:
                    throw new ArgumentOutOfRangeException("condition", condition, null);
            }
        }
    }
}
