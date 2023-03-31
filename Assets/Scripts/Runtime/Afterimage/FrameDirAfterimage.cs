using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace gtm.Scene
{
    [ExecuteInEditMode]
    public class FrameDirAfterimage : BaseAfterimage
    {
        [Serializable]
        public class Target
        {
            public Target()
            {
                moveSpeed = 20f;
            }

            /// <summary>
            /// 残影的路点目标位置GameObject
            /// </summary>
            [Header("残影的路点目标位置GameObject")]
            public GameObject dstPosGo;

            /// <summary>
            /// 移动速度
            /// </summary>
            [Header("残影移动到路点目标的速度, 默认可以填写20")]
            [Min(0f)]
            public float moveSpeed = 20f;
        }

        public struct TargetInfo
        {
            /// <summary>
            /// 是否有效
            /// </summary>
            public bool isValid;

            /// <summary>
            /// 源位置
            /// </summary>
            public GameObject srcPosGo;

            /// <summary>
            /// 目标位置
            /// </summary>
            public GameObject dstPosGo;

            /// <summary>
            /// 开始时间
            /// </summary>
            public float startTime;

            /// <summary>
            /// 目标时间
            /// </summary>
            public float dstTime;

            /// <summary>
            /// 获取当前位置
            /// </summary>
            /// <param name="time"></param>
            /// <returns></returns>
            public Vector3 GetCurPos(float time)
            {
                float percent = Mathf.Clamp01((time - startTime) / (dstTime - startTime));

                var srcPos = srcPosGo.transform.position;
                var dstPos = dstPosGo.transform.position;
                var pos = Vector3.Lerp(srcPos, dstPos, percent);
                return pos;
            }

            /// <summary>
            /// 复制信息
            /// </summary>
            public void Copy(TargetInfo info)
            {
                srcPosGo = info.srcPosGo;
                dstPosGo = info.dstPosGo;
                startTime = info.startTime;
                dstTime = info.dstTime;
            }
        }

        /// <summary>
        /// 性能名字
        /// </summary>
        const string PROFILING_NAME = "FrameDirAfterimage";

        /// <summary>
        /// 初始化旋转矩阵
        /// </summary>
        Matrix4x4 m_RotMatrix = Matrix4x4.identity;

        /// <summary>
        /// 位移矩阵
        /// </summary>
        Matrix4x4 m_TransMatrix = Matrix4x4.identity;

        /// <summary>
        /// 移动时间
        /// </summary>
        float m_MoveTime;

        /// <summary>
        /// 目标信息列表
        /// </summary>
        List<TargetInfo> m_TargetInfoList = new List<TargetInfo>(NUM_8);

        /// <summary>
        /// 当前目标信息
        /// </summary>
        TargetInfo m_CurTargetInfo = new TargetInfo();

        /// <summary>
        /// 
        /// </summary>
        [Header("残影的资源GameObject")]
        public GameObject resGo;

        /// <summary>
        /// 目标信息列表
        /// </summary>
        [Header("目标信息列表")]
        public List<Target> targetList = new List<Target>();

        /// <summary>
        /// 动画名字
        /// </summary>
        [Header("残影动画名字")]
        public string animName;

        /// <summary>
        /// 动画层级
        /// </summary>
        [Header("残影动画层")]
        public int animLayer;

        /// <summary>
        /// 动画帧
        /// </summary>
        [Header("残影动画帧")]
        public float animFrame;

        public FrameDirAfterimage()
        {
            m_AfterimageType = AfterimageType.FrameDir;
        }

        public void RefreshPrepareData()
        {
            ReCalcTargetInfoList();
            CreatePrepareData(resGo);
        }

        protected override void OnRealDestroy()
        {
            base.OnRealDestroy();

            m_TargetInfoList.Clear();
        }

        protected override void OnRealStart()
        {
            base.OnRealStart();

            SetAnimData(animName, animLayer, animFrame);
            RefreshPrepareData();
        }

        protected override void OnRealUpdate()
        {
            
        }

        protected override void OnRealLateUpdate()
        {
#if DEBUG_MODE
            UnityEngine.Profiling.Profiler.BeginSample(PROFILING_NAME);
#endif

#if !SOUL_ENGINE
            ReCalcTargetInfoList();
#endif

            float deltatime = Time.deltaTime;
            ProduceAfterimage(deltatime);

            bool hasExist = HasActiveAfterimageExist();
            if (hasExist)
            {
                CalcCurTargetInfo(deltatime);
                CalcRotMatrix();
                CalcTransMatrix(deltatime);
            }

            CalcAllAfterimageMatrix(deltatime);
            RenderAllExistAfterimage(deltatime);

#if DEBUG_MODE
            UnityEngine.Profiling.Profiler.EndSample();
#endif
        }

        protected override void OnRealOnValidate()
        {
            SetAnimData(animName, animLayer, animFrame);
        }

        /// <summary>
        /// 是否激活残影存在
        /// </summary>
        /// <returns></returns>
        bool HasActiveAfterimageExist()
        {
            bool hasExist = false;

            foreach (var afterimage in m_AfterimageList)
            {
                if (afterimage == null)
                    continue;

                if (afterimage.IsDead())
                    continue;

                hasExist = true;
                break;
            }

            return hasExist;
        }

        /// <summary>
        /// 计算所有的残影矩阵
        /// </summary>
        /// <param name="deltatime"></param>
        void CalcAllAfterimageMatrix(float deltatime)
        {
            foreach (var afterimage in m_AfterimageList)
            {
                if (afterimage == null)
                    continue;

                if (afterimage.IsDead())
                    continue;

                afterimage.matrix = m_TransMatrix * m_RotMatrix * afterimage.initMatrix;
            }
        }

        /// <summary>
        /// 计算目标信息列表
        /// </summary>
        void ReCalcTargetInfoList()
        {
            if (targetList == null || targetList.Count <= 0)
                return;

            if (resGo == null)
                return;

            m_TargetInfoList.Clear();

            float totalTime = 0f;
            int num = targetList.Count;
            for (int i = 0; i < num; i++)
            {
                var target = targetList[i];
                if (target == null)
                    continue;

                var dstPosGo = target.dstPosGo;
                GameObject srcTempGo = null;
                GameObject dstTempGo = null;

                if (i == 0)
                {
                    srcTempGo = resGo;
                    dstTempGo = dstPosGo;
                }
                else
                {
                    var lastTarget = targetList[i - 1];
                    if (lastTarget != null)
                    {
                        srcTempGo = lastTarget.dstPosGo;
                    }

                    dstTempGo = target.dstPosGo;
                }

                if (srcTempGo == null || dstTempGo == null)
                    continue;

                TargetInfo targetInfo = new TargetInfo();
                targetInfo.isValid = true;
                targetInfo.srcPosGo = srcTempGo;
                targetInfo.dstPosGo = dstTempGo;
                // 开始时间
                targetInfo.startTime = totalTime;
                // 结束时间
                var moveSpeed = target.moveSpeed;
                var moveDis = Vector3.Distance(srcTempGo.transform.position, dstTempGo.transform.position);
                totalTime = moveDis / moveSpeed;
                targetInfo.dstTime = targetInfo.startTime + totalTime;

                m_TargetInfoList.Add(targetInfo);
            }
        }

        /// <summary>
        /// 计算当前目标信息
        /// </summary>
        void CalcCurTargetInfo(float deltatime)
        {
            m_MoveTime += deltatime;
            m_CurTargetInfo.isValid = false;

            if (m_TargetInfoList.Count > 0)
            {
                bool find = false;
                for (int i = 0; i < m_TargetInfoList.Count; i++)
                {
                    var target = m_TargetInfoList[i];
                    if (m_MoveTime < target.dstTime)
                    {
                        find = true;
                        m_CurTargetInfo.isValid = true;
                        m_CurTargetInfo.Copy(target);
                        break;
                    }
                }

                if (!find)
                {
                    m_CurTargetInfo.isValid = true;
                    m_CurTargetInfo.Copy(m_TargetInfoList[m_TargetInfoList.Count - 1]);
                }
            }
        }

        /// <summary>
        /// 计算平移矩阵
        /// </summary>
        /// <param name="deltatime"></param>
        void CalcTransMatrix(float deltatime)
        {
            m_TransMatrix = Matrix4x4.identity;

            if (!m_CurTargetInfo.isValid)
                return;

            if (resGo == null)
                return;

            var srcPos = resGo.transform.position;
            var dstPos = m_CurTargetInfo.GetCurPos(m_MoveTime);
            var dstVec = dstPos - srcPos;
            m_TransMatrix = Matrix4x4.Translate(dstVec);
        }

        /// <summary>
        /// 计算旋转矩阵
        /// </summary>
        void CalcRotMatrix()
        {
            m_RotMatrix = Matrix4x4.identity;

            if (!m_CurTargetInfo.isValid)
                return;

            var dstPosGo = m_CurTargetInfo.dstPosGo;
            if (dstPosGo == null)
                return;

            var rot = dstPosGo.transform.rotation;
            m_RotMatrix = Matrix4x4.TRS(Vector3.zero, rot, Vector3.one);
        }

        /// <summary>
        /// 产生残影
        /// </summary>
        /// <param name="deltaTime"></param>
        void ProduceAfterimage(float deltaTime)
        {
            if (m_PreDataList.Count <= 0)
                return;

            if (resGo == null)
                return;

            if (!createAfterimage)
                return;

            CreateAfterimage();
            createAfterimage = false;
            m_MoveTime = 0f;
        }
    }
}

