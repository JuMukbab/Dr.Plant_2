using System;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1000)]
[DisallowMultipleComponent]
public sealed class ClinicStartScreen : MonoBehaviour
{
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private Button startButton;
    [SerializeField] private GameObject portraitNotice;
    [SerializeField] private PatientManager patientManager;
    [SerializeField] private ClinicAudioManager audioManager;

    private bool clinicStarted;

    public void Configure(
        GameObject menu,
        Button button,
        GameObject orientationNotice,
        PatientManager patients,
        ClinicAudioManager audio)
    {
        menuRoot = menu;
        startButton = button;
        portraitNotice = orientationNotice;
        patientManager = patients;
        audioManager = audio;
    }

    private void Awake()
    {
        bool bypassMenu = HasCommandLineFlag("-drplant-smoke-test");

        if (startButton != null)
            startButton.onClick.AddListener(StartClinic);

        if (bypassMenu)
        {
            clinicStarted = true;

            if (menuRoot != null)
                menuRoot.SetActive(false);

            return;
        }

        if (menuRoot != null)
            menuRoot.SetActive(true);

        if (patientManager != null)
            patientManager.enabled = false;
    }

    private void OnDestroy()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(StartClinic);
    }

    private void Update()
    {
        if (portraitNotice == null)
            return;

        bool shouldShow = Application.isMobilePlatform
            && Screen.height > Screen.width;

        if (portraitNotice.activeSelf != shouldShow)
            portraitNotice.SetActive(shouldShow);
    }

    public void StartClinic()
    {
        if (clinicStarted)
            return;

        clinicStarted = true;

        if (menuRoot != null)
            menuRoot.SetActive(false);

        if (patientManager != null)
            patientManager.enabled = true;

        audioManager?.BeginClinicAudio();
    }

    private static bool HasCommandLineFlag(string flag)
    {
        string[] arguments = Environment.GetCommandLineArgs();

        for (int index = 0; index < arguments.Length; index++)
        {
            if (string.Equals(
                arguments[index],
                flag,
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
