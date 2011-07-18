using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Functional
{
    namespace Data
    {
        public class List : Prelude
        {
            public static IEnumerable<T> intersperse<T>(T sep, IEnumerable<T> xs)
            {
                if (empty(xs)) return Null<T>();
                if (empty(tail(xs))) return xs;

                return cons(head(xs),
                 () => cons(sep,
                      () => intersperse(sep, tail(xs))));
            }

            // 'intercalate' @xs @xss is equivalent to @concat(intersperse(xs, xss))@.
            // It inserts the list @xs@ in between the lists in @xss@ and concatenates the
            // result.
            public static IEnumerable<T> intercalate<T>(IEnumerable<T> xs, IEnumerable<IEnumerable<T>> xss)
            {
                return concat(intersperse(xs, xss));
            }
        }
    }
}
