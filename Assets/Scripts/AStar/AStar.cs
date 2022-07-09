using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EvaluationFunctionType {
    Euclidean,
    Manhattan,
    Diagonal,
}



public class AStar {
    static int FACTOR = 10;//水平竖直相邻格子的距离
    static int FACTOR_DIAGONAL = 14;//对角线相邻格子的距离

    bool m_isInit = false;

    public bool isGoDiagonally=false;
    public bool isInit => m_isInit;

    public Grid[,] m_map;//地图数据
    Int2 m_mapSize;
    Int2 m_player, m_destination;//起始点和结束点坐标
    EvaluationFunctionType m_evaluationFunctionType;//估价方式

    Dictionary<Int2, Node> m_openDic = new Dictionary<Int2, Node>();//准备处理的网格
    Dictionary<Int2, Node> m_closeDic = new Dictionary<Int2, Node>();//完成处理的网格

    Node m_destinationNode;

    public List<Stack<Node>> pathNodeList=new List<Stack<Node>>();

    public void Init(Int2 mapSize, Int2 player, Int2 destination, EvaluationFunctionType type = EvaluationFunctionType.Diagonal) {
        m_mapSize = mapSize;
        m_player = player;
        m_destination = destination;
        m_evaluationFunctionType = type;

        m_openDic.Clear();
        m_closeDic.Clear();

        m_destinationNode = null;

        //将起始点加入open中
        AddNodeInOpenQueue(new Node(m_player, null, 0, 0));
        m_isInit = true;
    }


    public void ThreadStart()
    {
        while (m_openDic.Count > 0 && m_destinationNode == null)
        {
            //按照f的值升序排列
            m_openDic = m_openDic.OrderBy(kv => kv.Value.f).ToDictionary(p => p.Key, o => o.Value);
            //提取排序后的第一个节点
            Node node = m_openDic.First().Value;
            //因为使用的不是Queue，因此要从open中手动删除该节点
            m_openDic.Remove(node.position);
            //处理该节点相邻的节点
            OperateNeighborNode(node);
            //处理完后将该节点加入close中
            AddNodeInCloseDic(node);
            //yield return null;
        }
        if (m_destinationNode == null)
            Debug.LogError("找不到可用路径");
        else
            ShowPath(m_destinationNode);
    }

    //计算寻路
    public IEnumerator Start() {
        while(m_openDic.Count > 0 && m_destinationNode == null) {
            //按照f的值升序排列
            m_openDic = m_openDic.OrderBy(kv => kv.Value.f).ToDictionary(p => p.Key, o => o.Value);
            //提取排序后的第一个节点
            Node node = m_openDic.First().Value;
            //因为使用的不是Queue，因此要从open中手动删除该节点
            m_openDic.Remove(node.position);
            //处理该节点相邻的节点
            OperateNeighborNode(node);
            //处理完后将该节点加入close中
            AddNodeInCloseDic(node);
            yield return null;
        }
        if(m_destinationNode == null)
            Debug.LogError("找不到可用路径");
        else
            ShowPath(m_destinationNode);
    }

    //处理相邻的节点
    void OperateNeighborNode(Node node) {
        for(int i = -1; i < 2; i++) {
            for(int j = -1; j < 2; j++) {
                if(i == 0 && j == 0)
                    continue;
                Int2 pos = new Int2(node.position.x + i, node.position.y + j);
                //超出地图范围
                if(pos.x < 0 || pos.x >= m_mapSize.x || pos.y < 0 || pos.y >= m_mapSize.y)
                    continue;
                //已经处理过的节点
                if(m_closeDic.ContainsKey(pos))
                    continue;
                //障碍物节点
                if(m_map[pos.x, pos.y].state == GridState.Obstacle)
                    continue;
                //将相邻节点加入open中
                if(i == 0 || j == 0)
                    AddNeighborNodeInQueue(node, pos, FACTOR);
                else
                {
                    if (isGoDiagonally)
                    {
                        AddNeighborNodeInQueue(node, pos, FACTOR_DIAGONAL);
                    }

                }
                   
            }
        }
    }

    //将节点加入到open中
    void AddNeighborNodeInQueue(Node parentNode, Int2 position, int g) {
        //当前节点的实际距离g等于上个节点的实际距离加上自己到上个节点的实际距离
        int nodeG = parentNode.g + g;
        //如果该位置的节点已经在open中
        if(m_openDic.ContainsKey(position)) {
            //比较实际距离g的值，用更小的值替换
            if(nodeG < m_openDic[position].g) {
                Debug.LogError("123");
                m_openDic[position].g = nodeG;
                m_openDic[position].parent = parentNode;
                ShowOrUpdateAStarHint(m_openDic[position]);
            }
        }
        else {
            //生成新的节点并加入到open中
            Node node = new Node(position, parentNode, nodeG, GetH(position));
            //如果周边有一个是终点，那么说明已经找到了。
            if(position == m_destination)
                m_destinationNode = node;
            else
                AddNodeInOpenQueue(node);
        }
    }

    //加入open中，并更新网格状态
    void AddNodeInOpenQueue(Node node) {
        m_openDic[node.position] = node;
    }

    void ShowOrUpdateAStarHint(Node node) {

    }

    //加入close中，并更新网格状态
    void AddNodeInCloseDic(Node node) {
        if (!m_closeDic.ContainsKey(node.position))
        {
            m_closeDic.Add(node.position, node);
        }
       
    }

    //寻路完成，显示路径
    void ShowPath(Node node) {
       Stack<Node> nodeStack=new Stack<Node>();
       //nodeStack.Push(node);
        while(node != null) {
            //m_map[node.position.x, node.position.y].ChangeToPathState();
            nodeStack.Push(node);
            node = node.parent;
        }

        Debug.Log("寻路完成");
        string path = "";
        foreach (var itemNode in nodeStack)
        {
            path += itemNode.position.ToString()+"\n";
        }
        Debug.Log(path);

        pathNodeList.Add(nodeStack);
    }

    //获取估价距离
    int GetH(Int2 position) {
        if(m_evaluationFunctionType == EvaluationFunctionType.Manhattan)
            return GetManhattanDistance(position);
        else if(m_evaluationFunctionType == EvaluationFunctionType.Diagonal)
            return GetDiagonalDistance(position);
        else
            return Mathf.CeilToInt(GetEuclideanDistance(position));
    }

    //获取曼哈顿距离
    int GetDiagonalDistance(Int2 position) {
        int x = Mathf.Abs(m_destination.x - position.x);
        int y = Mathf.Abs(m_destination.y - position.y);
        int min = Mathf.Min(x, y);
        return min * FACTOR_DIAGONAL + Mathf.Abs(x - y) * FACTOR;
    }

    //获取对角线距离
    int GetManhattanDistance(Int2 position) {
        return Mathf.Abs(m_destination.x - position.x) * FACTOR + Mathf.Abs(m_destination.y - position.y) * FACTOR;
    }

    //获取欧几里得距离,测试下来并不合适
    float GetEuclideanDistance(Int2 position) {
        return Mathf.Sqrt(Mathf.Pow((m_destination.x - position.x) * FACTOR, 2) + Mathf.Pow((m_destination.y - position.y) * FACTOR, 2));
    }

    public void Clear() {
        m_openDic.Clear();

        m_closeDic.Clear();

        m_destinationNode = null;

        m_isInit = false;
    }

}

