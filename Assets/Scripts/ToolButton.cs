using UnityEngine;

public class ToolButton : MonoBehaviour
{
    public ToolManager.ToolType toolType;

    public void OnClick()
    {
        ToolManager.Instance.ToggleTool(toolType);
    }
}
