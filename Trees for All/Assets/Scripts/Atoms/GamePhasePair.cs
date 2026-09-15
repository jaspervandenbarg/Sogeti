using System;
using Sogeti.Game;
using UnityAtoms;
using UnityEngine;

namespace Sogeti.Atoms
{
    /// <summary>IPair of type `GamePhase`. Feeds the changed with history Event.</summary>
    [Serializable]
    public struct GamePhasePair : IPair<GamePhase>
    {
        public GamePhase Item1 { get => item1; set => item1 = value; }

        public GamePhase Item2 { get => item2; set => item2 = value; }

        [SerializeField]
        private GamePhase item1;

        [SerializeField]
        private GamePhase item2;

        public void Deconstruct(out GamePhase first, out GamePhase second)
        {
            first = Item1;
            second = Item2;
        }
    }
}
