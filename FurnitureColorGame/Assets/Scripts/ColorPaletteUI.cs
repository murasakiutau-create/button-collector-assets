using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FurnitureWorkshop
{
    public class ColorPaletteUI : MonoBehaviour
    {
        [SerializeField] private Transform swatchContainer;
        [SerializeField] private GameObject swatchButtonPrefab;
        [SerializeField] private List<Color> palette = new()
        {
            Color.white,
            new Color(0.85f, 0.85f, 0.85f),
            new Color(0.5f, 0.5f, 0.5f),
            Color.black,
            new Color(0.87f, 0.72f, 0.53f),   // BurlyWood
            new Color(0.55f, 0.27f, 0.07f),   // SaddleBrown
            new Color(0.82f, 0.71f, 0.55f),   // Tan
            new Color(0.96f, 0.96f, 0.86f),   // Beige
            new Color(0.39f, 0.58f, 0.93f),   // CornflowerBlue
            new Color(0.27f, 0.51f, 0.71f),   // SteelBlue
            new Color(0f, 0.5f, 0.5f),         // Teal
            new Color(0.96f, 1f, 0.98f),       // MintCream
            new Color(0.80f, 0.36f, 0.36f),   // IndianRed
            new Color(1f, 0.39f, 0.28f),       // Tomato
            new Color(1f, 0.84f, 0f),          // Gold
            new Color(0.42f, 0.56f, 0.14f),   // OliveDrab
        };

        public UnityEvent<Color> OnColorChosen = new();

        private Color currentColor = Color.white;

        void Start()
        {
            BuildSwatches();
            Hide();
        }

        private void BuildSwatches()
        {
            foreach (Transform child in swatchContainer)
                Destroy(child.gameObject);

            foreach (var color in palette)
            {
                var btn = Instantiate(swatchButtonPrefab, swatchContainer);
                btn.GetComponent<Image>().color = color;
                Color captured = color;
                btn.GetComponent<Button>().onClick.AddListener(() => SelectColor(captured));
            }
        }

        private void SelectColor(Color color)
        {
            currentColor = color;
            OnColorChosen.Invoke(color);
        }

        public void AddCustomColor(Color color)
        {
            palette.Add(color);
            var btn = Instantiate(swatchButtonPrefab, swatchContainer);
            btn.GetComponent<Image>().color = color;
            btn.GetComponent<Button>().onClick.AddListener(() => SelectColor(color));
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}
