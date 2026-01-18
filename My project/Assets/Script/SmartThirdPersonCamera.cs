// SmartThirdPersonCamera.cs
using UnityEngine;

public class SmartThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 5f;      // 与目标的距离
    public float height = 2f;        // 摄像机高度
    public float rotationSpeed = 100f; // 旋转速度

    private float currentRotationX = 0f;
    private float currentRotationY = 0f;

    void Start()
    {
        // 初始化角度
        Vector3 angles = transform.eulerAngles;
        currentRotationX = angles.y;
        currentRotationY = angles.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 鼠标控制旋转
        if (Input.GetMouseButton(1)) // 右键按住旋转视角
        {
            currentRotationX += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            currentRotationY -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
            currentRotationY = Mathf.Clamp(currentRotationY, -20, 60); // 限制上下角度
        }

        // 计算摄像机位置
        Quaternion rotation = Quaternion.Euler(currentRotationY, currentRotationX, 0);
        Vector3 position = target.position - (rotation * Vector3.forward * distance) + Vector3.up * height;

        // 防止穿墙（简单的射线检测）
        RaycastHit hit;
        if (Physics.Linecast(target.position + Vector3.up * 1.5f, position, out hit))
        {
            position = hit.point + hit.normal * 0.5f;
        }

        // 应用位置和旋转
        transform.position = position;
        transform.LookAt(target.position + Vector3.up * 1f); // 稍微看向目标上方
    }
}