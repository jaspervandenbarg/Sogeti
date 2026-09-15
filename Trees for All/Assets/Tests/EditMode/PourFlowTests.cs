using NUnit.Framework;
using Sogeti.Watering;

namespace Sogeti.Planting.Tests
{
    public class PourFlowTests
    {
        private const float Start = PourFlow.DefaultStartDegrees;
        private const float Full = PourFlow.DefaultFullDegrees;

        private static float Flow(float tiltDegrees) => PourFlow.For(tiltDegrees, Start, Full);

        #region Ramp

        [Test]
        public void For_UprightCan_PoursNothing()
        {
            Assert.AreEqual(0f, Flow(0f));
        }

        [Test]
        public void For_BelowStartAngle_PoursNothing()
        {
            // A resting hand is never level. The can must not leak while the player walks.
            Assert.AreEqual(0f, Flow(Start - 1f));
        }

        [Test]
        public void For_ExactlyStartAngle_PoursNothing()
        {
            Assert.AreEqual(0f, Flow(Start));
        }

        [Test]
        public void For_HalfwayUpTheRamp_PoursHalf()
        {
            float halfway = Start + (Full - Start) * 0.5f;
            Assert.AreEqual(0.5f, Flow(halfway), 0.0001f);
        }

        [Test]
        public void For_ExactlyFullAngle_PoursFully()
        {
            Assert.AreEqual(1f, Flow(Full), 0.0001f);
        }

        [Test]
        public void For_PastFullAngle_StaysAtFull()
        {
            Assert.AreEqual(1f, Flow(Full + 20f), 0.0001f);
        }

        [Test]
        public void For_UpsideDownCan_PoursFully()
        {
            Assert.AreEqual(1f, Flow(180f), 0.0001f);
        }

        #endregion

        #region Bad input

        [Test]
        public void For_NaNTilt_PoursNothing()
        {
            Assert.AreEqual(0f, Flow(float.NaN));
        }

        [Test]
        public void For_NaNAngles_PoursNothing()
        {
            Assert.AreEqual(0f, PourFlow.For(90f, float.NaN, Full));
            Assert.AreEqual(0f, PourFlow.For(90f, Start, float.NaN));
        }

        [Test]
        public void For_NegativeTilt_PoursNothing()
        {
            Assert.AreEqual(0f, Flow(-30f));
        }

        [Test]
        public void For_FullAngleBelowStartAngle_PoursFullyPastTheStart()
        {
            // A mistyped inspector leaves no ramp. Pouring beats a dead tool.
            Assert.AreEqual(1f, PourFlow.For(60f, 50f, 20f));
            Assert.AreEqual(0f, PourFlow.For(40f, 50f, 20f));
        }

        #endregion
    }
}
