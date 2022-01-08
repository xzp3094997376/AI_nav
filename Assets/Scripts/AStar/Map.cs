using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    public Int2 position;
    GridState m_state;
    public GridState state
    {
        get => m_state;
        set
        {
            m_state = value;
        }
    }

    public override string ToString()
    {
        return string.Format("{0},{1}",position.x,position.y);
    }
}

public class XMap
{
    public Grid[,] grid;
}

public enum GridState
{
    Default,
    Player,
    Obstacle,
    Destination,
    Path,
    InOpen,
    InClose
}

