using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BoardLocationUIControl : MonoBehaviour
{

    public UnityEngine.UI.Text stateLabel;
    public UnityEngine.UI.Text effectLabel;

    public Vector3 stateOffset = Vector3.zero;

    public Actor myActor = null;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        stateLabel.transform.position = Camera.main.WorldToScreenPoint(transform.position + stateOffset);
        if (myActor != null)
        {
            stateLabel.text = string.Format("{0}\n{1}/{2}", myActor.actorName, myActor.hitPoints, myActor.maxHitPoints);
        }
        else
        {
            stateLabel.text = string.Empty;
        }
    }
}
