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

    private void Awake()
    {
        _inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        _inputActions.Player.Enable();
        
        //이동
        _inputActions.Player.Move.performed += OnMove;
        _inputActions.Player.Move.canceled += OnMove;
        
        //공격
        _inputActions.Player.LightAttack.performed += OnLightAttack; 
        _inputActions.Player.HeavyAttack.performed += OnHeavyAttack;
        
        //줍기 버리기
        _inputActions.Player.WeaponInteract.performed += OnWeaponInteract;
    }

    private void OnDisable()
    {
        _inputActions.Player.Disable();
        
        //이동
        _inputActions.Player.Move.performed -= OnMove;
        _inputActions.Player.Move.canceled -= OnMove;
        
        //공격
        _inputActions.Player.LightAttack.performed -= OnLightAttack;
        _inputActions.Player.HeavyAttack.performed -= OnHeavyAttack;

        //줍기 버리기
        _inputActions.Player.WeaponInteract.performed -= OnWeaponInteract;
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
    
    //강공격
    private void OnHeavyAttack(InputAction.CallbackContext context)
    {
        OnAttackPerformed?.Invoke(AttackType.HeavyAttack, false);
    }
    
    //줍기 버리기
    private void OnWeaponInteract(InputAction.CallbackContext context)
    {
        OnWeaponInteractPerformed?.Invoke();
    }
}