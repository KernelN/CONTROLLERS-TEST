using UnityEngine;

namespace Flame.Gameplay.Player.Throw
{
    public class ThrowableStraightMover : MonoBehaviour
    {
        [SerializeField] Rigidbody rb;
        [SerializeField] float distToStartFall = 10;
        float currentDistSqr;
        Vector3 startPos;
        Universal.UpdateManager updateManager;

        float SqrDistToFall => distToStartFall * distToStartFall;

        void OnEnable()
        {
            if (!rb) rb = GetComponent<Rigidbody>();
            
            startPos = transform.position;
            currentDistSqr = 0;
            rb.useGravity = false;
            updateManager = Universal.UpdateManager.inst;
            updateManager.SuscribeToScaled(updateRate, _Update);
        }
        void OnCollisionEnter(Collision collision)
        {
            rb.useGravity = true;
            updateManager.RemoveFromScaled(updateRate, _Update);
        }
        const int updateRate = 4;
        void _Update()
        {
            currentDistSqr = (transform.position - startPos).sqrMagnitude;
            if (currentDistSqr > SqrDistToFall)
            {
                rb.useGravity = true;
                updateManager.RemoveFromScaled(updateRate, _Update);
            }
        }
    }
}