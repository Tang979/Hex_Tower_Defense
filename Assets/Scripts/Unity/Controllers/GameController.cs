using UnityEngine;
using System.Collections.Generic;
using Domain.Entities;
using Domain.Services;
using Domain.Core.Data;
using Domain.Services.Pathfinding;
using TMPro;

public class GameController : SceneSingleton<GameController>
{
    [Header("Configuration")]
    [SerializeField] private TextAsset mapJson;
    [SerializeField] private TextAsset playerDataJson;
    [SerializeField] private TextAsset towerStatsJson;
    [SerializeField] private TextAsset enemyStatsJson;
    [SerializeField] private TextAsset mergeRecipesJson;

    private IPathfinder pathfinder;
    private Dictionary<string, TowerStatData> _towerStatsDict = new Dictionary<string, TowerStatData>();
    private Dictionary<string, EnemyStatData> _enemyStatsDict = new Dictionary<string, EnemyStatData>();

    // [THÊM MỚI] Lưu trữ các công thức hợp lệ để sau này các Controller khác có thể dùng để kiểm tra Kéo-Thả
    public List<MergeRecipeData> ValidMergeRecipes { get; private set; } = new List<MergeRecipeData>();

    public Dictionary<HexTile, TowerView> ActiveTowers { get; private set; } = new Dictionary<HexTile, TowerView>();

    [Header("Components")]
    [SerializeField] private LayoutAdapter layoutAdapter;
    [SerializeField] private PoolManager poolManager;
    public Dictionary<Enemy, EnemyView> EnemyRegistry = new Dictionary<Enemy, EnemyView>();

    [Header("UI Controllers")]
    [SerializeField] private UIController endGameUI;

    [Header("References")]
    [SerializeField] private HexMeshView mapView;
    [SerializeField] private HexOutlineView outlineView;
    [SerializeField] private CameraController cameraController;
    public CameraController CameraController => cameraController; // Thêm property này
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private TextMeshProUGUI baseHealthText;
    [SerializeField] private CrystalButton[] towerButtons;
    private List<TowerOS> teamLoadout = new List<TowerOS>(5);

    // Controller con cần GameController
    [Header("Controllers")]
    [SerializeField] private CrystalPlacementController placementController;
    [SerializeField] private WaveController waveController;

    // Infrastructure
    private InputService inputService;
    private TowerView _selectedTowerView;
    private int _selectedTowerRefund;

    // Nút nhấn và Kéo thả (Map-to-Map)
    private bool _isPointerPressed = false;
    private float _pointerPressTime = 0f;
    private TowerView _pointerDownTower;
    private bool _isLongPressTriggered = false;

    // --- BIẾN TRẠNG THÁI THẮNG / THUA ---
    private bool _isGameOver = false;
    private bool _isAllWavesSpawned = false;

    // --- DOMAIN OBJECTS ---
    public HexMap Map { get; private set; }
    public CurrencyService CurrencyService { get; private set; }
    public BaseHealthService BaseHealthService { get; private set; }
    public EnemyService EnemyService { get; private set; }
    public LaneService LaneService { get; private set; }
    public PlacementService PlacementService { get; private set; }
    public List<LaneData> Lanes { get; private set; }

    public PoolManager PoolManager => poolManager;

