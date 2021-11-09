using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class MainController : MonoBehaviour {

    enum State{
        Starting,
        ScannerActive,
        Relocating,
        ARView,
        MapView,
        ManualRelocate
    }
    private State MainState;
    public GameObject MarkerScanner;
    public GameObject ScanStart;
    public GameObject ARCoreDevice;
    public GameObject NavigationController;

    public GameObject MinimapFrame;
    public GameObject QRBackImage;
    public GameObject ScanOverlay;
    public GameObject DebugText;
    public Button ScanButton;

    public Camera fullscreenCamera;     // The camera that captures the map (follow camera)
    private RenderTexture texture;      // field to save texture to set again after view switch

    private bool CameraReady = false;
    #if PLATFORM_ANDROID
    public void RequestPermission()
    {
        UniAndroidPermission.RequestPermission(AndroidPermission.CAMERA, OnAllow, OnDeny, OnDenyAndNeverAskAgain);
    }
    private void OnAllow()
    {
        bool CameraReady = true;
        MainState = State.ScannerActive;
        // Get the TargetTexture of Fullscreen Camera because we will overwrite this when we call StartMapView
        StartScanner();
    }
    private void OnDeny()
    {
        Application.Quit();
    }
    private void OnDenyAndNeverAskAgain()
    {
        Application.Quit();
    }
    #endif
    public void Start (){

        MainState = State.Starting;
        // Check for camera permission at the very start and ask if not provided
        #if PLATFORM_ANDROID
        if (!UniAndroidPermission.IsPermitted (AndroidPermission.CAMERA))
        {
            RequestPermission();
        }
        else {
            bool CameraReady = true;
        }
        #endif
        if(CameraReady) {
            MainState = State.ScannerActive;
            // Get the TargetTexture of Fullscreen Camera because we will overwrite this when we call StartMapView
            StartScanner();
        }
        texture = fullscreenCamera.targetTexture;
        ScanButton.onClick.AddListener(ScanButtonClick);

    }

    void StartScanner() {
        DebugText.GetComponent<Text>().text = "Trying to start Scanner";
        MainState = State.ScannerActive;
        ARCoreDevice.SetActive(false);
        NavigationController.SetActive(false);
        MarkerScanner.SetActive(true);
        ScanOverlay.SetActive(true);
        ScanStart.SetActive(true);
        MarkerScanner.GetComponent<ImageRecognition>().ScannerActive = true;
        QRBackImage.SetActive(true);
        MinimapFrame.SetActive(false);
        DebugText.GetComponent<Text>().text = "Started Scanner";

    }
    public void StartARView() {
        if (MainState != State.Starting && MainState != State.Relocating) {
            DebugText.GetComponent<Text>().text = "Trying to start AR View";
            MainState = State.ARView;
            ARCoreDevice.SetActive(true);
            NavigationController.SetActive(true);
            ScanStart.SetActive(false);
            MarkerScanner.SetActive(false);
            ScanOverlay.SetActive(false);
            MarkerScanner.GetComponent<ImageRecognition>().ScannerActive = false;
            QRBackImage.SetActive(false);
            MinimapFrame.SetActive(true);

            // Enabling CameraView
            ARCoreDevice.GetComponent<Camera>().enabled = true;
            fullscreenCamera.targetTexture = texture;
            fullscreenCamera.orthographicSize = 7;
            DebugText.GetComponent<Text>().text = "Started AR View";
        }
    }
    public void SwitchtoMapView() {
        if (MainState == State.ARView || MainState == State.ManualRelocate) {
            DebugText.GetComponent<Text>().text = "Trying to start Map View";
            MainState = State.MapView;
            ARCoreDevice.SetActive(true);
            NavigationController.SetActive(true);
            MarkerScanner.SetActive(false);
            ScanStart.SetActive(false);
            ScanOverlay.SetActive(false);
            MarkerScanner.GetComponent<ImageRecognition>().ScannerActive = false;
            QRBackImage.SetActive(false);
            MinimapFrame.SetActive(false);

            // Enabling MapView
            ARCoreDevice.GetComponent<Camera>().enabled = false;
            fullscreenCamera.targetTexture = null;
            fullscreenCamera.orthographicSize = 15;
            DebugText.GetComponent<Text>().text = "Started Map View";
        }
    }
    public void ScanButtonClick() {
        //if( MainState == State.ARView || MainState == State.MapView || MainState == State.ManualRelocate ) {
            DebugText.GetComponent<Text>().text = "Button Click Registered";
            StartScanner();
        //}
    }
    public void FinishScan(){
        DebugText.GetComponent<Text>().text = "Relocate Successfull, Opening AR View";
        MainState = State.ARView;
        StartARView();
    }
}