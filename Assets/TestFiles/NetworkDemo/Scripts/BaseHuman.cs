using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHuman : MonoBehaviour
{
    /// <summary>
    /// 描述
    /// </summary>
    public string desc = "";

    /// <summary>
    /// 移动速度
    /// </summary>
    /// 
    public float speed = 5f;

    internal bool isAttacking;
    internal float attackTime = float.MinValue;

    /// <summary>
    /// 是否正在移动
    /// </summary>
    protected bool isMoving;

    /// <summary>
    /// 移动目标点
    /// </summary>
    private Vector3 targetPos;

    /// <summary>
    /// 动画组件
    /// </summary>
    private Animator animator;

    private int IS_MOVING_ANIM_PARAM;
    private int IS_ATTACK_ANIM_PARAM;

    protected void Awake()
    {
        IS_MOVING_ANIM_PARAM = Animator.StringToHash("isMoving");
        IS_ATTACK_ANIM_PARAM = Animator.StringToHash("isAttacking");
    }

    protected void Start()
    {
        animator = GetComponent<Animator>();
    }

    protected void Update()
    {
        MoveUpdate();
        AttackUpdate();
    }

    /// <summary>
    /// 移动至某一点
    /// </summary>
    /// <param name="pos">移动位置</param>
    public void MoveTo(Vector3 pos)
    {
        targetPos = pos;
        isMoving = true;
        animator.SetBool(IS_MOVING_ANIM_PARAM, true);
    }

    /// <summary>
    /// 移动Update
    /// </summary>
    public void MoveUpdate()
    {
        if (!isMoving)
            return;

        Vector3 pos = transform.position;
        transform.position = Vector3.MoveTowards(pos, targetPos, speed * Time.deltaTime);
        transform.LookAt(targetPos);
        if(Vector3.Distance(pos,targetPos)<0.05f)
        {
            isMoving = false;
            animator.SetBool(IS_MOVING_ANIM_PARAM, false);
        }
    }

    //发起攻击
    public void Attack()
    {
        isAttacking = true;
        attackTime = Time.time;
        animator.SetBool(IS_ATTACK_ANIM_PARAM, true);
    }

    public void AttackUpdate()
    {
        if (!isAttacking) 
            return;

        //等待动画播放完
        if (Time.time - attackTime < 0.24f)
            return;
        
        isAttacking = false;
        animator.SetBool(IS_ATTACK_ANIM_PARAM, false);
    }


}
