using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Sử dụng TextMeshPro cho Text

public class UIController : MonoBehaviour
{
    [Header("End Game UI")]
    [SerializeField] private GameObject endGameOverlayPanel; 
    [SerializeField] private TextMeshProUGUI headerText;     

    // --- THÊM PHẦN NÀY ---
    [Header("In-Game Tower UI")]
    [SerializeField] private GameObject towerActionPanel; // Panel hiện ra khi click vào tháp
    [SerializeField] private TextMeshProUGUI sellAmountText; // Chữ hiển thị "+150 vàng"
    // ---------------------

    private void Awake()
    {
        if (endGameOverlayPanel != null) endGameOverlayPanel.SetActive(false);
        if (towerActionPanel != null) towerActionPanel.SetActive(false); // Ẩn lúc mới vào
    }

    // --- API TOWER ACTION ---
    public void ShowTowerAction(int sellAmount)
    {
        towerActionPanel.SetActive(true);
        
        if (sellAmountText != null) sellAmountText.text = $"Sell (+{sellAmount})";
    }

    public void HideTowerAction()
    {
        towerActionPanel.SetActive(false);
    }

    // Gắn hàm này vào sự kiện OnClick() của nút Bán trên Unity
    public void OnSellButtonClicked()
    {
        GameController.Instance.ExecuteSellSelectedTower();
    }

    // --- API ĐƯỢC GỌI BỞI GAME CONTROLLER ---
    public void ShowVictory()
    {
        endGameOverlayPanel.SetActive(true);
        headerText.text = "VICTORY!";
        headerText.color = ColorUtility.TryParseHtmlString("#FFD700", out Color gold) ? gold : Color.gold;
        Time.timeScale = 0f; // Đóng băng game
    }

    public void ShowDefeat()
    {
        endGameOverlayPanel.SetActive(true);
        headerText.text = "GAME OVER";
        headerText.color = ColorUtility.TryParseHtmlString("#E63946", out Color red) ? red : Color.red;
        Time.timeScale = 0f; // Đóng băng game
    }

    // --- SỰ KIỆN NÚT BẤM (Gắn vào OnClick của Button) ---
    public void OnRestartButtonClicked()
    {
        Time.timeScale = 1f; // Phải mở khóa thời gian trước khi load lại Scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnQuitButtonClicked()
    {
        Time.timeScale = 1f;

        // Thoát game khi đã Build ra file
        Application.Quit();

        // Code này giúp nút Quit hoạt động ngay cả khi đang chạy test trong Unity Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}