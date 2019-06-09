using UnityEngine;

public class InputManager : MonoBehaviour {

    public float touchSensitivity = 0.1f;
    public static InputManager instance = null;
    private Vector2 dragSpeed;

    private Vector3 initialMousePosition;
    private Vector3 lastMousePosition;
    private Vector3 currentMousePosition;
    private float pinchDiff;
    private bool portraitMode;

    private Camera cam;
    /// <summary>
    /// For when the player returns from Pause, ignore the first input, i.e. the touch that Disabled the pause
    /// </summary>
    private bool wasPaused = false;

    //Events and Delegates
    public delegate void OnClickEventHandler(Vector3 pos);
    public event OnClickEventHandler ClickedEvent;

    public delegate void OnDragEventHandler(Vector3 start, Vector3 diff);
    public event OnDragEventHandler DraggingEvent;

    public delegate void OnDragEndEventHandler(Vector3 speed);
    public event OnDragEndEventHandler DragEndEvent;

    public delegate void OnPinchEventHandler(float diff);
    public event OnPinchEventHandler PinchEvent;

    public delegate void OnOrientationEventHandler();
    public event OnOrientationEventHandler OrientationEvent;

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

        portraitMode = Screen.height > Screen.width;
    }

    // Update is called once per frame
    void Update () {
        if (!GameManager.isPaused)
        {
#if UNITY_ANDROID
                if (Input.touchCount == 1)
                {
                    Touch touch = Input.GetTouch(0);
                    switch (touch.phase)
                    {
                        case TouchPhase.Began:
                            BeginInput(touch.position);
                            break;
                        case TouchPhase.Moved:
                            currentMousePosition = touch.position;
                            ContinueInput(touch.deltaPosition * Time.deltaTime / touch.deltaTime);
                            lastMousePosition = touch.position;
                            break;
                        case TouchPhase.Ended:
                            EndInput(touch.deltaPosition * Time.deltaTime / touch.deltaTime);
                            lastMousePosition = touch.position;
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
#endif

            if(Input.mousePresent)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    BeginInput(Input.mousePosition);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    currentMousePosition = Input.mousePosition;
                    EndInput(currentMousePosition - lastMousePosition);
                    lastMousePosition = currentMousePosition;
                }
                else if (Input.GetMouseButton(0))
                {
                    currentMousePosition = Input.mousePosition;
                    ContinueInput(currentMousePosition - lastMousePosition);
                    lastMousePosition = currentMousePosition;
                }
                else if(Input.mousePresent)
                {
                    pinchDiff = -Input.mouseScrollDelta.y;
                    OnPinch();
                }
            }   //Not using touch

            if (Screen.height > Screen.width)
            {
                if(!portraitMode)
                {
                    portraitMode = true;
                    OnOrientationChange();
                }
            }
            else
            {
                if(portraitMode)
                {
                    portraitMode = false;
                    OnOrientationChange();
                }
            }
        }   //Game Loop
        else
        {
            wasPaused = true;
        }
    }

    private void ContinueInput(Vector2 deltaPosition)
    {
        dragSpeed = deltaPosition;
        OnDragging();
    }

    private void EndInput(Vector2 deltaPosition)
    {
        if (wasPaused)
        {
            wasPaused = false;
            return;
        }

        if (Vector3.Distance(initialMousePosition, lastMousePosition) < touchSensitivity)
        {
            OnClick();
        }
        else
        {
            dragSpeed = (deltaPosition) * Time.deltaTime;
            OnDragEnd();
        }
    }

    private void BeginInput(Vector2 position)
    {
        initialMousePosition = position;
        lastMousePosition = position;
        currentMousePosition = position;
    }

    protected virtual void OnClick()
    {
        if (ClickedEvent != null)
            ClickedEvent(cam.ScreenToWorldPoint(new Vector3(currentMousePosition.x, currentMousePosition.y, cam.nearClipPlane)));
    }

    protected virtual void OnDragging()
    {
        if(DraggingEvent != null)
        {
            DraggingEvent(currentMousePosition, dragSpeed);
        }
    }

    protected virtual void OnDragEnd()
    {
        if(DragEndEvent != null)
        {
            DragEndEvent(dragSpeed);
        }
    }

    protected virtual void OnPinch()
    {
        if(PinchEvent != null)
        {
            PinchEvent(pinchDiff);
        }
    }

    protected virtual void OnOrientationChange()
    {
        if(OrientationEvent != null)
        {
            OrientationEvent();
        }
    }

    public bool IsPortrait()
    {
        return portraitMode;
    }
}
