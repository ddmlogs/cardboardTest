using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickManager : MonoBehaviour
{

    public UnityEvent events;

    float time_for_double_click;

    public Cardboard cardb;


    bool one_click = false;
    bool timer_running;
    float timer_for_double_click;
    public float delay = 1.0f;

    // Use this for initialization
    void Start()
    {
        cardb = GameObject.FindObjectOfType<Cardboard>();
    }

    // Update is called once per frame
    void Update()
    {


        //this is how long in seconds to allow for a double click

        if (Input.GetMouseButtonDown(0))
        {
            if (!one_click) // first click no previous clicks
            {
                one_click = true;

                timer_for_double_click = Time.time; // save the current time
                // do one click things;
            }
            else
            {
                one_click = false; // found a double click, now reset
               // events.Invoke();

                cardb.VRModeEnabled = !cardb.VRModeEnabled;
            }
        }
        if (one_click)
        {
            // if the time now is delay seconds more than when the first click started. 
            if ((Time.time - time_for_double_click) > delay)
            {

                //basically if thats true its been too long and we want to reset so the next click is simply a single click and not a double click.

                one_click = false;

            }
        }
    }
}
