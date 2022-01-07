using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public MapGenerator_ZH _mapGeneratorZh;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            for (int i = 0; i < _mapGeneratorZh.mapObstract.GetLength(0); i++)
            {
                for (int j = 0; j < _mapGeneratorZh.mapObstract.GetLength(1); j++)
                {
                    if (i==0||j==0||i==100||j==100)
                    {
                        Debug.Log(i+",  "+j);
                    }
                }
            }
        }
    }
}
