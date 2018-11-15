using UnityEngine;

public class InputManager : MonoBehaviour {

    public static InputManager instance = null;
    private Vector3 clickPos;
    private float dragTime = 0;
    private Vector3 dragLength;

    private Vector3 initialMousePosition;
    private Vector3 finalMousePosition;

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
            DontDestroyOnLoad(this);
        }
        else if (instance != this)
        {
            Destroy(this);
        }
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButtonDown(0))
        {
            initialMousePosition = Input.mousePosition;
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
        else if(Input.GetMouseButton(0))
        {
            finalMousePosition = Input.mousePosition;
            dragTime += Time.deltaTime;
            if (initialMousePosition != finalMousePosition)
            {
                dragLength = finalMousePosition - initialMousePosition;
                Debug.Log("Dragging");
                OnDragging();
            }
        }
        
    }

    protected virtual void OnClick()
    {
        if (ClickedEvent != null)
            ClickedEvent(Camera.main.ScreenToWorldPoint(clickPos));
    }

    protected virtual void OnDragging()
    {
        if(DraggingEvent != null)
        {
            DraggingEvent(Camera.main.ScreenToWorldPoint(dragLength));
        }
    }

    protected virtual void OnDragEnd()
    {
        if(DragEndEvent != null)
        {
            DragEndEvent(Camera.main.ScreenToWorldPoint(dragLength) / dragTime);
        }
    }
}
