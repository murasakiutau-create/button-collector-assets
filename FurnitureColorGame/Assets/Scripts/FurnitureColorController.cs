using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FurnitureWorkshop
{
    public class FurnitureColorController : MonoBehaviour
    {
        [SerializeField] private LayerMask furnitureLayerMask = ~0;
        [SerializeField] private Color highlightTint = new Color(1f, 1f, 0.5f, 1f);

        public UnityEvent<GameObject> OnPartSelected = new();

        private Camera mainCamera;
        private GameObject currentSelectedPart;
        private readonly Dictionary<GameObject, Material[]> originalMaterials = new();

        void Start()
        {
            mainCamera = Camera.main;
        }

        void Update()
        {
            if (!Input.GetMouseButtonDown(0)) return;

            // Ignore clicks on UI elements
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 200f, furnitureLayerMask))
            {
                if (hit.collider.CompareTag("FurniturePart"))
                    SelectPart(hit.collider.gameObject);
            }
        }

        private void SelectPart(GameObject part)
        {
            if (currentSelectedPart != null)
                RestoreOriginal(currentSelectedPart);

            var renderer = part.GetComponent<Renderer>();
            if (renderer == null) return;

            // Store originals
            originalMaterials[part] = renderer.sharedMaterials;

            // Apply highlight by cloning materials and tinting them
            var clones = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < clones.Length; i++)
            {
                clones[i] = new Material(renderer.sharedMaterials[i]);
                clones[i].color *= highlightTint;
            }
            renderer.materials = clones;

            currentSelectedPart = part;
            OnPartSelected.Invoke(part);
        }

        private void RestoreOriginal(GameObject part)
        {
            if (!originalMaterials.TryGetValue(part, out var originals)) return;
            var renderer = part.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterials = originals;
            originalMaterials.Remove(part);
        }

        public void ApplyColor(Color color)
        {
            if (currentSelectedPart == null) return;
            var renderer = currentSelectedPart.GetComponent<Renderer>();
            if (renderer == null) return;

            var mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            renderer.material = mat;

            // Update stored original so next deselect restores the new color
            originalMaterials[currentSelectedPart] = new[] { mat };
        }

        public void ApplyColorToAllParts(Color color)
        {
            if (currentSelectedPart == null) return;

            // Find the FurnitureRoot parent
            Transform t = currentSelectedPart.transform;
            while (t != null && !t.CompareTag("FurnitureRoot"))
                t = t.parent;

            if (t == null) return;

            foreach (var mr in t.GetComponentsInChildren<MeshRenderer>(true))
            {
                var mat = new Material(Shader.Find("Standard"));
                mat.color = color;
                mr.material = mat;
            }
        }

        public void DeselectAll()
        {
            if (currentSelectedPart != null)
            {
                RestoreOriginal(currentSelectedPart);
                currentSelectedPart = null;
            }
        }
    }
}
