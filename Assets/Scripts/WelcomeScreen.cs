using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GoogleARCore;

public class WelcomeScreen : MonoBehaviour
{

    public Text CameraErrorText;
    public Text ARCoreErrorText;
    public GameObject Popup;
    public GameObject BlurPanel;
    public GameObject[] Pages;

    int TutorialCompleted = 0;
    bool WaitingForCameraPerms = false;
    bool HaveCameraPerms = false;
    bool HaveARCoreSetup = false;
    bool StartupChecksDone = false;
    bool SwipeEnabled = false;

    int CurrentPage=0;
    private Vector3 fp;   //First touch position
    private Vector3 lp;   //Last touch position
    private float dragDistance;  //minimum distance for a swipe to be registered
    // Start is called before the first frame update
    void Start()
    {
        dragDistance = Screen.height * 15 / 100; //dragDistance is 15% height of the screen
        StartCoroutine(StartupChecks());
    }
    void NextPage(){
        if (!StartupChecksDone) {
            return;
        }
        if (!HaveCameraPerms) {
            Pages[CurrentPage].SetActive(false);
            CurrentPage = 1;
            Pages[CurrentPage].SetActive(true);
            return;
        }
        else if (!HaveARCoreSetup) {
            Pages[CurrentPage].SetActive(false);
            CurrentPage = 2;
            Pages[CurrentPage].SetActive(true);
            return;
        }
        else if (TutorialCompleted==0 && CurrentPage<8) {
            Pages[CurrentPage].SetActive(false);
            CurrentPage++;
            Pages[CurrentPage].SetActive(true);
        }
        else if (TutorialCompleted==0 && CurrentPage==8) {
            ShowPopup(true);
            SwipeEnabled = false;
            TutorialCompleted = 1;
            PlayerPrefs.SetInt("TutorialCompleted", 1);
            PlayerPrefs.Save();
            Pages[CurrentPage].SetActive(false);
            SceneManager.LoadScene(1);
        }
        else if(TutorialCompleted == 1) {
            ShowPopup(true);
            SwipeEnabled = false;
            SceneManager.LoadScene(1);
        }
    }
    void PrevPage() {
        //We will only allow going backwards on tutorial page and not any other pages
        if (CurrentPage<=3) {
            return;
        }
        else {
            Pages[CurrentPage].SetActive(false);
            CurrentPage--;
            Pages[CurrentPage].SetActive(true);  
        }
    }
    IEnumerator StartupChecks() {
        #if PLATFORM_ANDROID
            if (UniAndroidPermission.IsPermitted (AndroidPermission.CAMERA))
            {
                HaveCameraPerms = true;
            }
            AsyncTask<ApkAvailabilityStatus> checkTask = Session.CheckApkAvailability();
            CustomYieldInstruction customYield = checkTask.WaitForCompletion();
            yield return customYield;
            ApkAvailabilityStatus result = checkTask.Result;
            if (result == ApkAvailabilityStatus.SupportedInstalled) {
                HaveARCoreSetup = true;
            }
            else {
                HaveARCoreSetup = false;
            }
        #endif
        TutorialCompleted = PlayerPrefs.GetInt("TutorialCompleted", 0); 
        NextPage();
        StartupChecksDone = true;
        SwipeEnabled = true;
        yield return new WaitForSeconds(2);
        NextPage();
    }
    private void OnAllow()
    {
        CameraErrorText.color = Color.green;
        CameraErrorText.text = "Camera Permission Granted Swipe Left to continue";
        HaveCameraPerms = true;
    }
    private void OnDeny()
    {
        CameraErrorText.text = "You have rejected our request for Camera Permissions. You cannot use the app without this. Please press grant and press allow when popup comes";
    }
    private void OnDenyAndNeverAskAgain()
    {
        CameraErrorText.text = "You have chosen to not ask permission again. You must manually grant camera permission from App Settings if you wanna continue using the app";
    }

