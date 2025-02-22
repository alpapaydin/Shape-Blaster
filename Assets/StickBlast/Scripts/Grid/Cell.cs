using UnityEngine;
using System.Collections;

public class Cell : MonoBehaviour
{
    [SerializeField] private Material blastPreviewMaterialPrefab;
    [SerializeField] private Material blastInnerMaterialPrefab;
    [SerializeField] private SpriteRenderer outerGlowRenderer;
    private SpriteRenderer mainRenderer;
    private bool isComplete;
    private Material fillMaterial;
    private Material blastMaterial;
    private Material glowMaterial;
    private Material innerBlastMaterial;
    [SerializeField] private float fillDuration = 0.5f;
    private Coroutine fillCoroutine;
    private Color currentColor;

    private void Awake()
    {
        SetupRenderers();
        InitializeMaterials();
    }

    private void SetupRenderers()
    {
        mainRenderer = GetComponent<SpriteRenderer>();
        mainRenderer.sortingLayerName = "Grid";
        mainRenderer.sortingOrder = -1;

        if (outerGlowRenderer != null)
        {
            outerGlowRenderer.sortingLayerName = "Grid";
            outerGlowRenderer.sortingOrder = 2;
            glowMaterial = blastPreviewMaterialPrefab != null ? 
                new Material(blastPreviewMaterialPrefab) : 
                new Material(Shader.Find("Custom/CellBlastPreview"));
            outerGlowRenderer.material = glowMaterial;
            outerGlowRenderer.enabled = false;
            
            glowMaterial.SetFloat("_FillAmount", 0);
            glowMaterial.SetColor("_Color", Color.clear);
        }

        innerBlastMaterial = blastInnerMaterialPrefab != null ? 
            new Material(blastInnerMaterialPrefab) : 
            new Material(Shader.Find("Custom/CellBlastInner"));
        innerBlastMaterial.SetFloat("_FillAmount", 0);
    }

    private void InitializeMaterials()
    {
        fillMaterial = new Material(Shader.Find("Custom/CellFill"));
        blastMaterial = blastPreviewMaterialPrefab != null ? 
            new Material(blastPreviewMaterialPrefab) : 
            new Material(Shader.Find("Custom/CellBlastPreview"));
        
        mainRenderer.material = fillMaterial;
        fillMaterial.SetFloat("_FillAmount", 0);
        blastMaterial.SetFloat("_FillAmount", 0);
    }

    private void SwapToFillMaterial()
    {
        mainRenderer.material = fillMaterial;
        if (!isComplete)
        {
            fillMaterial.SetFloat("_FillAmount", 0);
            fillMaterial.SetColor("_Color", Color.clear);
        }
        else
        {
            fillMaterial.SetColor("_Color", currentColor);
            fillMaterial.SetFloat("_FillAmount", 1f);
        }
    }

    public void SetComplete(Color color, bool animate = false)
    {
        isComplete = true;
        currentColor = color;
        SwapToFillMaterial();
        
        if (animate)
        {
            AnimateComplete();
        }
        else
        {
            fillMaterial.SetFloat("_FillAmount", 1f);
        }
    }

    public void AnimateComplete()
    {
        if (fillCoroutine != null)
        {
            StopCoroutine(fillCoroutine);
        }
        fillMaterial.SetFloat("_FillAmount", 0f);
        fillCoroutine = StartCoroutine(AnimateFill());
    }

    private IEnumerator AnimateFill()
    {
        float elapsed = 0;
        while (elapsed < fillDuration)
        {
            elapsed += Time.deltaTime;
            float fillAmount = Mathf.Lerp(0, 1.2f, elapsed / fillDuration);
            fillMaterial.SetFloat("_FillAmount", fillAmount);
            yield return null;
        }
    }

    public bool IsComplete() => isComplete;

    private void ResetGlowMaterial()
    {
        if (outerGlowRenderer != null)
        {
            glowMaterial.SetFloat("_FillAmount", 0);
            glowMaterial.SetColor("_Color", Color.clear);
            outerGlowRenderer.enabled = false;
        }
        mainRenderer.material = fillMaterial;
    }

    public void Reset()
    {
        isComplete = false;
        if (fillCoroutine != null)
        {
            StopCoroutine(fillCoroutine);
        }
        ResetGlowMaterial();
        SwapToFillMaterial();
    }

    public void ShowBlastPreview(Color color)
    {
        if (outerGlowRenderer != null)
        {
            outerGlowRenderer.enabled = true;
            glowMaterial.SetColor("_Color", new Color(color.r, color.g, color.b, 0.3f));
            glowMaterial.SetFloat("_FillAmount", 1);
        }

        mainRenderer.material = innerBlastMaterial;
        innerBlastMaterial.SetColor("_Color", new Color(color.r, color.g, color.b, 0.7f));
        innerBlastMaterial.SetFloat("_FillAmount", 1);
    }

    public void ClearBlastPreview()
    {
        ResetGlowMaterial();
        SwapToFillMaterial();
    }

    private void OnDestroy()
    {
        if (fillMaterial != null)
            Destroy(fillMaterial);
        if (blastMaterial != null)
            Destroy(blastMaterial);
        if (glowMaterial != null)
            Destroy(glowMaterial);
        if (innerBlastMaterial != null)
            Destroy(innerBlastMaterial);
    }
}
