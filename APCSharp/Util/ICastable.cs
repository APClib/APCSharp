using System;
using System.Collections.Generic;
using System.Text;

namespace APCSharp.Util
{
    /// <summary>
    /// A good idea is to implement this as an implicit operator using
    /// public static implicit operator TDest(ICastable<TDest/> p) => p.Cast();
    /// </summary>
    /// <typeparam name="TDest"></typeparam>
    internal interface ICastable<out TDest>
    where TDest : new()
    {
        public TDest Cast();
    }
}
