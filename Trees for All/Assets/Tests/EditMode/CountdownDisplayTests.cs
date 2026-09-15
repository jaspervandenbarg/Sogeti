using NUnit.Framework;
using Sogeti.Game;

namespace Sogeti.Planting.Tests
{
    public class CountdownDisplayTests
    {
        [TestCase(180f, "3:00")]
        [TestCase(120f, "2:00")]
        [TestCase(60f, "1:00")]
        [TestCase(59f, "0:59")]
        [TestCase(10f, "0:10")]
        [TestCase(9f, "0:09")]
        [TestCase(0f, "0:00")]
        public void Format_WholeSeconds_ReadsAsMinutesAndSeconds(float seconds, string expected)
        {
            Assert.AreEqual(expected, CountdownDisplay.Format(seconds));
        }

        [Test]
        public void Format_PadsSecondsToTwoDigits()
        {
            Assert.AreEqual("1:05", CountdownDisplay.Format(65f));
        }

        [Test]
        public void Format_DoesNotPadMinutes()
        {
            Assert.AreEqual("2:30", CountdownDisplay.Format(150f));
        }

        #region Rounding

        [TestCase(0.01f)]
        [TestCase(0.5f)]
        [TestCase(0.99f)]
        public void Format_APartialSecondLeft_HoldsAtOne(float seconds)
        {
            // A clock reading 0:00 while the player can still plant looks broken.
            Assert.AreEqual("0:01", CountdownDisplay.Format(seconds));
        }

        [Test]
        public void Format_JustUnderAMinute_ShowsFiftyNine()
        {
            Assert.AreEqual("0:59", CountdownDisplay.Format(58.2f));
        }

        [Test]
        public void Format_JustUnderTheFullRound_ShowsTheFullRound()
        {
            Assert.AreEqual("3:00", CountdownDisplay.Format(179.5f));
        }

        #endregion

        #region Bad input

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(-180f)]
        [TestCase(float.NaN)]
        [TestCase(float.NegativeInfinity)]
        public void Format_NothingLeft_ReadsZero(float seconds)
        {
            Assert.AreEqual("0:00", CountdownDisplay.Format(seconds));
        }

        [Test]
        public void Format_Infinity_DoesNotOverflowIntoANegativeClock()
        {
            string text = CountdownDisplay.Format(float.PositiveInfinity);

            Assert.IsFalse(text.Contains("-"));
        }

        #endregion
    }
}
