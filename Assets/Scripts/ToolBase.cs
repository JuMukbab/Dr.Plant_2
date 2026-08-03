using UnityEngine;

public class ToolBase : MonoBehaviour
{
    //모든 도구의 부모
    public string toolName;

    public virtual void Use(PlantStatus target)
    {

    }
}
