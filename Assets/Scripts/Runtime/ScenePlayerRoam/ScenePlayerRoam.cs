using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.ShortcutManagement;

namespace PlayerRoam
{
    [ExecuteInEditMode]
    public class ScenePlayerRoam : MonoBehaviour
    {
        [Header("摄像机焦点的偏移")]
        public Vector3 lookatPosOffset = new Vector3(0f, 1f, 0f);

        [Header("摄像机距离角色的最小最大距离（模拟值）")]
        public Vector2 camDisRange = new Vector2(1f, 5f);

        [Header("摄像机垂直角度范围")]
        public Vector2 camVerticalAngleRange = new Vector2(-60f, 30f);

        [Header("摄像机水平移动速度")]
        public float camHorizontalAimingSpeed = 400f;

        [Header("摄像机垂直移动速度")]
        public float camVerticalAimingSpeed = 400f;

        [Header("人物移动的普通速度")]
        public float playerNormalMoveSpeed = 3.5f;

        [Header("人物移动按住SHIFT的加速速度")]
        public float playerSpeedUpMult = 20f;

        /// <summary>
        /// .
        /// </summary>
        ScenePlayerRoamAnim m_Anim = new ScenePlayerRoamAnim();

        /// <summary>
        /// .
        /// </summary>
        ScenePlayerRoamMove m_Move = new ScenePlayerRoamMove();

        /// <summary>
        /// .
        /// </summary>
        ScenePlayerRoamCam m_Camera = new ScenePlayerRoamCam();

        /// <summary>
        /// 当前输入向量
        /// </summary>
        Vector3 m_CurrentInputVector;

        /// <summary>
        /// 方向键按下
        /// </summary>
        HashSet<KeyCode> m_DirKeysDown = new HashSet<KeyCode>();

        /// <summary>
        /// 方向键函数绑定
        /// </summary>
        Dictionary<KeyCode, Action<ShortcutStage>> m_DirKeyBindings = new Dictionary<KeyCode, Action<ShortcutStage>>();

        /// <summary>
        /// SceneView
        /// </summary>
        private SceneView m_SceneView;

        /// <summary>
        /// 是否可以使用鼠标
        /// </summary>
        bool m_CanUseInput = true;

        void OnEnable()
        {
            SceneView.duringSceneGui -= OnSceneGui;
            SceneView.duringSceneGui += OnSceneGui;

            EditorApplication.update -= OnEditorAppUpdate;
            EditorApplication.update += OnEditorAppUpdate;

            m_Anim.Init(gameObject);
            m_Move.Init(gameObject);
            m_Camera.Init(gameObject);

            m_DirKeyBindings.Clear();
            m_DirKeyBindings.Add(KeyCode.W, WalkForward);
            m_DirKeyBindings.Add(KeyCode.S, WalkBackward);
            m_DirKeyBindings.Add(KeyCode.A, WalkLeft);
            m_DirKeyBindings.Add(KeyCode.D, WalkRight);
        }

        void OnDestroy()
        {
            UnityEditor.SceneView.duringSceneGui -= OnSceneGui;
        }

        void OnDisable()
        {
            UnityEditor.SceneView.duringSceneGui -= OnSceneGui;
        }

        void Update()
        {
            if (m_SceneView == null)
            {
                m_SceneView = SceneView.lastActiveSceneView;
            }
        }

        void LateUpdate()
        {
            ///摄像机焦点的偏移
            m_Camera.sceneView = m_SceneView;
            m_Camera.lookatPosOffset = lookatPosOffset;
            m_Camera.camDisRange = camDisRange;
            m_Camera.camVerticalAngleRange = camVerticalAngleRange;
            m_Camera.camHorizontalAimingSpeed = camHorizontalAimingSpeed;
            m_Camera.camVerticalAimingSpeed = camVerticalAimingSpeed;
            // 移动体素检测
            m_Move.playerNormalMoveSpeed = playerNormalMoveSpeed;
            m_Move.playerSpeedUpMult = playerSpeedUpMult;

            float deltaTime = Time.deltaTime;

            if (m_CanUseInput)
            {
                m_Anim.LateUpdate(deltaTime);
                m_Move.LateUpdate(deltaTime);
                m_Camera.LateUpdate(deltaTime);
            }
        }

