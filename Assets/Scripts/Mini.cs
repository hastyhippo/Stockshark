using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mini : MonoBehaviour
{
    // access modifier, modifiers, type of thing, name = value
    public GameObject node;
    public GameObject nodeIndicator;
    private GameObject[] nodes = new GameObject[64];
    public List<int> bitboardVisualized = new List<int>();

    public ulong num;
    
    public Vector2 offset;

    private void Awake()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Instantiate(nodeIndicator, new Vector2(2 * (x + offset.x), 2 * (y + offset.y)), Quaternion.identity);
                nodes[y * 8 + x] = Instantiate(node, new Vector2(2*(x + offset.x), 2*(y + offset.y)), Quaternion.identity);
            }
        }
    }

    public void Update()
    {
        UpdateMini();
    }
    public void UpdateMini()
    {
        bitboardVisualized.Clear();
        if(num > long.MaxValue)
        {
            num -= long.MaxValue;
            nodes[63].SetActive(true);
        }
        else
        {
            nodes[63].SetActive(false);
        }

        foreach (char character in System.Convert.ToString((long)num, 2))
        {
            bitboardVisualized.Add(character - 48);
        }
        bitboardVisualized.Reverse();
        for (int i = 0; i < 64; i++)
        {
            if (i >= bitboardVisualized.Count)
            {
                nodes[i].SetActive(false);
            }
            else
            {
                if (bitboardVisualized[i] == 1)
                {
                    nodes[i].SetActive(true);
                }
                else
                {
                    nodes[i].SetActive(false);
                }
            }
        }
    }
}
