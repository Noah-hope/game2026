using UnityEngine;
using UnityEngine.UI;

public class DamageTextEffect : MonoBehaviour
{
    private float speed = 1.6f;
    private float lifetime = 0.7f;
    private float elapsed;

    public void Initialize(string text, Color color)
    {
        GameObject textChild = new GameObject("Text");
        textChild.transform.SetParent(transform, false);

        Text textComp = textChild.AddComponent<Text>();
        textComp.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComp.text = text;
        textComp.fontSize = 80;
        textComp.fontStyle = FontStyle.Bold;
        textComp.alignment = TextAnchor.MiddleCenter;
        textComp.color = color;
        textComp.raycastTarget = false;

        Outline outline = textChild.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        outline.effectDistance = new Vector2(2f, -2f);

        RectTransform textRect = textChild.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(150f, 60f);

        RectTransform canvasRect = GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(150f, 60f);
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        transform.position += Vector3.up * speed * Time.deltaTime;

        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
