#pragma warning disable 0618
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class DrPlantSourceUpgradeSeeder
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string TemplatePrefabPath = "Assets/Prefabs/Flower.prefab";
    private const string MainBackgroundPath = "Assets/Sprites/GPT_MainBackground.png";
    private const string ClinicBackgroundPath = "Assets/Sprites/GPT_Backgrond_2.png";
    private const string SunglassesPath = "Assets/Sprites/Sunglasses.png";

    private static readonly Color Ink = Html("#07131D");
    private static readonly Color Lime = Html("#A7F542");
    private static readonly Color Navy = Html("#0D2638");

    [MenuItem("Dr.Plant/Build/Apply Latest Source Upgrade")]
    public static void ApplyUpgrade()
    {
        ConfigureSourceTextures();
        ConfigurePatientPrefabs();
        DrPlantContentSeeder.CreateOrUpdateDefaultCatalog();
        DrPlantUiPolishSeeder.ApplyUiPolish();

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        ConfigureClinicBackground(scene);
        ConfigureSceneFlow(scene);

        EditorSceneManager.MarkSceneDirty(scene);
        Require(EditorSceneManager.SaveScene(scene), "Could not save the upgraded scene.");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Validate(scene);

        Debug.Log(
            "Dr.Plant latest source upgrade applied: menu, clinic art, five animated "
            + "patients, symptom effects, sunglasses, and six audio assets.");
    }

    private static void ConfigureSourceTextures()
    {
        ConfigureSingleSprite(MainBackgroundPath, 100f, FilterMode.Point);
        ConfigureSingleSprite(ClinicBackgroundPath, 100f, FilterMode.Point);
        ConfigureSingleSprite(SunglassesPath, 10f, FilterMode.Point);

        ConfigureSpriteSheet("Assets/Sprites/Flower.png", 5);
        ConfigureSpriteSheet("Assets/Sprites/Bean.png", 7);
        ConfigureSpriteSheet("Assets/Sprites/Cactus.png", 7);
        ConfigureSpriteSheet("Assets/Sprites/Sprout.png", 6);
        ConfigureSpriteSheet("Assets/Sprites/Succulent.png", 5);
        AssetDatabase.Refresh();
    }

    private static void ConfigureSingleSprite(
        string path,
        float pixelsPerUnit,
        FilterMode filterMode)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        Require(importer != null, $"Texture is missing: {path}");

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = filterMode;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    private static void ConfigureSpriteSheet(string path, int frameCount)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        Require(importer != null, $"Patient texture is missing: {path}");

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 10f;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        string baseName = Path.GetFileNameWithoutExtension(path);
        SpriteMetaData[] frames = new SpriteMetaData[frameCount];

        for (int index = 0; index < frameCount; index++)
        {
            frames[index] = new SpriteMetaData
            {
                name = $"{baseName}_{index}",
                rect = new Rect(index * 32f, 0f, 32f, 32f),
                alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f)
            };
        }

        importer.spritesheet = frames;
        importer.SaveAndReimport();
    }

    private static void ConfigurePatientPrefabs()
    {
        ConfigurePatientPrefab("Flower", "Assets/Sprites/Flower.png");
        ConfigurePatientPrefab("Mr.Bean", "Assets/Sprites/Bean.png");
        ConfigurePatientPrefab("Cactus", "Assets/Sprites/Cactus.png");
        ConfigurePatientPrefab("Sprout", "Assets/Sprites/Sprout.png");
        ConfigurePatientPrefab("Succulent", "Assets/Sprites/Succulent.png");
    }

    private static void ConfigurePatientPrefab(string name, string spritePath)
    {
        string prefabPath = $"Assets/Prefabs/{name}.prefab";
        EnsurePrefabExists(prefabPath, name);

        Sprite[] frames = LoadFrames(spritePath);
        Sprite sunglasses = AssetDatabase.LoadAssetAtPath<Sprite>(SunglassesPath);
        Require(frames.Length > 0, $"No animation frames found at {spritePath}.");
        Require(sunglasses != null, "Sunglasses sprite is missing.");

        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);

        try
        {
            root.name = name;
            SpriteRenderer renderer = RequireComponent<SpriteRenderer>(root);
            renderer.sprite = frames[0];

            PlantStatus status = RequireComponent<PlantStatus>(root);
            status.normalSprite = frames[0];
            status.deadSprite = frames[frames.Length - 1];

            PatientVisualController visual =
                RequireComponent<PatientVisualController>(root);
            visual.Configure(frames, sunglasses);

            BoxCollider2D collider = RequireComponent<BoxCollider2D>(root);
            collider.size = new Vector2(2.45f, 2.45f);
            collider.offset = Vector2.zero;

            RequireComponent<PatientMove>(root).speed = 5f;
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void EnsurePrefabExists(string prefabPath, string name)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            return;

        GameObject template = PrefabUtility.LoadPrefabContents(TemplatePrefabPath);

        try
        {
            template.name = name;
            Require(
                PrefabUtility.SaveAsPrefabAsset(template, prefabPath) != null,
                $"Could not create patient prefab: {prefabPath}");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(template);
        }
    }

    private static Sprite[] LoadFrames(string path)
    {
        return AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Sprite>()
            .OrderBy(sprite => FrameIndex(sprite.name))
            .ToArray();
    }

    private static int FrameIndex(string name)
    {
        int separator = name.LastIndexOf('_');
        return separator >= 0
            && int.TryParse(name.Substring(separator + 1), out int index)
                ? index
                : int.MaxValue;
    }

    private static void ConfigureClinicBackground(Scene scene)
    {
        Transform background = FindRoot(scene, "Background");
        Require(background != null, "Background object is missing.");

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ClinicBackgroundPath);
        Require(sprite != null, "Clinic background sprite is missing.");

        SpriteRenderer renderer = RequireComponent<SpriteRenderer>(background.gameObject);
        renderer.sprite = sprite;
        renderer.color = Color.white;
        background.GetComponent<ViewportBackgroundFitter>()?.FitNow();
    }

    private static void ConfigureSceneFlow(Scene scene)
    {
        Transform canvas = FindRoot(scene, "Canvas");
        Transform gameManagerTransform = FindRoot(scene, "GameManager");
        Require(canvas != null, "Canvas is missing.");
        Require(gameManagerTransform != null, "GameManager is missing.");

        TMP_FontAsset displayFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/Font/Ramche SDF.asset");
        TMP_FontAsset bodyFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/Font/NanumHumanBold SDF.asset");
        Require(displayFont != null && bodyFont != null, "UI fonts are missing.");

        GameObject menu = GetOrCreateUiObject("MainMenuPanel", canvas);
        SetStretch(menu.GetComponent<RectTransform>(), 0f);
        Image menuImage = RequireComponent<Image>(menu);
        menuImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(MainBackgroundPath);
        menuImage.color = Color.white;
        menuImage.preserveAspect = false;
        menuImage.raycastTarget = true;

        Button startButton = ConfigureStartButton(menu.transform, displayFont);
        GameObject portraitNotice = ConfigurePortraitNotice(canvas, bodyFont);

        ClinicAudioManager audioManager = ConfigureAudio(gameManagerTransform.gameObject);
        PatientManager patientManager =
            UnityEngine.Object.FindFirstObjectByType<PatientManager>(
                FindObjectsInactive.Include);
        Require(patientManager != null, "PatientManager is missing.");

        ClinicStartScreen flow =
            RequireComponent<ClinicStartScreen>(gameManagerTransform.gameObject);
        flow.Configure(
            menu,
            startButton,
            portraitNotice,
            patientManager,
            audioManager);

        menu.transform.SetAsLastSibling();
        portraitNotice.transform.SetAsLastSibling();
        portraitNotice.SetActive(false);
    }

    private static Button ConfigureStartButton(
        Transform menu,
        TMP_FontAsset displayFont)
    {
        GameObject buttonObject = GetOrCreateUiObject("StartButton", menu);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        SetRect(
            rect,
            Vector2.one * 0.5f,
            Vector2.one * 0.5f,
            new Vector2(0f, -300f),
            new Vector2(432f, 126f),
            Vector2.one * 0.5f);

        Image border = RequireComponent<Image>(buttonObject);
        border.sprite = null;
        border.color = Ink;

        GameObject fillObject = GetOrCreateUiObject("Fill", buttonObject.transform);
        SetStretch(fillObject.GetComponent<RectTransform>(), 7f);
        Image fill = RequireComponent<Image>(fillObject);
        fill.sprite = null;
        fill.color = Lime;
        fill.raycastTarget = false;

        TextMeshProUGUI label = GetOrCreateText("Label", buttonObject.transform);
        SetStretch(label.rectTransform, 12f);
        label.font = displayFont;
        label.text = "START";
        label.fontSize = 58f;
        label.color = Ink;
        label.alignment = TextAlignmentOptions.Center;
        label.characterSpacing = 0f;
        label.raycastTarget = false;

        Button button = RequireComponent<Button>(buttonObject);
        button.targetGraphic = fill;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Html("#E3FF8A");
        colors.pressedColor = Html("#75C928");
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        return button;
    }

    private static GameObject ConfigurePortraitNotice(
        Transform canvas,
        TMP_FontAsset bodyFont)
    {
        GameObject notice = GetOrCreateUiObject("PortraitOrientationNotice", canvas);
        SetStretch(notice.GetComponent<RectTransform>(), 0f);
        Image background = RequireComponent<Image>(notice);
        background.sprite = null;
        background.color = Navy;
        background.raycastTarget = true;

        TextMeshProUGUI label = GetOrCreateText("Message", notice.transform);
        SetRect(
            label.rectTransform,
            Vector2.one * 0.5f,
            Vector2.one * 0.5f,
            Vector2.zero,
            new Vector2(720f, 180f),
            Vector2.one * 0.5f);
        label.font = bodyFont;
        label.text = "가로 화면으로 돌려주세요";
        label.fontSize = 54f;
        label.enableAutoSizing = true;
        label.fontSizeMin = 28f;
        label.fontSizeMax = 54f;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;
        label.characterSpacing = 0f;
        label.raycastTarget = false;
        return notice;
    }

    private static ClinicAudioManager ConfigureAudio(GameObject host)
    {
        AudioSource[] sources = host.GetComponents<AudioSource>();
        List<AudioSource> configuredSources = new List<AudioSource>(sources);

        while (configuredSources.Count < 2)
            configuredSources.Add(host.AddComponent<AudioSource>());

        ClinicAudioManager manager = RequireComponent<ClinicAudioManager>(host);
        manager.Configure(
            configuredSources[0],
            configuredSources[1],
            LoadAudio("CalmBGM"),
            LoadAudio("GameStart"),
            LoadAudio("Purchase"),
            LoadAudio("Pop"),
            LoadAudio("RelaxingBGM"),
            LoadAudio("Speechsound_1"));
        return manager;
    }

    private static AudioClip LoadAudio(string name)
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(
            $"Assets/Audio/{name}.mp3");
        Require(clip != null, $"Audio clip is missing: {name}");
        return clip;
    }

    private static void Validate(Scene scene)
    {
        string[] patients = { "Flower", "Mr.Bean", "Cactus", "Sprout", "Succulent" };

        foreach (string patient in patients)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                $"Assets/Prefabs/{patient}.prefab");
            Require(prefab != null, $"Patient prefab is missing: {patient}");
            PatientVisualController visual = prefab.GetComponent<PatientVisualController>();
            Require(
                visual != null && visual.IsConfigured,
                $"Patient visual is not configured: {patient}");
        }

        Transform canvas = FindRoot(scene, "Canvas");
        Require(canvas != null, "Canvas is missing after upgrade.");
        Require(FindChild(canvas, "MainMenuPanel") != null, "Main menu is missing.");
        Require(
            FindChild(canvas, "PortraitOrientationNotice") != null,
            "Portrait orientation notice is missing.");
    }

    private static GameObject GetOrCreateUiObject(string name, Transform parent)
    {
        Transform existing = FindChild(parent, name);
        if (existing != null)
            return existing.gameObject;

        GameObject created = new GameObject(
            name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        created.layer = 5;
        created.transform.SetParent(parent, false);
        return created;
    }

    private static TextMeshProUGUI GetOrCreateText(string name, Transform parent)
    {
        Transform existing = FindChild(parent, name);
        if (existing != null)
            return RequireComponent<TextMeshProUGUI>(existing.gameObject);

        GameObject created = new GameObject(
            name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        created.layer = 5;
        created.transform.SetParent(parent, false);
        return created.GetComponent<TextMeshProUGUI>();
    }

    private static void SetRect(
        RectTransform rect,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 position,
        Vector2 size,
        Vector2 pivot)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static void SetStretch(RectTransform rect, float inset)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = Vector2.one * 0.5f;
        rect.offsetMin = Vector2.one * inset;
        rect.offsetMax = Vector2.one * -inset;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static Transform FindRoot(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == name)
                return root.transform;
        }

        return null;
    }

    private static Transform FindChild(Transform parent, string name)
    {
        for (int index = 0; index < parent.childCount; index++)
        {
            Transform child = parent.GetChild(index);
            if (child.name == name)
                return child;
        }

        return null;
    }

    private static T RequireComponent<T>(GameObject gameObject)
        where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component == null)
            component = gameObject.AddComponent<T>();
        return component;
    }

    private static Color Html(string value)
    {
        if (!ColorUtility.TryParseHtmlString(value, out Color color))
            throw new InvalidOperationException($"Invalid color: {value}");
        return color;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
#pragma warning restore 0618
