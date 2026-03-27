using UnityEngine;
using UnityEngine.UI;

public class MenuParallax : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public RectTransform layer;
        public float strength; // how much this layer moves
    }

    [SerializeField] ParallaxLayer[] layers;
    [SerializeField] float smoothing = 8f;

    Vector2 targetOffset;
    Vector2 screenCentre;

    void Start()
    {
        screenCentre = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }

    void Update()
    {
        Vector2 mouseOffset = ((Vector2)Input.mousePosition - screenCentre) / screenCentre;

        foreach (var l in layers)
        {
            targetOffset = mouseOffset * l.strength;
            l.layer.anchoredPosition = Vector2.Lerp(
                l.layer.anchoredPosition,
                targetOffset,
                Time.deltaTime * smoothing);
        }
    }
}