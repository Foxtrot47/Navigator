using GoogleARCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using ZXing;

// class used for QR code detection, place on a gameobject
public class ImageRecognition : MonoBehaviour
{
    public GameObject FitToScanOverlay; //screen overlay for scanning
    public GameObject scanOverlay2; //screen overlay for scanning (start phase)
    public GameObject calibrationLocations; // transforms with calibration positions
    public GameObject person; // person indicator
    public GameObject controller; // indoornavcontroller object
    public GameObject MainController;

    public bool ScannerActive = true; // bool to say if scanner is active
    private bool first = true; // bool to fix multiple scan findings
    public GameObject DebugText;

    public Text textField; // information text

    // Update is called once per frame
    void Update()
    {
        /*
        if (ScannerActive)
        {
            FitToScanOverlay.SetActive(true);
            Scan();
        }
        */
    }

    /// <summary>
    /// Capture and scan the current frame 
    /// </summary>
    void Scan()
    {
        System.Action<byte[], int, int> callback = (bytes, width, height) =>
        {
            if (bytes == null)
            {
                // No image is available.
                return;
            }
            try {
                // Decode the image using ZXing parser
                IBarcodeReader barcodeReader = new BarcodeReader();
                var result = barcodeReader.Decode(bytes, width, height, RGBLuminanceSource.BitmapFormat.Gray8);
                var resultText = result.Text;

                // result action
                if (first)
                {
                    Relocate(resultText);
                    first = false;
                }
            }
            catch (Exception ex) { DebugText.GetComponent<Text>().text = ex.Message; }
        };

        CaptureScreenAsync(callback);
    }

    // move to person indicator to the new spot
    private bool Relocate(string text)
    {
        bool qrMatched = false;
        text = text.Trim(); //remove spaces
        //find the correct location scanned and move the person to its position
        foreach (Transform child in calibrationLocations.transform)
        {
            if(child.name.Equals(text))
            {
                person.transform.position = child.position;
                textField.text = "";
                MainController.GetComponent<MainController>().StartARView();
                ScannerActive = false;
                qrMatched = true;
                DebugText.GetComponent<Text>().text = "QR Matched";
                break;
            }
        }
        if (!qrMatched) {
            textField.text = "Invalid QR";
            DebugText.GetComponent<Text>().text = "QR Didn't Match";
        }
        return qrMatched;
    }
    /// <summary>
    /// Capture the screen using CameraImage.AcquireCameraImageBytes.
    /// </summary>
    /// <param name="callback"></param>
    void CaptureScreenAsync(Action<byte[], int, int> callback)
    {
        Task.Run(() =>
        {
            byte[] imageByteArray = null;
            int width;
            int height;

            using (var imageBytes = Frame.CameraImage.AcquireCameraImageBytes())
            {
                if (!imageBytes.IsAvailable)
                {
                    callback(null, 0, 0);
                    return;
                }

                int bufferSize = imageBytes.YRowStride * imageBytes.Height;

                imageByteArray = new byte[bufferSize];

                Marshal.Copy(imageBytes.Y, imageByteArray, 0, bufferSize);

                width = imageBytes.Width;
                height = imageBytes.Height;
            }

            callback(imageByteArray, width, height);
        });
    }

    // is used at start of application to set initial position
    public bool StartPosition(WebCamTexture wt)
    {
        bool succeeded = false;
        try
        {
            IBarcodeReader barcodeReader = new BarcodeReader();
            // decode the current frame
            var result = barcodeReader.Decode(wt.GetPixels32(),
              wt.width, wt.height);
            if (result != null)
            {
                Debug.Log("found: " + result.Text);
                DebugText.GetComponent<Text>().text = "Bar Code Reader found: " + result.Text;
                succeeded = Relocate(result.Text);
            }
        }
        catch (Exception ex) { Debug.LogWarning(ex.Message); }
        return succeeded;
    }

    void OnDisable() {
        ScannerActive = false;
        FitToScanOverlay.SetActive(false);
    }
    void OnEnable() {
        ScannerActive = true;
        FitToScanOverlay.SetActive(true);
        first = true;
    }
}
