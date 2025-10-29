using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
public class MyBT : MonoBehaviour
{
    private MyNode _root;
    private Animator _animator;
    
    public Transform target;
    public float speed = 2.0f;
    public float chaseRange =  5.0f;
    public float attackRange = 1.5f;
    
    // 공격 커밋 관련 변수 추가
    private bool _isAttackCommitted = false;
    private float _attackCommitTime = 0f;
    private float _attackAnimationLength = 1.0f;

    

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _root = new MySelector(new List<MyNode>
            {
                new MySequence(new List<MyNode>
                {
                    new MyLeaf(ShouldContinueAttack),
                    new MyLeaf(AttackPlayer)
                }),
                
                new MySequence(new List<MyNode>
                {
                    new MyLeaf(ShouldContinueAttack),
                    new MyLeaf(AttackPlayer)
                }),
                new MySequence(new List<MyNode>
                {
                    new MyLeaf(CheckPlayerInRange), 
                    new MyLeaf(CommitToAttack)
                    
                }),
                new MySequence(new List<MyNode>
                {
                    new MyLeaf(CheckChaseRange),
                    new MyLeaf(ChasePlayer)
                }),
                new MyLeaf(IDLE)
            });
        
            AnimationClip[] clips = _animator.runtimeAnimatorController.animationClips;
            foreach (var clip in clips)
            {
                if (clip.name == "ATTACK")
                {
                    _attackAnimationLength = clip.length;
                    break;
                }
            }
    }

    private void Update()
    {
        _root.Evaluate();
        
        if (_isAttackCommitted)
        {
            _attackCommitTime -= Time.deltaTime;
            if (_attackCommitTime <= 0f)
            {
                _isAttackCommitted = false;
            }
        }
    }

    MyNodeStatus ShouldContinueAttack()
    {
        return _isAttackCommitted ? MyNodeStatus.Success : MyNodeStatus.Failure;
    }

    private MyNodeStatus RangeCheck(float range)
    {
        float distance = Vector3.Distance(transform.position, target.position);
        return distance < range ? MyNodeStatus.Success : MyNodeStatus.Failure;
    }
    MyNodeStatus CheckChaseRange()
    {
        
        if (_isAttackCommitted) return MyNodeStatus.Failure;
        return RangeCheck(chaseRange);
    }
    MyNodeStatus CheckPlayerInRange()
    {
        if (_isAttackCommitted) return MyNodeStatus.Success;
        return RangeCheck(attackRange);
    }

    MyNodeStatus CommitToAttack()
    {
        if (!_isAttackCommitted)
        {
            _isAttackCommitted = true;
            _attackCommitTime = _attackAnimationLength;
        }
        return MyNodeStatus.Success;
    }

    MyNodeStatus AttackPlayer()
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        
        if (stateInfo.IsName("ATTACK"))
        {
            // 애니메이션 진행 중
            if (stateInfo.normalizedTime < 1.0f)
            {
                return MyNodeStatus.Running;
            }
            else
            {
                // 애니메이션 완료
                return MyNodeStatus.Success;
            }
        }
        else
        {
            // 공격 시작
            Rotate();
            AnimatorChange("ATTACK");
            return MyNodeStatus.Running;
        }
    }


    MyNodeStatus IDLE()
    {
        AnimatorChange("IDLE");
        return MyNodeStatus.Success;
    }

    MyNodeStatus ChasePlayer()
    {
        
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
  

        Rotate();
        AnimatorChange("MOVE");
        
        return MyNodeStatus.Running;
        
    }

    void Rotate()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0.0f;
        transform.forward = direction;
    }
    private void AnimatorChange(string temp)
    {
        _animator.SetBool("IDLE", false);
        _animator.SetBool("MOVE", false);
        _animator.SetBool("ATTACK", false);
        
        _animator.SetBool(temp, true);
    }
}
