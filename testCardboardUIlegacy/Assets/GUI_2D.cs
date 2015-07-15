using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;


public class GUI_2D : MonoBehaviour {

    bool toggleState = false;

    Touch simulateTouch;

    public Cardboard cardb;

    int touchCountNeeded;

	// Use this for initialization
	void Start () {
      /*  simulateTouch = new Touch();
        using UnityEngine.EventSystems.EventSystem.current.
        */
      //  cardb.VRModeEnabled = false;
	}
	
	// Update is called once per frame
	void Update () {

   /*     if (Time.time > 2.0f)
        {
            cardb.VRModeEnabled = true;
        }*/

        if (cardb.VRModeEnabled)
        {
            touchCountNeeded = 2;
        }
        else
        {
            touchCountNeeded = 3;
        }

        cardb.VRModeEnabled = toggleState;
	}

    void OnGUI()
    {
        Input.simulateMouseWithTouches = true;
        Input.multiTouchEnabled = true;
        

        Rect guiZone = new Rect(0,0,128,128);
        GUI.Button( guiZone, toggleState.ToString() );

/*        if (GUI.Button(guiZone, toggleState.ToString()))
        {
            toggleState = !toggleState;
        }*/

        Event currentEvent = Event.current;

 /*            //var e : Event = Event.current;
        if (currentEvent.isMouse && currentEvent.type == EventType.MouseDown && currentEvent.clickCount == 2)
     {
         // Double click event
         Debug.Log("Double click");
         cardb.VRModeEnabled = !cardb.VRModeEnabled;
     }*/

        

        if (Input.touchCount >= touchCountNeeded)
        {
            for(int i=0; i< touchCountNeeded; i++)
            {

            }
        }


        /*
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch currentTouch = Input.GetTouch(i);
            Vector2 touchPos = currentTouch.position;
            touchPos.y = Screen.height-touchPos.y;
            //touchPos.y = -touchPos.y;


            if (touchPos.x == 0 && touchPos.y == 0)
            {
                continue;
            }

                GUI.Button(new Rect(touchPos.x, touchPos.y, 64, 64), currentTouch.fingerId.ToString());

                if (guiZone.Contains(touchPos))
                {
                   // if (currentTouch.tapCount > 1)//.phase == TouchPhase.Ended)
                    //{

                    

                    if (currentEvent.clickCount >1 || currentTouch.tapCount>1)
                    {
                        toggleState = !toggleState;
                    }
                   // }
                }
            
        }
        */
/*        for (int i = 0; i < AndroidInput.touchCountSecondary; i++)
        {
            Touch currentTouch = AndroidInput.GetSecondaryTouch(i);
            Vector2 touchPos = currentTouch.position;
            touchPos.y = Screen.height - touchPos.y;
            //touchPos.y = -touchPos.y;


            if (touchPos.x == 0 && touchPos.y == 0)
            {
                continue;
            }

            GUI.Button(new Rect(touchPos.x, touchPos.y, 64, 64), "S_"+currentTouch.fingerId.ToString());

            if (guiZone.Contains(touchPos))
            {
                // if (currentTouch.tapCount > 1)//.phase == TouchPhase.Ended)
                //{



                if (currentTouch.phase == TouchPhase.Ended)
                {
                    toggleState = !toggleState;
                }
                // }
            }

        }
  * */

/*
        
        if(currentEvent.isMouse && (currentEvent.type == EventType.MouseUp || currentEvent.type == EventType.mouseDown ))
        {
            GUI.Button(new Rect(256, 0, 64, 64), currentEvent.type.ToString());

            if (guiZone.Contains(currentEvent.mousePosition))
            {
                toggleState = !toggleState;
            }
        }
      
      */
    }


}
