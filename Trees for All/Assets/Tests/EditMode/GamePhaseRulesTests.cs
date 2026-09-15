using NUnit.Framework;
using Sogeti.Game;

namespace Sogeti.Planting.Tests
{
    public class GamePhaseRulesTests
    {
        #region Who acts

        [Test]
        public void PlayerActsIn_PlayingOnly()
        {
            Assert.IsFalse(GamePhaseRules.PlayerActsIn(GamePhase.Ready));
            Assert.IsTrue(GamePhaseRules.PlayerActsIn(GamePhase.Playing));
            Assert.IsFalse(GamePhaseRules.PlayerActsIn(GamePhase.Ended));
        }

        [TestCase(GamePhase.Ready)]
        [TestCase(GamePhase.Playing)]
        [TestCase(GamePhase.Ended)]
        public void PanelShowsIn_IsTheOppositeOfPlayerActsIn(GamePhase phase)
        {
            Assert.AreNotEqual(GamePhaseRules.PlayerActsIn(phase), GamePhaseRules.PanelShowsIn(phase));
        }

        [Test]
        public void ReadyIsTheDefaultPhase()
        {
            // A fresh GameSession must wait for the player, not start a round.
            Assert.AreEqual(GamePhase.Ready, default(GamePhase));
        }

        #endregion

        #region Legal transitions

        [TestCase(GamePhase.Ready, GamePhase.Playing)]
        [TestCase(GamePhase.Playing, GamePhase.Ended)]
        [TestCase(GamePhase.Ended, GamePhase.Ready)]
        public void CanTransition_AllowsTheRoundLoop(GamePhase from, GamePhase to)
        {
            Assert.IsTrue(GamePhaseRules.CanTransition(from, to));
        }

        #endregion

        #region Illegal transitions

        [TestCase(GamePhase.Ready, GamePhase.Ended)]
        [TestCase(GamePhase.Playing, GamePhase.Ready)]
        [TestCase(GamePhase.Ended, GamePhase.Playing)]
        public void CanTransition_RefusesASkippedStep(GamePhase from, GamePhase to)
        {
            Assert.IsFalse(GamePhaseRules.CanTransition(from, to));
        }

        [TestCase(GamePhase.Ready)]
        [TestCase(GamePhase.Playing)]
        [TestCase(GamePhase.Ended)]
        public void CanTransition_RefusesAMoveToTheSamePhase(GamePhase phase)
        {
            // A repeat Start press must not restart the clock mid round.
            Assert.IsFalse(GamePhaseRules.CanTransition(phase, phase));
        }

        #endregion
    }
}
