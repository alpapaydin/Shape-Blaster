using UnityEngine;

public class BlastEffect : MonoBehaviour
{
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private bool isVertical = false;
    private Material material;
    private float startTime;

    private static readonly int ProgressProperty = Shader.PropertyToID("_Progress");
    private static readonly int IsVerticalProperty = Shader.PropertyToID("_IsVertical");

    private void Start()
    {
        material = GetComponent<SpriteRenderer>().material;
        material.SetFloat(IsVerticalProperty, isVertical ? 1f : 0f);
        startTime = Time.time;
    }

    private void Update()
    {
        float progress = (Time.time - startTime) / duration;
        material.SetFloat(ProgressProperty, progress);

        if (progress >= 1f)
        {
            Destroy(gameObject);
        }
    }

    public void SetDirection(bool vertical)
    {
        isVertical = vertical;
        if (material != null)
        {
            material.SetFloat(IsVerticalProperty, isVertical ? 1f : 0f);
        }
    }

    private void OnDestroy()
    {
        if (material != null)
        {
            Destroy(material);
        }
    }
}
