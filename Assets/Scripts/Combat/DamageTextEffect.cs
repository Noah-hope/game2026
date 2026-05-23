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

    public void Initialize(int damage, Color color)
    {
        GameObject textChild = new GameObject("Text");
        textChild.transform.SetParent(transform, false);

        textComponent = textChild.AddComponent<Text>();
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.text = damage.ToString();
        textComponent.fontSize = 30;
        textComponent.alignment = TextAnchor.MiddleCenter;
        textComponent.color = color;
        textComponent.raycastTarget = false;

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
