using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace gtm.Scene
{
    [ExecuteInEditMode]
    public class Afterimage : BaseAfterimage
    {
        /// <summary>
        /// ��������
        /// </summary>
        const string PROFILING_NAME = "Afterimage";

        /// <summary>
        /// ��ǰ�ļ��ʱ��
        /// </summary>
        float m_CurIntervalTime;

        /// <summary>
        /// ʱ����
        /// </summary>
        [Header("ÿ�����ɲ�Ӱ֮��ļ��ʱ�䣬��λ�� Time between each generation of afterimages��In seconds")]
        public float timeInterval = 0.075f;

        public Afterimage()
        {
            m_AfterimageType = AfterimageType.Default;
        }

        public void RefreshPrepareData()
        {
            CreatePrepareData(gameObject);
        }

        // Start is called before the first frame update
        protected override void OnRealStart()
        {
            base.OnRealStart();

            m_CurIntervalTime = 0f;
            RefreshPrepareData();
        }

        // Update is called once per frame
        protected override void OnRealUpdate()
        {

        }

        protected override void OnRealLateUpdate()
        {
#if DEBUG_MODE
            UnityEngine.Profiling.Profiler.BeginSample(PROFILING_NAME);
#endif

            float deltatime = Time.deltaTime;
            ProduceAfterimage(deltatime);
            RenderAllExistAfterimage(deltatime);

#if DEBUG_MODE
            UnityEngine.Profiling.Profiler.EndSample();
#endif
        }

        protected override void OnRealDestroy()
        {
            base.OnRealDestroy();
        }

        /// <summary>
        /// ������Ӱ
        /// </summary>
        void ProduceAfterimage(float deltaTime)
        {
            if (m_PreDataList.Count <= 0)
                return;

            bool cancreate = createAfterimage;

            m_CurIntervalTime += deltaTime;

            if (cancreate)
            {
                if (m_CurIntervalTime > timeInterval)
                {
                    CreateAfterimage();
                    m_CurIntervalTime = 0f;
                }
            }
        }
    }
}

