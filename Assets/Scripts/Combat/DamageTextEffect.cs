using UnityEngine;
using UnityEngine.UI;

public class DamageTextEffect : MonoBehaviour
{
    private float speed = 1.6f;
    private float lifetime = 0.7f;
    private float elapsed;
    private Text textComponent;
    private Color startColor;
    private CanvasGroup canvasGroup;

    public void Initialize(string text, Color color)
    {
        GameObject textChild = new GameObject("Text");
        textChild.transform.SetParent(transform, false);

        textComponent = textChild.AddComponent<Text>();
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.text = text;
        textComponent.fontSize = 70;
        textComponent.fontStyle = FontStyle.Bold;
        textComponent.alignment = TextAnchor.MiddleCenter;
        textComponent.color = color;
        textComponent.raycastTarget = false;

        Outline outline = textChild.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        outline.effectDistance = new Vector2(2f, -2f);

        RectTransform textRect = textChild.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(200f, 50f);

        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;

        startColor = color;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        transform.position += Vector3.up * speed * Time.deltaTime;

        if (canvasGroup != null)
        {
            float alpha = 1f - Mathf.Clamp01(elapsed / lifetime);
            canvasGroup.alpha = alpha;
        }

        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