    public void RequestPermission()
    {
        UniAndroidPermission.RequestPermission(AndroidPermission.CAMERA, OnAllow, OnDeny, OnDenyAndNeverAskAgain);
    }
    void OnApplicationFocus(bool hasFocus){
        ShowPopup(true);
        if (hasFocus == true && WaitingForCameraPerms == true){
            #if PLATFORM_ANDROID
            if (UniAndroidPermission.IsPermitted (AndroidPermission.CAMERA))
            {
                WaitingForCameraPerms = false;
                HaveCameraPerms = true;
                CameraErrorText.color = Color.green;
                CameraErrorText.text = "Camera Permission Granted Swipe Left to continue";
            }
            #endif
        }
        ShowPopup(false);
    }
    public void OnClickGrant(){
        #if PLATFORM_ANDROID
        if (!UniAndroidPermission.IsPermitted (AndroidPermission.CAMERA))
        {
            RequestPermission();
            WaitingForCameraPerms = true;
        }
        else {
            HaveCameraPerms = true;
        }
        #endif
    }

    IEnumerator InstallARCore() {
        ShowPopup(true);
        AsyncTask<ApkInstallationStatus> installTask = Session.RequestApkInstallation(true);
        CustomYieldInstruction customYield = installTask.WaitForCompletion();
        yield return customYield;
        ApkInstallationStatus result = installTask.Result;
        yield return new WaitForSeconds(0.5f);
        ShowPopup(false);
    }
    IEnumerator CheckARCore() {
        ShowPopup(true);
        AsyncTask<ApkAvailabilityStatus> checkTask = Session.CheckApkAvailability();
        CustomYieldInstruction customYield = checkTask.WaitForCompletion();
        yield return customYield;
        ApkAvailabilityStatus result = checkTask.Result;
        if (result == ApkAvailabilityStatus.SupportedInstalled) {
            HaveARCoreSetup = true;
            ARCoreErrorText.color = Color.green;
            ARCoreErrorText.text = "AR Core Installed Swipe Left to continue";
        }
        else {
            HaveARCoreSetup = true;
            ARCoreErrorText.text = "AR Core is not supported on your device. But we are skipping this atm"; 
        }
        yield return new WaitForSeconds(0.5f);
        ShowPopup(false);
    }
    public void OnClickARInstall(){
        ShowPopup(true,"Installing AR Core");
        StartCoroutine(InstallARCore());
        StartCoroutine(CheckARCore());
    }

    void ShowPopup(bool enable,string message = "") {
        if (enable) {
            SwipeEnabled = false;
            Popup.SetActive(true);
            BlurPanel.SetActive(true);
            if (message != "") {
                Popup.transform.GetChild(1).gameObject.GetComponent<Text>().text = message;
            }
            else {
                Popup.transform.GetChild(1).gameObject.GetComponent<Text>().text = "Loading...";
            }
        }
        else {
            Popup.SetActive(false);
            BlurPanel.SetActive(false);
            SwipeEnabled = true;
        }
    }
    void Update()
    {
        if (SwipeEnabled) {
            if (Input.touchCount == 1) // user is touching the screen with a single touch
            {
                Touch touch = Input.GetTouch(0); // get the touch
                if (touch.phase == TouchPhase.Began) //check for the first touch
                {
                    fp = touch.position;
                    lp = touch.position;
                }
                else if (touch.phase == TouchPhase.Moved) // update the last position based on where they moved
                {
                    lp = touch.position;
                }
                else if (touch.phase == TouchPhase.Ended) //check if the finger is removed from the screen
                {
                    lp = touch.position;  //last touch position. Ommitted if you use list
    
                    //Check if drag distance is greater than 20% of the screen height
                    if (Mathf.Abs(lp.x - fp.x) > dragDistance || Mathf.Abs(lp.y - fp.y) > dragDistance)
                    {//It's a drag
                    //check if the drag is vertical or horizontal
                        if (Mathf.Abs(lp.x - fp.x) > Mathf.Abs(lp.y - fp.y))
                        {   //If the horizontal movement is greater than the vertical movement...
                            if ((lp.x > fp.x))  //If the movement was to the right)
                            {   //Right swipe
                                PrevPage();
                            }
                            else
                            {   //Left swipe
                                NextPage();
                            }
                        }
                    }
                }
            }
        }
    }
}
