using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FurnitureWorkshop
{
    public class ModelSelectorUI : MonoBehaviour
    {
        [SerializeField] private Dropdown modelDropdown;
        [SerializeField] private Button loadButton;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private GameObject loadingPanel;

        private List<string> availableModels = new();

        async void Start()
        {
            loadButton.onClick.AddListener(OnLoadButtonClicked);
            await RefreshModelList();
        }

        private async System.Threading.Tasks.Task RefreshModelList()
        {
            var loader = gameManager.GetComponent<FurnitureLoader>();
            availableModels = await loader.GetAvailableModels();

            modelDropdown.ClearOptions();

            if (availableModels.Count == 0)
            {
                modelDropdown.AddOptions(new List<string> { "モデルがありません" });
                loadButton.interactable = false;
                return;
            }

            modelDropdown.AddOptions(availableModels);
            loadButton.interactable = true;
        }

        private void OnLoadButtonClicked()
        {
            if (availableModels.Count == 0) return;

            string fileName = availableModels[modelDropdown.value];
            if (loadingPanel != null) loadingPanel.SetActive(true);

            gameManager.LoadFurniture(fileName);
            StartCoroutine(HideLoadingWhenDone());
        }

        private System.Collections.IEnumerator HideLoadingWhenDone()
        {
            yield return new WaitUntil(() =>
                gameManager.CurrentState != GameManager.GameState.Loading);

            if (loadingPanel != null) loadingPanel.SetActive(false);
        }
    }
}
