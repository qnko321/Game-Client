using UnityEngine;

public class NoPlayerCamera : MonoBehaviour
{
    private void FixedUpdate()
    {
        if (Camera.main != null)
        {
            this.gameObject.SetActive(false);
        }
    }
}
