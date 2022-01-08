using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.AI;

public class Test : MonoBehaviour
{
    public MapGenerator_ZH _mapGeneratorZh;

    private AStar aStar;

    [Header("是否可以斜着走")]
    public bool isGoDiagonally;

    public List<Grid> gridListDes=new List<Grid>();
    // Start is called before the first frame update
    IEnumerator StartAstar()
    {
       

        aStar=new AStar(); 
        gridListDes.Clear();
        InitMap();

        isGoDiagonally = aStar.isGoDiagonally;

        //return;
        Int2 maxSize= new Int2((int)_mapGeneratorZh._MapMaxSize.x,(int)_mapGeneratorZh._MapMaxSize.y);
        Int2 player = new Int2(Mathf.RoundToInt(_mapGeneratorZh._MapMaxSize.x/2f/*+0.5f*/),Mathf.CeilToInt(_mapGeneratorZh._MapMaxSize.y/2f/*+0.5f*/));
        Debug.Log(player.ToString());
        Debug.Log("总共出口数 ："+gridListDes.Count);
        for (int i = 0; i < gridListDes.Count; i++)
        {
            Int2 destination = gridListDes[i].position;
            aStar.Init(maxSize, player, destination, EvaluationFunctionType.Diagonal);
            aStar.m_map = xmap.grid;
            yield return StartCoroutine(aStar.Start());
        }

        yield return null;
        PathShow();
        
    }

    private XMap xmap;
    private Grid leftDown, leftUp, RightDown, rightUp;
    void InitMap()
    {
        int x_arr = _mapGeneratorZh.mapObstract.GetLength(0);
        int y_arr= _mapGeneratorZh.mapObstract.GetLength(1); 
        Debug.Log(x_arr);
        Debug.Log(y_arr);
        xmap =new XMap();
        xmap.grid =new Grid[x_arr,y_arr];

        for (int i = 0; i < x_arr; i++)
        {
            for (int j = 0; j < y_arr; j++)
            {
                Grid grid = new Grid();
                grid.position = new Int2(i, j);
                xmap.grid[i, j] = grid;
                if (_mapGeneratorZh.mapObstract[i,j])//障碍物
                {
                    grid.state = GridState.Obstacle;
                    ///Debug.Log(grid.ToString());
                }
                else// 将边界出口点加入
                {
                    if (i==0)
                    {
                        grid.state = GridState.Destination;
                        gridListDes.Add(xmap.grid[i, j]);
                    }
                    else
                    {
                        if (j==0)
                        {
                            grid.state = GridState.Destination;
                            gridListDes.Add(xmap.grid[i, j]);
                        }
                        else if (j==_mapGeneratorZh._MapMaxSize.y - 1)
                        {
                            grid.state = GridState.Destination;
                            gridListDes.Add(xmap.grid[i, j]);
                        }
                        else if (i==_mapGeneratorZh._MapMaxSize.x - 1)
                        {
                            grid.state = GridState.Destination;
                            gridListDes.Add(xmap.grid[i, j]);
                        }
                    }
                }
            }
        }

    }

    List<Vector3> pathList=new List<Vector3>();
    void PathShow()
    {
        List<Stack<Node>> nodeStackList = aStar.pathNodeList.OrderBy(stk => stk.Count).ToList();
        Debug.Log("找到的路径数目 " + nodeStackList.Count);
        string _path = string.Empty;
        foreach (Node node in nodeStackList[0])
        {
            _path += node.position.ToString() + "=>";

            Vector3 _NewPos = new Vector3(-_mapGeneratorZh._MapMaxSize.x / 2 + 0.5f + node.position.x, 0, -_mapGeneratorZh._MapMaxSize.y / 2 + 0.5f + node.position.y);
            pathList.Add(_NewPos);
        }
        Debug.Log("最短路径为  " + _path);

        Transform mapHolder= _mapGeneratorZh.transform.Find("MapHolder");
        foreach (var localPos in pathList)
        {
            Vector3 worldPos = mapHolder.TransformPoint(localPos);
            Vector3 screenPos=Camera.main.WorldToScreenPoint(worldPos);
            Ray _RayCamera = Camera.main.ScreenPointToRay(screenPos);
            RaycastHit _CameraHit;
            if (Physics.Raycast(_RayCamera, out _CameraHit, 1000,1<<LayerMask.NameToLayer("Tile")))
            {
                print("鼠标点击的位置是： " + _CameraHit.point);

                MeshRenderer render=_CameraHit.transform.GetComponent<MeshRenderer>();
                render.material.color=Color.green;
            }
        }

    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(StartAstar());
        }
    }
}
