public class Node
{
    Int2 m_position;//下标
    public Int2 position => m_position;
    public Node parent;//上一个node

    //角色到该节点的实际距离
    int m_g;
    public int g
    {
        get => m_g;
        set
        {
            m_g = value;
            m_f = m_g + m_h;
        }
    }

    //该节点到目的地的估价距离
    int m_h;
    public int h
    {
        get => m_h;
        set
        {
            m_h = value;
            m_f = m_g + m_h;
        }
    }

    int m_f;
    public int f => m_f;

    public Node(Int2 pos, Node parent, int g, int h)
    {
        m_position = pos;
        this.parent = parent;
        m_g = g;
        m_h = h;
        m_f = m_g + m_h;
    }
}





public struct Int2
{
    public int x;
    public int y;

    public Int2(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public override string ToString()
    {
        return $"x:{x.ToString()}   y:{y.ToString()}";
    }

    //为什么要重写GetHashCode方法，阅读： https://www.cnblogs.com/xiaochen-vip8/p/5506478.html
    public override int GetHashCode()
    {
        return x ^ (y * 256);
    }

    public override bool Equals(object obj)
    {
        if (obj.GetType() != typeof(Int2))
            return false;
        Int2 int2 = (Int2)obj;
        return x == int2.x && y == int2.y;
    }

    public static bool operator ==(Int2 a, Int2 b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Int2 a, Int2 b)
    {
        return !a.Equals(b);
    }
}