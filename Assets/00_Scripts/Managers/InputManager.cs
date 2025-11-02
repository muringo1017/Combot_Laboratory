using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerInputActions _inputActions;
    
    //이동 벡터
    public Vector2 MoveInput { get; private set; }
    
    //공격실행
    public event Action<AttackType, bool> OnAttackPerformed;
    
    //줍기,버리기 실행
    public event Action OnWeaponInteractPerformed;
    
    //회피 실행
    public event Action OnDodgePerformed;

    private void Awake()
    {
        _inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        if (_inputActions == null)
        {
            _inputActions = new PlayerInputActions();
        }
        _inputActions.Player.Enable();
        
        //이동
        _inputActions.Player.Move.performed += OnMove;
        _inputActions.Player.Move.canceled += OnMove;
        
        //공격
        _inputActions.Player.LightAttack.performed += OnLightAttack; 
        _inputActions.Player.LightAttack.canceled += OnLightAttackCanceled;
        _inputActions.Player.HeavyAttack.performed += OnHeavyAttack;
        _inputActions.Player.HeavyAttack.canceled += OnHeavyAttackCanceled;
        
        //줍기 버리기
        _inputActions.Player.WeaponInteract.performed += OnWeaponInteract;
        
        //회피
        _inputActions.Player.Dodge.performed += OnDodge;
    }

    private void OnDisable()
    {
        if (_inputActions == null)
        {
            return;
        }
        _inputActions.Player.Disable();
        
        //이동
        _inputActions.Player.Move.performed -= OnMove;
        _inputActions.Player.Move.canceled -= OnMove;
        
        //공격
        _inputActions.Player.LightAttack.performed -= OnLightAttack;
        _inputActions.Player.LightAttack.canceled -= OnLightAttackCanceled;
        _inputActions.Player.HeavyAttack.performed -= OnHeavyAttack;
        _inputActions.Player.HeavyAttack.canceled -= OnHeavyAttackCanceled;

        //줍기 버리기
        _inputActions.Player.WeaponInteract.performed -= OnWeaponInteract;
        
        //회피
        _inputActions.Player.Dodge.performed -= OnDodge;
    }

    //이동
    private void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }
    
    //약공격
    private void OnLightAttack(InputAction.CallbackContext context)
    {
       
        OnAttackPerformed?.Invoke(AttackType.LightAttack, false);
    }
    private void OnLightAttackCanceled(InputAction.CallbackContext context)
    {
        OnAttackPerformed?.Invoke(AttackType.LightAttack, true);
    }
    
    //강공격
    private void OnHeavyAttack(InputAction.CallbackContext context)
    {
        OnAttackPerformed?.Invoke(AttackType.HeavyAttack, false);
    }
    private void OnHeavyAttackCanceled(InputAction.CallbackContext context)
    {
        OnAttackPerformed?.Invoke(AttackType.HeavyAttack, true);
    }
    
    //줍기 버리기
    private void OnWeaponInteract(InputAction.CallbackContext context)
    {
        OnWeaponInteractPerformed?.Invoke();
    }
    
    //회피
    private void OnDodge(InputAction.CallbackContext context)
    {
        OnDodgePerformed?.Invoke();
    }

    private void OnDestroy()
    {
        if (_inputActions != null)
        {
            _inputActions.Dispose();
            _inputActions = null;
        }
    }
}