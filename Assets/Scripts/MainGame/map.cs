using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

public class Map : MonoBehaviour
{
    public GameObject treesFolder;
    public GameObject villageFolder;
    public GameObject village;
    public int seed ;
    public List<GameObject> deadTrees;
    public List<GameObject> hugeTrees;
    public List<GameObject> longTrees;
    public List<GameObject> simpleTrees;

    // Start is called before the first frame update
    public void StartMap()
    {
        Random.seed = seed;
        AddVillage();
        AddTrees();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void AddVillage()
    {
        Vector2 v = Random.insideUnitCircle * 100;
        float x = v.x;
        float z = v.y;
        float y = 0;
        RaycastHit hit;
        int layerMask = 1 << 3; // define the only layer that the raycast can touch
        layerMask = ~layerMask; // inverse the bits to ignore specifically the 3; 
        if (Physics.Raycast(new Vector3(x, 100, z), Vector3.down, out hit, 500, layerMask))
        {
            if (hit.transform.tag == "mapFloor")
            {
                y = hit.point.y + 1;
                Vector3 direction = hit.normal;
                Instantiate(village, new Vector3(x, y, z), RandomRotation(), villageFolder.transform);
                
            }

        }
}

    public void AddTrees()
    {
        List<float[]> possibleTrees = RandomListXY(); //List of position of all trees (maybe not possible)
        foreach (float[] possibleTree in possibleTrees)
        {
            float y = 0f;
            if (PositionValid(possibleTree, ref y))
            { 
                GameObject tree = Instantiate(
                    RandomTree(), new Vector3(possibleTree[0], y,possibleTree[1]), 
                    RandomRotation(), treesFolder.transform);
                tree.tag = "tree";
            }
        }
    }

    public static List<float[]> RandomListXY()
    {
        List<float[]> list = new List<float[]>();
        for (int x = -490; x <= 490; x+=4)
        {
            for (int z = -490; z <= 490; z+=4)
            {
                float randomX = Random.Range(-1.5f, 1.5f);
                float randomZ = Random.Range(-1.5f, 1.5f);
                list.Add(new float[] {x+randomX, z+randomZ});
            }
        }
        return list;
    }
    
    public static bool PositionValid(float[] possibleTree,ref float y)
    {
        RaycastHit hit;
        float x = possibleTree[0];
        float z = possibleTree[1];
        if (Physics.Raycast(new Vector3(x, 100, z), Vector3.down, out hit))
        {
            if (hit.transform.tag == "mapFloor")
            {
                y = hit.point.y;
                Vector3 direction = hit.normal;
                if (direction.y > 0.95f)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static Quaternion RandomRotation()
    {
        return Quaternion.Euler(new Vector3(0,Random.Range(-360, 360), 0));
    }

    public GameObject RandomTree()
    {
        float x = Random.value;
        if (x < 0.5f)
        {
            return simpleTrees[Random.Range(0,simpleTrees.Count)];
        }
        if(x < 0.90)
        {
            return longTrees[Random.Range(0, longTrees.Count)];
        }

        if (x < 0.995)
        {
            return deadTrees[Random.Range(0, deadTrees.Count)];
        }
        else
        {
            return hugeTrees[Random.Range(0, hugeTrees.Count)];
        }
    }
}
