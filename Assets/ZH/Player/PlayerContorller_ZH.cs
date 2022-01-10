using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerContorller_ZH : MonoBehaviour
{
    private Camera _MainCamrea;

    [Header("Camera旋转速度")]
    public float _CameraRotationSpeed = 0.5f;

    [Header("Camera移动速度")]
    public float _CameraMoveSpeed = 1.5f;

    [Header("Camera加速度")]
    public float _CameraMoveSpeedUP = 1.5f;

    [Header("上下限制视角")]
    public float _MinX = -85.0f;
    public float _MaxX = 85.0f;

    [Header("远近限制视角")]
    public float _FovMin = 40.0f;
    public float _FovMax = 85.0f;

    //旋转
    float _EulerX = 0.0f;
    float _EulerY = 0.0f;
    //移动
    float _MoveX = 0.0f;
    float _MoveY = 0.0f;
    float _MoveZ = 0.0f;

    //自动漫游初始旋转角度
    Quaternion _RoamEuler;

    //目标点位置
    private Vector3 _TragetVecAI;

    //人物AI
    private NavMeshAgent _PalyerAI;


    //单例
    public static Transform _RoamTransform;

    void Start()
    {
        _RoamTransform = Camera.main.transform;
        _MainCamrea = _RoamTransform.GetComponent<Camera>();
        _RoamEuler = _RoamTransform.rotation;

        _PalyerAI = GetComponent<NavMeshAgent>();

        //控制物理是否会改变物体的旋转。
        if (GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().freezeRotation = true;
        }
    }

    void Update()
    {

        //鼠标左键
        if (Input.GetMouseButtonDown(0))
        {
            //屏幕坐标转换 世界空间位置
            Ray _RayCamera = Camera.main.ScreenPointToRay(Input.mousePosition);
            //射线结构体 对象 包含各种信息
            RaycastHit _CameraHit;
            //射线布尔 判断
            //参数 射线  返回结构体  最大距离  忽略层
            if (Physics.Raycast(_RayCamera, out _CameraHit, 1000))
            {
                print("鼠标点击的位置是： " + _CameraHit.point);

                //目标点 坐标存储
                _TragetVecAI = _CameraHit.point;
                //目标路径存储
                _PalyerAI.SetDestination(_CameraHit.point);

               
            }

        }

        //如果当前AI 有目标点
        if (_PalyerAI.hasPath)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                foreach (Vector3 pos in _PalyerAI.path.corners)
                {
                    Debug.Log(pos);
                }
            }
         
            //判断当前AI 与目标点之间的 直线距离 如果小于 0.1f 
            if (Vector3.Distance(_TragetVecAI,transform.position)<=0.1f)
            {
                //设置当前AI 目标路径为 Null
                _PalyerAI.path = null;
             
            }
        }
    }

    private void LateUpdate()
    {
        CameraRotation();
        CameraMove();
    }

    /// <summary>
    /// Camera 旋转
    /// </summary>
    public void CameraRotation()
    {
        if (Input.GetMouseButton(1))
        {
            _EulerY += Input.GetAxis("Mouse X") * _CameraRotationSpeed;
            _EulerX -= Input.GetAxis("Mouse Y") * _CameraRotationSpeed;

            //角度限制，如果rorationY小于min返回min，大于max返回max
            _EulerX = Clam(_EulerX, _MinX, _MaxX);
            Quaternion _Euler = Quaternion.Euler(_EulerX, _EulerY, 0.0f);
            _RoamTransform.rotation = (_RoamEuler * _Euler);

        }

        //Camera 视角变更
        float _Mouse2 = 0.0f;
        _Mouse2 = Input.GetAxis("Mouse ScrollWheel");
        _MainCamrea.fieldOfView = Clam(_MainCamrea.fieldOfView + _Mouse2 * 5, _FovMin, _FovMax);

    }

    /// <summary>
    /// Camera 移动
    /// </summary>
    public void CameraMove()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _CameraMoveSpeedUP = 8.0f;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            _CameraMoveSpeedUP = 1.0f;
        }
        //键盘控制
        _MoveZ = Input.GetAxis("Vertical") * _CameraMoveSpeed * _CameraMoveSpeedUP;
        _MoveX = Input.GetAxis("Horizontal") * _CameraMoveSpeed * _CameraMoveSpeedUP;
        _MoveY = Input.GetAxis("GoUpAndDown") * _CameraMoveSpeed * _CameraMoveSpeedUP;
        _RoamTransform.Translate(_MoveX, _MoveY, _MoveZ);
    }

    /// <summary>
    /// 角度限制
    /// </summary>
    /// <param 返回值="_Value"></param>
    /// <param 最小值="_Min"></param>
    /// <param 最大值="_Max"></param>
    /// <returns></returns>
    public float Clam(float _Value, float _Min, float _Max)
    {
        if (_Value < _Min)
        {
            return _Min;
        }

        if (_Value > _Max)
        {
            return _Max;
        }
        return _Value;
    }

}
