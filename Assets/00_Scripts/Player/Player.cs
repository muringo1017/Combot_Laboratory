using System;
using UnityEngine;

[RequireComponent(typeof(PlayerStateMachine))]
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerHealth))]
public class Player : MonoBehaviour
{
    private PlayerStateMachine _stateMachine;
    private PlayerController _controller;
    private PlayerCombat _combat;
    private CharacterAnimation _characterAnimation;
    private PlayerHealth _health;

    private Rigidbody _rigidbody;
    private CapsuleCollider _capsuleCollider;

    
    public PlayerStateMachine StateMachine => _stateMachine;
    public PlayerController Controller => _controller;
    public PlayerCombat Combat => _combat;
    public CharacterAnimation CharacterAnimation => _characterAnimation;
    public PlayerHealth Health => _health;
    
    public Rigidbody Rigidbody => _rigidbody;
    public CapsuleCollider CapsuleCollider => _capsuleCollider;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        _stateMachine?.TransitionTo(PlayerState.Idle);
    }

    private void InitializeComponents()
    {
        
        _stateMachine = GetComponent<PlayerStateMachine>();
        _controller = GetComponent<PlayerController>();
        _combat = GetComponent<PlayerCombat>();
        _characterAnimation = GetComponentInChildren<CharacterAnimation>();
        _health = GetComponent<PlayerHealth>();

        _rigidbody = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        if (_stateMachine == null) Debug.LogWarning("Player: Missing PlayerStateMachine.");
        if (_controller == null) Debug.LogWarning("Player: Missing PlayerController.");
        if (_combat == null) Debug.LogWarning("Player: Missing PlayerCombat.");
        if (_characterAnimation == null) Debug.LogWarning("Player: Missing CharacterAnimation in children.");
        if (_health == null) Debug.LogWarning("Player: Missing PlayerHealth.");
        if (_rigidbody == null) Debug.LogWarning("Player: Missing Rigidbody.");
        if (_capsuleCollider == null) Debug.LogWarning("Player: Missing CapsuleCollider.");
    }

    
}