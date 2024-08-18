using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{

    private GridManager gridManager;
    public bool isSelected = false;

    public bool isMatched = false;

    private Color currentColor;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        SetRandomColor();
    }

    void SetRandomColor()
    {
        int randomColor = Random.Range(0, 5);
        switch (randomColor)
        {
            case 0:
                GetComponent<Renderer>().material.color = Color.red;
                currentColor = Color.red;
                break;
            case 1:
                GetComponent<Renderer>().material.color = Color.blue;
                currentColor = Color.blue;
                break;
            case 2:
                GetComponent<Renderer>().material.color = Color.green;
                currentColor = Color.green;
                break;
            case 3:
                GetComponent<Renderer>().material.color = Color.yellow;
                currentColor = Color.yellow;
                break;
            case 4:
                GetComponent<Renderer>().material.color = Color.magenta;
                currentColor = Color.magenta;
                break;
        }
    }

    void OnMouseDown()
    {
        if (!isSelected)
        {
            isSelected = true;
            Renderer renderer = GetComponent<Renderer>();
            renderer.material.color = Color.white;
            StartCoroutine(WaitAndSelectBlock());
        }
        else
        {
            isSelected = false;
            GetComponent<Renderer>().material.color = currentColor;
            Debug.Log("Block color:" + GetComponent<Renderer>().material.color);
            StartCoroutine(WaitAndSelectBlock());
        }
    }

    IEnumerator WaitAndSelectBlock()
    {
        yield return new WaitForSeconds(0.1f); // Wait for 0.1 seconds
        gridManager.BlockSelected(this);
    }

    public Material GetBlockMaterial()
    {
        return GetComponent<Renderer>().material;
    }
    public void Deselect()
    {
        isSelected = false;
        GetComponent<Renderer>().material.color = currentColor;
        Debug.Log("Block color:" + GetComponent<Renderer>().material.color);
    }


}
