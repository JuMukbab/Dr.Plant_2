using System;
using System.Collections;
using System.IO;
using UnityEngine;

public sealed class DrPlantUiScreenshotVerifier : MonoBehaviour
{
    private const string CommandLineFlag = "-drplant-ui-screenshot";
    private const string OutputArgument = "-drplant-ui-output";
    private const string WidthArgument = "-drplant-ui-width";
    private const string HeightArgument = "-drplant-ui-height";
    private const float LoadTimeoutSeconds = 8f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void StartWhenRequested()
    {
        if (!HasArgument(CommandLineFlag))
            return;

        GameObject verifierObject = new GameObject(nameof(DrPlantUiScreenshotVerifier));
        DontDestroyOnLoad(verifierObject);
        verifierObject.AddComponent<DrPlantUiScreenshotVerifier>();
    }

    private IEnumerator Start()
    {
        string outputDirectory = GetArgument(
            OutputArgument,
            Application.persistentDataPath);
        int width = GetIntArgument(WidthArgument, 1280);
        int height = GetIntArgument(HeightArgument, 720);

        Directory.CreateDirectory(outputDirectory);
        Screen.SetResolution(width, height, false);

        yield return new WaitForSecondsRealtime(1f);

        ClinicStartScreen startScreen = FindFirstObjectByType<ClinicStartScreen>();
        if (startScreen != null)
        {
            yield return Capture(
                Path.Combine(outputDirectory, $"menu-{width}x{height}.png"));
            startScreen.StartClinic();
            yield return null;
        }

        float deadline = Time.realtimeSinceStartup + LoadTimeoutSeconds;
        while (!IsClinicReady() && Time.realtimeSinceStartup < deadline)
            yield return null;

        if (!IsClinicReady())
        {
            Debug.LogError("Dr.Plant UI screenshot verification failed to load the clinic.");
            Application.Quit(1);
            yield break;
        }

        if (!PatientManager.Instance.PatientReady)
        {
            PatientMove move = PatientManager.Instance.CurrentPatient
                .GetComponent<PatientMove>();

            PatientManager.Instance.CurrentPatient.transform.position =
                PatientManager.Instance.centerPoint.position;
            move?.onArrive?.Invoke();
            yield return null;
        }

        yield return new WaitForSecondsRealtime(1.25f);
        yield return Capture(
            Path.Combine(outputDirectory, $"clinic-{width}x{height}.png"));

        ShopUI shopUi = FindFirstObjectByType<ShopUI>();
        shopUi.OpenShop();
        yield return new WaitForSecondsRealtime(0.25f);
        yield return Capture(
            Path.Combine(outputDirectory, $"shop-{width}x{height}.png"));

        shopUi.CloseShop();
        ChecklistUI.Instance.Open();
        yield return new WaitForSecondsRealtime(0.25f);
        yield return Capture(
            Path.Combine(outputDirectory, $"checklist-{width}x{height}.png"));

        Debug.Log($"Dr.Plant UI screenshot verification passed at {width}x{height}.");
        Application.Quit(0);
    }

    private static IEnumerator Capture(string path)
    {
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(path);

        float deadline = Time.realtimeSinceStartup + 3f;
        while (!File.Exists(path) && Time.realtimeSinceStartup < deadline)
            yield return null;
    }

    private static bool IsClinicReady()
    {
        return PatientManager.Instance != null
            && PatientManager.Instance.CurrentPatient != null
            && ClinicProgressReady()
            && FindFirstObjectByType<ClinicHud>() != null
            && ShopManager.Instance != null
            && ChecklistUI.Instance != null;
    }

    private static bool ClinicProgressReady()
    {
        return MoneyManager.Instance != null
            && DrPlant.Data.DrPlantContent.Catalog != null;
    }

    private static bool HasArgument(string argument)
    {
        string[] arguments = Environment.GetCommandLineArgs();

        for (int index = 0; index < arguments.Length; index++)
        {
            if (string.Equals(
                arguments[index],
                argument,
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string GetArgument(string argument, string fallback)
    {
        string[] arguments = Environment.GetCommandLineArgs();

        for (int index = 0; index < arguments.Length - 1; index++)
        {
            if (string.Equals(
                arguments[index],
                argument,
                StringComparison.OrdinalIgnoreCase))
            {
                return arguments[index + 1];
            }
        }

        return fallback;
    }

    private static int GetIntArgument(string argument, int fallback)
    {
        string value = GetArgument(argument, fallback.ToString());
        return int.TryParse(value, out int result) && result > 0
            ? result
            : fallback;
    }
}
