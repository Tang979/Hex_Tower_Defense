using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class ResourceManager
{
    private static Dictionary<string, EnemyOS> _cachedEnemies = new Dictionary<string, EnemyOS>();
    private static Dictionary<string, TowerOS> _cachedTowers = new Dictionary<string, TowerOS>();
    private static Dictionary<string, GameObject> _cachedVisuals = new Dictionary<string, GameObject>();

    public static async Task LoadEnemyAsync(string enemyId)
    {
        if (_cachedEnemies.ContainsKey(enemyId))
            return;
        
        AsyncOperationHandle<EnemyOS> handle = Addressables.LoadAssetAsync<EnemyOS>(enemyId);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _cachedEnemies[enemyId] = handle.Result;
        }
        else
        {
            Debug.LogError($"[ResourceManager] Lỗi: Không tìm thấy Addressable Enemy nào có ID: {enemyId}");
        }
    }

    public static EnemyOS GetEnemyOS(string enemyId)
    {
        if (_cachedEnemies.TryGetValue(enemyId, out EnemyOS enemyOS))
        {
            return enemyOS;
        }
        Debug.LogError($"[ResourceManager] Lỗi: EnemyOS với ID {enemyId} chưa được tải hoặc không tồn tại.");
        return null;
    }

    public static async Task LoadTowerAsync(string towerId)
    {
        if (_cachedTowers.ContainsKey(towerId))
            return;
        
        AsyncOperationHandle<TowerOS> handle = Addressables.LoadAssetAsync<TowerOS>(towerId);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _cachedTowers[towerId] = handle.Result;
        }
        else
        {
            Debug.LogError($"[ResourceManager] Lỗi: Không tìm thấy Addressable Tower nào có ID: {towerId}");
        }
    }

    public static TowerOS GetTowerOS(string towerId)
    {
        if (_cachedTowers.TryGetValue(towerId, out TowerOS towerOS))
        {
            return towerOS;
        }
        Debug.LogError($"[ResourceManager] Lỗi: TowerOS với ID {towerId} chưa được tải hoặc không tồn tại.");
        return null;
    }

    public static async Task LoadVisualAsync(string visualId)
    {
        if (_cachedVisuals.ContainsKey(visualId))
            return;

        try
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(visualId);
            GameObject visual = await handle.Task;
            if (visual != null)
            {
                _cachedVisuals[visualId] = visual;
                Debug.Log($"[ResourceManager] Đã tải trước Visual: {visualId}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ResourceManager] Lỗi tải Visual '{visualId}': {e.Message}");
        }
    }

    public static GameObject GetVisual(string visualId)
    {
        if (_cachedVisuals.TryGetValue(visualId, out GameObject visual))
        {
            return visual;
        }
        
        var handle = Addressables.LoadAssetAsync<GameObject>(visualId);
        GameObject result = handle.WaitForCompletion();
        
        if (result != null) 
        {
            _cachedVisuals[visualId] = result;
            return result;
        }
        
        Debug.LogError($"[ResourceManager] Lỗi: Visual với ID {visualId} không tồn tại.");
        return null;
    }
}