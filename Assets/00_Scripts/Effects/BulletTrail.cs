using UnityEngine;

/// <summary>
/// 총알에 Trail Renderer를 자동으로 추가하고 설정합니다.
/// </summary>
[RequireComponent(typeof(TrailRenderer))]
public class BulletTrail : MonoBehaviour
{
    [Header("Trail Settings")]
    [SerializeField] private float trailTime = 0.3f;
    [SerializeField] private float trailWidth = 0.1f;
    [SerializeField] private Color trailStartColor = Color.yellow;
    [SerializeField] private Color trailEndColor = new Color(1f, 1f, 0f, 0f); // 투명한 노란색
    [SerializeField] private Material trailMaterial;
    
    private TrailRenderer _trailRenderer;
    
    private void Awake()
    {
        _trailRenderer = GetComponent<TrailRenderer>();
        SetupTrail();
    }
    
    private void SetupTrail()
    {
        if (_trailRenderer == null) return;
        
        // Trail 기본 설정
        _trailRenderer.time = trailTime;
        _trailRenderer.startWidth = trailWidth;
        _trailRenderer.endWidth = 0f; // 끝으로 갈수록 가늘어짐
        
        // 색상 그라디언트 설정
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] 
            { 
                new GradientColorKey(trailStartColor, 0.0f), 
                new GradientColorKey(trailEndColor, 1.0f) 
            },
            new GradientAlphaKey[] 
            { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        _trailRenderer.colorGradient = gradient;
        
        // Material 설정
        if (trailMaterial != null)
        {
            _trailRenderer.material = trailMaterial;
        }
        else
        {
            // 기본 Material 생성 (없을 경우)
            CreateDefaultMaterial();
        }
        
        // 기타 설정
        _trailRenderer.minVertexDistance = 0.01f; // 더 부드러운 궤적
        _trailRenderer.textureMode = LineTextureMode.Stretch;
        _trailRenderer.alignment = LineAlignment.View; // 카메라를 향하도록
        
        Debug.Log($"[BulletTrail] Trail 설정 완료: time={trailTime}, width={trailWidth}");
    }
    
    private void CreateDefaultMaterial()
    {
        // 간단한 발광 Material 생성
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = trailStartColor;
        _trailRenderer.material = mat;
        
        Debug.LogWarning("[BulletTrail] Material이 없어 기본 Material을 생성했습니다.");
    }
    
    /// <summary>
    /// Trail 색상 변경
    /// </summary>
    public void SetTrailColor(Color startColor, Color endColor)
    {
        if (_trailRenderer == null) return;
        
        trailStartColor = startColor;
        trailEndColor = endColor;
        
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] 
            { 
                new GradientColorKey(startColor, 0.0f), 
                new GradientColorKey(endColor, 1.0f) 
            },
            new GradientAlphaKey[] 
            { 
                new GradientAlphaKey(startColor.a, 0.0f), 
                new GradientAlphaKey(endColor.a, 1.0f) 
            }
        );
        _trailRenderer.colorGradient = gradient;
    }
    
    /// <summary>
    /// Trail 초기화 (재사용 시)
    /// </summary>
    public void ClearTrail()
    {
        if (_trailRenderer != null)
        {
            _trailRenderer.Clear();
        }
    }
}