    protected override async void Awake()
    {
        base.Awake();

        inputService = new InputService();

        // 1. TẢI THÁP MANG VÀO TRẬN
        if (playerDataJson != null)
        {
            var playerData = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerData>(playerDataJson.text);

            foreach (var towerId in playerData.SelectedTowerIds)
            {
                await ResourceManager.LoadTowerAsync(towerId);

                var towerOS = ResourceManager.GetTowerOS(towerId);
                if (towerOS != null)
                {
                    teamLoadout.Add(towerOS);
                }
            }
        }

        if (mergeRecipesJson != null)
        {
            List<MergeRecipeData> allRecipes = null;

            var wrapper = Newtonsoft.Json.JsonConvert.DeserializeObject<MergeRecipeDatabase>(mergeRecipesJson.text);
            if (wrapper != null && wrapper.Recipes != null)
            {
                allRecipes = wrapper.Recipes;
            }
            else
            {
                allRecipes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MergeRecipeData>>(mergeRecipesJson.text);
            }

            if (allRecipes == null)
            {
                Debug.LogError("Không thể giải mã mergeRecipesJson: định dạng JSON không hợp lệ hoặc không chứa dữ liệu Recipes.");
                allRecipes = new List<MergeRecipeData>();
            }

            foreach (var recipe in allRecipes)
            {
                // Kiểm tra xem người chơi có mang theo tháp nguyên liệu A và B không
                bool hasComponentA = teamLoadout.Exists(t => t.Id == recipe.ComponentA_Id);
                bool hasComponentB = teamLoadout.Exists(t => t.Id == recipe.ComponentB_Id);

                // Điều kiện hợp lệ: Mang đủ nguyên liệu (Nếu A giống B thì chỉ cần check A)
                bool canMerge = (recipe.ComponentA_Id == recipe.ComponentB_Id) ? hasComponentA : (hasComponentA && hasComponentB);

                if (canMerge)
                {
                    ValidMergeRecipes.Add(recipe); // Lưu lại để logic Kéo-Thả tra cứu sau này

                    // Tải model 3D của Tháp Kết quả
                    await ResourceManager.LoadTowerAsync(recipe.ResultTower_Id);
                    var resultTowerOS = ResourceManager.GetTowerOS(recipe.ResultTower_Id);
                }
            }
        }
        // ==============================================================

        if (towerStatsJson != null)
        {
            var towerDb = Newtonsoft.Json.JsonConvert.DeserializeObject<TowerDatabase>(towerStatsJson.text);
            foreach (var t in towerDb.Towers)
            {
                _towerStatsDict[t.Id] = t;

                await ResourceManager.LoadTowerAsync(t.Id);
                var towerOS = ResourceManager.GetTowerOS(t.Id);
                if (towerOS != null)
                {
                    if (towerOS.towerPrefab != null) GameController.Instance.PoolManager.Prewarm(towerOS.towerPrefab, 3);
                    if (towerOS.bulletPrefab != null) GameController.Instance.PoolManager.Prewarm(towerOS.bulletPrefab, 3);
                }

                if (t.Modifiers != null)
                {
                    foreach (var mod in t.Modifiers)
                    {
                        if (mod.StringArgs != null)
                        {
                            foreach (var kvp in mod.StringArgs)
                            {
                                if (kvp.Key.StartsWith("Visual"))
                                {
                                    await ResourceManager.LoadVisualAsync(kvp.Value);
                                    GameObject visualObj = ResourceManager.GetVisual(kvp.Value);
                                    if (visualObj != null)
                                    {
                                        GameController.Instance.PoolManager.Prewarm(visualObj, 3);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if (enemyStatsJson != null)
        {
            var enemyDb = Newtonsoft.Json.JsonConvert.DeserializeObject<EnemyDatabase>(enemyStatsJson.text);
            foreach (var e in enemyDb.Enemies)
            {
                _enemyStatsDict[e.Id] = e;
            }
        }

        if (mapJson != null)
        {
            var mapData = Newtonsoft.Json.JsonConvert.DeserializeObject<HexMapDefinition>(mapJson.text);
            CurrencyService = new CurrencyService(mapData.StartCurrency);
            BaseHealthService = new BaseHealthService(mapData.BaseHealth);
            Lanes = mapData.Lanes;
        }

        foreach (var lane in Lanes)
        {
            foreach (var wave in lane.Waves)
            {
                if (wave.SpawnList == null) continue;
                foreach (var enemy in wave.SpawnList)
                {
                    await ResourceManager.LoadEnemyAsync(enemy.EnemyId);
                }
            }
        }

        layoutAdapter.Initialize();
        cameraController.Initialize(inputService);
        placementController.Initialize(inputService, _towerStatsDict, teamLoadout);

        pathfinder = new AStarPathfinder();
        Map = MapLoader.LoadMap(mapJson);
        LaneService = new LaneService(Map, pathfinder);
        LaneService.RecalculateAllPaths();
        EnemyService = new EnemyService(Map, LaneService, BaseHealthService, CurrencyService);

        PlacementService = new PlacementService(Map, pathfinder, LaneService);
        outlineView.Initialize(Map, LaneService, layoutAdapter);

        InitializeUI();
        FinishSetupAndStartGame();
    }

    private void FinishSetupAndStartGame()
    {
        mapView.RenderMap(Map, layoutAdapter);
        outlineView.BuildOutlines();
        PlacementService.OnMapChanged += EnemyService.HandleOnMapChanged;

        inputService.OnPressStarted += HandlePressStarted;
        inputService.OnPressCanceled += HandlePressCanceled;

        CurrencyService.OnCurrencyChanged += OnCurrencyChanged;
        currencyText.text = CurrencyService.CurrentCurrency.ToString();

        BaseHealthService.OnHealthChanged += OnHealthChanged;
        baseHealthText.text = BaseHealthService.CurrentHealth.ToString();

        waveController.OnWaveChanged += HandleWaveChanged;

        // --- ĐĂNG KÝ SỰ KIỆN ĐIỀU KIỆN THẮNG / THUA ---
        BaseHealthService.OnBaseDestroyed += HandleDefeat;
        waveController.OnAllWavesSpawned += () => _isAllWavesSpawned = true;
        _isGameOver = false;

        waveController.Initialize(Lanes, _enemyStatsDict);
    }

    void Update()
    {
        // Chỉ chạy logic game nếu chưa Game Over
        if (EnemyService != null && !_isGameOver)
        {
            EnemyService.UpdateEnemies(Time.deltaTime);

            if (_isAllWavesSpawned && EnemyService.ActiveEnemies.Count == 0)
            {
                HandleVictory();
            }
        }

        // Long press check
        if (_isPointerPressed && _pointerDownTower != null && !_isLongPressTriggered)
        {
            if (Time.unscaledTime - _pointerPressTime >= 1.0f)
            {
                _isLongPressTriggered = true;
                DeselectTower();
                placementController.StartMapDragging(_pointerDownTower);
            }
        }
    }

    // --- API PUBLIC ---
    public void OnMapChanged()
    {
        outlineView.BuildOutlines();
    }

    private void HandleWaveChanged(int currentWave, int totalWaves)
    {
        if (waveText != null)
        {
            waveText.text = $"Wave: {currentWave} / {totalWaves}";
        }
    }

    private void HandleDefeat()
    {
        if (_isGameOver) return;
        _isGameOver = true;

        if (endGameUI != null) endGameUI.ShowDefeat();
    }

    private void HandleVictory()
    {
        if (_isGameOver) return;
        _isGameOver = true;

        if (endGameUI != null) endGameUI.ShowVictory();
    }

    private void HandlePressStarted()
    {
        if (_isGameOver || Time.timeScale == 0f || inputService.IsPointerOverUI()) return;

        _isPointerPressed = true;
        _pointerPressTime = Time.unscaledTime;
        _isLongPressTriggered = false;

        Ray ray = Camera.main.ScreenPointToRay(inputService.GetPointerPosition());
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            TowerView tower = hit.collider.GetComponent<TowerView>();
            if (tower != null)
            {
                _pointerDownTower = tower;
                _selectedTowerView = tower;
            }
            else
            {
                _pointerDownTower = null;
            }
        }
        else
        {
            _pointerDownTower = null;
        }
    }

    private void HandlePressCanceled()
    {
        if (!_isPointerPressed) return;
        
        _isPointerPressed = false;

        if (!_isLongPressTriggered)
        {
            if (_pointerDownTower != null)
            {
                SelectTowerLocally(_pointerDownTower);
            }
            else
            {
                DeselectTower();
            }
        }

        if (_isLongPressTriggered)
        {
            placementController.EndDragging();
        }

        _pointerDownTower = null;
        _isLongPressTriggered = false;

        if (placementController.IsMapDragging)
        {
            placementController.EndMapDraggingCancel();
        }
    }

    private void SelectTowerLocally(TowerView tower)
    {
        _selectedTowerView = tower;

        // Tính toán tiền hoàn lại (Ví dụ: Bán lỗ, hoàn 50% vốn)
        int cost = _towerStatsDict[tower.Tower.Id].Cost;
        _selectedTowerRefund = Mathf.FloorToInt(cost * 0.5f);

        // Hiện UI
        if (endGameUI != null) endGameUI.ShowTowerAction(_selectedTowerRefund);
    }

    public void DeselectTower()
    {
        _selectedTowerView = null;
        if (endGameUI != null) endGameUI.HideTowerAction();
    }

    public void ExecuteSellSelectedTower()
    {
        if (_selectedTowerView != null)
        {
            CurrencyService.AddCurrency(_selectedTowerRefund);
            DestroyTowerViewSilently(_selectedTowerView);
            DeselectTower();
        }
    }

    public void RegisterTower(HexTile tile, TowerView towerView)
    {
        ActiveTowers[tile] = towerView;
    }

    public void UnregisterTower(HexTile tile)
    {
        if (ActiveTowers.ContainsKey(tile))
        {
            ActiveTowers.Remove(tile);
        }
    }

    public TowerView GetTowerAt(HexTile tile)
    {
        if (tile != null && ActiveTowers.TryGetValue(tile, out TowerView tv))
            return tv;
        return null;
    }

    public void DestroyTowerViewSilently(TowerView towerView)
    {
        if (towerView == null || towerView.CurrentTile == null) return;

        HexTile tile = towerView.CurrentTile;
        PlacementService.RemoveCrystal(tile);
        UnregisterTower(tile);

        if (towerView.OriginalPrefab != null)
        {
            PoolManager.ReturnPool(towerView.OriginalPrefab, towerView.gameObject);
        }
        else
        {
            Destroy(towerView.gameObject);
        }
        OnMapChanged();
    }

    public void OnCurrencyChanged(int newCurrency)
    {
        if (currencyText != null) currencyText.text = newCurrency.ToString();
    }

    public void OnHealthChanged(int newHealth)
    {
        if (baseHealthText != null) baseHealthText.text = newHealth.ToString();
    }

    private void InitializeUI()
    {
        for (int i = 0; i < teamLoadout.Count; i++)
        {
            if (i < towerButtons.Length)
            {
                string towerId = teamLoadout[i].Id;
                int cost = 0;

                // Lấy thông số giá tiền từ JSON Dictionary
                if (_towerStatsDict.TryGetValue(towerId, out TowerStatData stats))
                {
                    cost = stats.Cost;
                }

                // Truyền thêm 'cost' vào hàm SetupButton
                towerButtons[i].SetupButton(teamLoadout[i], cost, placementController);
                towerButtons[i].gameObject.SetActive(true);
            }
        }
    }
}