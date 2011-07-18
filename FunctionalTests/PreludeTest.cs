using Functional;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace FunctionalTests
{
    [TestClass]
    public class PreludeTest : Prelude
    {
        [TestMethod]
        public void Break_test()
        {
            var expected = Tuple.Create(new int[] { 1, 2 }, new int[] { 3, 4, 5 });
            var actual = eval(Break(x => x == 3, new int[] { 1, 2, 3, 4, 5 }));

            CollectionAssert.AreEqual(expected.Item1, actual.Item1);
            CollectionAssert.AreEqual(expected.Item2, actual.Item2);
        }

        [TestMethod]
        public void all_test()
        {
            int[] nums = { 1, 2, 7, 4, 8, 2010, 4 };

            Assert.IsTrue(all(x => x < 2011, nums));
            Assert.IsFalse(all(x => x < 2010, nums));
        }

        [TestMethod]
        public void any_test()
        {
            int[] nums = { 1, 2, 7, 4, 8, 2010, 4 };

            Assert.IsTrue(any(x => x == 4, nums));
            Assert.IsFalse(any(x => x < 1, nums));
        }

        [TestMethod]
        public void compose_test()
        {
            Assert.AreEqual(25, compose<Int32, Int32, Int32>(x => x + 1, y => y * 2)(12));
        }

        [TestMethod]
        public void concat_test()
        {
            CollectionAssert.AreEqual(
                new int[] { 1, 2, 3, 4, 5, 6 },
                eval(concat(new int[] { 1, 2, 3 }, () => new int[] { 4, 5, 6 })));
        }

        [TestMethod]
        public void concat2_test()
        {
            CollectionAssert.AreEqual(
                new int[] { 1, 2, 3, 4, 5, 6 },
                eval(concat(new int[][] { new int[] { 1, 2 },
                                          new int[] { 3, 4 },
                                          new int[] { 5, 6 } })));
        }

        [TestMethod]
        public void cons_test()
        {
            CollectionAssert.AreEqual(
                new int[] { 1, 2, 3 },
                eval(cons(1, () => new int[] { 2, 3 })));
        }

        [TestMethod]
        public void cycle_test()
        {
            CollectionAssert.AreEqual(
                new int[] { 1, 2, 1, 2, 1, 2 },
                eval(take(6, cycle(new int[] { 1, 2 }))));
        }
        
        [TestMethod]
        public void drop_test()
        {
            CollectionAssert.AreEqual(
                new int[] { 3, 2, 1 },
                eval(drop(3, new int[] { 6, 5, 4, 3, 2, 1 })));
        }

        [TestMethod]
        public void filter_test()
        {
            CollectionAssert.AreEqual(
                new int[] { 1, 2, 3 },
                eval(filter(x => x <= 3, new int[] { 1, 5, 2, 4, 3, 8, 8, 999 })));
        }

        [TestMethod]
        public void dropWhile_test()
        {
            CollectionAssert.AreEqual(
                new int[] { 3, 2, 1 },
                eval(dropWhile(x => x > 3, new int[] { 6, 5, 4, 3, 2, 1 })));
        }

        [TestMethod]
        public void flip_test()
        {
            Func<int, int, int> left = (x, _) => x;

            Assert.AreEqual(3, flip(left)(2, 3));
        }

        [TestMethod]
        public void foldl_test()
        {
            Assert.AreEqual("abcd",
                foldl((x, y) => x + y, "a",new string[] { "b", "c", "d" }));
        }

        [TestMethod]
        public void foldl1_test()
        {
            Assert.AreEqual(10,
                foldl1((x, y) => x + y, new int[] { 1, 2, 3, 4 }));
        }

        [TestMethod]
        public void foldr_test()
        {
            Assert.AreEqual("bcda",
                foldr((x, y) => x + y, "a", new string[] { "b", "c", "d" }));
        }

        [TestMethod]
        public void foldr1_test()
        {
            Assert.AreEqual(10,
                foldr1((x, y) => x + y, new int[] { 1, 2, 3, 4 }));
        }

        [TestMethod]
        public void fst_test()
        {
            Assert.AreEqual(1, fst(Tuple.Create(1, 2)));
        }

        [TestMethod]
        public void head_test()
        {
            Assert.AreEqual(1, head(new int[] { 1, 2, 3 }));

            // infinite head.
            Assert.AreEqual(2, head(repeat(2)));
        }

        [TestMethod]
        public void init_test()
        {
            CollectionAssert.AreEqual(
                new int[] { 1, 2, 3 },
                eval(init(new int[] { 1, 2, 3, 4 })));
        }

        [TestMethod]
        public void iterate_test()
        {
            CollectionAssert.AreEqual(
                new int[] { 1, 2, 3, 4, 5 },
                eval(take(5, iterate(x => x + 1, 1))));
        }

        [TestMethod]
        public void last_test()
        {
            int[] xs = { 1, 2, 3 };

            Assert.AreEqual(3, last(xs));
        }

        [TestMethod]
        public void length_test()
        {
            Assert.AreEqual(3, length(new int[] { 1, 2, 3 }));
        }

        [TestMethod]
        public void map_test()
        {
            CollectionAssert.AreEqual(
                new int[] { 2, 4, 6, 8 },
                eval(map(x => x * 2, new int[] { 1, 2, 3, 4 })));
        }

        private static bool prime(int x)
        {
            foreach (var n in range(2, x-1))
                if (x % n == 0)
                    return false;
            return true;
        }

        [TestMethod]
        public void pmap_test()
        {
            Assert.AreEqual(
                4,
                length(filter(id, pmap(prime, range(2, 10)))));
        }

        [TestMethod]
        public void reverse_test()
        {
            CollectionAssert.AreEqual(
                new int[] { 3, 2, 1 },
                eval(reverse(new int[] { 1, 2, 3 })));
        }

        [TestMethod]
        public void repeat_test()
        {
            CollectionAssert.AreEqual(
                new int[] { 2, 2, 2, 2 },
                eval(take(4, repeat(2))));
        }

        [TestMethod]
        public void scanl_test()
        {
            CollectionAssert.AreEqual(
                new int[] { 3, 7, 12, 18 },
                eval(scanl((x, y) => x + y, 3, new int[] { 4, 5, 6 })));
        }

        [TestMethod]
        public void scanr_test()
        {
            CollectionAssert.AreEqual(
                new int[] { 18, 14, 9, 3 },
                eval(scanr((x, y) => x + y,
                           3,
                           new int[] { 4, 5, 6 })));
        }

        [TestMethod]
        public void snd_test()
        {
            Assert.AreEqual(2, snd(Tuple.Create(1, 2)));
        }

        [TestMethod]
        public void span_test()
        {
            var expected = Tuple.Create(new int[] { 1, 2 }, new int[] { 3, 4, 5 });
            var actual   = eval(span(x => x < 3, new int[] { 1, 2, 3, 4, 5 }));

            CollectionAssert.AreEqual(expected.Item1, actual.Item1);
            CollectionAssert.AreEqual(expected.Item2, actual.Item2);
        }

        [TestMethod]
        public void splitAt_test()
        {
            var expected = Tuple.Create(new int[] { 1, 2, 3 }, new int[] { 4, 5, 6 });
            var actual   = eval(splitAt(3, new int[] { 1, 2, 3, 4, 5, 6 }));

            CollectionAssert.AreEqual(expected.Item1, actual.Item1);
            CollectionAssert.AreEqual(expected.Item2, actual.Item2);
        }

        [TestMethod]
        public void tail_test()
        {
            CollectionAssert.AreEqual(
                new int[] { 2, 3 },
                eval(tail(new int[] { 1, 2, 3 })));
        }

        [TestMethod]
        public void take_test()
        {
            CollectionAssert.AreEqual(
                new int[] { 1, 2, 3 },
                eval(take(3, new int[] { 1, 2, 3, 4, 5 })));
        }

        [TestMethod]
        public void takeWhile_test()
        {
            CollectionAssert.AreEqual(
                new int[] { 1, 2, 3 },
                eval(takeWhile(x => x <= 3, new int[] { 1, 2, 3, 4, 5, 6 })));
        }

        [TestMethod]
        public void unzip_test()
        {
            var lst_of_tup = new Tuple<int, int>[] { Tuple.Create(1, 2), Tuple.Create(3, 4) };
            var expected = Tuple.Create(new int[] { 1, 3 }, new int[] { 2, 4 });
            var actual   = eval(unzip(lst_of_tup));

            CollectionAssert.AreEqual(expected.Item1, actual.Item1);
            CollectionAssert.AreEqual(expected.Item2, actual.Item2);
        }

        [TestMethod]
        public void zip_test()
        {
            CollectionAssert.AreEqual(
                new Tuple<int, int>[] { Tuple.Create(1, 3), Tuple.Create(2, 4) },
                eval(zip(new int[] { 1, 2 }, new int[] { 3, 4 })));
        }

        [TestMethod]
        public void zipWith_test()
        {
            CollectionAssert.AreEqual(
                new int[] { 5, 7, 9 },
                eval(zipWith((x, y) => x + y,
                             new int[] { 1, 2, 3 },
                             new int[] { 4, 5, 6 })));
        }
    }
}
