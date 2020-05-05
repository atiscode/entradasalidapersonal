using NonFactors.Mvc.Grid;
using System;

namespace EntradaSalidaRRHH.UI.Models
{
    public static class Tools
    {
        public static IGridColumn<T, TValue> RawNamed<T, TValue>(this IGridColumn<T, TValue> column, String name)
        {
            column.Name = name;
            return column;
        }
    }
}