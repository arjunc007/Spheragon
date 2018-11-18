using UnityEngine;

public class InputManager : MonoBehaviour {

    public static InputManager instance = null;
    private Vector3 clickPos;
    private float dragTime = 0;
    private Vector3 dragLength;

    private Vector3 initialMousePosition;
    private Vector3 lastMousePosition;
    private Vector3 finalMousePosition;

    Camera cam;

    //Events and Delegates
    public delegate void OnClickEventHandler(Vector3 pos);
    public event OnClickEventHandler ClickedEvent;

    public delegate void OnDragEventHandler(Vector3 diff);
    public event OnDragEventHandler DraggingEvent;

    public delegate void OnDragEndEventHandler(Vector3 speed);
    public event OnDragEndEventHandler DragEndEvent;

    // Use this for initialization
    void Awake () {
        if(instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(this);
        }
        else if (instance != this)
        {
            Destroy(this);
        }
	}

    private void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update () {

        if (!GameManager.isPaused)
        {
            if (Input.GetMouseButtonDown(0))
            {
                initialMousePosition = Input.mousePosition;
                lastMousePosition = initialMousePosition;
                finalMousePosition = initialMousePosition;
                dragTime = 0;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                finalMousePosition = Input.mousePosition;
                if (initialMousePosition == finalMousePosition)
                {
                    Debug.Log("Clicked");
                    clickPos = finalMousePosition;
                    OnClick();
                }
                else
                {
                    Debug.Log("Drag ended");
                    dragLength = finalMousePosition - initialMousePosition;
                    OnDragEnd();
                }
            }
            else if (Input.GetMouseButton(0))
            {
                finalMousePosition = Input.mousePosition;
                dragTime += Time.deltaTime;
                if (lastMousePosition != finalMousePosition)
                {
                    dragLength = finalMousePosition - initialMousePosition;
                    Debug.Log("Dragging");
                    OnDragging();
                }

                lastMousePosition = finalMousePosition;
            }
        }
    }

    protected virtual void OnClick()
    {
        if (ClickedEvent != null)
            ClickedEvent(cam.ScreenToWorldPoint(new Vector3(clickPos.x, clickPos.y, cam.nearClipPlane)));
    }

    protected virtual void OnDragging()
    {
        if(DraggingEvent != null)
        {
            DraggingEvent(dragLength);
            //DraggingEvent(cam.ScreenToWorldPoint(new Vector3(dragLength.x, dragLength.y, cam.nearClipPlane)));
        }
    }

    protected virtual void OnDragEnd()
    {
        if(DragEndEvent != null)
        {
            DragEndEvent(dragLength / dragTime);
        }
    }
}
