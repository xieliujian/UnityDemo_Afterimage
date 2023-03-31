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
        [Header("����������ƫ��")]
        public Vector3 lookatPosOffset = new Vector3(0f, 1f, 0f);

        [Header("����������ɫ����С�����루ģ��ֵ��")]
        public Vector2 camDisRange = new Vector2(1f, 5f);

        [Header("�������ֱ�Ƕȷ�Χ")]
        public Vector2 camVerticalAngleRange = new Vector2(-60f, 30f);

        [Header("�����ˮƽ�ƶ��ٶ�")]
        public float camHorizontalAimingSpeed = 400f;

        [Header("�������ֱ�ƶ��ٶ�")]
        public float camVerticalAimingSpeed = 400f;

        [Header("�����ƶ�����ͨ�ٶ�")]
        public float playerNormalMoveSpeed = 3.5f;

        [Header("�����ƶ���סSHIFT�ļ����ٶ�")]
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
        /// ��ǰ��������
        /// </summary>
        Vector3 m_CurrentInputVector;

        /// <summary>
        /// ���������
        /// </summary>
        HashSet<KeyCode> m_DirKeysDown = new HashSet<KeyCode>();

        /// <summary>
        /// �����������
        /// </summary>
        Dictionary<KeyCode, Action<ShortcutStage>> m_DirKeyBindings = new Dictionary<KeyCode, Action<ShortcutStage>>();

        /// <summary>
        /// SceneView
        /// </summary>
        private SceneView m_SceneView;

        /// <summary>
        /// �Ƿ����ʹ�����
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
            ///����������ƫ��
            m_Camera.sceneView = m_SceneView;
            m_Camera.lookatPosOffset = lookatPosOffset;
            m_Camera.camDisRange = camDisRange;
            m_Camera.camVerticalAngleRange = camVerticalAngleRange;
            m_Camera.camHorizontalAimingSpeed = camHorizontalAimingSpeed;
            m_Camera.camVerticalAimingSpeed = camVerticalAimingSpeed;
            // �ƶ����ؼ��
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
        /// �����ƶ�����ͨ�ٶ�
        /// </summary>
        public float playerNormalMoveSpeed;

        /// <summary>
        /// �����ƶ���סSHIFT�ļ����ٶ�
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
        #region ����

        public const string INPUT_MOUSE_SCROLLWHEEL = "Mouse ScrollWheel";
        public const string ERROR_UN_PLAYER = "ThirdPersonCam�ű�û��ָ�����";

        #endregion

        #region ����

        /// <summary>
        /// �����
        /// </summary>
        SceneView m_SceneView;

        /// <summary>
        /// ���transform
        /// </summary>
        Transform m_Player;

        /// <summary>
        /// ˮƽ��ת�ĽǶ�
        /// </summary>
        private float m_AngleH = 0.0f;

        /// <summary>
        /// ��ֱ��ת�ĽǶ�
        /// </summary>
        private float m_AngleV = -30.0f;

        /// <summary>
        /// ����������ƫ��
        /// </summary>
        public Vector3 lookatPosOffset;

        /// <summary>
        /// ����������ɫ����С�����루ģ��ֵ��
        /// </summary>
        public Vector2 camDisRange;

        /// <summary>
        /// �������ֱ�Ƕȷ�Χ
        /// </summary>
        public Vector2 camVerticalAngleRange;

        /// <summary>
        /// ˮƽ��׼�ٶ�
        /// </summary>
        public float camHorizontalAimingSpeed;

        /// <summary>
        /// ��ֱ��׼�ٶ�
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

            // ��ֱ�Ƕȵķ�Χ
            m_AngleV = Mathf.Clamp(m_AngleV, camVerticalAngleRange.x, camVerticalAngleRange.y);

            // ģ������������������С���ֵ
            m_SceneView.size = Mathf.Clamp(m_SceneView.size, camDisRange.x, camDisRange.y);

            // ������ͼ����ת�����ĵ�
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