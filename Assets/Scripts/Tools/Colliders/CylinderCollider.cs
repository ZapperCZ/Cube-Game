using UnityEngine;

[ExecuteInEditMode]
public class CylinderCollider : MonoBehaviour
{
    [Header ("Cylinder Parameters")]
    [Range(6, 50)]
    [SerializeField] int cylinderSides = 30;
    [Range(0.1f, 10)]
    [SerializeField] float cylinderSideWidth = 0.1051042f;
    [SerializeField] bool sideWidthLocked = true;
    [Range(0.1f, 5)]
    [SerializeField] float cylinderDiameter = 1;
    [SerializeField] bool diameterLocked = true;
    public float cylinderHeight = 2;

    [Header ("Component Settings")]
    [SerializeField] string colliderPartName = "CyllinderColliderPart";
    [SerializeField] bool isTrigger = false;                    //Determines whether the colliders should be a trigger or not
    [SerializeField] bool usePhysicsMaterial = false;           //Whether the physics material should be used or not
    [SerializeField] PhysicMaterial CylinderPhysicsMaterial;    //The physics material that should be applied to the colliders

    [Header ("Debug")] 
    [SerializeField] bool runOutsideEditMode = false;           //When enabled, the colliders cannot be altered by this script when play mode is enabled 
    [SerializeField] bool deleteColliders = false;              //A poor mans button for deleting colliders
    public bool changed = false;                                //A bool other scripts can reference to detect when the collider has been changed by user

    Transform Parent;               //The parent of this script
    bool prevSideWidthLocked;
    bool prevDiameterLocked;
    //float prevSides;
    float prevSideWidth;
    float prevDiameter;
    bool regenerate = false;        
    bool destroyColliders = false;
    bool destroyManager = false;
    bool addManager = false;

