using NUnit.Framework;
using Sogeti.Watering;
using UnityEngine;

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

        #region Spout direction

        // The body of the can sits at height 0. The spout height says which way the player tilts it.
        private const float BodyHeight = 0f;
        private const float SpoutBelowBody = -0.3f;
        private const float SpoutAboveBody = 0.3f;

        // The can tips over its X axis. A positive angle dips the spout, a negative one raises it.
        private static Vector3 TiltedUpAxis(float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            return new Vector3(0f, Mathf.Cos(radians), Mathf.Sin(radians));
        }

        [Test]
        public void SpoutTiltDegrees_SpoutBelowBody_ReportsTheTilt()
        {
            float tilt = PourFlow.SpoutTiltDegrees(TiltedUpAxis(60f), SpoutBelowBody, BodyHeight);

            Assert.AreEqual(60f, tilt, 0.001f);
        }

        [Test]
        public void SpoutTiltDegrees_SpoutAboveBody_ReportsNoTilt()
        {
            // The player tilts the can up. A watering can holds its water in that pose.
            float tilt = PourFlow.SpoutTiltDegrees(TiltedUpAxis(-60f), SpoutAboveBody, BodyHeight);

            Assert.AreEqual(0f, tilt);
        }

        [Test]
        public void SpoutTiltDegrees_SpoutAboveBody_PoursNothing()
        {
            float tilt = PourFlow.SpoutTiltDegrees(TiltedUpAxis(-90f), SpoutAboveBody, BodyHeight);

            Assert.AreEqual(0f, PourFlow.For(tilt, Start, Full));
        }

        [Test]
        public void SpoutTiltDegrees_SpoutLevelWithBody_ReportsNoTilt()
        {
            Assert.AreEqual(0f, PourFlow.SpoutTiltDegrees(TiltedUpAxis(90f), BodyHeight, BodyHeight));
        }

        [Test]
        public void SpoutTiltDegrees_UprightCan_ReportsNoTilt()
        {
            Assert.AreEqual(0f, PourFlow.SpoutTiltDegrees(Vector3.up, SpoutAboveBody, BodyHeight));
        }

        [Test]
        public void SpoutTiltDegrees_UpsideDownCan_ReportsFullTilt()
        {
            float tilt = PourFlow.SpoutTiltDegrees(Vector3.down, SpoutBelowBody, BodyHeight);

            Assert.AreEqual(180f, tilt, 0.001f);
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
