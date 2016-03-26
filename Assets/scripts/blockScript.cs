using UnityEngine;
using System.Collections;

public class blockScript : MonoBehaviour
{

    private GameObject gameController;

    private int colourID;
    private Color colour;
    private bool drop, toBeDestroyed, toBeDropped;
    private float dropDistance, travelledDistance, speed;
    private Vector3 startPosition;
    public int x, y;

    // Use this for initialization
    void Start()
    {
        speed = 7f;
    }

    // Update is called once per frame
    void Update()
    {
        if (drop)
        {
            travelledDistance += Time.deltaTime * speed;
            float fractionOfTravel = travelledDistance / dropDistance;
            transform.position = Vector3.Lerp(startPosition, startPosition - new Vector3(0, 0, dropDistance), fractionOfTravel);
            Debug.Log("whee");
            if (fractionOfTravel > 1)
            {
                transform.position = startPosition - new Vector3(0, 0, dropDistance);
                drop = false;
            }
        }
    }

    public void dropMe(float f)
    {
            travelledDistance = 0;
            startPosition = transform.position;
            dropDistance = f;
        drop = true;
    }

    public void dropMeMore(float f)
    {
        dropDistance += f;
    }
   
    public void destroyMe()
    {
        Destroy(gameObject);
    }

    public void setCoords(int a, int b)
    {
        x = a;
        y = b;
    }

    public void changeCoords(int a, int b)
    {
        x += a;
        y += b;
    }

    public int getCoord(int a)
    {
        switch (a)
        {
            case 0:
                return x;
            default:
                return y;
        }
    }
    
    public bool amIFalling()
    {
        return drop;
    }

    public void setGC(GameObject go)
    {
        gameController = go;
    }

    public int ColourID
    {
        get
        {
            return colourID;
        }
        set
        {
            colourID = value;
        }
    }

    public Color Colour
    {
        get
        {
            return colour;
        }
        set
        {
            colour = value;
            GetComponent<Renderer>().material.color = colour;
        }
    }
}
