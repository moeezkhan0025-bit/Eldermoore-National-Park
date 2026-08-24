using UnityEngine;
using TMPro;

// A hovering prompt ("Exit", a warp symbol) that fades in when the player is near.
// NO Canvas needed. Put this on a child "Prompt" object of your interactable that
// has a trigger Collider2D (the show range). Give it a 3D TextMeshPro child for
// text and/or a SpriteRenderer child for an icon, and drag them into the slots.
//
//   • Self-detect (default): the trigger shows/hides it automatically.
//   • Driven: uncheck Self Detect and call Show() / Hide() from your own code.
[RequireComponent(typeof(Collider2D))]
public class WorldPrompt : MonoBehaviour
{
    [Header("Content (assign at least one)")]
    [SerializeField] private TMP_Text label;        // a 3D TextMeshPro child
    [SerializeField] private SpriteRenderer icon;   // optional symbol child
    [SerializeField] private string text = "Exit";

    [Header("Behaviour")]
    [SerializeField] private bool selfDetect = true;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float fadeSpeed = 8f;

    private float alpha;        // current opacity 0..1
    private float targetAlpha;  // where we're fading to

    void Awake()
    {
        if (label != null && !string.IsNullOrEmpty(text)) label.text = text;
        alpha = targetAlpha = 0f;
        ApplyAlpha();
    }

    void Update()
    {
        if (!Mathf.Approximately(alpha, targetAlpha))
        {
            alpha = Mathf.MoveTowards(alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            ApplyAlpha();
        }
    }

    void ApplyAlpha()
    {
        if (label != null) label.alpha = alpha;                 // TMP alpha is 0..1
        if (icon != null) { var c = icon.color; c.a = alpha; icon.color = c; }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (selfDetect && other.CompareTag(playerTag)) Show();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (selfDetect && other.CompareTag(playerTag)) Hide();
    }

    public void Show() => targetAlpha = 1f;
    public void Hide() => targetAlpha = 0f;
    public void SetText(string t) { if (label != null) label.text = t; }
}