using System.Collections;
using Domain.Entities;
using Domain.Enums;
using Domain.Services;
using UnityEngine;

public class EnemyView : MonoBehaviour
{
    private Enemy _enemy;
    private EnemyService _enemyService;
    private LayoutAdapter _layoutAdapter;

    public float turnSpeed = 120f;
    [SerializeField] private Transform headPos;
    
    private bool _isKnockingBack = false; // Cờ chặn di chuyển bình thường

    public void Initialize(Enemy enemy, EnemyService enemyService, LayoutAdapter layout)
    {
        _enemy = enemy;
        _enemyService = enemyService;
        _layoutAdapter = layout;
        _enemy.KnockBack += OnKnockBack;
    }

    // --- LOGIC KNOCKBACK MỚI ---
    private void OnKnockBack()
    {
        // Nếu đang bay rồi thì thôi
        if (_isKnockingBack) return;

        // 1. Xác định đích đến (Tâm của ô hiện tại - CurrentTile)
        Vector3 targetPos = _layoutAdapter.HexToWorld(_enemy.CurrentTile);
        // Giữ độ cao Y bằng độ cao hiện tại của Enemy (để tính toán trên mặt phẳng ngang)
        targetPos.y = transform.position.y;

        // 2. Bắt đầu quy trình bay giả lập vật lý
        StartCoroutine(SimulateProjectileRoutine(targetPos));
    }

    private IEnumerator SimulateProjectileRoutine(Vector3 targetPos)
    {

        Vector3 startPos = transform.position;
        targetPos.y = transform.position.y;
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

    void Start()
    {
        _enemyService.SpawnEnemy(_enemy, 0);
        transform.position = _layoutAdapter.HexToWorld(_enemy.CurrentTile);
    }

    void Update()
    {
        _enemyService.UpdateEnemies(Time.deltaTime);

        // CHỈNH SỬA: Nếu đang bị Knockback thì KHÔNG chạy logic di chuyển thường
        if (_isKnockingBack || _enemy.IsStunned)
            return;
        else
            Move();

        if (HeadInNextTile())
            _enemy.HeadInNextTile = true;
        else
            _enemy.HeadInNextTile = false;
    }

    void OnDestroy()
    {
        _enemy.KnockBack -= OnKnockBack;
    }

    public void Move()
    {

        if (_enemy.NextTile == null)
        {
            Debug.Log("Enemy has reached the end of the path.");
            return;
        }
        var targetPos = _layoutAdapter.HexToWorld(_enemy.NextTile);
        targetPos.y = transform.position.y;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, _enemy.CurrentSpeed * Time.deltaTime);
        Vector3 direction = targetPos - transform.position;
        if (direction != Vector3.zero)
        {
            direction.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
            if (_enemy.NextTile.State == HexState.Target)
            {
                _enemyService.EnemyDie(_enemy);
                _enemy.ResetEnemy();
                _enemyService.SpawnEnemy(_enemy, 0);
                transform.position = _layoutAdapter.HexToWorld(_enemy.CurrentTile);
                transform.LookAt(_layoutAdapter.HexToWorld(_enemy.NextTile));
                return;
            }
            _enemy.MoveNexTile();
        }
    }

    public bool HeadInNextTile()
    {
        var tileCheck = _layoutAdapter.WorldToHexCoords(headPos.position);
        if (tileCheck.q < 0 || tileCheck.r < 0)
            return false;
        if (tileCheck.q == _enemy.NextTile.Q && tileCheck.r == _enemy.NextTile.R)
            return true;
        return false;
    }
}