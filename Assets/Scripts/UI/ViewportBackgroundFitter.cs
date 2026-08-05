using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class ViewportBackgroundFitter : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField, Min(1f)] private float overscan = 1.02f;

    private SpriteRenderer spriteRenderer;
    private float lastAspect = -1f;
    private float lastOrthographicSize = -1f;

    public void Configure(Camera cameraToFit)
    {
        targetCamera = cameraToFit;
        FitNow();
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        FitNow();
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null
            || !targetCamera.orthographic
            || Mathf.Approximately(lastAspect, targetCamera.aspect)
               && Mathf.Approximately(
                   lastOrthographicSize,
                   targetCamera.orthographicSize))
        {
            return;
        }

        FitNow();
    }

    public void FitNow()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (spriteRenderer == null
            || spriteRenderer.sprite == null
            || targetCamera == null
            || !targetCamera.orthographic)
        {
            return;
        }

        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
        float viewportHeight = targetCamera.orthographicSize * 2f;
        float viewportWidth = viewportHeight * targetCamera.aspect;
        float scale = Mathf.Max(
            viewportWidth / spriteSize.x,
            viewportHeight / spriteSize.y) * overscan;

        transform.localScale = new Vector3(scale, scale, 1f);
        lastAspect = targetCamera.aspect;
        lastOrthographicSize = targetCamera.orthographicSize;
    }
}
