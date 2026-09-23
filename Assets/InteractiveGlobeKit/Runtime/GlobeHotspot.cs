using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace InteractiveGlobeKit
{
    public sealed class GlobeHotspot : MonoBehaviour, IPointerClickHandler
    {
        public string locationName;
        [TextArea] public string description;
        public float latitude;
        public float longitude;
        public UnityEvent onSelected;

        public void OnPointerClick(PointerEventData eventData) => Select();
        private void OnMouseDown() => Select();

        public void Select()
        {
            Debug.Log($"{locationName}: {latitude:0.##}°, {longitude:0.##}° — {description}", this);
            onSelected?.Invoke();
        }
    }
}
