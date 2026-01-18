using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    [Header("移动设置")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpHeight = 1.2f;
    public float gravity = -15f;

    [Header("视角设置")]
    public float mouseSensitivity = 2f;
    public float cameraDistance = 3f;
    public float cameraHeight = 1.7f;
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 60f;

    [Header("CharacterController 设置")]
    public float controllerHeight = 2f;
    public float controllerRadius = 0.3f;
    public Vector3 controllerCenter = new Vector3(0, 1, 0);

    // 私有变量
    private CharacterController characterController;
    private Camera mainCamera;
    private Vector3 cameraOffset;
    private Vector3 velocity;
    private float currentSpeed;
    private bool isGrounded;

    // 鼠标旋转角度
    private float mouseX = 0f;
    private float mouseY = 0f;

    void Start()
    {
        // 清理多余的组件
        RemoveExtraComponents();

        // 获取或添加CharacterController
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
        }

        // 配置CharacterController
        characterController.height = controllerHeight;
        characterController.radius = controllerRadius;
        characterController.center = controllerCenter;
        characterController.slopeLimit = 45f;
        characterController.stepOffset = 0.3f;

        // 获取主摄像机
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("需要主摄像机！");
            return;
        }

        // 锁定鼠标到屏幕中心
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 初始化摄像机位置
        UpdateCameraPosition();
    }

    void RemoveExtraComponents()
    {
        // 删除可能冲突的组件
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);

        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            if (!(col is CharacterController))
                Destroy(col);
        }
    }

    void Update()
    {
        if (mainCamera == null) return;

        HandleMouseLook();
        HandleMovement();
        HandleCamera();
    }

    void HandleMouseLook()
    {
        // 获取鼠标输入
        mouseX += Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        mouseY = Mathf.Clamp(mouseY, minVerticalAngle, maxVerticalAngle);

        // 角色水平旋转跟随鼠标X轴
        transform.rotation = Quaternion.Euler(0, mouseX, 0);
    }

    void HandleMovement()
    {
        // 检测是否在地面
        isGrounded = characterController.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // 速度切换
        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // 获取移动输入
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 计算移动方向（相对于角色自身方向）
        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        moveDirection = moveDirection.normalized;

        // 应用移动
        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);

        // 跳跃
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 应用重力
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    void HandleCamera()
    {
        // 计算摄像机应该的旋转
        Quaternion cameraRotation = Quaternion.Euler(mouseY, mouseX, 0);

        // 计算摄像机偏移
        cameraOffset = new Vector3(0, cameraHeight, -cameraDistance);

        // 应用旋转和偏移
        Vector3 desiredPosition = transform.position + cameraRotation * cameraOffset;

        // 摄像机碰撞检测（防止穿墙）
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * controllerCenter.y;
        if (Physics.Linecast(rayStart, desiredPosition, out hit))
        {
            // 如果碰到东西，把摄像机拉近
            desiredPosition = hit.point + hit.normal * 0.3f;
        }

        // 更新摄像机位置
        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            desiredPosition,
            10f * Time.deltaTime
        );

        // 摄像机看向角色（看向角色头部位置）
        Vector3 lookAtPoint = transform.position + Vector3.up * (controllerHeight * 0.8f);
        mainCamera.transform.LookAt(lookAtPoint);
    }

    void UpdateCameraPosition()
    {
        Quaternion cameraRotation = Quaternion.Euler(mouseY, mouseX, 0);
        cameraOffset = new Vector3(0, cameraHeight, -cameraDistance);
        mainCamera.transform.position = transform.position + cameraRotation * cameraOffset;

        Vector3 lookAtPoint = transform.position + Vector3.up * (controllerHeight * 0.8f);
        mainCamera.transform.LookAt(lookAtPoint);
    }

    // 获取屏幕中心点射线
    public Ray GetCenterScreenRay()
    {
        if (mainCamera == null) return new Ray();
        return mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
    }
}