using UnityEngine;

// Dùng Generic <T> để class này có thể đại diện cho bất kỳ script nào kế thừa từ nó
public class SceneSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                // Dùng FindFirstObjectByType thay cho FindObjectOfType (tối ưu hơn trong Unity đời mới)
                _instance = FindFirstObjectByType<T>();

                if (_instance == null)
                {
                    Debug.LogWarning($"[SceneSingleton] Không tìm thấy GameObject nào chứa script {typeof(T).Name} trong Scene!");
                }
            }
            return _instance;
        }
    }

    // Dùng protected virtual để class con có thể ghi đè (override) và thêm logic
    protected virtual void Awake()
    {
        // 1. Kiểm tra trùng lặp: Nếu đã có một bản thể khác tồn tại -> Hủy bản thể mới này
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"[SceneSingleton] Phát hiện nhiều bản thể của {typeof(T).Name}. Đang hủy bản thể mới...");
            Destroy(gameObject);
            return;
        }

        // 2. Nếu là duy nhất, tự gán bản thân làm Instance
        _instance = this as T;

        // KHÔNG CÓ DontDestroyOnLoad ở đây, để nó tự hủy khi đổi Scene.
    }

    protected virtual void OnDestroy()
    {
        // 3. Dọn dẹp: Khi Scene đóng lại, xóa Instance để dọn đường cho Scene tiếp theo
        if (_instance == this)
        {
            _instance = null;
        }
    }
}