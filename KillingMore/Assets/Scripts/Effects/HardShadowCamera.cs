using UnityEngine;
using System.Collections;

public class HardShadowCamera : MonoBehaviour {

    HardShadow mShadowCtrl;

	// Use this for initialization
	void Start () {

        mShadowCtrl = transform.parent.GetComponent<HardShadow>();

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    // FIMXE
    Collider[] mColliders;

    void OnPreCull()
    {
        int layerMask = LayerMask.NameToLayer("LaserActive");
        layerMask = 1 << layerMask;
        mColliders = Physics.OverlapSphere(transform.position, 500f, layerMask);

        if (mColliders != null && mColliders.Length > 0)
        {
            for (int i = 0; i < mColliders.Length; i++)
            {
                mColliders[i].gameObject.transform.localScale = mColliders[i].gameObject.transform.localScale * mShadowCtrl.Scale;
            }
        }
    }

    void OnPostRender()
    {
        if (mColliders != null && mColliders.Length > 0)
        {
            for (int i = 0; i < mColliders.Length; i++)
            {
                mColliders[i].gameObject.transform.localScale = mColliders[i].gameObject.transform.localScale / mShadowCtrl.Scale;
            }
        }
    }
}