        void OnEditorAppUpdate()
        {
            EditorApplication.QueuePlayerLoopUpdate();
            //SceneView.RepaintAll();
        }

        void OnSceneGui(SceneView sceneView)
        {
            bool isKeyDown = Event.current.type == EventType.KeyDown;
            bool isCtrl = Event.current.control;
            if (isCtrl && isKeyDown && Event.current.keyCode == KeyCode.L)
            {
                m_CanUseInput = !m_CanUseInput;
            }

            if (m_CanUseInput)
            {
                WADSKeysProcess(sceneView);
                MouseProcess(sceneView);
            }
        }

        void MouseProcess(SceneView sceneView)
        {
            var evt = Event.current;
            bool isMidMouse = evt.isMouse && evt.button == 1;
            var mouseDelta = evt.delta;
            if (isMidMouse)
            {
                m_Camera.OnCamDrag(mouseDelta);
            }
        }

        void WADSKeysProcess(SceneView sceneView)
        {
            var evt = Event.current;

            bool isSpeedUp = evt.shift;
            Action<ShortcutStage> action;

            switch (evt.type)
            {
                case EventType.KeyDown:
                    {
                        KeyCode keyCode = evt.keyCode;

                        if (m_DirKeyBindings.TryGetValue(keyCode, out action))
                        {
                            action(ShortcutStage.Begin);
                            m_Anim.Run();
                            m_Move.SetMoveParam(m_CurrentInputVector.x, m_CurrentInputVector.y, isSpeedUp);
                            m_DirKeysDown.Add(keyCode);
                            evt.Use();
                        }
                    }
                    break;
                case EventType.KeyUp:
                    {
                        KeyCode keyCode = evt.keyCode;

                        if (m_DirKeyBindings.TryGetValue(keyCode, out action))
                        {
                            action(ShortcutStage.End);
                            m_Move.SetMoveParam(m_CurrentInputVector.x, m_CurrentInputVector.y, isSpeedUp);

                            m_DirKeysDown.Remove(keyCode);

                            if (m_DirKeysDown.Count == 0)
                            {
                                m_CurrentInputVector = Vector3.zero;
                                m_Anim.Idle();
                                m_Move.SetMoveParam(0, 0, false);
                            }

                            evt.Use();
                        }
                    }
                    break;
            }
        }

        void WalkForward(ShortcutStage stage)
        {
            m_CurrentInputVector.y = (stage == ShortcutStage.Begin) ? 0.5f : ((m_CurrentInputVector.y > 0f) ? 0f : m_CurrentInputVector.y);

            if (m_SceneView != null)
            {
                m_SceneView.Repaint();
            }
        }

        void WalkBackward(ShortcutStage stage)
        {
            m_CurrentInputVector.y = (stage == ShortcutStage.Begin) ? -0.5f : ((m_CurrentInputVector.y < 0f) ? 0f : m_CurrentInputVector.y);

            if (m_SceneView != null)
            {
                m_SceneView.Repaint();
            }
        }

        void WalkLeft(ShortcutStage stage)
        {
            m_CurrentInputVector.x = (stage == ShortcutStage.Begin) ? -0.5f : ((m_CurrentInputVector.x < 0f) ? 0f : m_CurrentInputVector.x);

            if (m_SceneView != null)
            {
                m_SceneView.Repaint();
            }
        }

        void WalkRight(ShortcutStage stage)
        {
            m_CurrentInputVector.x = (stage == ShortcutStage.Begin) ? 0.5f : ((m_CurrentInputVector.x > 0f) ? 0f : m_CurrentInputVector.x);

            if (m_SceneView != null)
            {
                m_SceneView.Repaint();
            }
        }
    }

    public class ScenePlayerRoamAnim
    {
        const string IDLE_CLIP_NAME = "idle";

        const string RUN_CLIP_NAME = "run";

        Animator m_Anim;

        AnimationClip m_IdleClip;

        AnimationClip m_RunClip;

        bool m_IsInIdle = true;

        float m_AnimPlayTime;

        public void Init(GameObject go)
        {
            m_Anim = go.GetComponentInChildren<Animator>();

            AnimationClip[] animationClips = m_Anim != null && m_Anim.runtimeAnimatorController != null ?
                m_Anim.runtimeAnimatorController.animationClips : new AnimationClip[0];

            InitIdleClip(animationClips);
            InitRunClip(animationClips);

            AnimationMode.StartAnimationMode();
        }

