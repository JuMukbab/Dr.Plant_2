using UnityEngine;

namespace DrPlant.Data
{
    public static class DrPlantContent
    {
        public const string ResourceName = "DrPlantContentCatalog";

        private static DrPlantContentCatalog cachedCatalog;

        public static DrPlantContentCatalog Catalog
        {
            get
            {
                if (cachedCatalog == null)
                    cachedCatalog = Resources.Load<DrPlantContentCatalog>(ResourceName);

                if (cachedCatalog == null)
                {
                    Debug.LogError(
                        $"Missing Resources/{ResourceName}.asset. "
                        + "Run Dr.Plant > Content > Rebuild Default Catalog in the Unity Editor.");
                }

                return cachedCatalog;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache()
        {
            cachedCatalog = null;
        }
    }
}
