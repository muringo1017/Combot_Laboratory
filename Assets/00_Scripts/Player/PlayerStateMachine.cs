using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum PlayerState
{
    Idle,
    Move,
    Attack,
    Equip,
    Reload,
    SwitchWeapon,
    Dodge,
    Hit,
    Dead
}

public class PlayerStateMachine : MonoBehaviour
{
    private Player _player;
    private Dictionary<PlayerState, IPlayerState> _states;
    private IPlayerState _currentState;
    private PlayerState _lastLoggedState;
    
    public Player Player => _player;
    public IPlayerState CurrentState => _currentState;
    public PlayerState CurrentStateType => GetCurrentStateType();
    
    [SerializeField] private Text stateDebugText; // 인스펙터에서 연결
    
    private void Awake()
    {
        _player = GetComponent<Player>();
        InitializeStates();
    }

    
    private void OnEnable()
    {
        Managers.InputManager.OnAttackPerformed += HandleAttack;
        Managers.InputManager.OnWeaponInteractPerformed += HandleWeaponInteraction;
        Managers.InputManager.OnDodgePerformed += HandleDodge;
    }

    private void OnDisable()
    {
        Managers.InputManager.OnAttackPerformed -= HandleAttack;
        Managers.InputManager.OnWeaponInteractPerformed -= HandleWeaponInteraction;
        Managers.InputManager.OnDodgePerformed -= HandleDodge;
    }

    private void Start()
    {
        TransitionTo(PlayerState.Idle);
    }

    private void Update()
    {
        _currentState?.OnUpdate();
        
        UpdateStateUI();
    }
    
    private void OnGUI()
    {
        // 화면 오른쪽 상단에 State 정보 표시
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.yellow;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.UpperRight; // 우측 정렬
        
        string stateInfo = $"[ State: {CurrentStateType} ]";
        string weaponInfo = GetWeaponInfo();
        
        // 화면 너비 - 여유 공간 = 오른쪽 정렬 위치
        float rightMargin = 10f;
        float width = 300f;
        float xPos = Screen.width - width - rightMargin;
        
        GUI.Label(new Rect(xPos, 10, width, 25), stateInfo, style);
        
        // 무기 정보 (흰색)
        style.normal.textColor = Color.white;
        style.fontSize = 18;
        GUI.Label(new Rect(xPos, 35, width, 25), weaponInfo, style);
    }
    
    private void UpdateStateUI()
    {
        if (stateDebugText != null)
        {
            stateDebugText.text = GetDebugInfo();
        }
    }
    
    private string GetDebugInfo()
    {
        string stateInfo = $"<color=yellow>[ State: {CurrentStateType} ]</color>\n";
        string weaponInfo = GetWeaponInfo();
        string healthInfo = GetHealthInfo();
        
        return stateInfo + weaponInfo + "\n" + healthInfo;
    }
    
    private string GetHealthInfo()
    {
        if (_player == null || _player.Health == null)
            return "HP: --- / SP: --- / Dodge: ---";
        
        var health = _player.Health;
        return $"HP: {health.CurrentHealth:F0}/{health.MaxHealth:F0} | " +
               $"SP: {health.CurrentStamina:F1}/{health.MaxStamina:F0} | " +
               $"Dodge: {health.CurrentDodgeGauge:F1}/{health.MaxDodgeCount:F0}";
    }
    private void InitializeStates()
    {
        _states = new Dictionary<PlayerState, IPlayerState>
        {
            { PlayerState.Idle, new IdleState() },
            { PlayerState.Move, new MoveState() },
            { PlayerState.Attack, new AttackState() },
            { PlayerState.Equip, new EquipState() },
            { PlayerState.Dodge, new DodgeState() },
            { PlayerState.Hit, new HitState() },
            { PlayerState.Dead, new DeadState() }
        };
    }

    public void TransitionTo(PlayerState newState)
    {
        if (_currentState != null && !_currentState.CanTransitionTo(newState))
        {
            return; // 현재 상태가 전이를 허용하지 않음
        }
        _currentState?.OnExit();
        TransitionTo(_states[newState]);
    }

    public void TransitionTo(IPlayerState newState)
    {
        _currentState = newState;
        _currentState.OnEnter(this);
        UpdateStateUI();
    }

    private PlayerState GetCurrentStateType()
    {
        foreach (var pair in _states)
        {
            if (pair.Value == _currentState)
                return pair.Key;
        }
        return PlayerState.Idle;
    }

    private void HandleAttack(AttackType attackType, bool isInputReleased)
    {
        // 공격 가능한 상태일 때만 공격을 요청
        if (CurrentStateType == PlayerState.Idle || 
            CurrentStateType == PlayerState.Move || 
            CurrentStateType == PlayerState.Dodge)
        {
            _player.Combat.RequestAttack(attackType, isInputReleased);
        }
        // Attack 상태일 때는 콤보 버퍼 타임(애니메이션이 끝난 후)에만 입력 허용
        else if (CurrentStateType == PlayerState.Attack)
        {
            var attackState = _currentState as AttackState;
            if (attackState != null && attackState.IsInComboBuffer)
            {
                _player.Combat.RequestAttack(attackType, isInputReleased);
            }
        }
    }
    private void HandleWeaponInteraction()
    {
        // 현재 이동 또는 대기 상태일 때만 장착 상태로 전환 가능
        if (CurrentStateType == PlayerState.Idle || CurrentStateType == PlayerState.Move)
        {
            TransitionTo(PlayerState.Equip);
        }
    }
    
    private void HandleDodge()
    {
        // 현재 이동, 대기, 공격 상태일 때 회피 가능
        if (CurrentStateType == PlayerState.Idle || 
            CurrentStateType == PlayerState.Move || 
            CurrentStateType == PlayerState.Attack)
        {
            TransitionTo(PlayerState.Dodge);
        }
    }
    private string GetWeaponInfo()
    {
        if (_player == null || _player.Combat == null)
            return "Weapon: Initializing...";
        
        var weaponManager = _player.Combat.GetComponent<WeaponManager>();
        if (weaponManager == null)
            return "Weapon: No WeaponManager";
        
        if (!weaponManager.HasWeapon)
            return "Weapon: Unarmed";
        
        var currentWeapon = weaponManager.CurrentWeapon;
        if (currentWeapon == null)
            return "Weapon: None";
        
        // 무기 타입에 따라 다른 정보 표시
        switch (currentWeapon)
        {
            case LongSword longSword:
                return "Weapon: LongSword";
            case Pistol pistol:
                return "Weapon: Pistol";
            case Unarmed unarmed:
                return "Weapon: Unarmed";
            default:
                return $"Weapon: {currentWeapon.GetType().Name}";
        }
    }
    
}