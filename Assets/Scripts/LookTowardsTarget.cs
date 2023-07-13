using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    public Transform target;

    private void Update()
    {
        if (target != null)
        {
            // Calculate the direction from the current position to the target
            Vector3 direction = target.position - transform.position;

            // Rotate the object to face the target
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}