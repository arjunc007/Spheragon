using UnityEngine;

public class InputManager : MonoBehaviour {

    public static InputManager instance = null;
    private Vector3 clickPos;
    private float dragTime = 0;
    private Vector3 dragLength;

    private bool useTouch = false;

    private Vector3 initialMousePosition;
    private Vector3 lastMousePosition;
    private Vector3 finalMousePosition;
    private float pinchDiff;

    Camera cam;

    //Events and Delegates
    public delegate void OnClickEventHandler(Vector3 pos);
    public event OnClickEventHandler ClickedEvent;

    public delegate void OnDragEventHandler(Vector3 diff);
    public event OnDragEventHandler DraggingEvent;

    public delegate void OnDragEndEventHandler(Vector3 speed);
    public event OnDragEndEventHandler DragEndEvent;

    public delegate void OnPinchEventHandler(float diff);
    public event OnPinchEventHandler PinchEvent;

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

        if (Application.platform == RuntimePlatform.Android)
            useTouch = true;
    }

    // Update is called once per frame
    void Update () {

        if (!GameManager.isPaused)
        {
            if(useTouch)
            {
                if (Input.touchCount == 1)
                {
                    Touch touch = Input.GetTouch(0);
                    switch (touch.phase)
                    {
                        case TouchPhase.Began:
                            BeginInput(touch.position);
                            break;
                        case TouchPhase.Moved:
                            finalMousePosition = touch.position;
                            dragTime += Time.deltaTime;
                            if (lastMousePosition != finalMousePosition)
                            {
                                dragLength = finalMousePosition - initialMousePosition;
                                //Debug.Log("Dragging");
                                OnDragging();
                            }

                            lastMousePosition = finalMousePosition;
                            break;
                        case TouchPhase.Ended:
                            finalMousePosition = touch.position;
                            if (initialMousePosition == finalMousePosition)
                            {
                                //Debug.Log("Clicked");
                                clickPos = finalMousePosition;
                                OnClick();
                            }
                            else
                            {
                                //Debug.Log("Drag ended");
                                dragLength = finalMousePosition - initialMousePosition;
                                OnDragEnd();
                            }
                            break;
                    }
                }
                else if (Input.touchCount == 2)
                {
                        // Store both touches.
                        Touch touchZero = Input.GetTouch(0);
                        Touch touchOne = Input.GetTouch(1);

                        // Find the position in the previous frame of each touch.
                        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                        // Find the magnitude of the vector (the distance) between the touches in each frame.
                        float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                        float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                        // Find the difference in the distances between each frame.
                        pinchDiff = prevTouchDeltaMag - touchDeltaMag;

                        OnPinch();
                    }

                }   //Use Touch
            else
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
                        //Debug.Log("Clicked");
                        clickPos = finalMousePosition;
                        OnClick();
                    }
                    else
                    {
                        //Debug.Log("Drag ended");
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
                        //Debug.Log("Dragging");
                        OnDragging();
                    }

                    lastMousePosition = finalMousePosition;
                }
            }   //Not using touch
        }   //Game Loop
    }

    private void BeginInput(Vector2 position)
    {
        initialMousePosition = position;
        lastMousePosition = initialMousePosition;
        finalMousePosition = initialMousePosition;
        dragTime = 0;
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

    protected virtual void OnPinch()
    {
        if(PinchEvent != null)
        {
            PinchEvent(pinchDiff);
        }
    }
}
