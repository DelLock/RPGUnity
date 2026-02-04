using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Movement : MonoBehaviour
{
    public float speed;
    public float attackRange = 2;
    public float attackCooldown = 2;
    public float playerDetectRange = 5;
    public Transform detectionPoint;
    public LayerMask playerLayer;
    public float attackAnimationDuration = 0.8f; // Длительность анимации атаки

    private Rigidbody2D rb;
    private Transform player;
    private float attackCooldownTimer;
    private float attackAnimationTimer = 0; // Таймер для анимации атаки
    private bool isInAttackAnimation = false; // Флаг анимации атаки

    private int facingDirection = 1;
    private Animator anim;
    private EnemyState enemyState;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        ChangeState(EnemyState.Idle);
    }

    void Update()
    {
        // Если в анимации атаки - отсчитываем время
        if (isInAttackAnimation)
        {
            attackAnimationTimer -= Time.deltaTime;
            if (attackAnimationTimer <= 0)
            {
                EndAttackAnimation();
            }
        }
        else
        {
            CheckForPlayer();
        }
        
        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
        }
        
        // Двигаемся только если не в анимации атаки
        if (enemyState == EnemyState.Chasing && !isInAttackAnimation)
        {
            Chase();
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    void Chase()
    {
        if (player == null) return;
        
        if ((player.position.x > transform.position.x && facingDirection == -1) || 
            (player.position.x < transform.position.x && facingDirection == 1))
        {
            Flip();
        }
        
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * speed;
    }
    
    void Flip()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    private void CheckForPlayer()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(detectionPoint.position, playerDetectRange, playerLayer);
        
        if (hits.Length > 0)
        {
            player = hits[0].transform;
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            
            // Если игрок в радиусе атаки
            if (distanceToPlayer <= attackRange)
            {
                // Если можно атаковать (нет кулдауна)
                if (attackCooldownTimer <= 0 && !isInAttackAnimation)
                {
                    StartAttack();
                }
                // Если кулдаун еще не закончился и не в анимации
                else if (!isInAttackAnimation)
                {
                    rb.velocity = Vector2.zero;
                    ChangeState(EnemyState.Idle);
                }
            }
            // Если игрок вне радиуса атаки, но в зоне обнаружения
            else if (distanceToPlayer <= playerDetectRange && !isInAttackAnimation)
            {
                ChangeState(EnemyState.Chasing);
            }
        }
        else if (player != null && !isInAttackAnimation) // Игрок вышел из зоны обнаружения
        {
            rb.velocity = Vector2.zero;
            ChangeState(EnemyState.Idle);
            player = null;
        }
    }
    
    void StartAttack()
    {
        attackCooldownTimer = attackCooldown;
        attackAnimationTimer = attackAnimationDuration;
        isInAttackAnimation = true;
        ChangeState(EnemyState.Attacking);
        // НЕ вызываем атаку здесь - она вызовется через Animation Event
    }
    
    void EndAttackAnimation()
    {
        isInAttackAnimation = false;
        attackAnimationTimer = 0;
        
        // Проверяем, что делать после атаки
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            
            if (distanceToPlayer <= attackRange)
            {
                // Игрок все еще в радиусе - ждем следующую атаку
                rb.velocity = Vector2.zero;
                ChangeState(EnemyState.Idle);
            }
            else if (distanceToPlayer <= playerDetectRange)
            {
                // Игрок отошел - начинаем преследование
                ChangeState(EnemyState.Chasing);
            }
            else
            {
                // Игрок ушел из зоны обнаружения
                ChangeState(EnemyState.Idle);
                player = null;
            }
        }
        else
        {
            // Игрок пропал из виду
            ChangeState(EnemyState.Idle);
        }
    }
    
    // Метод для вызова через Animation Event в нужном кадре анимации
    public void PerformAttackDamage()
    {
        // Вызываем атаку из Enemy_Combat только если все еще в анимации
        if (isInAttackAnimation && enemyState == EnemyState.Attacking)
        {
            GetComponent<Enemy_Combat>().Attack();
        }
    }
    
    void ChangeState(EnemyState newState)
    {
        if (isInAttackAnimation && newState != EnemyState.Attacking) 
        {
            return; // Не меняем состояние во время анимации атаки
        }
        
        // Выход из текущей анимации
        if (enemyState == EnemyState.Idle)
            anim.SetBool("isIdle", false);
        else if (enemyState == EnemyState.Chasing)
            anim.SetBool("isChasing", false);
        else if (enemyState == EnemyState.Attacking)
            anim.SetBool("isAttacking", false);
        
        // Обновляем текущее состояние
        enemyState = newState;
        
        // Вход в новую анимацию
        if (enemyState == EnemyState.Idle)
            anim.SetBool("isIdle", true);
        else if (enemyState == EnemyState.Chasing)
            anim.SetBool("isChasing", true);
        else if (enemyState == EnemyState.Attacking)
            anim.SetBool("isAttacking", true);
    }
}

public enum EnemyState
{
    Idle,
    Chasing,
    Attacking
}