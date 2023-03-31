using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace gtm.Scene
{
    public enum AfterimageType
    {
        Invalid = -1,   // 无效
        Default,        // 默认
        FrameDir,       // 帧方向
    }

    public abstract class BaseAfterimage : MonoBehaviour
    {
        public const int NUM_8 = 8;

        public const int NUM_32 = 32;

        public const int NUM_64 = 64;

        const int DEFAULT_RENDER_QUEUE = 3010;

        const int HAIR_RENDER_QUEUE = 3020;

        /// <summary>
        /// 头发名字
        /// </summary>
        const string HAIR_NAME = "hair";

        const string MAT_COLOR_PROPERTY = "_BaseColor";

        const string MAT_RIM_COLOR = "_RimColor";

        const string MAT_RIM_POWER = "_RimPower";

        const string MAT_RIM_INTENSITY = "_RimIntensity";

        const string MAT_GLOBAL_COLOR_PROPERTY = "_GlobalColor";

        const string AFTERIMAGE_SHADER_NAME = "LingRen/Scene/Afterimage";

        /// <summary>
        /// 指定的动画数据
        /// </summary>
        public class AnimData
        {
            /// <summary>
            /// 使用指定动画数据
            /// </summary>
            public bool useAnim;

            /// <summary>
            /// 指定的动画名字
            /// </summary>
            public string animName;

            /// <summary>
            /// 动画层级
            /// </summary>
            public int animLayer;

            /// <summary>
            /// 指定的动画帧
            /// </summary>
            public float animFrame;
        }

        /// <summary>
        /// 准备数据
        /// </summary>
        public class PrepareData
        {
            /// <summary>
            /// 是否skin
            /// </summary>
            public bool isSkin;

            /// <summary>
            /// 是否头发
            /// </summary>
            public bool isHair;

            /// <summary>
            /// 纹皮render
            /// </summary>
            public SkinnedMeshRenderer skinRender;

            /// <summary>
            /// mesh filter
            /// </summary>
            public MeshFilter meshFilter;

            /// <summary>
            /// mesh render
            /// </summary>
            public MeshRenderer meshRender;

            /// <summary>
            /// 主材质
            /// </summary>
            public Texture mainTex;

            /// <summary>
            /// 颜色
            /// </summary>
            public Color baseColor;
        }

        /// <summary>
        /// 残影数据
        /// </summary>
        public class AfterimageMesh
        {
            public void Clear()
            {
                mesh = null;
                material = null;
            }

            public bool IsDead()
            {
                return leftTime <= 0;
            }

            public bool isSkinMesh;
            public Mesh mesh;
            public Material material;
            public Matrix4x4 initMatrix;
            public Matrix4x4 matrix;
            public float leftTime;
        }

        /// <summary>
        /// 层
        /// </summary>
        int m_Layer;

        /// <summary>
        /// 残影材质
        /// </summary>
        Material m_AfterimageMat;

        /// <summary>
        /// 动画
        /// </summary>
        Animator m_Anim;

        /// <summary>
        /// 指定的动画数据
        /// </summary>
        AnimData m_AnimData = new AnimData();

        /// <summary>
        /// 可使用的材质
        /// </summary>
        List<Material> m_FreeMatList = new List<Material>(NUM_32);

        /// <summary>
        /// 可使用的Mesh
        /// </summary>
        List<Mesh> m_FreeMeshList = new List<Mesh>(NUM_32);

        /// <summary>
        /// 可使用的afterimage
        /// </summary>
        List<AfterimageMesh> m_FreeAfterimageList = new List<AfterimageMesh>(NUM_32);

        /// <summary>
        /// 每帧需要销毁的残影
        /// </summary>
        List<AfterimageMesh> m_DeadAfterimageList = new List<AfterimageMesh>(NUM_8);

        /// <summary>
        /// 残影类型
        /// </summary>
        protected AfterimageType m_AfterimageType;

        /// <summary>
        /// 残影列表
        /// </summary>
        protected List<AfterimageMesh> m_AfterimageList = new List<AfterimageMesh>(NUM_32);

        /// <summary>
        /// 准备数据
        /// </summary>
        protected List<PrepareData> m_PreDataList = new List<PrepareData>(NUM_8);

        /// <summary>
        /// 开始创建残影
        /// </summary>
        [Header("是否创建残影")]
        public bool createAfterimage = false;

        /// <summary>
        /// 残影显示时间
        /// </summary>
        [Header("Afterimage display time, in seconds 残影显示的时间，单位秒")]
        public float timeAfterimageShow = 0.4f;

        /// <summary>
        /// 最大alpha百分比
        /// </summary>
        [Header("残影的最大Alpha值，残影从这个值变淡消失")]
        [Range(0, 1)]
        public float maxAlphaPercent = 0.25f;

        /// <summary>
        /// 全局颜色
        /// </summary>
        [Header("作用于全部残影的颜色")]
        [ColorUsage(true, true)]
        public Color globalColor = Color.white;

        [Header("Rim颜色")]
        [ColorUsage(true, true)]
        public Color rimColor = Color.black;

        [Header("Rim的power")]
        public float rimPower = 2;

        [Header("Rim的强度")]
        public float rimIntensity = 1;

        /// <summary>
        /// 不显示的模型名字列表
        /// </summary>
        [Header("不可见模型名字列表")]
        [SerializeField]
        public List<string> unVisMeshNameList = new List<string>(NUM_8);

        /// <summary>
        /// Start
        /// </summary>
        protected virtual void OnRealStart()
        {
            CalcLayer();
        }

        /// <summary>
        /// Update
        /// </summary>
        protected virtual void OnRealUpdate()
        {

        }

        /// <summary>
        /// LateUpdate
        /// </summary>
        protected virtual void OnRealLateUpdate()
        {

        }

        /// <summary>
        /// Destroy
        /// </summary>
        protected virtual void OnRealDestroy()
        {
            m_FreeMeshList.Clear();
            m_FreeMatList.Clear();
            m_FreeAfterimageList.Clear();
            m_DeadAfterimageList.Clear();
            m_AfterimageList.Clear();
            m_PreDataList.Clear();
            m_AfterimageMat = null;
        }

        protected virtual void OnRealOnValidate()
        {

        }

        /// <summary>
        /// 执行动画数据
        /// </summary>
        protected void ExecAnimData()
        {
            if (m_Anim == null)
                return;

            if (!m_AnimData.useAnim)
                return;

            m_Anim.Play(m_AnimData.animName, m_AnimData.animLayer, m_AnimData.animFrame);
            m_Anim.Update(0);
        }

        /// <summary>
        /// 设置动画数据
        /// </summary>
        /// <param name="animName"></param>
        /// <param name="animFrame"></param>
        protected void SetAnimData(string animName, int animLayer, float animFrame)
        {
            if (string.IsNullOrEmpty(animName))
                return;

            m_AnimData.useAnim = true;
            m_AnimData.animName = animName;
            m_AnimData.animLayer = animLayer;
            m_AnimData.animFrame = animFrame;
        }

        /// <summary>
        /// 创建准备数据
        /// </summary>
        protected void CreatePrepareData(GameObject resGo)
        {
            if (resGo == null)
                return;

            // 获取Animator
            m_Anim = resGo.GetComponentInChildren<Animator>();

            m_PreDataList.Clear();

            SkinnedMeshRenderer[] skinrenderarray = resGo.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (skinrenderarray != null && skinrenderarray.Length > 0)
            {
                foreach (var skinrender in skinrenderarray)
                {
                    if (skinrender == null)
                        continue;

                    if (!skinrender.gameObject.activeSelf)
                        continue;

                    // 粒子不添加
                    var particlecom = skinrender.GetComponent<ParticleSystem>();
                    if (particlecom != null)
                        continue;

                    // 是否不可见模型名字
                    bool isunvismeshname = IsUnVisMeshName(skinrender.name);
                    if (isunvismeshname)
                        continue;

                    PrepareData data = new PrepareData();
                    data.isSkin = true;

                    data.isHair = false;
                    if (skinrender.name.Contains(HAIR_NAME))
                    {
                        data.isHair = true;
                    }

                    data.skinRender = skinrender;

                    Material mat = skinrender.sharedMaterial;
                    if (mat != null)
                    {
                        data.mainTex = GetSkinMeshTexture(mat);
                        data.baseColor = GetSkinMeshColor(mat);
                    }


                    m_PreDataList.Add(data);
                }
            }

            MeshRenderer[] meshrenderarray = resGo.GetComponentsInChildren<MeshRenderer>();
            if (meshrenderarray != null && meshrenderarray.Length > 0)
            {
                foreach (var meshrender in meshrenderarray)
                {
                    if (meshrender == null)
                        continue;

                    if (!meshrender.gameObject.activeSelf)
                        continue;

                    // 粒子不添加
                    var particlecom = meshrender.GetComponent<ParticleSystem>();
                    if (particlecom != null)
                        continue;

                    // 是否不可见模型名字
                    bool isunvismeshname = IsUnVisMeshName(meshrender.name);
                    if (isunvismeshname)
                        continue;

                    PrepareData data = new PrepareData();
                    data.isSkin = false;
                    data.isHair = false;
                    data.meshRender = meshrender;
                    data.meshFilter = meshrender.GetComponent<MeshFilter>();

                    Material mat = meshrender.sharedMaterial;
                    if (mat != null)
                    {
                        data.mainTex = GetMeshTexture(mat);
                        data.baseColor = GetMeshColor(mat);
                    }

                    m_PreDataList.Add(data);
                }
            }
        }

        /// <summary>
        /// 创建残影
        /// </summary>
        protected void CreateAfterimage()
        {
            // 执行动画数据
            ExecAnimData();

            foreach (var predata in m_PreDataList)
            {
                if (predata == null)
                    continue;

                bool isSkinMesh = false;
                Matrix4x4 matrix = Matrix4x4.identity;
                var mesh = CreateAfterimageMesh(predata, ref matrix, ref isSkinMesh);
                if (mesh == null)
                    continue;

                var mat = CreateAfterimageMat(predata);
                if (mat == null)
                    continue;

                AfterimageMesh aftermesh = GetFreeAfterimage();
                if (aftermesh != null)
                {
                    aftermesh.isSkinMesh = isSkinMesh;
                    aftermesh.mesh = mesh;
                    aftermesh.material = mat;
                    aftermesh.leftTime = timeAfterimageShow;
                    aftermesh.initMatrix = matrix;
                    aftermesh.matrix = matrix;

                    m_AfterimageList.Add(aftermesh);
                }
            }
        }

        /// <summary>
        /// 渲染所有的残影
        /// </summary>
        protected void RenderAllExistAfterimage(float deltaTime)
        {
            m_DeadAfterimageList.Clear();

            foreach (var afterimage in m_AfterimageList)
            {
                if (afterimage == null)
                    continue;

                if (afterimage.IsDead())
                {
                    m_DeadAfterimageList.Add(afterimage);
                    continue;
                }

                Material mat = afterimage.material;
                if (mat == null)
                {
                    m_DeadAfterimageList.Add(afterimage);
                    continue;
                }

                Mesh mesh = afterimage.mesh;
                if (mesh == null)
                {
                    m_DeadAfterimageList.Add(afterimage);
                    continue;
                }

                Graphics.DrawMesh(mesh, afterimage.matrix, mat, m_Layer, null, 0, null, false);

                afterimage.leftTime -= deltaTime;

                if (mat.HasProperty(MAT_COLOR_PROPERTY))
                {
                    Color c = mat.GetColor(MAT_COLOR_PROPERTY);
                    c.a = Mathf.Max(0, afterimage.leftTime / timeAfterimageShow * maxAlphaPercent);

                    mat.SetColor(MAT_COLOR_PROPERTY, c);
                }
            }

            foreach (var deadobj in m_DeadAfterimageList)
            {
                if (deadobj == null)
                    continue;

                Material mat = deadobj.material;
                if (mat != null)
                {
                    m_FreeMatList.Add(mat);
                }

                if (deadobj.isSkinMesh)
                {
                    Mesh mesh = deadobj.mesh;
                    if (mesh != null)
                    {
                        m_FreeMeshList.Add(mesh);
                    }
                }

                deadobj.Clear();

                m_FreeAfterimageList.Add(deadobj);
            }

            foreach (var deadobj in m_DeadAfterimageList)
            {
                if (deadobj == null)
                    continue;

                m_AfterimageList.Remove(deadobj);
            }
        }

        /// <summary>
        /// 开始
        /// </summary>
        void Start()
        {
            OnRealStart();
        }

        /// <summary>
        /// 刷新
        /// </summary>
        void Update()
        {
            OnRealUpdate();
        }

        /// <summary>
        /// LateUpdate
        /// </summary>
        void LateUpdate()
        {
            OnRealLateUpdate();
        }

        /// <summary>
        /// Destroy
        /// </summary>
        void OnDestroy()
        {
            OnRealDestroy();
        }

        void OnValidate()
        {
            OnRealOnValidate();
        }

        /// <summary>
        /// 创建模型
        /// </summary>
        /// <param name="predata"></param>
        /// <param name="matrix"></param>
        /// <param name="isSkinMesh"></param>
        /// <returns></returns>
        Mesh CreateAfterimageMesh(PrepareData predata, ref Matrix4x4 matrix, ref bool isSkinMesh)
        {
            if (predata == null)
                return null;

            Mesh mesh = null;

            if (predata.isSkin)
            {
                var skinrender = predata.skinRender;
                if (skinrender != null)
                {
                    isSkinMesh = true;

                    mesh = GetFreeMesh();
                    if (mesh != null)
                    {
                        skinrender.BakeMesh(mesh);

                        //mesh.name = skinrender.name;

                        if (skinrender.gameObject != null)
                        {
                            //matrix = skinrender.gameObject.transform.localToWorldMatrix;

                            var trans = skinrender.gameObject.transform;
                            if (trans != null)
                            {
                                matrix = Matrix4x4.TRS(trans.position, trans.rotation, Vector3.one);
                            }
                        }
                    }
                }
            }
            else
            {
                var render = predata.meshFilter;
                if (render != null)
                {
                    if (render.sharedMesh != null)
                    {
                        mesh = render.sharedMesh;
                    }
                    else if (render.mesh != null)
                    {
                        mesh = render.mesh;
                    }

                    if (render.gameObject != null)
                    {
                        matrix = render.gameObject.transform.localToWorldMatrix;
                    }
                }
            }

            return mesh;
        }

        /// <summary>
        /// 创建材质
        /// </summary>
        /// <param name="predata"></param>
        /// <returns></returns>
        Material CreateAfterimageMat(PrepareData predata)
        {
            if (predata == null)
                return null;

            Material mat = null;

            if (predata.isSkin)
            {
                var skinrender = predata.skinRender;
                if (skinrender != null)
                {
                    mat = CreateMat();
                }
            }
            else
            {
                var render = predata.meshRender;
                if (render != null)
                {
                    mat = CreateMat();
                }
            }

            if (mat != null)
            {
                if (predata.isHair)
                {
                    mat.renderQueue = HAIR_RENDER_QUEUE;
                }
                else
                {
                    mat.renderQueue = DEFAULT_RENDER_QUEUE;
                }

                if (predata.mainTex != null)
                {
                    mat.mainTexture = predata.mainTex;
                }
                else
                {
                    mat.mainTexture = null;
                }

                mat.SetColor(MAT_COLOR_PROPERTY, predata.baseColor);
                mat.SetColor(MAT_GLOBAL_COLOR_PROPERTY, globalColor);

                mat.SetColor(MAT_RIM_COLOR, rimColor);
                mat.SetFloat(MAT_RIM_POWER, rimPower);
                mat.SetFloat(MAT_RIM_INTENSITY, rimIntensity);
            }

            return mat;
        }

        /// <summary>
        /// 创建材质
        /// </summary>
        /// <returns></returns>
        Material CreateMat()
        {
#if SOUL_ENGINE && !UNITY_EDITOR
            if (m_AfterimageMat == null)
            {
                m_AfterimageMat = LoadBundleMaterial(AFTERIMAGE_SHADER_NAME);
            }
#else
            if (m_AfterimageMat == null)
            {
                Shader shader = Shader.Find(AFTERIMAGE_SHADER_NAME);
                if (shader != null)
                {
                    m_AfterimageMat = new Material(shader);
                }
            }
#endif
            Material mat = GetFreeMat();
            return mat;
        }
        
#if SOUL_ENGINE
        Material LoadBundleMaterial(string shadername)
        {
            SoulEngine.IKernel kernel = SoulEngine.GameWorld.gameKernel;
            if (kernel == null)
                return null;
            
            PostEffectsModule module = kernel.GetModule<PostEffectsModule>();
            if (module == null)
                return null;

            return module.GetPostEffectMaterial(shadername);
        }
#endif

        /// <summary>
        /// 是否不可见模型名字
        /// </summary>
        /// <returns></returns>
        bool IsUnVisMeshName(string meshname)
        {
            foreach (var name in unVisMeshNameList)
            {
                if (name == meshname)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 获取纹皮模型主贴图
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        Texture GetSkinMeshTexture(Material mat)
        {
            Texture tex = null;
            if (tex == null)
            {
                if (mat.HasProperty("_BaseMap"))
                {
                    tex = mat.GetTexture("_BaseMap");
                }
            }

            if (tex == null)
            {
                if (mat.HasProperty("_MainTex"))
                {
                    tex = mat.GetTexture("_MainTex");
                }
            }

            if (tex == null)
            {
                if (mat.HasProperty("_MatcapTex"))
                {
                    tex = mat.GetTexture("_MatcapTex");
                }
            }

            return tex;
        }

        /// <summary>
        /// 获取纹皮模型颜色
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        Color GetSkinMeshColor(Material mat)
        {
            Color newcolor = Color.white;
            bool find = false;

            if (mat.HasProperty("_BaseColor"))
            {
                newcolor = mat.GetColor("_BaseColor");
                find = true;
            }

            if (!find)
            {
                if (mat.HasProperty("_Color"))
                {
                    newcolor = mat.GetColor("_Color");
                    find = true;
                }
            }

            return newcolor;
        }

        /// <summary>
        /// 获取模型材质
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        Texture GetMeshTexture(Material mat)
        {
            Texture tex = null;
            if (tex == null)
            {
                if (mat.HasProperty("_BaseMap"))
                {
                    tex = mat.GetTexture("_BaseMap");
                }
            }

            if (tex == null)
            {
                if (mat.HasProperty("_MainTex"))
                {
                    tex = mat.GetTexture("_MainTex");
                }
            }

            if (tex == null)
            {
                if (mat.HasProperty("_MatcapTex"))
                {
                    tex = mat.GetTexture("_MatcapTex");
                }
            }

            return tex;
        }

        /// <summary>
        /// 获取模型颜色
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        Color GetMeshColor(Material mat)
        {
            Color newcolor = Color.white;
            bool find = false;

            if (mat.HasProperty("_BaseColor"))
            {
                newcolor = mat.GetColor("_BaseColor");
                find = true;
            }

            if (!find)
            {
                if (mat.HasProperty("_Color"))
                {
                    newcolor = mat.GetColor("_Color");
                    find = true;
                }
            }

            return newcolor;
        }

        /// <summary>
        /// 创建空闲的残影
        /// </summary>
        /// <returns></returns>
        AfterimageMesh GetFreeAfterimage()
        {
            AfterimageMesh aftermesh = null;
            bool find = false;
            if (m_FreeAfterimageList.Count > 0)
            {
                var cachemesh = m_FreeAfterimageList[0];
                m_FreeAfterimageList.RemoveAt(0);

                if (cachemesh != null)
                {
                    aftermesh = cachemesh;
                    find = true;
                }
            }

            if (!find)
            {
                aftermesh = new AfterimageMesh();
            }

            return aftermesh;
        }

        /// <summary>
        /// 创建空闲的模型
        /// </summary>
        /// <returns></returns>
        Mesh GetFreeMesh()
        {
            Mesh mesh = null;
            bool find = false;
            if (m_FreeMeshList.Count > 0)
            {
                var cachemesh = m_FreeMeshList[0];
                m_FreeMeshList.RemoveAt(0);

                if (cachemesh != null)
                {
                    mesh = cachemesh;
                    find = true;
                }
            }

            if (!find)
            {
                mesh = new Mesh();
            }

            return mesh;
        }

        /// <summary>
        /// 获取空闲的材质
        /// </summary>
        /// <returns></returns>
        Material GetFreeMat()
        {
            Material mat = null;

            bool find = false;
            if (m_FreeMatList.Count > 0)
            {
                var cachemat = m_FreeMatList[0];
                m_FreeMatList.RemoveAt(0);

                if (cachemat != null)
                {
                    mat = cachemat;
                    find = true;
                }
            }

            if (!find)
            {
                if (m_AfterimageMat != null)
                {
                    mat = new Material(m_AfterimageMat);
                }
            }

            return mat;
        }

        /// <summary>
        /// 计算层级
        /// </summary>
        void CalcLayer()
        {
            m_Layer = gameObject.layer;
        }
    }
}

