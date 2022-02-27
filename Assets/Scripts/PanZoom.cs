using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// class that handles map zooming and panning
public class PanZoom : MonoBehaviour
{
    // change variables by preference
    public float orthoZoomSpeed = 0.01f;
    public float zoomOutMin = 5;
    public float zoomOutMax = 15;
    public bool enableTouch = false;
    public GameObject dropdown; //dropdown UI, needed to stop panning when scrolling in dropdown
    public Camera ChosenCamera;
    private Vector3 touchStart; // start of finger touch

    void Update()
    {
        if(enableTouch) {
            // mouse of fingertouch moves camera
            if (Input.GetMouseButtonDown(0))
            {
                touchStart = ChosenCamera.ScreenToWorldPoint(Input.mousePosition);
            }

            // detect double tap
            for (var i = 0; i < Input.touchCount; ++i)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                {
                    if (Input.GetTouch(i).tapCount == 2)
                    {
                        StartCoroutine(Pause());
                    }
                }
            }

            // If there are two touches on the device...
            if (Input.touchCount == 2)
            {
                // Store both touches.
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                // Find the position in the previous frame of each touch.
                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                // Find the magnitude of the vector (the distance) between the touches in each frame.
                float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                // Find the difference in the distances between each frame.
                float deltaMagnitudeDiff = touchDeltaMag - prevTouchDeltaMag;

                Zoom(deltaMagnitudeDiff * orthoZoomSpeed);
            }
            //single press pan with camera, only if not scrolling in dropdown
            else if (Input.GetMouseButton(0))
            {
                bool selectingDest = false;
                foreach (Transform child in dropdown.transform)
                {
                    if (child.name.Equals("Dropdown List"))
                    {
                        selectingDest = true;
                        break;
                    }
                }
                if (!selectingDest)
                {
                    Vector3 direction = touchStart - ChosenCamera.ScreenToWorldPoint(Input.mousePosition);
                    ChosenCamera.transform.position += direction;
                }
            } 
            //zoom with scroll wheel
            Zoom(Input.GetAxis("Mouse ScrollWheel"));
            }
    }

    //wait some second before handling double tap
    IEnumerator Pause()
    {
        yield return new WaitForSeconds(0.1f);

        ChosenCamera.transform.localPosition = new Vector3(0, 20, 0);
    }

    // zoom with camera
    private void Zoom(float incr)
    {
        ChosenCamera.orthographicSize = Mathf.Clamp(ChosenCamera.orthographicSize - incr, zoomOutMin, zoomOutMax);
    }
}
