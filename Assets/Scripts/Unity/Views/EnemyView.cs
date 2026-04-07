using System.Collections;
using System.Linq;
using Domain.Entities;
using Domain.Enums;
using Domain.Services;
using Domain.ValueObject;
using UnityEngine;
using UnityEngine.UI;

public class EnemyView : MonoBehaviour
{
    public Enemy Enemy { get; private set; }
    public float Health => Enemy.CurrentHealth;
    private EnemyService _enemyService;
    private LayoutAdapter _layoutAdapter;

    [Header("Movement Settings")]
    public float turnSpeed = 120f;

    [Header("Animation Sync")]
    [SerializeField] private Animator animator;
    [Tooltip("Độ dài sải bước thực tế của model (mét).")]
    [SerializeField] private float stepDistance = 1.0f;

    [SerializeField] private Transform headPos;
    [SerializeField] private Transform healthBar;
    [SerializeField] private Image healthFillImage;

    private bool _isKnockingBack = false;

    [SerializeField] private FloatingHealthBar floatingHealthBar;
    private GameObject _prefab;

    public void Initialize(Enemy enemy, LayoutAdapter layout, GameObject prefab)
    {
        Enemy = enemy;
        Enemy.KnockBack += OnKnockBack;
        Enemy.OnHealthChanged += floatingHealthBar.UpdateHealth;
        _enemyService = GameController.Instance.EnemyService;
        _layoutAdapter = layout;
        _prefab = prefab;
        healthFillImage.fillAmount = 1f;
        healthBar.gameObject.SetActive(false);
        _isKnockingBack = false;
        transform.position = _layoutAdapter.HexToWorld(Enemy.CurrentTile);
        transform.LookAt(_layoutAdapter.HexToWorld(Enemy.NextTile));

        ApplyAnimation();
    }

    private void ApplyAnimation()
    {
        if (animator == null || Enemy == null) return;

        if (Enemy.IsStunned || Enemy.IsDead)
        {
            animator.SetFloat("Speed", 0);
            return;
        }

        float currentMoveSpeed = Enemy.CurrentSpeed; 
        float scaleFactor = transform.localScale.x;
        float animSpeed = (currentMoveSpeed / stepDistance) * scaleFactor * Enemy.SpeedModifier;

        animator.SetFloat("Speed", animSpeed);
    }

    private void OnKnockBack()
    {
        if (_isKnockingBack) return;
        _isKnockingBack = true;

        Vector3 targetPos = _layoutAdapter.HexToWorld(Enemy.CurrentTile);
        targetPos.y = transform.position.y;

        StartCoroutine(SimulateProjectileRoutine(targetPos));
    }

    private IEnumerator SimulateProjectileRoutine(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;
        float dist = Vector3.Distance(startPos, targetPos);

        float jumpDuration = Mathf.Clamp(dist / 5f, 0.4f, 0.7f);
        float jumpHeight = Mathf.Clamp(dist * 0.5f, 0.5f, 1.5f);

        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / jumpDuration);

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
            currentPos.y += Mathf.Sin(t * Mathf.PI) * jumpHeight;

            transform.position = currentPos;
            yield return null;
        }

        transform.position = targetPos;
        _isKnockingBack = false;
    }

    void Update()
    {
        if (Enemy == null) return;

        if (Enemy.IsDead)
        {
            _enemyService.EnemyDie(Enemy);
            ReturnToPool();
            return;
        }
        else
        {
            Enemy.Position = new Position(transform.position.x, transform.position.z);
            Enemy.HeadInNextTile = HeadInNextTile();
        }
        ApplyAnimation();

        if (_isKnockingBack || Enemy.IsStunned)
            return;
        
        Move();
    }

    public void Move()
    {
        if (Enemy.NextTile == null) return;

        var targetPos = _layoutAdapter.HexToWorld(Enemy.NextTile);
        targetPos.y = transform.position.y;
        
        transform.position = Vector3.MoveTowards(transform.position, targetPos, Enemy.CurrentSpeed * Time.deltaTime);
        
        Vector3 direction = targetPos - transform.position;
        if (direction != Vector3.zero)
        {
            direction.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
            Enemy.MoveNexTile();
            if (Enemy.CurrentTile.State == HexState.Target)
            {
                _enemyService.EnemyDie(Enemy);
                ReturnToPool();
                return;
            }
        }
    }

    public bool HeadInNextTile()
    {
        if (headPos == null || Enemy.NextTile == null) return false;
        var tileCheck = _layoutAdapter.WorldToHexCoords(headPos.position);
        return tileCheck.q == Enemy.NextTile.Q && tileCheck.r == Enemy.NextTile.R;
    }

    private void ReturnToPool()
    {
        if (GameController.Instance != null && GameController.Instance.EnemyRegistry != null)
        {
            if (Enemy != null)
                GameController.Instance.EnemyRegistry.Remove(Enemy);
        }
        Enemy = null;
        GameController.Instance.PoolManager.ReturnPool(_prefab, gameObject);
    }

    private void OnDisable()
    {
        if (Enemy != null)
        {
            Enemy.KnockBack -= OnKnockBack;
            Enemy.OnHealthChanged -= floatingHealthBar.UpdateHealth;
        }
    }
}