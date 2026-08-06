using System;
using System.Collections.Generic;
using DrPlant.Data;
using UnityEditor;
using UnityEngine;

public static class DrPlantContentSeeder
{
    private const string CatalogAssetPath =
        "Assets/Resources/DrPlantContentCatalog.asset";

    [MenuItem("Dr.Plant/Content/Rebuild Default Catalog")]
    public static void CreateOrUpdateDefaultCatalog()
    {
        EnsureResourcesFolder();

        DrPlantContentCatalog catalog =
            AssetDatabase.LoadAssetAtPath<DrPlantContentCatalog>(CatalogAssetPath);

        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<DrPlantContentCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogAssetPath);
        }

        catalog.ConfigureForEditor(
            BuildRules(),
            BuildPatients(),
            BuildSymptoms(),
            BuildTreatments(),
            BuildShopItems(),
            BuildDialogues());

        List<string> errors = catalog.GetValidationErrors();
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "Dr.Plant content catalog is invalid:"
                + Environment.NewLine
                + string.Join(Environment.NewLine, errors));
        }

        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        int playablePatients = 0;
        foreach (PatientDefinition patient in catalog.Patients)
        {
            if (patient.IsPlayable)
                playablePatients++;
        }

        Debug.Log(
            $"Dr.Plant catalog rebuilt: {catalog.Patients.Count} patients "
            + $"({playablePatients} playable), {catalog.Symptoms.Count} symptoms, "
            + $"{catalog.Treatments.Count} treatments, {catalog.ShopItems.Count} shop items.");
    }

    private static ClinicRules BuildRules()
    {
        return new ClinicRules(
            compositeSymptomChance: 0.30f,
            normalDialogueChance: 0.30f,
            maxSymptoms: 2,
            correctRewardMin: 150,
            correctRewardMax: 200,
            incorrectRewardMin: 5,
            incorrectRewardMax: 10);
    }

    private static PatientDefinition[] BuildPatients()
    {
        return new[]
        {
            new PatientDefinition(
                PatientId.Flower,
                "꽃 환자",
                LoadPrefab("Flower"),
                animationSpeed: 0.65f,
                displayScale: 6.5f,
                voice: Voice(
                    frequency: 650f,
                    waveform: PatientVoiceWaveform.Sine,
                    volume: 0.045f,
                    duration: 0.045f,
                    pitchVariation: 45f)),
            new PatientDefinition(
                PatientId.Bean,
                "콩 환자",
                LoadPrefab("Mr.Bean"),
                animationSpeed: 0.80f,
                displayScale: 6.5f,
                voice: Voice(
                    frequency: 500f,
                    waveform: PatientVoiceWaveform.Square,
                    volume: 0.025f,
                    duration: 0.035f,
                    pitchVariation: 60f)),
            new PatientDefinition(
                PatientId.Cactus,
                "선인장 환자",
                LoadPrefab("Cactus"),
                animationSpeed: 0.70f,
                displayScale: 6.5f,
                voice: Voice(
                    frequency: 340f,
                    waveform: PatientVoiceWaveform.Triangle,
                    volume: 0.040f,
                    duration: 0.055f,
                    pitchVariation: 30f)),
            new PatientDefinition(
                PatientId.Sprout,
                "새싹 환자",
                LoadPrefab("Sprout"),
                animationSpeed: 0.75f,
                displayScale: 7.0f,
                voice: Voice(
                    frequency: 760f,
                    waveform: PatientVoiceWaveform.Sine,
                    volume: 0.035f,
                    duration: 0.035f,
                    pitchVariation: 70f)),
            new PatientDefinition(
                PatientId.Succulent,
                "다육이 환자",
                LoadPrefab("Succulent"),
                animationSpeed: 0.80f,
                displayScale: 6.5f,
                voice: Voice(
                    frequency: 430f,
                    waveform: PatientVoiceWaveform.Triangle,
                    volume: 0.040f,
                    duration: 0.060f,
                    pitchVariation: 35f))
        };
    }

    private static PatientVoiceProfile Voice(
        float frequency,
        PatientVoiceWaveform waveform,
        float volume,
        float duration,
        float pitchVariation)
    {
        return new PatientVoiceProfile(
            frequency,
            waveform,
            volume,
            duration,
            pitchVariation);
    }

    private static TreatmentDefinition[] BuildTreatments()
    {
        return new[]
        {
            new TreatmentDefinition(
                TreatmentId.Water,
                "물 주기",
                ShopItemId.None),
            new TreatmentDefinition(
                TreatmentId.Cool,
                "쿨팩 대주기",
                ShopItemId.None),
            new TreatmentDefinition(
                TreatmentId.Warm,
                "온열매트 위로 옮기기",
                ShopItemId.None),
            new TreatmentDefinition(
                TreatmentId.Fertilizer,
                "영양제 주기",
                ShopItemId.None),
            new TreatmentDefinition(
                TreatmentId.Music,
                "음악 들려주기",
                ShopItemId.Instrument),
            new TreatmentDefinition(
                TreatmentId.Prune,
                "가지치기",
                ShopItemId.Scissors),
            new TreatmentDefinition(
                TreatmentId.Sunglasses,
                "선글라스 씌워주기",
                ShopItemId.Sunglasses)
        };
    }

    private static ShopItemDefinition[] BuildShopItems()
    {
        return new[]
        {
            new ShopItemDefinition(
                ShopItemId.Instrument,
                "악기",
                "음악 치료와 지루함 증상이 해금됩니다.",
                price: 400,
                TreatmentId.Music),
            new ShopItemDefinition(
                ShopItemId.Scissors,
                "가지치기 가위",
                "가지치기 치료와 과성장 증상이 해금됩니다.",
                price: 900,
                TreatmentId.Prune),
            new ShopItemDefinition(
                ShopItemId.Sunglasses,
                "선글라스",
                "고온 환자에게 선글라스 치료를 사용할 수 있습니다.",
                price: 1500,
                TreatmentId.Sunglasses)
        };
    }

    private static SymptomDefinition[] BuildSymptoms()
    {
        return new[]
        {
            new SymptomDefinition(
                SymptomId.Dehydration,
                "수분 부족",
                new[] { TreatmentId.Water },
                ShopItemId.None,
                Array.Empty<SymptomId>(),
                new[]
                {
                    "목이 너무 말라요...",
                    "흙이 바싹 말라버린 것 같아요.",
                    "물을 마신 지 너무 오래됐어요...",
                    "잎에 힘이 하나도 없어요.",
                    "흙이 점점 딱딱해지고 있어요..."
                }),
            new SymptomDefinition(
                SymptomId.Hot,
                "고온",
                new[] { TreatmentId.Cool, TreatmentId.Sunglasses },
                ShopItemId.None,
                new[] { SymptomId.Cold },
                new[]
                {
                    "몸이 너무 뜨거워요...",
                    "잎 끝이 뜨겁게 달아올랐어요.",
                    "조금 시원한 곳으로 가고 싶어요.",
                    "햇빛이 오늘따라 너무 따가워요.",
                    "몸에서 계속 열이 나는 기분이에요..."
                }),
            new SymptomDefinition(
                SymptomId.Cold,
                "저온",
                new[] { TreatmentId.Warm },
                ShopItemId.None,
                new[] { SymptomId.Hot },
                new[]
                {
                    "몸이 자꾸 덜덜 떨려요...",
                    "뿌리까지 얼어붙는 것 같아요.",
                    "조금 더 따뜻한 곳은 없나요?",
                    "따뜻한 햇빛이 그리워요.",
                    "화분 안까지 너무 차가워졌어요."
                }),
            new SymptomDefinition(
                SymptomId.Malnutrition,
                "영양 부족",
                new[] { TreatmentId.Fertilizer },
                ShopItemId.None,
                Array.Empty<SymptomId>(),
                new[]
                {
                    "물을 마셔도 계속 기운이 없어요...",
                    "요즘 새잎이 잘 자라지 않아요.",
                    "몸에 필요한 게 부족한 것 같아요.",
                    "잎 색이 전보다 옅어진 것 같아요.",
                    "충분히 쉬었는데도 힘이 없어요..."
                }),
            new SymptomDefinition(
                SymptomId.Boredom,
                "지루함",
                new[] { TreatmentId.Music },
                ShopItemId.Instrument,
                Array.Empty<SymptomId>(),
                new[]
                {
                    "저 너무 지루해요...",
                    "누가 음악이라도 들려줬으면 좋겠어요.",
                    "벽 무늬를 세는 것도 이제 지겨워요.",
                    "신나는 노래 같은 건 없나요?",
                    "계속 가만히 있으니까 너무 심심해요..."
                }),
            new SymptomDefinition(
                SymptomId.Overgrown,
                "과성장",
                new[] { TreatmentId.Prune },
                ShopItemId.Scissors,
                Array.Empty<SymptomId>(),
                new[]
                {
                    "불필요한 잎들이 너무 많아졌어요...",
                    "잎이 너무 무성해서 움직이기 힘들어요.",
                    "오래된 잎을 조금 정리하고 싶어요.",
                    "새잎이 자랄 공간이 부족해요.",
                    "몸이 너무 복잡하고 답답해요..."
                })
        };
    }

    private static DialogueLibrary BuildDialogues()
    {
        return new DialogueLibrary(
            arrival: new[]
            {
                "안녕하세요!",
                "안녕요...!",
                "잘 부탁드려요.",
                "오늘 진료 잘 부탁드릴게요.",
                "여기가 Dr.Plant 맞죠?",
                "선생님, 안녕하세요.",
                "조금 긴장되네요..."
            },
            normal: new[]
            {
                "오늘 날씨가 좋네요... 그죠?",
                "저 조금 긴장했어요.",
                "선생님은 식물을 좋아하세요?",
                "여기 병원 분위기가 신기하네요.",
                "요즘 어떻게 지내세요?",
                "저 잘 부탁드릴게요.",
                "병원은 처음이라 신기해요."
            },
            goodReviews: new[]
            {
                "이제 괜찮아졌어요!",
                "몸이 한결 가벼워졌어요!",
                "정확한 치료였어요. 감사합니다!",
                "다음에도 여기로 올게요!",
                "선생님을 믿길 잘했어요!"
            },
            badReviews: new[]
            {
                "기분이 많이 상했어요...",
                "이거 돌팔이 아니야?",
                "전보다 더 아픈 것 같은데요...",
                "정말 이 치료가 맞는 건가요?",
                "다음에는 다른 병원에 갈래요."
            });
    }

    private static GameObject LoadPrefab(string prefabName)
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>(
            $"Assets/Prefabs/{prefabName}.prefab");
    }

    private static void EnsureResourcesFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
    }
}