        public void LateUpdate(float deltaTime)
        {
            m_AnimPlayTime += deltaTime;

            if (m_IsInIdle)
            {
                if (m_AnimPlayTime > m_IdleClip.length)
                {
                    m_AnimPlayTime -= m_IdleClip.length;
                }

                AnimationMode.SampleAnimationClip(m_Anim.gameObject, m_IdleClip, m_AnimPlayTime);
            }
            else
            {
                if (m_AnimPlayTime > m_RunClip.length)
                {
                    m_AnimPlayTime -= m_RunClip.length;
                }

                AnimationMode.SampleAnimationClip(m_Anim.gameObject, m_RunClip, m_AnimPlayTime);
            }
        }

        public void Run()
        {
            m_IsInIdle = false;
        }

        public void Idle()
        {
            m_IsInIdle = true;
        }

        void InitRunClip(AnimationClip[] animationClips)
        {
            if (animationClips == null)
                return;

            foreach (var clip in animationClips)
            {
                if (clip == null)
                    continue;

                if (clip.name.Contains(RUN_CLIP_NAME))
                {
                    m_RunClip = clip;
                    break;
                }
            }
        }

        void InitIdleClip(AnimationClip[] animationClips)
        {
            if (animationClips == null)
                return;

            foreach (var clip in animationClips)
            {
                if (clip == null)
                    continue;

                if (clip.name.Contains(IDLE_CLIP_NAME))
                {
                    m_IdleClip = clip;
                    break;
                }
            }
        }
    }

    public class ScenePlayerRoamMove
    {
        public enum ForwardMode
        {
            Camera,
            Player,
            World
        };

        float m_Speed;

        float m_VelocityDamping;

        ForwardMode m_InputForward;

        bool m_RotatePlayer = true;

        Vector3 m_CurrentVelocity;

        float m_MoveX;

        float m_MoveY;

        bool m_isUseSpeedUp;

        Transform transform;

        /// <summary>
        /// 人物移动的普通速度
        /// </summary>
        public float playerNormalMoveSpeed;

        /// <summary>
        /// 人物移动按住SHIFT的加速速度
        /// </summary>
        public float playerSpeedUpMult;

        public void Init(GameObject gameObject)
        {
            transform = gameObject.transform;
            m_Speed = 5;
            m_InputForward = ForwardMode.Camera;
            m_RotatePlayer = true;
            m_VelocityDamping = 0.5f;
            m_CurrentVelocity = Vector3.zero;
        }

        public void LateUpdate(float deltaTime)
        {
            Move(m_MoveX, m_MoveY, m_isUseSpeedUp);
        }

        public void SetMoveParam(float x, float y, bool isSpeedUp)
        {
            m_MoveX = x;
            m_MoveY = y;
            m_isUseSpeedUp = isSpeedUp;
        }

        Vector3 GetForward()
        {
            Vector3 fwd;

            switch (m_InputForward)
            {
                case ForwardMode.Camera:
                    fwd = Vector3.forward;

                    var cam = SceneView.lastActiveSceneView.camera;
                    if (cam != null)
                    {
                        fwd = cam.transform.forward;
                    }
                    break;
                case ForwardMode.Player:
                    fwd = transform.forward;
                    break;
                case ForwardMode.World:
                default:
                    fwd = Vector3.forward;
                    break;
            }

            fwd.y = 0;
            fwd = fwd.normalized;

            return fwd;
        }

        void SetTransform(float x, float z, bool isUseSpeedUp, Vector3 fwd)
        {
            Quaternion inputFrame = Quaternion.LookRotation(fwd, Vector3.up);
            Vector3 input = new Vector3(x, 0, z);
            input = inputFrame * input;
            var dt = Time.deltaTime;
            var desiredVelocity = input * m_Speed;

            var deltaVel = desiredVelocity - m_CurrentVelocity;
            m_CurrentVelocity += Damper.Damp(deltaVel, m_VelocityDamping, dt);

            var deltaPos = m_CurrentVelocity * dt * (isUseSpeedUp ? playerSpeedUpMult : playerNormalMoveSpeed);
            transform.position += deltaPos;
            if (m_RotatePlayer && m_CurrentVelocity.sqrMagnitude > 0.01f)
            {
                var qA = transform.rotation;
                var qB = Quaternion.LookRotation((m_InputForward == ForwardMode.Player && Vector3.Dot(fwd, m_CurrentVelocity) < 0) ? -m_CurrentVelocity : m_CurrentVelocity);
                transform.rotation = Quaternion.Slerp(qA, qB, Damper.Damp(1, m_VelocityDamping, dt));
            }
        }

