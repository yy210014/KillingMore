using UnityEngine;
using System.Collections;

public class HardShadow : MonoBehaviour
{

    Camera mChildCamera = null;

    //     // 直接引用，避免在设备上找不到
    //     public Material ReplaceMat;

    public GameObject ShadowQuad;

    // planar shadow param
    public Color ShadowColor = Color.black;
    //public float ShadowStrength = 1.0f;
    //public float PlaneD;
    //public Transform VirtualLight;

    [Range(0.01f, 1f)]
    public float Scale = 0.7f;

    public Vector2 ShadowOffset = new Vector2(0.1f, 0.1f);

    static HardShadow msHardShadow = null;

    public static HardShadow HardShadow_
    {
        get
        {
            return msHardShadow;
        }
    }

    RenderTexture mRT;

    // FIMXE
    //RaycastHit[] mColliders;

    // Use this for initialization
    void Start()
    {

        msHardShadow = this;

        //Shader blurShader = Shader.Find("Hidden/SxShadowBlur");
        //mBlurMat = new Material(blurShader);

        //Shader overlappShader = Shader.Find("Hidden/SxShadowComposer");
        //mOverlappingMat = new Material(overlappShader);

        Transform childTf = transform.FindChild("HardShadow");
        mChildCamera = childTf.GetComponent<Camera>();
        mChildCamera.clearFlags = CameraClearFlags.SolidColor;
        mChildCamera.backgroundColor = new Color(0, 0, 0, 0);
        mChildCamera.cullingMask = 1 << LayerMask.NameToLayer("LaserActive");
        mChildCamera.depth = GetComponent<Camera>().depth - 4;

        mRT = new RenderTexture(Screen.width, Screen.height, 0);
        mChildCamera.targetTexture = mRT;
        mRT.wrapMode = TextureWrapMode.Clamp;
    }

    // Update is called once per frame
    void Update()
    {
        mChildCamera.transform.position = GetComponent<Camera>().transform.position;

        Vector3 pos = mChildCamera.transform.localPosition;
        pos.x = ShadowOffset.x;
        pos.y = ShadowOffset.y;
        mChildCamera.transform.localPosition = pos;

        mChildCamera.transform.rotation = GetComponent<Camera>().transform.rotation;
        mChildCamera.fieldOfView = GetComponent<Camera>().fieldOfView;
        mChildCamera.aspect = GetComponent<Camera>().aspect;
        mChildCamera.nearClipPlane = GetComponent<Camera>().nearClipPlane;
        mChildCamera.farClipPlane = GetComponent<Camera>().farClipPlane;
        //mChildCamera.orthographicSize = camera.orthographicSize;

        ShadowQuad.GetComponent<Renderer>().material.SetTexture("_MainTex", mRT);

//         Vector3 pos = ShadowQuad.transform.localPosition;
//         pos.x = ShadowOffset.x;
//         pos.y = ShadowOffset.y;
//         ShadowQuad.transform.localPosition = pos;

        Shader.SetGlobalColor("_ShadowColor", ShadowColor);
    }

//     void OnPreRender()
//     {
//         if (mColliders != null && mColliders.Length > 0)
//         {
//             for (int i = 0; i < mColliders.Length; i++)
//             {
//                 mColliders[i].collider.transform.localScale /= Scale;
//             }
//         }
// 
//         Shader.SetGlobalFloat("_Scale", 1.0f);
//     }
// 
//     void OnPostRender()
//     {
    //         int layerMask = LayerMask.NameToLayer("LaserActive");
//         layerMask = 1 << layerMask;
//         mColliders = Physics.SphereCastAll(transform.position, 100f, Vector3.zero, 0f, layerMask);
// 
//         if (mColliders != null && mColliders.Length > 0)
//         {
//             for (int i = 0; i < mColliders.Length; i++)
//             {
//                 mColliders[i].collider.transform.localScale *= Scale;
//             }
//         }
// 
//         Shader.SetGlobalFloat("_Scale", Scale);
//     }
}