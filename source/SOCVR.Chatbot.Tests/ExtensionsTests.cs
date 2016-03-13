using System;
using NUnit.Framework;
using System.Linq;

namespace SOCVR.Chatbot.Tests
{
    [TestFixture]
    public class ExtensionsTests
    {
        [TestCase(0, 5, 2, 16, "5 hours, 2 minutes, and 16 seconds")]
        [TestCase(1, 1, 1, 1, "1 day, 1 hour, 1 minute, and 1 second")]
        [TestCase(1, 0, 6, 0, "1 day and 6 minutes")]
        [TestCase(0, 0, 1, 0, "1 minute")]
        [TestCase(0, 0, 2, 0, "2 minutes")]
        public void ToUserFriendlyString(int days, int hours, int minutes, int seconds, string expectedResult)
        {
            var input = new TimeSpan(days, hours, minutes, seconds);
            var actual = input.ToUserFriendlyString();
            Assert.AreEqual(expectedResult, actual);
        }

        [TestCase("a, b, and c", "a", "b", "c")]
        [TestCase("apples", "apples")]
        [TestCase("apples and bananas", "apples", "bananas")]
        [TestCase("apples, bananas, cats, and dogs", "apples", "bananas", "cats", "dogs")]
        public void CreateOxforCommaListString(string expectedResult, params object[] inputs)
        {
            var inputList = inputs.Select(x => x.ToString()).ToList();
            var actual = inputList.CreateOxforCommaListString();
            Assert.AreEqual(expectedResult, actual);
        }
    }
}
