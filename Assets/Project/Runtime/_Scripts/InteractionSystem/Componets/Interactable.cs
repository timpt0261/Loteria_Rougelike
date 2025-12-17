using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour, IInteractable
{
    [field: SerializeField] private string displayName = "";
    [field: SerializeField] private bool isEnabled = true;


    [field: Header("Interaction Outline")]
    [field: SerializeField] private Renderer renderer;
    [field: SerializeField] private Ease outlineScaleEase = Ease.OutQuad;
    [field: SerializeField] private Color outlineColor;
    [field: SerializeField] private float outlineScale = 1.1f;
    [field: SerializeField] private float outlineDuration = 0.5f;
    private const int ZERO = 0;
    private int _shader_ref_outline_scale = Shader.PropertyToID("_Outline_Scale");
    private int _shader_ref_outline_color = Shader.PropertyToID("_Outline_Color");

    public string DisplayName => displayName;

    public Ease OutlineScaleEase => outlineScaleEase;
    public Color OutlineColor => outlineColor;

    public float OutlineScale => outlineScale;

    public float OutlineDuration => outlineDuration;

    public bool CanInteract() => isEnabled;

    [field: SerializeField] private UnityEvent OnInteraction;

    void Start()
    {
        if (renderer == null)
        {
            renderer = GetComponent<Renderer>();
        }
        renderer.material.SetColor(_shader_ref_outline_color, outlineColor);
        renderer.material.SetFloat(_shader_ref_outline_scale, ZERO);
        if (displayName == "") displayName = gameObject.name;
    }

    public void Interact(GameObject interactor)
    {
        OnInteraction?.Invoke();
    }

    public void OnFocusGained()
    {
        // sets the shader material to be visible 
        renderer.material.DOFloat(outlineScale, _shader_ref_outline_scale, outlineDuration).SetEase(outlineScaleEase);
    }

    public void OnFocusLost()
    {
        // sets the shader material to be invisible 
        renderer.material.DOFloat(ZERO, _shader_ref_outline_scale, outlineDuration).SetEase(outlineScaleEase);
    }

}
