using NUnit.Framework;
using Sogeti.Game;

namespace Sogeti.Planting.Tests
{
    public class GameCountdownTests
    {
        private const float Round = 180f;

        private static GameCountdown RunningClock()
        {
            GameCountdown clock = new GameCountdown(Round);
            clock.Start();
            return clock;
        }

        #region Construction

        [Test]
        public void New_StartsFullAndStopped()
        {
            GameCountdown clock = new GameCountdown(Round);

            Assert.AreEqual(Round, clock.SecondsRemaining);
            Assert.IsFalse(clock.IsRunning);
            Assert.IsFalse(clock.HasExpired);
        }

        [Test]
        public void New_ZeroDuration_ClampsSoTheRoundIsPlayable()
        {
            GameCountdown clock = new GameCountdown(0f);

            Assert.Greater(clock.DurationSeconds, 0f);
            Assert.AreEqual(clock.DurationSeconds, clock.SecondsRemaining);
        }

        [Test]
        public void New_NegativeDuration_Clamps()
        {
            GameCountdown clock = new GameCountdown(-30f);

            Assert.Greater(clock.DurationSeconds, 0f);
        }

        #endregion

        #region Ticking

        [Test]
        public void Tick_BeforeStart_DoesNothing()
        {
            GameCountdown clock = new GameCountdown(Round);

            clock.Tick(10f);

            Assert.AreEqual(Round, clock.SecondsRemaining);
        }

        [Test]
        public void Tick_WhileRunning_SpendsTheDeltaTime()
        {
            GameCountdown clock = RunningClock();

            clock.Tick(1.5f);

            Assert.AreEqual(Round - 1.5f, clock.SecondsRemaining, 0.0001f);
        }

        [Test]
        public void Tick_ManyTimes_Accumulates()
        {
            GameCountdown clock = RunningClock();

            for (int i = 0; i < 10; i++)
            {
                clock.Tick(0.5f);
            }

            Assert.AreEqual(Round - 5f, clock.SecondsRemaining, 0.0001f);
        }

        [Test]
        public void Tick_AfterStop_DoesNothing()
        {
            GameCountdown clock = RunningClock();
            clock.Tick(10f);

            clock.Stop();
            clock.Tick(10f);

            Assert.AreEqual(Round - 10f, clock.SecondsRemaining, 0.0001f);
        }

        [Test]
        public void Stop_DoesNotExpire()
        {
            GameCountdown clock = RunningClock();

            clock.Stop();

            Assert.IsFalse(clock.HasExpired);
            Assert.IsFalse(clock.IsRunning);
        }

        #endregion

        #region Bad delta time

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Tick_UnusableDeltaTime_LeavesTheClockAlone(float deltaTime)
        {
            GameCountdown clock = RunningClock();

            clock.Tick(deltaTime);

            Assert.AreEqual(Round, clock.SecondsRemaining);
            Assert.IsTrue(clock.IsRunning);
        }

        #endregion

        #region Expiry

        [Test]
        public void Tick_ReachingZero_Expires()
        {
            GameCountdown clock = RunningClock();

            clock.Tick(Round);

            Assert.AreEqual(0f, clock.SecondsRemaining);
            Assert.IsTrue(clock.HasExpired);
            Assert.IsFalse(clock.IsRunning);
        }

        [Test]
        public void Tick_FrameHitchLongerThanTheRound_StopsAtZero()
        {
            GameCountdown clock = RunningClock();

            clock.Tick(Round * 10f);

            Assert.AreEqual(0f, clock.SecondsRemaining);
        }

        [Test]
        public void Tick_ReachingZero_RaisesExpiredOnce()
        {
            GameCountdown clock = RunningClock();
            int raised = 0;
            clock.Expired += () => raised++;

            clock.Tick(Round);
            clock.Tick(Round);
            clock.Tick(Round);

            Assert.AreEqual(1, raised);
        }

        [Test]
        public void Tick_ShortOfZero_DoesNotExpire()
        {
            GameCountdown clock = RunningClock();
            int raised = 0;
            clock.Expired += () => raised++;

            clock.Tick(Round - 0.01f);

            Assert.AreEqual(0, raised);
            Assert.IsFalse(clock.HasExpired);
            Assert.IsTrue(clock.IsRunning);
        }

        #endregion

        #region Restart

        [Test]
        public void Start_AfterExpiry_RefillsAndClearsTheFlag()
        {
            GameCountdown clock = RunningClock();
            clock.Tick(Round);

            clock.Start();

            Assert.AreEqual(Round, clock.SecondsRemaining);
            Assert.IsFalse(clock.HasExpired);
            Assert.IsTrue(clock.IsRunning);
        }

        [Test]
        public void Start_AfterExpiry_CanExpireAgain()
        {
            GameCountdown clock = RunningClock();
            int raised = 0;
            clock.Expired += () => raised++;
            clock.Tick(Round);

            clock.Start();
            clock.Tick(Round);

            Assert.AreEqual(2, raised);
        }

        #endregion
    }
}
