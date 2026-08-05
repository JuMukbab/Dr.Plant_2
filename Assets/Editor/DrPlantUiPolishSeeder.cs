using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class DrPlantUiPolishSeeder
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string ShopItemPath = "Assets/Prefabs/ShopItem.prefab";
    private const string TreatmentTogglePath = "Assets/Prefabs/TreatmentToggle.prefab";

    private static readonly Color Ink = Html("#142638");
    private static readonly Color DeepInk = Html("#07131D");
    private static readonly Color Green = Html("#4DCC58");
    private static readonly Color Lime = Html("#9AF03B");
    private static readonly Color Paper = Html("#F4F8F2");
    private static readonly Color PaleGreen = Html("#E4F1E5");
    private static readonly Color Muted = Html("#52695B");
    private static readonly Color Coral = Html("#D76559");

    [MenuItem("Dr.Plant/Build/Apply UI Polish")]
    public static void ApplyUiPolish()
    {
        TMP_FontAsset displayFont =
            AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/Font/Ramche SDF.asset");
        TMP_FontAsset bodyFont =
            AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/Font/NanumHumanBold SDF.asset");

        Require(displayFont != null, "Ramche SDF font is missing.");
        Require(bodyFont != null, "NanumHumanBold SDF font is missing.");

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Transform canvas = FindRoot(scene, "Canvas");
        Require(canvas != null, "Canvas could not be found.");

        ConfigureCanvas(canvas);
        ConfigureClinicHud(canvas, displayFont, bodyFont);
        ConfigureMoneyButton(canvas, displayFont);
        ConfigureTreatButton(canvas, displayFont);
        ConfigureTalkBubble(canvas, bodyFont);
        ConfigureShop(canvas, displayFont, bodyFont);
        ConfigureChecklist(canvas, displayFont, bodyFont);
        ConfigureBackground(scene);

        EditorSceneManager.MarkSceneDirty(scene);
        Require(EditorSceneManager.SaveScene(scene), "Could not save the polished scene.");

        ConfigureShopItemPrefab(displayFont, bodyFont);
        ConfigureTreatmentTogglePrefab(bodyFont);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Validate(scene);

        Debug.Log(
            "Dr.Plant UI polish applied: clinic HUD, responsive background, "
            + "shop, checklist, and primary action styling.");
    }

    private static void ConfigureCanvas(Transform canvas)
    {
        CanvasScaler scaler = RequireComponent<CanvasScaler>(canvas.gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        scaler.referencePixelsPerUnit = 100f;
    }

    private static void ConfigureClinicHud(
        Transform canvas,
        TMP_FontAsset displayFont,
        TMP_FontAsset bodyFont)
    {
        GameObject panel = GetOrCreateUiObject("PatientInfoPanel", canvas);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        SetRect(
            panelRect,
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(24f, -24f),
            new Vector2(350f, 176f),
            new Vector2(0f, 1f));

        Image panelImage = RequireComponent<Image>(panel);
        panelImage.sprite = null;
        panelImage.type = Image.Type.Simple;
        panelImage.color = new Color(Ink.r, Ink.g, Ink.b, 0.96f);
        panelImage.raycastTarget = false;
        SetOutline(panel, DeepInk, new Vector2(5f, -5f));

        TextMeshProUGUI title = GetOrCreateText("ChartTitle", panel.transform);
        SetRect(
            title.rectTransform,
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(18f, -12f),
            new Vector2(314f, 34f),
            new Vector2(0f, 1f));
        SetText(title, displayFont, 26f, Lime, TextAlignmentOptions.Left, false);
        title.text = "진료 차트";

        TextMeshProUGUI patient = GetOrCreateText("PatientLabel", panel.transform);
        SetRect(
            patient.rectTransform,
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(18f, -52f),
            new Vector2(314f, 32f),
            new Vector2(0f, 1f));
        SetText(patient, bodyFont, 24f, Color.white, TextAlignmentOptions.Left, false);
        patient.text = "환자  대기 중";

        TextMeshProUGUI symptom = GetOrCreateText("SymptomLabel", panel.transform);
        SetRect(
            symptom.rectTransform,
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(18f, -88f),
            new Vector2(314f, 48f),
            new Vector2(0f, 1f));
        SetText(symptom, bodyFont, 23f, Html("#D9F7D9"), TextAlignmentOptions.TopLeft, true);
        symptom.text = "증상  확인 중";

        TextMeshProUGUI progress = GetOrCreateText("ProgressLabel", panel.transform);
        SetRect(
            progress.rectTransform,
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(18f, 14f),
            new Vector2(314f, 30f),
            new Vector2(0f, 0f));
        SetText(progress, bodyFont, 20f, Html("#AFC4B5"), TextAlignmentOptions.Left, false);
        progress.text = "진료 0명  |  도구 0/3";

        ClinicHud hud = RequireComponent<ClinicHud>(panel);
        hud.Configure(patient, symptom, progress);
        panel.transform.SetAsFirstSibling();
    }

    private static void ConfigureMoneyButton(
        Transform canvas,
        TMP_FontAsset displayFont)
    {
        Transform moneyPanel = FindChild(canvas, "MonyIcon");
        Require(moneyPanel != null, "MonyIcon could not be found.");

        SetRect(
            moneyPanel.GetComponent<RectTransform>(),
            Vector2.one,
            Vector2.one,
            new Vector2(-24f, -24f),
            new Vector2(280f, 116f),
            Vector2.one);

        Image image = RequireComponent<Image>(moneyPanel.gameObject);
        image.sprite = null;
        image.type = Image.Type.Simple;
        image.color = Green;
        SetOutline(moneyPanel.gameObject, DeepInk, new Vector2(6f, -6f));

        Button button = RequireComponent<Button>(moneyPanel.gameObject);
        SetButton(button, image, Html("#71DD78"), Html("#36A944"));

        TextMeshProUGUI moneyText =
            RequireComponent<TextMeshProUGUI>(
                RequireChild(moneyPanel, "MonyText").gameObject);
        SetRect(
            moneyText.rectTransform,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, 15f),
            new Vector2(250f, 52f),
            new Vector2(0.5f, 0.5f));
        SetText(
            moneyText,
            displayFont,
            44f,
            Color.white,
            TextAlignmentOptions.Center,
            false);

        TextMeshProUGUI shopLabel =
            GetOrCreateText("ShopLabel", moneyPanel);
        SetRect(
            shopLabel.rectTransform,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, -34f),
            new Vector2(220f, 25f),
            new Vector2(0.5f, 0.5f));
        SetText(
            shopLabel,
            displayFont,
            20f,
            Html("#E9FFE9"),
            TextAlignmentOptions.Center,
            false);
        shopLabel.text = "상점";
    }

    private static void ConfigureTreatButton(
        Transform canvas,
        TMP_FontAsset displayFont)
    {
        Transform treat = FindChild(canvas, "TreatButton");
        Require(treat != null, "TreatButton could not be found.");

        SetRect(
            treat.GetComponent<RectTransform>(),
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(-24f, 24f),
            new Vector2(250f, 92f),
            new Vector2(1f, 0f));

        Image image = RequireComponent<Image>(treat.gameObject);
        image.sprite = null;
        image.type = Image.Type.Simple;
        image.color = Lime;
        SetOutline(treat.gameObject, DeepInk, new Vector2(6f, -6f));

        Button button = RequireComponent<Button>(treat.gameObject);
        SetButton(button, image, Html("#B5FA65"), Html("#78C62E"));

        TextMeshProUGUI label = treat.GetComponentInChildren<TextMeshProUGUI>(true);
        Require(label != null, "Treat button label is missing.");
        label.gameObject.SetActive(true);
        SetStretch(label.rectTransform, 14f, 14f, 8f, 8f);
        SetText(label, displayFont, 34f, Ink, TextAlignmentOptions.Center, false);
        label.text = "진료하기";
    }

    private static void ConfigureTalkBubble(
        Transform canvas,
        TMP_FontAsset bodyFont)
    {
        Transform bubble = FindChild(canvas, "TalkCircle");
        Require(bubble != null, "TalkCircle could not be found.");

        SetRect(
            bubble.GetComponent<RectTransform>(),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -18f),
            new Vector2(760f, 230f),
            new Vector2(0.5f, 1f));

        Image image = RequireComponent<Image>(bubble.gameObject);
        image.color = Color.white;
        image.preserveAspect = false;

        TextMeshProUGUI talk =
            RequireComponent<TextMeshProUGUI>(
                RequireChild(bubble, "Talk").gameObject);
        SetStretch(talk.rectTransform, 72f, 72f, 50f, 54f);
        SetText(talk, bodyFont, 32f, Ink, TextAlignmentOptions.Center, true);
        talk.fontSizeMin = 22f;
        talk.fontSizeMax = 34f;
    }

    private static void ConfigureShop(
        Transform canvas,
        TMP_FontAsset displayFont,
        TMP_FontAsset bodyFont)
    {
        Transform panel = FindChild(canvas, "ShopPanel");
        Require(panel != null, "ShopPanel could not be found.");

        SetStretch(panel.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);
        Image overlay = RequireComponent<Image>(panel.gameObject);
        overlay.sprite = null;
        overlay.type = Image.Type.Simple;
        overlay.color = new Color(DeepInk.r, DeepInk.g, DeepInk.b, 0.78f);

        Transform card = RequireChild(panel, "ShopBackground");
        SetRect(
            card.GetComponent<RectTransform>(),
            Vector2.one * 0.5f,
            Vector2.one * 0.5f,
            Vector2.zero,
            new Vector2(1120f, 780f),
            Vector2.one * 0.5f);
        Image cardImage = RequireComponent<Image>(card.gameObject);
        cardImage.sprite = null;
        cardImage.type = Image.Type.Simple;
        cardImage.color = Paper;
        SetOutline(card.gameObject, DeepInk, new Vector2(9f, -9f));
        card.SetAsFirstSibling();

        TextMeshProUGUI title =
            RequireComponent<TextMeshProUGUI>(
                RequireChild(panel, "Title").gameObject);
        SetRect(
            title.rectTransform,
            Vector2.one * 0.5f,
            Vector2.one * 0.5f,
            new Vector2(0f, 312f),
            new Vector2(520f, 64f),
            Vector2.one * 0.5f);
        SetText(title, displayFont, 50f, Ink, TextAlignmentOptions.Center, false);
        title.text = "식물 상점";

        TextMeshProUGUI money =
            RequireComponent<TextMeshProUGUI>(
                RequireChild(panel, "MoneyText").gameObject);
        SetRect(
            money.rectTransform,
            Vector2.one * 0.5f,
            Vector2.one * 0.5f,
            new Vector2(360f, 312f),
            new Vector2(260f, 48f),
            Vector2.one * 0.5f);
        SetText(money, displayFont, 30f, Green, TextAlignmentOptions.Right, false);

        Transform close = RequireChild(panel, "CloseButton");
        SetRect(
            close.GetComponent<RectTransform>(),
            Vector2.one * 0.5f,
            Vector2.one * 0.5f,
            new Vector2(500f, 326f),
            new Vector2(64f, 64f),
            Vector2.one * 0.5f);
        Image closeImage = RequireComponent<Image>(close.gameObject);
        closeImage.sprite = null;
        closeImage.color = Coral;
        Button closeButton = RequireComponent<Button>(close.gameObject);
        SetButton(closeButton, closeImage, Html("#E88479"), Html("#B94C43"));
        TextMeshProUGUI closeLabel = close.GetComponentInChildren<TextMeshProUGUI>(true);
        Require(closeLabel != null, "Shop close label is missing.");
        SetStretch(closeLabel.rectTransform, 4f, 4f, 4f, 4f);
        SetText(closeLabel, displayFont, 30f, Color.white, TextAlignmentOptions.Center, false);
        closeLabel.text = "X";

        Transform content = RequireChild(panel, "Content");
        SetRect(
            content.GetComponent<RectTransform>(),
            Vector2.one * 0.5f,
            Vector2.one * 0.5f,
            new Vector2(0f, -8f),
            new Vector2(820f, 570f),
            Vector2.one * 0.5f);
        VerticalLayoutGroup layout = RequireComponent<VerticalLayoutGroup>(content.gameObject);
        layout.padding = new RectOffset(20, 20, 20, 20);
        layout.spacing = 20f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter =
            RequireComponent<ContentSizeFitter>(content.gameObject);
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        panel.SetAsLastSibling();
    }

    private static void ConfigureChecklist(
        Transform canvas,
        TMP_FontAsset displayFont,
        TMP_FontAsset bodyFont)
    {
        Transform panel = FindChild(canvas, "ChecklistPannel");
        Require(panel != null, "ChecklistPannel could not be found.");

        panel.gameObject.SetActive(true);
        SetStretch(panel.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);
        Image overlay = RequireComponent<Image>(panel.gameObject);
        overlay.sprite = null;
        overlay.type = Image.Type.Simple;
        overlay.color = new Color(DeepInk.r, DeepInk.g, DeepInk.b, 0.76f);

        Transform paper = RequireChild(panel, "Paper");
        SetRect(
            paper.GetComponent<RectTransform>(),
            Vector2.one * 0.5f,
            Vector2.one * 0.5f,
            Vector2.zero,
            new Vector2(820f, 820f),
            Vector2.one * 0.5f);
        Image paperImage = RequireComponent<Image>(paper.gameObject);
        paperImage.sprite = null;
        paperImage.type = Image.Type.Simple;
        paperImage.color = Paper;
        SetOutline(paper.gameObject, DeepInk, new Vector2(9f, -9f));

        TextMeshProUGUI title =
            RequireComponent<TextMeshProUGUI>(
                RequireChild(paper, "Title").gameObject);
        SetRect(
            title.rectTransform,
            Vector2.one * 0.5f,
            Vector2.one * 0.5f,
            new Vector2(0f, 330f),
            new Vector2(560f, 62f),
            Vector2.one * 0.5f);
        SetText(title, displayFont, 46f, Ink, TextAlignmentOptions.Center, false);
        title.text = "진료 체크리스트";

        Transform content = RequireChild(paper, "ChecklistContent");
        SetRect(
            content.GetComponent<RectTransform>(),
            Vector2.one * 0.5f,
            Vector2.one * 0.5f,
            new Vector2(0f, 55f),
            new Vector2(650f, 500f),
            Vector2.one * 0.5f);
        VerticalLayoutGroup layout = RequireComponent<VerticalLayoutGroup>(content.gameObject);
        layout.padding = new RectOffset(20, 20, 18, 18);
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        ConfigureModalButton(
            RequireChild(paper, "ConfirmButton"),
            new Vector2(175f, -330f),
            new Vector2(240f, 76f),
            "진료 완료",
            Green,
            displayFont);
        ConfigureModalButton(
            RequireChild(paper, "CancleButton"),
            new Vector2(-175f, -330f),
            new Vector2(210f, 76f),
            "돌아가기",
            Coral,
            displayFont);

        panel.SetAsLastSibling();
    }

    private static void ConfigureModalButton(
        Transform buttonTransform,
        Vector2 position,
        Vector2 size,
        string labelText,
        Color color,
        TMP_FontAsset font)
    {
        SetRect(
            buttonTransform.GetComponent<RectTransform>(),
            Vector2.one * 0.5f,
            Vector2.one * 0.5f,
            position,
            size,
            Vector2.one * 0.5f);

        Image image = RequireComponent<Image>(buttonTransform.gameObject);
        image.sprite = null;
        image.type = Image.Type.Simple;
        image.color = color;
        SetOutline(buttonTransform.gameObject, DeepInk, new Vector2(5f, -5f));

        Button button = RequireComponent<Button>(buttonTransform.gameObject);
        SetButton(
            button,
            image,
            Color.Lerp(color, Color.white, 0.18f),
            Color.Lerp(color, Color.black, 0.18f));

        TextMeshProUGUI label =
            buttonTransform.GetComponentInChildren<TextMeshProUGUI>(true);
        Require(label != null, $"{buttonTransform.name} label is missing.");
        SetStretch(label.rectTransform, 10f, 10f, 8f, 8f);
        SetText(label, font, 30f, Color.white, TextAlignmentOptions.Center, false);
        label.text = labelText;
    }

    private static void ConfigureBackground(Scene scene)
    {
        Transform background = FindRoot(scene, "Background");
        Transform cameraTransform = FindRoot(scene, "Main Camera");
        Require(background != null, "Background could not be found.");
        Require(cameraTransform != null, "Main Camera could not be found.");

        ViewportBackgroundFitter fitter =
            RequireComponent<ViewportBackgroundFitter>(background.gameObject);
        fitter.Configure(cameraTransform.GetComponent<Camera>());
    }

    private static void ConfigureShopItemPrefab(
        TMP_FontAsset displayFont,
        TMP_FontAsset bodyFont)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(ShopItemPath);

        try
        {
            RectTransform rootRect = RequireComponent<RectTransform>(root);
            rootRect.sizeDelta = new Vector2(760f, 150f);

            Image image = RequireComponent<Image>(root);
            image.sprite = null;
            image.type = Image.Type.Simple;
            image.color = PaleGreen;
            SetOutline(root, Html("#7B9681"), new Vector2(4f, -4f));

            HorizontalLayoutGroup layout = RequireComponent<HorizontalLayoutGroup>(root);
            layout.padding = new RectOffset(26, 26, 18, 18);
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            LayoutElement layoutElement = RequireComponent<LayoutElement>(root);
            layoutElement.enabled = true;
            layoutElement.preferredWidth = 760f;
            layoutElement.preferredHeight = 150f;

            Transform icon = RequireChild(root.transform, "Icon");
            icon.GetComponent<RectTransform>().sizeDelta = new Vector2(88f, 88f);
            RequireComponent<Image>(icon.gameObject).preserveAspect = true;

            TextMeshProUGUI name =
                RequireComponent<TextMeshProUGUI>(
                    RequireChild(root.transform, "Name").gameObject);
            name.rectTransform.sizeDelta = new Vector2(330f, 104f);
            SetText(name, bodyFont, 30f, Ink, TextAlignmentOptions.MidlineLeft, true);
            name.fontSizeMin = 20f;
            name.fontSizeMax = 30f;

            TextMeshProUGUI price =
                RequireComponent<TextMeshProUGUI>(
                    RequireChild(root.transform, "Price").gameObject);
            price.rectTransform.sizeDelta = new Vector2(120f, 54f);
            SetText(price, displayFont, 25f, Ink, TextAlignmentOptions.Center, false);

            Transform buy = RequireChild(root.transform, "BuyButton");
            buy.GetComponent<RectTransform>().sizeDelta = new Vector2(110f, 64f);
            Image buyImage = RequireComponent<Image>(buy.gameObject);
            buyImage.sprite = null;
            buyImage.color = Green;
            SetButton(
                RequireComponent<Button>(buy.gameObject),
                buyImage,
                Html("#72DD79"),
                Html("#36A944"));

            TextMeshProUGUI buyLabel =
                buy.GetComponentInChildren<TextMeshProUGUI>(true);
            Require(buyLabel != null, "Shop buy label is missing.");
            SetStretch(buyLabel.rectTransform, 4f, 4f, 4f, 4f);
            SetText(
                buyLabel,
                displayFont,
                24f,
                Color.white,
                TextAlignmentOptions.Center,
                false);
            buyLabel.text = "구매";

            PrefabUtility.SaveAsPrefabAsset(root, ShopItemPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void ConfigureTreatmentTogglePrefab(TMP_FontAsset bodyFont)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(TreatmentTogglePath);

        try
        {
            RectTransform rootRect = RequireComponent<RectTransform>(root);
            rootRect.sizeDelta = new Vector2(600f, 54f);

            LayoutElement layoutElement = RequireComponent<LayoutElement>(root);
            layoutElement.enabled = true;
            layoutElement.preferredWidth = 600f;
            layoutElement.preferredHeight = 54f;

            Toggle toggle = RequireComponent<Toggle>(root);
            ColorBlock colors = toggle.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Html("#E6F7E7");
            colors.pressedColor = Html("#C7E9C9");
            colors.selectedColor = Html("#D7F3D9");
            toggle.colors = colors;
            toggle.navigation = new Navigation { mode = Navigation.Mode.None };

            Transform background = RequireChild(root.transform, "Background");
            SetRect(
                background.GetComponent<RectTransform>(),
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(19f, 0f),
                new Vector2(36f, 36f),
                Vector2.one * 0.5f);

            Image backgroundImage = RequireComponent<Image>(background.gameObject);
            backgroundImage.color = Color.white;

            Transform checkmark = RequireChild(background, "Checkmark");
            RequireComponent<Image>(checkmark.gameObject).color = Green;

            TextMeshProUGUI label =
                RequireComponent<TextMeshProUGUI>(
                    RequireChild(root.transform, "Label").gameObject);
            SetStretch(label.rectTransform, 50f, 8f, 2f, 2f);
            SetText(label, bodyFont, 27f, Ink, TextAlignmentOptions.MidlineLeft, true);
            label.fontSizeMin = 20f;
            label.fontSizeMax = 27f;

            PrefabUtility.SaveAsPrefabAsset(root, TreatmentTogglePath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void Validate(Scene scene)
    {
        Transform canvas = FindRoot(scene, "Canvas");
        Require(canvas != null, "Polished Canvas is missing.");

        Transform info = FindChild(canvas, "PatientInfoPanel");
        Require(info != null, "Patient info panel was not created.");
        Require(
            RequireComponent<ClinicHud>(info.gameObject).IsConfigured,
            "Clinic HUD references are incomplete.");

        Require(
            FindChild(canvas, "MonyIcon")
                .GetComponent<RectTransform>().anchoredPosition.x < 0f,
            "Money panel needs a right-side inset.");
        Require(
            FindChild(canvas, "TreatButton")
                .GetComponent<RectTransform>().anchoredPosition.x < 0f,
            "Treat button needs a right-side inset.");

        GameObject shopPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>(ShopItemPath);
        GameObject togglePrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>(TreatmentTogglePath);
        Require(shopPrefab != null, "Shop item prefab is missing.");
        Require(togglePrefab != null, "Treatment toggle prefab is missing.");
        Require(
            shopPrefab.GetComponent<RectTransform>().sizeDelta.y >= 140f,
            "Shop item height is too small.");
        Require(
            togglePrefab.GetComponent<RectTransform>().sizeDelta.x >= 560f,
            "Treatment toggle width is too small.");

        foreach (GameObject root in scene.GetRootGameObjects())
            RequireNoMissingScripts(root);
    }

    private static void RequireNoMissingScripts(GameObject gameObject)
    {
        Require(
            GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject) == 0,
            $"Missing script on {gameObject.name}.");

        foreach (Transform child in gameObject.transform)
            RequireNoMissingScripts(child.gameObject);
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

    private static void SetText(
        TextMeshProUGUI text,
        TMP_FontAsset font,
        float size,
        Color color,
        TextAlignmentOptions alignment,
        bool autoSize)
    {
        text.font = font;
        text.fontSize = size;
        text.fontSizeMin = Mathf.Max(16f, size * 0.68f);
        text.fontSizeMax = size;
        text.enableAutoSizing = autoSize;
        text.color = color;
        text.alignment = alignment;
        text.characterSpacing = 0f;
        text.wordSpacing = 0f;
        text.lineSpacing = 0f;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
    }

    private static void SetButton(
        Button button,
        Image target,
        Color highlighted,
        Color pressed)
    {
        button.targetGraphic = target;
        button.transition = Selectable.Transition.ColorTint;
        button.navigation = new Navigation { mode = Navigation.Mode.None };

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = highlighted;
        colors.pressedColor = pressed;
        colors.selectedColor = highlighted;
        colors.disabledColor = new Color(0.62f, 0.67f, 0.62f, 0.65f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
    }

    private static void SetOutline(
        GameObject gameObject,
        Color color,
        Vector2 distance)
    {
        Outline outline = RequireComponent<Outline>(gameObject);
        outline.effectColor = color;
        outline.effectDistance = distance;
        outline.useGraphicAlpha = true;
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

    private static void SetStretch(
        RectTransform rect,
        float left,
        float right,
        float top,
        float bottom)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = Vector2.one * 0.5f;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
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

    private static Transform RequireChild(Transform parent, string name)
    {
        Transform child = FindChild(parent, name);
        Require(child != null, $"{parent.name}/{name} could not be found.");
        return child;
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
