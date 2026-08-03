using UnityEngine;
using UnityEngine.UI;

public class ToolManager : MonoBehaviour
{
    void Start()
    {
        if (cursorImage != null)
            cursorImage.enabled = false;
    }
    public enum ToolType
    {
        None,
        Stethoscope,
        Scissor,
        Watering,
        Music,
        AED
    }

    public static ToolManager Instance;

    public ToolType currentTool = ToolType.None;

    [Header("Cursor Tool")]
    public Image cursorImage;

    [Header("Sprites")]
    public Sprite stethoscopeSprite;
    public Sprite scissorSprite;
    public Sprite wateringSprite;
    public Sprite musicSprite;
    public Sprite aedSprite;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (cursorImage == null)
            return;

        cursorImage.transform.position = Input.mousePosition;
    }

    public void ToggleTool(ToolType tool)
    {
        if (currentTool == tool)
        {
            currentTool = ToolType.None;

            cursorImage.enabled = false;

            return;
        }

        currentTool = tool;

        cursorImage.enabled = true;

        switch (tool)
        {
            case ToolType.Stethoscope:
                cursorImage.sprite = stethoscopeSprite;
                break;

            case ToolType.Scissor:
                cursorImage.sprite = scissorSprite;
                break;

            case ToolType.Watering:
                cursorImage.sprite = wateringSprite;
                break;

            case ToolType.Music:
                cursorImage.sprite = musicSprite;
                break;

            case ToolType.AED:
                cursorImage.sprite = aedSprite;
                break;
        }
    }
}