using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif
using UnityEngine.SceneManagement;

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
    public GameObject ARCoreCamera;
    public GameObject NavigationController;

    public GameObject MinimapFrame;
    public GameObject QRBackImage;
    public GameObject ScanOverlay;
    public Button ScanButton;
    public GameObject ScanInfoText;
    public GameObject ErrorText;
    public Camera fullscreenCamera;     // The camera that captures the map (follow camera)
    private RenderTexture texture;      // field to save texture to set again after view switch

    private bool CameraReady = false;

    public void Start (){

        MainState = State.Starting;
        // Check for camera permission and push to welcome scene
        #if PLATFORM_ANDROID
        if (!UniAndroidPermission.IsPermitted (AndroidPermission.CAMERA))
        {
            SceneManager.LoadScene(0);
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
        MainState = State.ScannerActive;

        // Disable AR Core Components and related scripts first
        ARCoreDevice.SetActive(false);
        NavigationController.SetActive(false);
        ARCoreDevice.GetComponent<ARCoreSession>().enabled = false;
        ARCoreCamera.GetComponent<Camera>().enabled = false;
        MinimapFrame.SetActive(false);

        // Then enabled all QR Code Scanner Scripts and UI elements
        MarkerScanner.SetActive(true);
        ScanOverlay.SetActive(true);
        ScanStart.SetActive(true);
        MarkerScanner.GetComponent<ImageRecognition>().ScannerActive = true;
        QRBackImage.SetActive(true);
        ScanInfoText.SetActive(true);
    }
    public void StartARView() {
        if (MainState != State.Starting && MainState != State.Relocating) {
            MainState = State.ARView;

            // Disable all QR Code Scanner Scripts and UI elements
            ScanStart.SetActive(false);
            ScanOverlay.SetActive(false);
            MarkerScanner.SetActive(false);
            MarkerScanner.GetComponent<ImageRecognition>().ScannerActive = false;
            QRBackImage.SetActive(false);
            ScanInfoText.SetActive(false);

            // Then enable AR Core Components and related scripts
            ARCoreDevice.SetActive(true);
            ARCoreDevice.GetComponent<ARCoreSession>().enabled = true;
            ARCoreCamera.GetComponent<Camera>().enabled = true;
            fullscreenCamera.targetTexture = texture;
            fullscreenCamera.orthographicSize = 10;
            NavigationController.SetActive(true);
            MinimapFrame.SetActive(true);
        }
    }
    public void SwitchtoMapView() {
        if (MainState == State.ARView || MainState == State.ManualRelocate) {
            MainState = State.MapView;

            // Disable all QR Code Scanner Scripts and UI elements
            MarkerScanner.GetComponent<ImageRecognition>().ScannerActive = false;
            MinimapFrame.SetActive(false);
            QRBackImage.SetActive(false);
            ScanStart.SetActive(false);
            ScanOverlay.SetActive(false);
            ScanInfoText.SetActive(false);
            //Also disable Minimap cuz we don't need it on Map View
            MarkerScanner.SetActive(false);

            //Enable AR Core and nav scripts again and show Overhead camera over player
            ARCoreDevice.SetActive(true);
            ARCoreCamera.GetComponent<Camera>().enabled = true;
            fullscreenCamera.targetTexture = null;
            fullscreenCamera.orthographicSize = 20;
            NavigationController.SetActive(true);
        }
    }
    public void ScanButtonClick() {
        //if( MainState == State.ARView || MainState == State.MapView || MainState == State.ManualRelocate ) {
            StartScanner();
        //}
    }
    public void FinishScan(){
        MainState = State.ARView;
        StartARView();
    }
}