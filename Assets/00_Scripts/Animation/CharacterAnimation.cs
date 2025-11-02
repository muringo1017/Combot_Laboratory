using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    // 애니메이터 컴포넌트에 대한 참조
    private Animator _animator;

    // 애니메이터 파라미터를 정수(ID)로 미리 변환하여 성능을 높이고 오타를 방지합니다.
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int Dodging = Animator.StringToHash("Dodging");
    private static readonly int Damaged = Animator.StringToHash("Damaged"); // 피격 트리거
    
    // 콤보 공격용 트리거 파라미터 ID들
    private static readonly int LightAttack1 = Animator.StringToHash("LightAttack_1");
    private static readonly int LightAttack2 = Animator.StringToHash("LightAttack_2");
    private static readonly int LightAttack3 = Animator.StringToHash("LightAttack_3");
    private static readonly int HeavyAttack1 = Animator.StringToHash("HeavyAttack_1");
    private static readonly int HeavyAttack2 = Animator.StringToHash("HeavyAttack_2");
    private static readonly int HeavyAttack3 = Animator.StringToHash("HeavyAttack_3");
    
    
    // 기존의 일반 Attack 트리거는 이제 사용하지 않으므로 제거하거나 용도 변경이 필요합니다.
    // private static readonly int Attack = Animator.StringToHash("Attack"); 
    public float GetCurrentAnimationLength()
    {
        if (_animator == null) return 1.0f;
        
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.length;
    }
    public float GetAnimationLength(string animationName)
    {
        if (_animator == null || _animator.runtimeAnimatorController == null)
            return 1.0f;

        AnimationClip[] clips = _animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == animationName)
                return clip.length;
        }
        
        return 1.0f;
    }
    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        if (_animator == null)
        {
            Debug.LogError("Animator component not found on the character!");
        }
    }

    public void SetMoving(bool isMoving)
    {
        if (_animator == null) return;
        
        _animator.SetBool(IsMoving, isMoving);
        // Debug.Log("SetMoving: " + isMoving); // 디버그 로그는 필요한 경우에만 남겨두세요.
    }
    
    public void TriggerDodge()
    {
        if (_animator == null) return;
        
        _animator.SetTrigger(Dodging);
    }
    
    public void TriggerDamaged()
    {
        if (_animator == null) return;
        
        _animator.SetTrigger(Damaged);
        Debug.Log("[CharacterAnimation] Damaged 트리거 재생");
    }

    // --- 콤보 공격 애니메이션 함수들 ---

    public void LightAttack_1()
    {
        if (_animator == null) return;
        _animator.SetTrigger(LightAttack1);
     
    }

    public void LightAttack_2()
    {
        if (_animator == null) return;
        _animator.SetTrigger(LightAttack2);
       
    }

    public void LightAttack_3()
    {
        if (_animator == null) return;
        _animator.SetTrigger(LightAttack3);
        
    }

    public void HeavyAttack_1() => _animator.SetTrigger(HeavyAttack1);
    public void HeavyAttack_2() =>_animator.SetTrigger(HeavyAttack2);
    public void HeavyAttack_3() => _animator.SetTrigger(HeavyAttack3);
    public void LongSword_Light_1() => _animator.SetTrigger("LongSword_Light_1"); 
    public void LongSword_Light_2() => _animator.SetTrigger("LongSword_Light_2"); 
    public void LongSword_Light_3() => _animator.SetTrigger("LongSword_Light_3"); 
    public void LongSword_Heavy_1() => _animator.SetTrigger("LongSword_Heavy_1");
    public void LongSword_Heavy_2() => _animator.SetTrigger("LongSword_Heavy_2");
    public void LongSword_Heavy_3() => _animator.SetTrigger("LongSword_Heavy_3");
    
    
    public void Pistol_Attack_1() => _animator.SetTrigger("Pistol_Attack_1");
    public void Pistol_Attack_2() => _animator.SetTrigger("Pistol_Attack_2");
    
    [SerializeField] private AttackHitbox[] attackHitboxs;
    
    // 애니메이션 이벤트로 호출

}