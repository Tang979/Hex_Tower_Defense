using UnityEngine;
using System.Collections.Generic;
using Domain.Entities;
using Domain.Services;
using Domain.Core.Data;
using Domain.Services.Pathfinding;

public class GameController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private TextAsset mapJson;
    [SerializeField] private IPathfinder pathfinder;
    [SerializeField] private TextAsset towerStatsJson;
    private Dictionary<string, TowerStatData> _towerStatsDict = new Dictionary<string, TowerStatData>();

    [Header("Components")]
    [SerializeField] private LayoutAdapter layoutAdapter; // Class xử lý toán học
    public EnemyView enemy;

    [Header("References")]
    [SerializeField] private HexMeshView mapView;
    [SerializeField] private HexOutlineView outlineView;
    [SerializeField] private CameraController cameraController;
    public CrystalButton[] towerButtons; // Kéo 3-4 nút UI trống vào đây
    public List<TowerOS> teamLoadout;    // Danh sách tháp mang theo màn này

    // Controller con cần GameController
    [Header("Controllers")]
    [SerializeField] private CrystalPlacementController placementController;
    // [SerializeField] private WaveManager waveManager;

    // Infrastructure
    private InputService inputService;

    // --- DOMAIN OBJECTS ---
    public HexMap Map { get; private set; }
    public EnemyService EnemyService { get; private set; }
    public LaneService LaneService { get; private set; }
    public PlacementService PlacementService { get; private set; }

    // Lưu lại LanesData để dùng cho việc tính đường
    private List<LaneData> _lanesData;

    private void Awake()
    {
        inputService = new InputService();
        if (towerStatsJson != null)
        {
            var db = Newtonsoft.Json.JsonConvert.DeserializeObject<TowerDatabase>(towerStatsJson.text);
            foreach (var t in db.Towers)
            {
                _towerStatsDict[t.Id] = t;
            }
        }

        layoutAdapter.Initialize();
        cameraController.Initialize(inputService);
        placementController.Initialize(inputService, _towerStatsDict, teamLoadout);

        pathfinder = new AStarPathfinder();
        Map = MapLoader.LoadMap(mapJson);
        LaneService = new LaneService(Map, pathfinder);
        LaneService.RecalculateAllPaths();
        EnemyService = new EnemyService(Map, LaneService);
        enemy.Initialize(new Enemy(), EnemyService, layoutAdapter);

        PlacementService = new PlacementService(Map, pathfinder, LaneService);
        outlineView.Initialize(Map, LaneService, layoutAdapter);

        InitializeUI();
    }

    private void Start()
    {
        mapView.RenderMap(Map, layoutAdapter);
        outlineView.BuildOutlines();
        PlacementService.OnMapChanged += EnemyService.HandleOnMapChanged;
    }

    // --- API PUBLIC ---

    // Hàm này được gọi khi PlacementController đặt tháp thành công
    public void OnMapChanged()
    {
        outlineView.BuildOutlines();
    }
    private void InitializeUI()
    {
        for (int i = 0; i < teamLoadout.Count; i++)
        {
            if (i < towerButtons.Length)
            {
                // Bơm dữ liệu TowerOS vào cho Nút
                towerButtons[i].SetupButton(teamLoadout[i], placementController);
                towerButtons[i].gameObject.SetActive(true);
            }
        }
    }
}