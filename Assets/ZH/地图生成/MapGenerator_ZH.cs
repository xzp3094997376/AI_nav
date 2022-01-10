using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapGenerator_ZH : MonoBehaviour
{
    [Header("地图单元生成单位")]
    public GameObject _TilePrefab;

    //[Header("地图单元生成 位置")]
    //private Transform _MapHolder;
    [Header("地图单元大小")]
    [Range(0, 1)]
    public float _OutLinePercent;

    [Header("障碍物预制体")]
    public GameObject _ObsPrefab;
    //[Header("障碍物数量")]
    //public int _ObsCount;

    [Header("存储每个点的位置")]
    public List<Coord> _AllTilesCoords;

    //障碍物 Coord 队列
    private Queue<Coord> _ShuffledQueue;

    //任何随机地图 中心点的不能有任何障碍物 这个点用来生成人物以及 填充算法判定
    private Coord _MapCenter;
    //判断当前瓦片位置是否有障碍物
     bool[,] _MapObstacles;

     public bool[,] mapObstract {
         get { return _MapObstacles; }
     }

     [Header("地图最大尺寸")]
    public Vector2 _MapMaxSize = new Vector2(30, 30);

    [Header("边缘墙体预制体")]
    public GameObject _NavMeshObs;

    [Header("玩家")]
    public GameObject _Player;


    #region 地图生成

    //地图合集
    public Map[] _Maps;
    //地图数量
    public int _MapIndex;
    //当前地图
    private Map _CurrentMap;

    #endregion


    private void Start()
    {
        GeneratorMap();
        Init();
    }

    /// <summary>
    /// 玩家初始化
    /// </summary>
    private void Init()
    {
        //玩家生成位置在地图中心点  
        Instantiate(_Player, new Vector3(-_CurrentMap._MapSize.x / 2 + 0.5f + _CurrentMap._MapCenter._X, 0, -_CurrentMap._MapSize.y / 2 + 0.5f + _CurrentMap._MapCenter._Y), Quaternion.identity);
    }

    /// <summary>
    /// 地图生成
    /// </summary>
    private void GeneratorMap()
    {
        //当前地图
        _CurrentMap = _Maps[_MapIndex];
        //每次开启一个新的地图 初始化 遗弃前一个地图
        _AllTilesCoords = new List<Coord>();

        #region 地图重置

        string _HolderName = "MapHolder";
        if (transform.Find(_HolderName))
        {
            //立即销毁对象obj
            DestroyImmediate(transform.Find(_HolderName).gameObject);
        }
        //重新生成地图单元  生成位置
        Transform _MapHolder = new GameObject(_HolderName).transform;
        //设置人父类
        _MapHolder.parent = transform;

        #endregion

        //地图单元生成
        for (int i = 0; i < _CurrentMap._MapSize.x; i++)
        {
            for (int j = 0; j < _CurrentMap._MapSize.y; j++)
            {
                //以 （0，0）点作为位置生成中心  整体单元 纵横比的一半  加上单元地图网格的一半
                Vector3 _NewPos = new Vector3(-_CurrentMap._MapSize.x / 2 + 0.5f + i, 0, -_CurrentMap._MapSize.y / 2 + 0.5f + j);
                //预制体生成 位置赋予 旋转赋予
                GameObject _SpawnTile = Instantiate(_TilePrefab, _NewPos, Quaternion.Euler(90, 0, 0));
                //设置父类
                _SpawnTile.transform.SetParent(_MapHolder);
                //设置大小
                _SpawnTile.transform.localScale *= _OutLinePercent;
                //把所有位置数据  添加到 Coord 位置数组中
                _AllTilesCoords.Add(new Coord(i, j));
            }
        }

        ////障碍物生成
        //for (int k = 0; k < _ObsCount; k++)
        //{
        //    //随机位置坐标
        //    Coord _RandomCoord = _AllTilesCoord[Random.Range(0, _AllTilesCoord.Count)];

        //    Vector3 _NewPos = new Vector3(-_MapSize.x / 2 + 0.5f + _RandomCoord._X, 0, -_MapSize.y / 2 + 0.5f + _RandomCoord._Y);

        //    //预制体生成 位置赋予 旋转赋予
        //    GameObject _SpawnObs = Instantiate(_ObsPrefab, _NewPos, Quaternion.Euler(90, 0, 0));
        //    //设置父类
        //    _SpawnObs.transform.SetParent(_MapHolder);
        //    //设置大小
        //    _SpawnObs.transform.localScale *= _OutLinePercent;

        //}


        #region 障碍物生成
        _ShuffledQueue = new Queue<Coord>(ShuffleCoords(_AllTilesCoords.ToArray()));

        //障碍物 数量赋值
        int _ObsCount = (int)(_CurrentMap._MapSize.x * _CurrentMap._MapSize.y * _CurrentMap._ObsPercent);
        //判定随机地图中心点位置坐标
        _MapCenter = new Coord((int)_CurrentMap._MapSize.x / 2, (int)_CurrentMap._MapSize.y / 2);
        //初始化
        _MapObstacles = new bool[(int)_CurrentMap._MapSize.x, (int)_CurrentMap._MapSize.y];

        //场景当前障碍物数量
        int _CurrentObsCount = 0;

        for (int k = 0; k < _ObsCount; k++)
        {
            //随机位置坐标
            Coord _RandomCoord = GetRandomCoord();

            //随机到中心点
            _MapObstacles[_RandomCoord._X, _RandomCoord._Y] = true;
            _CurrentObsCount++;


            //判断随机位置是否是中心点位置 以及 是否可通行
            if (_RandomCoord != _MapCenter && MapIsFullAccessible(_MapObstacles, _CurrentObsCount))
            {
                //障碍物随机高度
                float _ObsHeight = Mathf.Lerp(_CurrentMap._MinObsHeight, _CurrentMap._MaxObsHeight, Random.Range(0.0f, 1.0f));

                //位置判定
                Vector3 _NewPos = new Vector3(-_CurrentMap._MapSize.x / 2 + 0.5f + _RandomCoord._X, _ObsHeight / 2, -_CurrentMap._MapSize.y / 2 + 0.5f + _RandomCoord._Y);

                //预制体生成 位置赋予 旋转赋予
                GameObject _SpawnObs = Instantiate(_ObsPrefab, _NewPos, Quaternion.identity);
                //设置父类
                _SpawnObs.transform.SetParent(_MapHolder);
                //设置大小
                _SpawnObs.transform.localScale = new Vector3(_OutLinePercent, _ObsHeight, _OutLinePercent);

                #region 障碍物渐变色

                //获取当前网格数据 渲染
                MeshRenderer _MeshRenderer = _SpawnObs.GetComponent<MeshRenderer>();

                Material _Material = new Material(_MeshRenderer.sharedMaterial);
                //随机坐标Y轴数值 减去 整张地图Y轴数值  就会得到整张地图的障碍物渐变色填充
                float _ColoPercent = _RandomCoord._Y / _CurrentMap._MapSize.y;
                //根据障碍物高度 进行Color 差值运算
                _Material.color = Color.Lerp(_CurrentMap._ForegroundColor, _CurrentMap._BackgroundColor, _ColoPercent);
                //材质 重新赋予
                _MeshRenderer.sharedMaterial = _Material;

                #endregion
            }
            else
            {
                _MapObstacles[_RandomCoord._X, _RandomCoord._Y] = false;
                _CurrentObsCount--;
            }
        }


        #region 动态生成空气墙  上 下 左 右 


        GameObject _NavMeshObsForward = Instantiate(_NavMeshObs, Vector3.forward * (_CurrentMap._MapSize.y + _MapMaxSize.y) / 4, Quaternion.identity);
        _NavMeshObsForward.transform.localScale = new Vector3(_CurrentMap._MapSize.x, 5, (_MapMaxSize.y  - _CurrentMap._MapSize.y )/ 2);

        GameObject _NavMeshObsBack = Instantiate(_NavMeshObs, Vector3.back * (_CurrentMap._MapSize.y + _MapMaxSize.y) / 4, Quaternion.identity);
        _NavMeshObsBack.transform.localScale = new Vector3(_CurrentMap._MapSize.x, 5, (_MapMaxSize.y - _CurrentMap._MapSize.y) / 2);

        GameObject _NavMeshObsLeft = Instantiate(_NavMeshObs, Vector3.left * (_CurrentMap._MapSize.x + _MapMaxSize.x) / 4, Quaternion.identity);
        _NavMeshObsLeft.transform.localScale = new Vector3((_MapMaxSize.x - _CurrentMap._MapSize.y) / 2, 5, _CurrentMap._MapSize.y);

        GameObject _NavMeshObsRight = Instantiate(_NavMeshObs, Vector3.right * (_CurrentMap._MapSize.x + _MapMaxSize.x) / 4, Quaternion.identity);
        _NavMeshObsRight.transform.localScale = new Vector3((_MapMaxSize.x - _CurrentMap._MapSize.x) / 2, 5, _CurrentMap._MapSize.y);

        #endregion
        #endregion
    }

    /// <summary>
    /// 洪水填充算法
    /// 如果返回为False 说明当前障碍物生成会 阻碍地图链接完成行
    /// 如果返回 True 说明当前瓦片可生成障碍物
    /// </summary>
    /// <param 当前瓦片是否有障碍物 ="_MapObstacles"></param>
    /// <param 场景当前障碍物假设数量="_CurrentObsCount"></param>
    /// <returns></returns>
    private bool MapIsFullAccessible(bool[,] _MapObstaclesIs, int _CurrentObsCountIs)
    {
        //当前瓦片是否经历过检查
        bool[,] _MapFlagsBool = new bool[_MapObstaclesIs.GetLength(0), _MapObstaclesIs.GetLength(1)];

        //可行走瓦片 队列
        Queue<Coord> _Queue = new Queue<Coord>();
        //添加中心点瓦片
        _Queue.Enqueue(_MapCenter);

        //标记中心点 为【已检测】
        _MapFlagsBool[_MapCenter._X, _MapCenter._Y] = true;

        //可行走瓦片数量
        int _AccessibleCount = 1;

        //瓦片检测 循环检测 逻辑
        while (_Queue.Count > 0)
        {
            //检测到的 瓦片移除队列
            Coord _CurrentTile = _Queue.Dequeue();

            //检测相邻四周坐标X轴
            for (int X = -1; X <= 1; X++)
            {
                //检测坐标相邻Y轴
                for (int Y = -1; Y <= 1; Y++)
                {
                    //8 瓦片检测  # 代表当前瓦片  * 代表相邻瓦片 % 代表对角线瓦片
                    //   ----------
                    //  | %  *  %  |
                    //  | *  #  *  |
                    //  | %  *  %  |
                    //   ----------
                    int _NeighborX = _CurrentTile._X + X;
                    int _NeighborY = _CurrentTile._Y + Y;

                    // 排除对角线瓦片
                    if (X == 0 || Y == 0)
                    {
                        //边界检测 相邻瓦片要小于地图最大范围
                        if (_NeighborX >= 0 && _NeighborX < _MapObstaclesIs.GetLength(0) && _NeighborY >= 0 && _NeighborY < _MapObstaclesIs.GetLength(1))
                        {
                            //保证相邻点 还没有被检测 
                            if (_MapFlagsBool[_NeighborX, _NeighborY]==false && _MapObstaclesIs[_NeighborX, _NeighborY]==false)
                            {
                                //获取的相邻瓦片是没有被检测过的
                                _MapFlagsBool[_NeighborX, _NeighborY] = true;

                                //添加到可行走瓦片队列当中 
                                _Queue.Enqueue(new Coord(_NeighborX, _NeighborY));

                                //添加可行走瓦片数量
                                _AccessibleCount++;

                            }
                        }
                    }
                }
            }
        }

        //假设可以行走的瓦片数量 
        //总的瓦片数量 减去 障碍物假设目标数量 _CurrentObsCount
        int _ObsTargetCount = (int)(_CurrentMap._MapSize.x * _CurrentMap._MapSize.y - _CurrentObsCountIs);
        //如果可行走瓦片数量 等于 假设瓦片数量  返回True
        return _ObsTargetCount == _AccessibleCount;
    }


    /// <summary>
    /// 获取随机 Coord 元素
    /// </summary>
    /// <returns></returns>
    private Coord GetRandomCoord()
    {
        //移除选择元素  队列：先进先出原则
        Coord _RandomCoord = _ShuffledQueue.Dequeue();
        //重新加入当前队列 保证队列完整性f
        _ShuffledQueue.Enqueue(_RandomCoord);
        //返回随机元素  返回队列第一个元素
        return _RandomCoord;
    }

    /// <summary>
    /// 洗牌算法
    /// 将传递进来的参数：_DataArray进行 重新洗牌，打乱顺序
    /// </summary>
    /// <param 传递的参数="_DataArray"></param>
    /// <returns></returns>
    public static T[] ShuffleCoords<T>(T[] _DataArray)
    {
        for (int i = 0; i < _DataArray.Length; i++)
        {
            int _RandomNum = Random.Range(i, _DataArray.Length);

            //  SWAP思路 :AB 互换
            T _Temp = _DataArray[_RandomNum];
            _DataArray[_RandomNum] = _DataArray[i];
            _DataArray[i] = _Temp;

        }
        return _DataArray;
    }
}

