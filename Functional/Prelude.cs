/*
    Copyright (c) 2011 Clark Gaebel

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Functional
{
    // Easymode usage of Prelude: inherit from it. I _would_ declare it a static class
    // (since that's what it really is), but then you have to prefix each name with
    // "Prelude." and I'm just way too lazy for that shit.
    public class Prelude
    {
        // Returns an empty list.
        public static IEnumerable<T> Null<T>()
        {
            yield break;
        }

        // This version of cons is prefereable, since the second argument is
        // evaluated after the first. This allows for infinite lists and truly
        // lazy computation.
        public static IEnumerable<T> cons<T>(T y, Func<IEnumerable<T>> xs)
        {
            yield return y;
            foreach (var x in xs()) yield return x;
        }

        // However, if the second argument is something that could be computed
        // eagerly (such as Null<T>), then it's fine to use this version of cons.
        // It leads to cleaner invocation.
        public static IEnumerable<T> cons<T>(T y, IEnumerable<T> xs)
        {
            return cons(y, () => xs);
        }

        public static IEnumerable<R> map<T, R>(Func<T, R> f, IEnumerable<T> xs)
        {
            if (empty(xs)) return Null<R>();
            return         cons(f(head(xs)), () => map(f, tail(xs)));
        }

        public static IEnumerable<R> pmap<T, R>(Func<T, R> f, IEnumerable<T> xs)
        {
            return eval(xs.AsParallel().Select(f));
        }

        public static void consume<T>(Action<T> f, IEnumerable<T> xs)
        {
            foreach (var x in xs) f(x);
        }

        // Returns a list of elements for which `f' returns true.
        public static IEnumerable<T> filter<T>(Func<T, bool> f, IEnumerable<T> xs)
        {
            if (empty(xs))   return Null<T>();
            if (f(head(xs))) return cons(head(xs), () => filter(f, tail(xs)));
            else             return filter(f, tail(xs));
        }

        public static Func<I, O> compose<I, M, O>(Func<M, O> outer, Func<I, M> inner)
        {
            return x => outer(inner(x));
        }

        public static Func<I2, I1, R> flip<I1, I2, R>(Func<I1, I2, R> f)
        {
            return (x, y) => f(y, x);
        }

        public static T id<T>(T x) { return x; }

        public static IEnumerable<Tuple<T1, T2>> zip<T1, T2>(IEnumerable<T1> xs, IEnumerable<T2> ys)
        {
            return zipWith(Tuple.Create, xs, ys);
        }

        public static IEnumerable<R> zipWith<T1, T2, R>(Func<T1, T2, R> f, IEnumerable<T1> xs, IEnumerable<T2> ys)
        {
            if (empty(xs) || empty(ys)) return Null<R>();
            else                        return cons(
                                                f(head(xs), head(ys)),
                                                () => zipWith(f, tail(xs), tail(ys)));
        }

        public static Tuple<IEnumerable<T1>, IEnumerable<T2>> unzip<T1, T2>(IEnumerable<Tuple<T1, T2>> xs)
        {
            return Tuple.Create(map(fst, xs), map(snd, xs));
        }

        // Equivalent to the haskell: [start, (start + step) .. end]
        // If end is -1, the list created is infinite.
        public static IEnumerable<Int32> range(Int32 start, Int32 end = -1, Int32 step = 1)
        {
            if (end == -1) return iterate(x => x += step, start);

            Func<Int32, bool> condition = x => step < 0
                                                ? end <= x
                                                : x <= end;

            return takeWhile(condition, iterate(x => x += step, start));
        }

        public static T head<T>(IEnumerable<T> xs)
        {
            Debug.Assert(!empty(xs), "Prelude.head: empty list");
            var e = xs.GetEnumerator(); e.MoveNext();
            return e.Current;
        }

        public static IEnumerable<T> tail<T>(IEnumerable<T> xs)
        {
            Debug.Assert(!empty(xs), "Prelude.tail: empty list");
            var e = xs.GetEnumerator(); e.MoveNext();
            while(e.MoveNext()) yield return e.Current;
        }

        public static IEnumerable<T> init<T>(IEnumerable<T> xs)
        {
            Debug.Assert(!empty(xs), "Prelude.init: empty list");

            var slow = xs.GetEnumerator();
            var fast = xs.GetEnumerator(); fast.MoveNext();

            while (fast.MoveNext())
            {
                slow.MoveNext();
                yield return slow.Current;
            }
        }

        public static T last<T>(IEnumerable<T> xs)
        {
            Debug.Assert(!empty(xs), "Prelude.last: empty list");

            var slow = xs.GetEnumerator();
            var fast = xs.GetEnumerator();

            while(fast.MoveNext()) slow.MoveNext();

            return slow.Current;
        }

        public static O foldl<I, O>(Func<O, I, O> f, O z, IEnumerable<I> xs)
        {
            O res = z;

            foreach (var x in xs)
                z = f(z, x);

            return z;
        }

        public static O foldr<I, O>(Func<I, O, O> f, O z, IEnumerable<I> xs)
        {
            if (empty(xs)) return z;
            return f(head(xs), foldr(f, z, tail(xs)));
        }

        public static T foldl1<T>(Func<T, T, T> f, IEnumerable<T> xs)
        {
            Debug.Assert(!empty(xs), "Prelude.foldl1: empty list");
            return foldl(f, head(xs), tail(xs));
        }

        public static T foldr1<T>(Func<T, T, T> f, IEnumerable<T> xs)
        {
            Debug.Assert(!empty(xs), "Prelude.foldr1: empty list");
            return foldr(f, head(xs), tail(xs));
        }

        public static IEnumerable<T1> scanl<T1, T2>(Func<T1, T2, T1> f, T1 q, IEnumerable<T2> xs)
        {
            T1 res = q;
            yield return res;

            foreach (var x in xs)
            {
                res = f(res, x);
                yield return res;
            }
        }

        public static IEnumerable<T> scanl1<T>(Func<T, T, T> f, IEnumerable<T> xs)
        {
            if (empty(xs)) return Null<T>();
            return scanl(f, head(xs), tail(xs));
        }

        public static IEnumerable<T2> scanr<T1, T2>(Func<T1, T2, T2> f, T2 q0, IEnumerable<T1> xs)
        {
            if (empty(xs)) return cons(q0, Null<T2>());

            var qs = scanr(f, q0, tail(xs));
            return cons(f(head(xs), head(qs)), () => qs);
        }

        public static IEnumerable<T> scanr1<T>(Func<T, T, T> f, IEnumerable<T> xs)
        {
            if (empty(xs)) return Null<T>();
            return scanr(f, last(xs), init(xs));
        }

        public static IEnumerable<T> iterate<T>(Func<T, T> f, T x)
        {
            return cons(x, () => iterate(f, f(x)));
        }

        // repeat x is an infinite list, with x the value of every element
        public static IEnumerable<T> repeat<T>(T x)
        {
            return cons(x, () => repeat(x));
        }

        // Cycle ties a finite list into a circular one, or equivalently,
        // the infinite repetition of the original list. It is the identity
        // on infinite lists.
        public static IEnumerable<T> cycle<T>(IEnumerable<T> xs)
        {
            Debug.Assert(!empty(xs), "Prelude.cycle: empty list");
            return concat(xs, () => cycle(xs));
        }

        public static IEnumerable<T> take<T>(int n, IEnumerable<T> xs)
        {
            return map(snd, takeWhile(nx => fst(nx) < n, zip(range(0), xs)));
        }

        public static IEnumerable<T> drop<T>(int n, IEnumerable<T> xs)
        {
            return map(snd, dropWhile(nx => fst(nx) < n, zip(range(0), xs)));
        }

        public static Tuple<IEnumerable<T>, IEnumerable<T>> splitAt<T>(int n, IEnumerable<T> xs)
        {
            return Tuple.Create(take(n, xs), drop(n, xs));
        }

        public static IEnumerable<T> takeWhile<T>(Func<T, bool> f, IEnumerable<T> xs)
        {
            if (empty(xs)) return Null<T>();
            if (!f(head(xs))) return Null<T>();
            return cons(head(xs), () => takeWhile(f, tail(xs)));
        }

        public static IEnumerable<T> dropWhile<T>(Func<T, bool> f, IEnumerable<T> xs)
        {
            if (empty(xs)) return Null<T>();
            if (!f(head(xs))) return xs;
            return dropWhile(f, tail(xs));
        }

        public static Tuple<IEnumerable<T>, IEnumerable<T>> span<T>(Func<T, bool> f, IEnumerable<T> xs)
        {
            return Tuple.Create(takeWhile(f, xs), dropWhile(f, xs));
        }

        public static Tuple<IEnumerable<T>, IEnumerable<T>> Break<T>(Func<T, bool> f, IEnumerable<T> xs)
        {
            return span(compose(x => !x, f), xs);
        }

        public static IEnumerable<T> reverse<T>(IEnumerable<T> xs)
        {
            return foldl((ys, y) => cons(y, () => ys), Null<T>(), xs);
        }

        public static bool any<T>(Func<T, bool> f, IEnumerable<T> xs)
        {
            if (empty(xs))   return false;
            if (f(head(xs))) return true;
            else             return any(f, tail(xs));
        }

        public static bool all<T>(Func<T, bool> f, IEnumerable<T> xs)
        {
            if (empty(xs))    return true;
            if (!f(head(xs))) return false;
            else              return all(f, tail(xs));
        }

        public static T1 fst<T1, T2>(Tuple<T1, T2> t) { return t.Item1; }
        public static T2 snd<T1, T2>(Tuple<T1, T2> t) { return t.Item2; }

        public static IEnumerable<T> concat<T>(IEnumerable<T> xs, Func<IEnumerable<T>> ys)
        {
            if (empty(xs)) return ys();
            else           return cons(head(xs), () => concat(tail(xs), ys));
        }

        public static IEnumerable<T> concat<T>(IEnumerable<IEnumerable<T>> xss)
        {
            if (empty(xss)) return Null<T>();
            else            return concat(head(xss), () => concat(tail(xss)));
        }

        public static IEnumerable<T2> concatMap<T1, T2>(Func<T1, IEnumerable<T2>> f, IEnumerable<T1> xs)
        {
            return concat(map(f, xs));
        }

        // TODO: words, unwords

        public static IEnumerable<String> lines(String s)
        {
            return s.Split('\n');
        }

        public static IEnumerable<Char> unlines(IEnumerable<String> xs)
        {
            return concatMap(x => x + "\n", xs);
        }

        public static bool empty<T>(IEnumerable<T> xs)
        {
            return !xs.GetEnumerator().MoveNext();
        }

        public static int length<T>(IEnumerable<T> xs)
        {
            return foldl((x, _) => x + 1, 0, xs);
        }

        // Uses a disposable resource with a function `user'.
        public static T2 use<T1, T2>(T1 used, Func<T1, T2> user) where T1:IDisposable
        {
            using (used) return user(used);
        }

        public static bool elem<T>(IEquatable<T> x, IEnumerable<IEquatable<T>> xs)
        {
            return any(y => y == x, xs);
        }

        public static bool notElem<T>(IEquatable<T> x, IEnumerable<IEquatable<T>> xs)
        {
            return all(y => y != x, xs);
        }

        public static IEnumerable<IComparable<T>> sort<T>(IEnumerable<IComparable<T>> xs)
        {
            var e = eval(xs);
            Array.Sort(e);
            return e;
        }

        // Returns the lazy list as an eager list.
        public static T[] eval<T>(IEnumerable<T> xs)
        {
            return xs.ToArray();
        }

        public static Tuple<T1[], T2[]> eval<T1, T2>(Tuple<IEnumerable<T1>, IEnumerable<T2>> xs)
        {
            return Tuple.Create(eval(fst(xs)), eval(snd(xs)));
        }

        public static T eval<T>(Func<T> f)
        {
            return f();
        }
        
        // PRELUDE IO

        public static IEnumerable<char> readFileStream(StreamReader s)
        {
            using (s)
                while (!s.EndOfStream)
                    yield return (char)s.Read();
        }

        // lazy file input =)
        public static IEnumerable<char> readFile(string path)
        {
            return readFileStream(File.OpenText(path));
        }
    }
}
