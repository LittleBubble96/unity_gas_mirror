using System;
using Mirror;
using UnityEngine;

/// <summary>
/// 类似《原神》和《鸣潮》的第三人称角色移动与相机控制脚本
/// 使用 CharacterController 组件，主相机为跟随相机
/// 支持 WASD 移动、Space 跳跃、鼠标控制视角、Alt 切换光标锁定
/// </summary>
public class ThirdPersonCharacterController : NetworkBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float walkSpeed = 5f;          // 移动速度
    [SerializeField] private float runSpeed = 8f;           // 奔跑速度（预留，当前未使用额外按键）
    [SerializeField] private float jumpHeight = 1.5f;       // 跳跃高度
    [SerializeField] private float gravity = -9.81f;        // 重力加速度
    [SerializeField] private float groundCheckRadius = 0.2f;// 地面检测半径
    [SerializeField] private float rotationSmoothTime = 0.1f; // 角色转向平滑时间

    [Header("相机设置")]
    [SerializeField] private float mouseSensitivity = 2f;    // 鼠标灵敏度
    [SerializeField] private float cameraDistance = 5f;      // 相机距离角色的距离
    [SerializeField] private float cameraHeight = 2f;        // 相机相对角色的高度偏移
    [SerializeField] private float minPitch = -30f;          // 相机俯仰最小角度（向下看）
    [SerializeField] private float maxPitch = 60f;           // 相机俯仰最大角度（向上看）
    [SerializeField] private float cameraCollisionRadius = 0.3f; // 相机碰撞检测半径
    [SerializeField] private LayerMask collisionLayers = -1; // 碰撞检测图层（默认所有）

    [SerializeField] private AnimatorBridge _animatorBridge;
    // 组件引用
    private CharacterController characterController;
    private Camera mainCamera;
    private Transform cameraTransform;

    // 移动相关变量
    private Vector3 velocity;                     // 当前垂直速度
    private bool isGrounded;
    private Vector3 moveInput;                    // 原始输入方向 (X:水平, Z:垂直)
    private Vector3 moveRealInput;                // 真实输入方向
    private Vector3 currentMoveDirection;         // 当前移动方向（世界空间）
    private Vector3 smoothMoveVelocity;           // 用于平滑移动方向的参考变量

    // 相机旋转相关变量
    private float currentYaw = 0f;                // 水平旋转角度
    private float currentPitch = 0f;              // 垂直旋转角度

    // Alt 键切换光标锁定状态
    private bool isCursorLocked = true;

    public bool IsWalking { get; set; }
    public bool IsGrounded => isGrounded;

    private void Awake()
    {
        // 获取 CharacterController 组件
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("ThirdPersonCharacterController 需要 CharacterController 组件！");
            enabled = false;
            return;
        }

        // 获取主相机
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("场景中没有标记为 MainCamera 的相机！");
            enabled = false;
            return;
        }
        cameraTransform = mainCamera.transform;

        // 初始化光标状态
        SetCursorLock(isCursorLocked);
        _animatorBridge.OnAnimatorMoveCall += OnAnimatorMoveCall;
    }

    private void OnDestroy()
    {
        _animatorBridge.OnAnimatorMoveCall -= OnAnimatorMoveCall;
    }

    private void Start()
    {
        // 初始化相机旋转角度为当前角色前方的方向（或者相机初始角度）
        Vector3 currentEuler = cameraTransform.eulerAngles;
        currentYaw = currentEuler.y;
        currentPitch = ClampPitch(currentEuler.x);
        
        // 可选：将角色初始朝向与相机一致
        transform.rotation = Quaternion.Euler(0f, currentYaw, 0f);
    }

    private void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        // 处理 Alt 键切换光标锁定
        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
        {
            isCursorLocked = !isCursorLocked;
            SetCursorLock(isCursorLocked);
        }

        // 仅当光标锁定时才处理鼠标视角输入
        if (isCursorLocked)
        {
            HandleMouseLook();
        }

        // 获取移动输入（WASD）
        HandleMovementInput();

        // 跳跃输入处理
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        // 更新地面检测和重力
        UpdateGravity();

        // 移动角色
        MoveCharacter();
    }

    private void LateUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        // 相机跟随与碰撞处理（放在 LateUpdate 中更平滑）
        UpdateCamera();
    }

    /// <summary>
    /// 处理鼠标输入，更新相机旋转角度（Yaw 和 Pitch）
    /// </summary>
    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        currentYaw += mouseX;
        currentPitch -= mouseY;
        currentPitch = ClampPitch(currentPitch);
    }

    /// <summary>
    /// 限制俯仰角范围
    /// </summary>
    private float ClampPitch(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return Mathf.Clamp(angle, minPitch, maxPitch);
    }

    /// <summary>
    /// 获取 WASD 输入，并转换为相对于相机视角的方向
    /// </summary>
    private void HandleMovementInput()
    {
        // 获取原始输入
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // 计算相机的正向和右向（忽略俯仰影响，只使用水平方向）
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();
        moveRealInput = new Vector3(horizontal, 0f, vertical).normalized;
        // 根据相机方向计算世界空间中的移动方向
        Vector3 desiredMove = (cameraForward * vertical) + (cameraRight * horizontal);
        desiredMove.Normalize();

        // 平滑移动方向（使转向更自然）
        currentMoveDirection = Vector3.SmoothDamp(
            currentMoveDirection,
            desiredMove,
            ref smoothMoveVelocity,
            rotationSmoothTime
        );

        moveInput = desiredMove;
    }

    /// <summary>
    /// 处理重力与地面检测
    /// </summary>
    private void UpdateGravity()
    {
        // 地面检测：使用 SphereCast 或简单检查 CharacterController 的 isGrounded 标志
        // 为了更精确，结合一个小半径的检测
        isGrounded = characterController.isGrounded;
        
        // 额外检查：如果刚刚跳起，避免立刻被判定为地面
        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f; // 轻微向下吸附，保持地面接触
        }

        // 应用重力
        velocity.y += gravity * Time.deltaTime;
    }

    /// <summary>
    /// 跳跃逻辑
    /// </summary>
    private void Jump()
    {
        // 计算跳跃初速度：v = sqrt(2 * g * h)
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    /// <summary>
    /// 执行角色移动
    /// </summary>
    private void MoveCharacter()
    {
        // 计算水平移动速度（移动输入方向 * 移动速度）
        float currentSpeed = walkSpeed;
        Vector3 horizontalMove = currentMoveDirection * currentSpeed;
        // 完整移动向量（水平 + 垂直）
        Vector3 move = horizontalMove * Time.deltaTime;
        move.y = velocity.y * Time.deltaTime;
        // 通过 CharacterController 移动
        // characterController.Move(move);
        IsWalking = horizontalMove.magnitude > 0.1f;
        if (_animatorBridge != null)
        {
            _animatorBridge.SetBool("Walk", IsWalking);
        }
        
        // 角色转向：如果有有效移动输入，让角色面朝移动方向
        if (moveInput.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveRealInput.x, moveRealInput.z) * Mathf.Rad2Deg;
            // 使用相机的 Yaw 作为参考，使角色朝向相对于相机前方
            float finalAngle = targetAngle + currentYaw;
            Quaternion targetRotation = Quaternion.Euler(0f, finalAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothTime);
        }
    }

    /// <summary>
    /// 更新相机位置与旋转，并处理相机碰撞
    /// </summary>
    private void UpdateCamera()
    {
        // 根据当前 Yaw 和 Pitch 计算相机旋转
        Quaternion cameraRotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        
        // 计算理想相机位置：角色位置 + 偏移（后方 + 上方）
        Vector3 desiredOffset = cameraRotation * new Vector3(0f, cameraHeight, -cameraDistance);
        Vector3 desiredPosition = transform.position + desiredOffset;
        
        // 相机碰撞检测：从角色位置向理想位置发射射线，如果碰撞则缩短相机距离
        Vector3 finalPosition = desiredPosition;
        Vector3 directionToCamera = desiredPosition - transform.position;
        float distance = directionToCamera.magnitude;
        RaycastHit hit;
        
        if (Physics.SphereCast(transform.position + Vector3.up * 0.5f, cameraCollisionRadius, directionToCamera.normalized, out hit, distance, collisionLayers))
        {
            // 如果检测到碰撞，将相机位置设置在碰撞点附近（留一点偏移）
            float hitDistance = Mathf.Max(0.5f, hit.distance - 0.2f);
            finalPosition = transform.position + directionToCamera.normalized * hitDistance;
        }
        
        // 设置相机位置和旋转
        cameraTransform.position = finalPosition;
        cameraTransform.rotation = cameraRotation;
        
        // 可选：让相机始终注视角色（增加跟随感，类似原神默认效果）
        // 如果希望相机严格保持旋转角度而不是自动注视，可以注释下面这行
        // cameraTransform.LookAt(transform.position + Vector3.up * 1f);
    }

    /// <summary>
    /// 设置光标锁定状态及可见性
    /// </summary>
    private void SetCursorLock(bool locked)
    {
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // 可选：在 Inspector 中调试显示信息
    private void OnDrawGizmosSelected()
    {
        if (characterController == null) return;
        // 绘制相机碰撞检测的调试球体
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, cameraCollisionRadius);
        
        // 绘制理想相机位置
        Gizmos.color = Color.green;
        Quaternion rot = Quaternion.Euler(currentPitch, currentYaw, 0f);
        Vector3 offset = rot * new Vector3(0f, cameraHeight, -cameraDistance);
        Gizmos.DrawWireSphere(transform.position + offset, 0.2f);
    }

    private void OnAnimatorMoveCall(Animator animator)
    {
        if (!isLocalPlayer)
        {
            return;
        }
        // 如果使用 Animator 进行根运动，可以在这里处理动画移动
        // 例如：根据动画的根运动来调整角色位置和旋转
        // 注意：需要在 Animator 中启用 "Apply Root Motion"
        if (animator != null && animator.applyRootMotion)
        {
            Vector3 rootMotion = animator.deltaPosition;
            rootMotion.y = velocity.y * Time.deltaTime; // 保持垂直速度
            characterController.Move(rootMotion);
        }
    }
}