/// <summary>
/// 障碍物 结构体
/// 序列化
/// </summary>
[Serializable]
public struct Coord
{
    public int _X;
    public int _Y;

    /// <summary>
    /// 构造函数
    /// </summary>
    public Coord(int _CoordX, int _CoordY)
    {
        _X = _CoordX;
        _Y = _CoordY;
    }
     
    public static bool operator !=(Coord _Coord1, Coord _Coord2)
    {
        return !(_Coord1==_Coord2);
    }

    public static bool operator ==(Coord _Coord1, Coord _Coord2)
    {
        return (_Coord1._X == _Coord2._X) && (_Coord1._Y == _Coord2._Y);
    }
}

/// <summary>
/// 地图 基础
/// </summary>
[Serializable]
public class Map
{
    [Header("地图大小")]
    public Vector2 _MapSize;

    [Header("地图开放程度")]
    [Range(0, 1)]
    public float _ObsPercent;

    [Header("地图种子")]
    public int _Seed;
    
    [Header("最小高度&最大高度")]
    public float _MinObsHeight, _MaxObsHeight;
    
    [Header("障碍物前景色 后景色")]
    public Color _ForegroundColor, _BackgroundColor;

    public Coord _MapCenter
    {
        get
        {
            return new Coord((int)(_MapSize.x / 2), (int)(_MapSize.y / 2));
        }
    }
}
