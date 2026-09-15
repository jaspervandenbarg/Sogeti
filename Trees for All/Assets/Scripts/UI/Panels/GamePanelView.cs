using System;
using System.Collections.Generic;
using Sogeti.Game;
using Sogeti.UI.ToolMenu;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sogeti.UI.Panels
{
    /// <summary>
    /// The one panel that serves the start screen, the controls and the game over
    /// screen. It knows nothing about the game. It draws what it is given and
    /// raises a button press, so the controller owns every decision.
    /// </summary>
    [DisallowMultipleComponent]
    public class GamePanelView : MonoBehaviour
    {
        [Tooltip("Holds the canvas. It starts inactive, so the panel is invisible until the placer lands it.")]
        [SerializeField]
        private GameObject panelRoot;

        [Header("Groups")]
        [SerializeField]
        private GameObject startGroup;
        [SerializeField]
        private GameObject helpGroup;
        [SerializeField]
        private GameObject gameOverGroup;

        [Header("Start")]
        [SerializeField]
        private Button playButton;
        [SerializeField]
        private Button controlsButton;

        [Header("Controls")]
        [SerializeField]
        private Button backButton;

        [Header("Game over")]
        [SerializeField]
        private Button restartButton;
        [SerializeField]
        private TMP_Text totalScoreText;
        [SerializeField]
        private TMP_Text lostText;
        [Tooltip("Holds one row per seed type. Hidden while the player planted nothing.")]
        [SerializeField]
        private GameObject seedList;
        [SerializeField]
        private Transform seedListParent;
        [Tooltip("ToolMenuEntry.prefab. Reused as a display-only card, the SelectedSeedReadout pattern.")]
        [SerializeField]
        private ToolMenuEntry seedRowPrefab;

        private readonly List<ToolMenuEntry> seedRows = new List<ToolMenuEntry>();

        public event Action PlayPressed;
        public event Action ControlsPressed;
        public event Action BackPressed;
        public event Action RestartPressed;

        public bool IsShowing => panelRoot != null && panelRoot.activeSelf;

        public void ShowStart()
        {
            SetGroup(startGroup);
            SetShowing(true);
        }

        public void ShowControls()
        {
            SetGroup(helpGroup);
            SetShowing(true);
        }

        /// <summary>
        /// Builds the seed rows at show time. The tally is empty at Awake, so a
        /// prebuilt list would always read zero.
        /// </summary>
        public void ShowGameOver(ScoreTally tally)
        {
            SetGroup(gameOverGroup);
            FillGameOver(tally);
            SetShowing(true);
        }

        public void Hide() => SetShowing(false);

        private void Awake()
        {
            SetShowing(false);
            SetGroup(null);
        }

        private void OnEnable()
        {
            AddListener(playButton, RaisePlay);
            AddListener(controlsButton, RaiseControls);
            AddListener(backButton, RaiseBack);
            AddListener(restartButton, RaiseRestart);
        }

        private void OnDisable()
        {
            RemoveListener(playButton, RaisePlay);
            RemoveListener(controlsButton, RaiseControls);
            RemoveListener(backButton, RaiseBack);
            RemoveListener(restartButton, RaiseRestart);
        }

        private void FillGameOver(ScoreTally tally)
        {
            int total = tally != null ? tally.Total : 0;
            int lost = tally != null ? tally.PlantsLost : 0;

            if (totalScoreText != null)
            {
                totalScoreText.text = total.ToString();
            }

            if (lostText != null)
            {
                lostText.text = lost == 1 ? "1 tree lost" : $"{lost} trees lost";
            }

            BuildSeedRows(tally);
        }

        private void BuildSeedRows(ScoreTally tally)
        {
            ClearSeedRows();

            IReadOnlyList<SeedTally> perSeed = tally != null ? tally.PerSeed : null;
            int count = perSeed != null ? perSeed.Count : 0;

            // An empty box reads as a bug, so the whole list leaves when nothing was planted.
            if (seedList != null)
            {
                seedList.SetActive(count > 0);
            }

            if (count == 0 || seedRowPrefab == null || seedListParent == null)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                SeedTally seed = perSeed[i];
                ToolMenuEntry row = Instantiate(seedRowPrefab, seedListParent);

                row.SetContent(
                    seed.Seed != null ? seed.Seed.MenuIcon : null,
                    $"{SeedName(seed)}  {seed.FullyGrown} grown / {seed.Planted} planted");

                // The rows report, they never take a click.
                row.SetSelected(false);
                seedRows.Add(row);
            }
        }

        private static string SeedName(SeedTally seed)
        {
            return seed.Seed != null ? seed.Seed.DisplayName : "Seed";
        }

        private void ClearSeedRows()
        {
            for (int i = 0; i < seedRows.Count; i++)
            {
                if (seedRows[i] != null)
                {
                    Destroy(seedRows[i].gameObject);
                }
            }

            seedRows.Clear();
        }

        private void SetShowing(bool showing)
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(showing);
            }
        }

        // One group at a time, so two screens can never overlap.
        private void SetGroup(GameObject active)
        {
            SetActiveIfPresent(startGroup, startGroup == active);
            SetActiveIfPresent(helpGroup, helpGroup == active);
            SetActiveIfPresent(gameOverGroup, gameOverGroup == active);
        }

        private static void SetActiveIfPresent(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        private static void AddListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.AddListener(action);
            }
        }

        private static void RemoveListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(action);
            }
        }

        private void RaisePlay() => PlayPressed?.Invoke();

        private void RaiseControls() => ControlsPressed?.Invoke();

        private void RaiseBack() => BackPressed?.Invoke();

        private void RaiseRestart() => RestartPressed?.Invoke();
    }
}
