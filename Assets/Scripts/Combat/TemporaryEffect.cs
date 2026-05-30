using UnityEngine;

public class TemporaryEffect : MonoBehaviour
{
    private float lifetime;
    private float totalLifetime;
    private bool fadeOut = true;
    private float endScaleMultiplier = 1f;
    private Vector3 velocity;
    private float rotationSpeed;
    private SpriteRenderer spriteRenderer;
    private Color startColor;
    private Vector3 startScale;

    public void Initialize(float effectLifetime)
    {
        Initialize(effectLifetime, true, 1f);
    }

    public void Initialize(float effectLifetime, bool shouldFadeOut, float finalScaleMultiplier)
    {
        lifetime = Mathf.Max(0.01f, effectLifetime);
        totalLifetime = lifetime;
        fadeOut = shouldFadeOut;
        endScaleMultiplier = Mathf.Max(0.01f, finalScaleMultiplier);
        spriteRenderer = GetComponent<SpriteRenderer>();
        startColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        startScale = transform.localScale;
    }

    public void SetMotion(Vector3 moveVelocity, float rotateDegreesPerSecond)
    {
        velocity = moveVelocity;
        rotationSpeed = rotateDegreesPerSecond;
    }

    private void Update()
    {
        lifetime -= Time.deltaTime;
        float progress = 1f - Mathf.Clamp01(lifetime / totalLifetime);

        if (velocity.sqrMagnitude > 0.0001f)
        {
            transform.position += velocity * Time.deltaTime;
        }

        if (Mathf.Abs(rotationSpeed) > 0.001f)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }

        if (spriteRenderer != null && fadeOut)
        {
            Color color = startColor;
            color.a = Mathf.Lerp(startColor.a, 0f, progress);
            spriteRenderer.color = color;
        }

        if (Mathf.Abs(endScaleMultiplier - 1f) > 0.001f)
        {
            transform.localScale = Vector3.Lerp(startScale, startScale * endScaleMultiplier, progress);
        }

        if (lifetime <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
