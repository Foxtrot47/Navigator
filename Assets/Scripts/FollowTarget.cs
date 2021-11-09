using UnityEngine;
using System.Collections;
using UnityEngine.UI;
// Attach this script to the AR camera
public class FollowTarget : MonoBehaviour
{
    public Transform targetToFollow;    // The transform of the gameobject that gets followed
    public Quaternion targetRot;        // The rotation of the device camera from Frame.Pose.rotation    
    public RawImage minimap;            // The rawimage the view of the camera gets rendered to
    public GameObject arrow;            // The direction indicator on the person indicator
    public float rotationSmoothingSpeed = 1.5f; // rotation speed, change to personal preference

    // Use lateUpdate to assure that the camera is updated after the target has been updated.
    void LateUpdate()
    {
        if (!targetToFollow)
            return;
        //receive rotation from camera
        Vector3 targetEulerAngles = targetRot.eulerAngles;

        // Calculate the current rotation angle around the Y axis we want to apply to the camera.
        // We add 180 degrees as the device camera points to the negative Z direction
        float rotationToApplyAroundY = targetEulerAngles.y; //+ 180;
        // Smooth interpolation between current camera rotation angle and the rotation angle we want to apply.
        // Use LerpAngle to handle correctly when angles > 360
        float newCamRotAngleY = Mathf.LerpAngle(arrow.transform.eulerAngles.y, rotationToApplyAroundY, rotationSmoothingSpeed * Time.deltaTime);
        Quaternion newCamRotYQuat = Quaternion.Euler(0, newCamRotAngleY, 0);
        //extra check to make sure that the rotation of the arrow does not change when accessing mapview from placing phone horizontal
        if(targetEulerAngles.x < 65)
        {
            arrow.transform.rotation = newCamRotYQuat;
        }
    }
}