        void CheckHeight()
        {
            var p = transform.position;

            var rayPos = new Vector3(p.x, p.y + 10f, p.z);
            var rayDir = Vector3.down;
            var rayLayer = 1 << LayerMask.NameToLayer("TERRAIN") | 1 << LayerMask.NameToLayer("FLOOR");
            RaycastHit rayhit;
            if (Physics.Raycast(rayPos, rayDir, out rayhit, 100f, rayLayer))
            {
                p.y = rayhit.point.y;
            }

            transform.position = p;
        }

        void Move(float x, float z, bool isUseSpeedUp)
        {
            Vector3 fwd = GetForward();
            if (fwd.sqrMagnitude < 0.01f)
                return;

            SetTransform(x, z, isUseSpeedUp, fwd);
            CheckHeight();
        }
    }

    public class ScenePlayerRoamCam
    {
        #region 常量

        public const string INPUT_MOUSE_SCROLLWHEEL = "Mouse ScrollWheel";
        public const string ERROR_UN_PLAYER = "ThirdPersonCam脚本没有指定玩家";

        #endregion

        #region 变量

        /// <summary>
        /// 摄像机
        /// </summary>
        SceneView m_SceneView;

        /// <summary>
        /// 玩家transform
        /// </summary>
        Transform m_Player;

        /// <summary>
        /// 水平旋转的角度
        /// </summary>
        private float m_AngleH = 0.0f;

        /// <summary>
        /// 垂直旋转的角度
        /// </summary>
        private float m_AngleV = -30.0f;

        /// <summary>
        /// 摄像机焦点的偏移
        /// </summary>
        public Vector3 lookatPosOffset;

        /// <summary>
        /// 摄像机距离角色的最小最大距离（模拟值）
        /// </summary>
        public Vector2 camDisRange;

        /// <summary>
        /// 摄像机垂直角度范围
        /// </summary>
        public Vector2 camVerticalAngleRange;

        /// <summary>
        /// 水平瞄准速度
        /// </summary>
        public float camHorizontalAimingSpeed;

        /// <summary>
        /// 垂直瞄准速度
        /// </summary>
        public float camVerticalAimingSpeed;

        /// <summary>
        /// .
        /// </summary>
        public SceneView sceneView
        {
            get { return m_SceneView; }
            set { m_SceneView = value; }
        }

        #endregion

        public void Init(GameObject gameObject)
        {
            SetPlayer(gameObject);
        }

        public void OnCamDrag(Vector2 delta)
        {
            m_AngleH += Mathf.Clamp(delta.x / Screen.width, -1.0f, 1.0f) * camHorizontalAimingSpeed;
            m_AngleV -= Mathf.Clamp(delta.y / Screen.height, -1.0f, 1.0f) * camVerticalAimingSpeed;
        }

        public void LateUpdate(float deltaTime)
        {
            if (m_SceneView == null)
                return;

            if (m_Player == null)
            {
                Debug.LogError(ERROR_UN_PLAYER);
                return;
            }

            // 垂直角度的范围
            m_AngleV = Mathf.Clamp(m_AngleV, camVerticalAngleRange.x, camVerticalAngleRange.y);

            // 模拟设置摄像机距离的最小最大值
            m_SceneView.size = Mathf.Clamp(m_SceneView.size, camDisRange.x, camDisRange.y);

            // 场景视图的旋转和中心点
            Quaternion animRotation = Quaternion.Euler(-m_AngleV, m_AngleH, 0.0f);
            m_SceneView.rotation = animRotation;
            m_SceneView.pivot = m_Player.position + lookatPosOffset;

            m_SceneView.Repaint();
        }

        void SetPlayer(GameObject gameObject)
        {
            m_Player = gameObject.transform;
        }
    }
}

#endif