    void Start()
    {
        if(Application.isPlaying && !runOutsideEditMode)
        {
            this.enabled = false;
            return;
        }

        Parent = this.transform;
        prevSideWidth = cylinderSideWidth;
        prevDiameter = cylinderDiameter;
        prevSideWidthLocked = sideWidthLocked;
        prevDiameterLocked = diameterLocked;
        CreateCylinderCollider();
    }
    void OnValidate()
    {
        /*
         * TODO: Only regenerate when number of sides is changed, otherwise change the transform of existing colliders 
         * as that is less resource intensive
        */

        //Allows only even numbers of sides
        if (cylinderSides % 2 == 1)
        {
            cylinderSides -= 1;
        }

        if (isTrigger && !HasComponent<TriggerChildManager>(this.gameObject))    //Collider changed to trigger
        {
            addManager = true;
        }
        else if (!isTrigger && HasComponent<TriggerChildManager>(this.gameObject))   //Collider changed from trigger
        {
            destroyManager = true;
        }

        //Locks the currently changing variable
        //Has to be an else if otherwise they would affect each other
        if (prevSideWidth != cylinderSideWidth)
        {
            prevSideWidth = cylinderSideWidth;
            sideWidthLocked = true;
        }
        else if (prevDiameter != cylinderDiameter)
        {
            prevDiameter = cylinderDiameter;
            diameterLocked = true;
        }

        if (deleteColliders == true)
        {
            deleteColliders = false;
            destroyColliders = true;
            return;
        }

        //Only 1 can be checked at the same time, but both can be unchecked
        if (sideWidthLocked && diameterLocked)
        {
            if (sideWidthLocked != prevSideWidthLocked)
            {
                prevSideWidthLocked = sideWidthLocked;
                diameterLocked = prevDiameterLocked = !sideWidthLocked;
            }
            if (diameterLocked != prevDiameterLocked)
            {
                prevDiameterLocked = diameterLocked;
                sideWidthLocked = prevSideWidthLocked = !diameterLocked;
            }
        }

        //TODO: Maybe switch the complex equation for my approximation once the n-gon gets complex

        if (sideWidthLocked)
        {
            //prevDiameter = cylinderDiameter = cylinderSides * cylinderSideWidth / Mathf.PI;               //My approximation
            prevDiameter = cylinderDiameter = cylinderSideWidth / Mathf.Tan(Mathf.PI / cylinderSides);      //Precise equation
        }
        if (diameterLocked)
        {
            //prevSideWidth = cylinderSideWidth = Mathf.PI * cylinderDiameter / cylinderSides;              //My approximation
            prevSideWidth = cylinderSideWidth = cylinderDiameter * Mathf.Tan(Mathf.PI / cylinderSides);     //Precise equation
        }

        //If no physics material is assigned, disable it's usage    
        if(usePhysicsMaterial && CylinderPhysicsMaterial == null)
        {
            usePhysicsMaterial = false;
        }

        regenerate = true;
    }
    void Update()
    {
        if (destroyColliders)
        {
            DestroyCylinderCollider();
            destroyColliders = false;
        }
        if (addManager)
        {
            Parent.gameObject.AddComponent<TriggerChildManager>();
            addManager = false;
        }
        if (destroyManager)
        {
            if (Application.isPlaying)
            {
                Destroy(Parent.GetComponent<TriggerChildManager>());
            }
            else if (Application.isEditor)
            {
                DestroyImmediate(Parent.GetComponent<TriggerChildManager>());
            }
            destroyManager = false;
        }
        if (regenerate)
        {
            //Debug.Log($"Cylinder Collider - {Parent.name} - Regenerating");
            regenerate = false;
            //TODO: Don't re-create the colliders, but instead change their values since that is less resource intensive
            CreateCylinderCollider();
        }
    }
    void DestroyCylinderCollider()
    {
        int childCount = Parent.childCount;

        for(int i = 0; i < childCount; i++)
        {
            Transform childToDelete = null;
            if (Application.isPlaying)
            {
                childToDelete = Parent.GetChild(i);
            }
            else if (Application.isEditor)
            {
                childToDelete = Parent.GetChild(0);
            }
            if (childToDelete.name == colliderPartName)      //The loaded child object is part of the collider - prevents the deletion of objects that are accidentally made children of the collider
            {
                DestroyGameObjectSafely(childToDelete.gameObject);
            }
        }

    }
    void CreateCylinderCollider()
    {
        DestroyCylinderCollider();
        for (int i = 0; i < cylinderSides / 2; i++)
        {
            GameObject collider = new GameObject();
            collider.layer = 7;                                             //Non-Static layer
            collider.name = colliderPartName;
            collider.AddComponent<BoxCollider>().isTrigger = isTrigger;
            if (isTrigger)
            {
                collider.AddComponent<Rigidbody>().useGravity = false;          //Trigger needs a Rigidbody to detect collisions properly
                collider.GetComponent<Rigidbody>().isKinematic = true;
                collider.AddComponent<TriggerChild>();                          //TODO: Pass a layermask into this
            }
            if (usePhysicsMaterial)
            {
                collider.GetComponent<BoxCollider>().material = CylinderPhysicsMaterial;
            }
            collider.transform.SetParent(Parent);
            collider.transform.localPosition = new Vector3(0, 0, 0);        //Set it at the position of it's parent
            float rotationY = (float)i * 180 / (float)cylinderSides * 2;

            //TODO: Set the variables to be global and not local
            collider.transform.localRotation = Quaternion.Euler(0, rotationY, 0);   //Rotate it so the final shape forms an n-gon
            collider.transform.localScale = new Vector3(cylinderSideWidth, cylinderHeight, cylinderDiameter);    //Set it's size according to parameters
        }
        if (isTrigger)
        {
            transform.GetComponent<TriggerChildManager>().isTriggered = false;
        }
        changed = true;
    }
    void DestroyGameObjectSafely(GameObject obj)            //Destroys the object safely regardless of the application state
    {
        if (Application.isPlaying)
        {
            Destroy(obj);
        }
        else if (Application.isEditor)
        {
            DestroyImmediate(obj);
        }
    }
    bool HasComponent<T>(GameObject inputObject) where T : Component //Returns whether the input object has a component or not
    {
        return inputObject.GetComponent<T>() != null;
    }
}