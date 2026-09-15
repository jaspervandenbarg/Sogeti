using System.Collections.Generic;
using Sogeti.Interaction;
using Sogeti.Planting;
using UnityEngine;

namespace Sogeti.Session
{
    /// <summary>
    /// Turns planting and growth events into score.
    /// It subscribes once per plant and never polls. Every point value arrives
    /// already computed by GrowthPoints, so no award is recalculated here.
    /// </summary>
    [DisallowMultipleComponent]
    public class ScoreCollector : MonoBehaviour
    {
        [SerializeField]
        private GameSession session;

        [Tooltip("The only source of new plants. Its PlantPlaced event carries the planting award.")]
        [SerializeField]
        private SeedPlanter planter;

        private readonly List<Plant> tracked = new List<Plant>();

        private void OnEnable()
        {
            if (session == null || planter == null)
            {
                Debug.LogError($"{name}: session or planter is missing, nothing will score.", this);
                return;
            }

            planter.PlantPlaced += OnPlantPlaced;
        }

        private void OnDisable()
        {
            if (planter != null)
            {
                planter.PlantPlaced -= OnPlantPlaced;
            }

            for (int i = 0; i < tracked.Count; i++)
            {
                Unsubscribe(tracked[i]);
            }

            tracked.Clear();
        }

        /// <summary>
        /// Plant.Planted carries the same award and fires first, inside Initialize.
        /// Reading both would count every seed twice, so this is the only planting hook.
        /// </summary>
        private void OnPlantPlaced(Plant plant, int points)
        {
            session.RecordPlanted(plant != null ? plant.Seed : null, points);

            if (plant == null)
            {
                return;
            }

            plant.StageAdvanced += OnStageAdvanced;
            plant.Died += OnDied;
            tracked.Add(plant);
        }

        private void OnStageAdvanced(Plant plant, int points)
        {
            session.RecordStageReached(plant.Seed, points, plant.IsFullyGrown);
        }

        private void OnDied(Plant plant)
        {
            session.RecordLost();
            Unsubscribe(plant);
            tracked.Remove(plant);
        }

        private void Unsubscribe(Plant plant)
        {
            if (plant == null)
            {
                return;
            }

            plant.StageAdvanced -= OnStageAdvanced;
            plant.Died -= OnDied;
        }
    }
}
