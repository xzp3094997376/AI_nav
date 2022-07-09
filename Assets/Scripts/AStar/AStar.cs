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
    static int FACTOR = 10;//ˮƽ��ֱ���ڸ��ӵľ���
    static int FACTOR_DIAGONAL = 14;//�Խ������ڸ��ӵľ���

    bool m_isInit = false;

    public bool isGoDiagonally=false;
    public bool isInit => m_isInit;

    public Grid[,] m_map;//��ͼ����
    Int2 m_mapSize;
    Int2 m_player, m_destination;//��ʼ��ͽ���������
    EvaluationFunctionType m_evaluationFunctionType;//���۷�ʽ

    Dictionary<Int2, Node> m_openDic = new Dictionary<Int2, Node>();//׼�����������
    Dictionary<Int2, Node> m_closeDic = new Dictionary<Int2, Node>();//��ɴ��������

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

        //����ʼ�����open��
        AddNodeInOpenQueue(new Node(m_player, null, 0, 0));
        m_isInit = true;
    }


    public void ThreadStart()
    {
        while (m_openDic.Count > 0 && m_destinationNode == null)
        {
            //����f��ֵ��������
            m_openDic = m_openDic.OrderBy(kv => kv.Value.f).ToDictionary(p => p.Key, o => o.Value);
            //��ȡ�����ĵ�һ���ڵ�
            Node node = m_openDic.First().Value;
            //��Ϊʹ�õĲ���Queue�����Ҫ��open���ֶ�ɾ���ýڵ�
            m_openDic.Remove(node.position);
            //����ýڵ����ڵĽڵ�
            OperateNeighborNode(node);
            //������󽫸ýڵ����close��
            AddNodeInCloseDic(node);
            //yield return null;
        }
        if (m_destinationNode == null)
            Debug.LogError("�Ҳ�������·��");
        else
            ShowPath(m_destinationNode);
    }

    //����Ѱ·
    public IEnumerator Start() {
        while(m_openDic.Count > 0 && m_destinationNode == null) {
            //����f��ֵ��������
            m_openDic = m_openDic.OrderBy(kv => kv.Value.f).ToDictionary(p => p.Key, o => o.Value);
            //��ȡ�����ĵ�һ���ڵ�
            Node node = m_openDic.First().Value;
            //��Ϊʹ�õĲ���Queue�����Ҫ��open���ֶ�ɾ���ýڵ�
            m_openDic.Remove(node.position);
            //����ýڵ����ڵĽڵ�
            OperateNeighborNode(node);
            //������󽫸ýڵ����close��
            AddNodeInCloseDic(node);
            yield return null;
        }
        if(m_destinationNode == null)
            Debug.LogError("�Ҳ�������·��");
        else
            ShowPath(m_destinationNode);
    }

    //�������ڵĽڵ�
    void OperateNeighborNode(Node node) {
        for(int i = -1; i < 2; i++) {
            for(int j = -1; j < 2; j++) {
                if(i == 0 && j == 0)
                    continue;
                Int2 pos = new Int2(node.position.x + i, node.position.y + j);
                //������ͼ��Χ
                if(pos.x < 0 || pos.x >= m_mapSize.x || pos.y < 0 || pos.y >= m_mapSize.y)
                    continue;
                //�Ѿ�������Ľڵ�
                if(m_closeDic.ContainsKey(pos))
                    continue;
                //�ϰ���ڵ�
                if(m_map[pos.x, pos.y].state == GridState.Obstacle)
                    continue;
                //�����ڽڵ����open��
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

    //���ڵ���뵽open��
    void AddNeighborNodeInQueue(Node parentNode, Int2 position, int g) {
        //��ǰ�ڵ��ʵ�ʾ���g�����ϸ��ڵ��ʵ�ʾ�������Լ����ϸ��ڵ��ʵ�ʾ���
        int nodeG = parentNode.g + g;
        //�����λ�õĽڵ��Ѿ���open��
        if(m_openDic.ContainsKey(position)) {
            //�Ƚ�ʵ�ʾ���g��ֵ���ø�С��ֵ�滻
            if(nodeG < m_openDic[position].g) {
                Debug.LogError("123");
                m_openDic[position].g = nodeG;
                m_openDic[position].parent = parentNode;
                ShowOrUpdateAStarHint(m_openDic[position]);
            }
        }
        else {
            //�����µĽڵ㲢���뵽open��
            Node node = new Node(position, parentNode, nodeG, GetH(position));
            //����ܱ���һ�����յ㣬��ô˵���Ѿ��ҵ��ˡ�
            if(position == m_destination)
                m_destinationNode = node;
            else
                AddNodeInOpenQueue(node);
        }
    }

    //����open�У�����������״̬
    void AddNodeInOpenQueue(Node node) {
        m_openDic[node.position] = node;
    }

    void ShowOrUpdateAStarHint(Node node) {

    }

    //����close�У�����������״̬
    void AddNodeInCloseDic(Node node) {
        if (!m_closeDic.ContainsKey(node.position))
        {
            m_closeDic.Add(node.position, node);
        }
       
    }

    //Ѱ·��ɣ���ʾ·��
    void ShowPath(Node node) {
       Stack<Node> nodeStack=new Stack<Node>();
       //nodeStack.Push(node);
        while(node != null) {
            //m_map[node.position.x, node.position.y].ChangeToPathState();
            nodeStack.Push(node);
            node = node.parent;
        }

        Debug.Log("Ѱ·���");
        string path = "";
        foreach (var itemNode in nodeStack)
        {
            path += itemNode.position.ToString()+"\n";
        }
        Debug.Log(path);

        pathNodeList.Add(nodeStack);
    }

    //��ȡ���۾���
    int GetH(Int2 position) {
        if(m_evaluationFunctionType == EvaluationFunctionType.Manhattan)
            return GetManhattanDistance(position);
        else if(m_evaluationFunctionType == EvaluationFunctionType.Diagonal)
            return GetDiagonalDistance(position);
        else
            return Mathf.CeilToInt(GetEuclideanDistance(position));
    }

    //��ȡ�����پ���
    int GetDiagonalDistance(Int2 position) {
        int x = Mathf.Abs(m_destination.x - position.x);
        int y = Mathf.Abs(m_destination.y - position.y);
        int min = Mathf.Min(x, y);
        return min * FACTOR_DIAGONAL + Mathf.Abs(x - y) * FACTOR;
    }

    //��ȡ�Խ��߾���
    int GetManhattanDistance(Int2 position) {
        return Mathf.Abs(m_destination.x - position.x) * FACTOR + Mathf.Abs(m_destination.y - position.y) * FACTOR;
    }

    //��ȡŷ����þ���,����������������
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

