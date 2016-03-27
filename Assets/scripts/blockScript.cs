using UnityEngine;
using System.Collections;

public class blockScript : MonoBehaviour
{

    private GameObject gameController;

    private int colourID;
    private Color colour;
    private bool drop, toBeDestroyed, toBeDropped, falling;
    private float dropDistance, travelledDistance, speed, defaultSpeed;
    private Vector3 startPosition;
    public int x, y;

    // Use this for initialization
    void Start()
    {
        defaultSpeed = 5f;
        speed = defaultSpeed;
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
                dropDistance = 0;
                drop = false;
                falling = false;
            }
        }

        if (toBeDestroyed)
        {
            transform.localScale *= 0.8f;
        }
    }

    private IEnumerator Destroyed()
    {
        falling = true;
        yield return new WaitForSeconds(0.3f);
        transform.position = new Vector3(100, 100, 100);
        toBeDestroyed = false;
        transform.localScale = new Vector3(1, 1, 1);

    }

    private IEnumerator Regenerate(Vector3 v, float f)
    {
        yield return new WaitForSeconds(0.5f);
        transform.position = v;
        GetComponent<Renderer>().material.color = colour;
        dropMe(f);

    }

    public void dropMe(float f)
    {
        startPosition = transform.position;
        travelledDistance = 0;
        dropDistance += f;
        drop = true;
    }

    public void dropMeMore(float f)
    {
        dropDistance += f;
    }

    public void setSpeed(int a)
    {
        speed = defaultSpeed + (5 + a) / 5;
    }

    public void destroyMe()
    {
        toBeDestroyed = true;
        StartCoroutine(Destroyed());
    }

    public void regenerateMe(Vector3 v, float f)
    {
        StartCoroutine(Regenerate(v, f));
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
        return (drop || falling);
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
        }
    }
}
