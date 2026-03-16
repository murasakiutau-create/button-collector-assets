using System.Collections.Generic;
using UnityEngine;

namespace FurnitureWorkshop
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum GameState { Idle, Loading, Selecting, Coloring }
        public GameState CurrentState { get; private set; } = GameState.Idle;

        [SerializeField] private FurnitureLoader furnitureLoader;
        [SerializeField] private ColorPaletteUI colorPaletteUI;
        [SerializeField] private FurnitureColorController colorController;

        private readonly List<GameObject> loadedFurniture = new();
        private GameObject selectedPart;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            colorPaletteUI.OnColorChosen.AddListener(OnColorSelected);
            colorController.OnPartSelected.AddListener(OnFurniturePartSelected);
            colorPaletteUI.Hide();
        }

        public async void LoadFurniture(string modelFileName)
        {
            if (CurrentState == GameState.Loading) return;

            CurrentState = GameState.Loading;
            ClearFurniture();

            var root = await furnitureLoader.LoadAsync(modelFileName);
            if (root != null)
            {
                loadedFurniture.Add(root);
                CurrentState = GameState.Selecting;
            }
            else
            {
                Debug.LogError($"[GameManager] Failed to load: {modelFileName}");
                CurrentState = GameState.Idle;
            }
        }

        private void OnFurniturePartSelected(GameObject part)
        {
            selectedPart = part;
            CurrentState = GameState.Coloring;
            colorPaletteUI.Show();
        }

        private void OnColorSelected(Color color)
        {
            if (selectedPart == null) return;
            colorController.ApplyColor(color);
        }

        public void ClearFurniture()
        {
            foreach (var go in loadedFurniture)
            {
                if (go != null) Destroy(go);
            }
            loadedFurniture.Clear();
            selectedPart = null;
            colorPaletteUI.Hide();
            CurrentState = GameState.Idle;
        }
    }
}
