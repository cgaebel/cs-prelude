using Functional;

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionalTests
{
    namespace Data
    {
        [TestClass]
        public class ListTest : Functional.Data.List
        {
            [TestMethod]
            public void intersperse_test()
            {
                CollectionAssert.AreEqual(
                    new int[] { 1, 1, 2, 1, 3, 1, 4 },
                    eval(intersperse(1, new int[] { 1, 2, 3, 4 })));
            }

            [TestMethod]
            public void intercalate_test()
            {
                CollectionAssert.AreEqual(
                    eval("hello, there, world".AsEnumerable()),
                    eval(intercalate(", ", new string[] { "hello", "there", "world" })));
            }
        }
    }
}
