using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

public class map : MonoBehaviour
{
    public GameObject tree1;
    public GameObject tree2;
    public GameObject bush1;
    static List<Vector2> allTrees = new List<Vector2> { };
    static List<Vector2> allBushes = new List<Vector2> { };
    public int seed ;
    
    // Start is called before the first frame update
    void Start()
    {
        Random.seed = seed;
        float x;
        float y;
        for (int i = 0; i < 10; i++)
        {
            x = 0;
            y = 0;
            SeedTree(x,y);
            SeedBush(x+3, y);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void SeedTree(float x, float y)
    {
        if (PosTreeValid(x, y))
        {
            float addX = 0;
            float addY = 0;
            GameObject theTree = Instantiate(RandomTree(), new Vector3(x, 0, y), Quaternion.identity);
            theTree.transform.localScale = new Vector3(Random.Range(1f, 1.6f), Random.Range(1, 1.6f),
                Random.Range(1, 1.6f));
            theTree.transform.rotation =RandomRotation();
            allTrees.Add(new Vector2(x, y));
            for (int i = 0; i < 4; i++)
            {
                addX = x;
                addY = y;
                RandomXY(ref addX,ref addY, 5);
                SeedTree(x+addX, y+addY);
            }
        }
    }
    
    private void SeedBush(float x, float y)
    {
        float addX = 0;
        float addY = 0;
        GameObject theBush = Instantiate(bush1, new Vector3(x, 0, y), Quaternion.identity);
        theBush.transform.localScale = new Vector3(Random.Range(1f, 1.6f), Random.Range(1f, 1.6f),
            Random.Range(1f, 1.6f));
        theBush.transform.rotation =RandomRotation();
        allBushes.Add(new Vector2(x, y));
        for (int i = 0; i < 4; i++)
        {
            addX = x;
            addY = y;
            RandomXY(ref addX,ref addY, 10);
            if (PosBushValid(x+addX, y+addY))
            {
                SeedBush(x+addX, y+addY);
            }
        }
    }

    public static Quaternion RandomRotation()
    {
        return Quaternion.Euler(new Vector3(0,Random.Range(-360, 360), 0));
    }

    public static void RandomXY(ref float x, ref float y, float distance)
    {
        x = Random.value;
        y = (float) Math.Sqrt(1 - x * x);
        float coef = Random.Range(distance, distance*2f);
        x *= coef*(1 - 2 * Random.Range(0, 2));
        y *= coef*(1 - 2 * Random.Range(0, 2));
    }
    public static bool PosTreeValid(float x, float y)
    {
        foreach (Vector2 tree in allTrees)
        {
            float distance = (x - tree.x) * (x - tree.x) + (tree.y - y) * (tree.y - y);
            if (x*x+y*y>45000|| distance<30)
            {
                return false;
            }
        }

        return true;
    }
    
    public static bool PosBushValid(float x, float y)
    {
        foreach (Vector2 tree in allTrees)
        {
            float distance = (x - tree.x) * (x - tree.x) + (tree.y - y) * (tree.y - y);
            if (x*x+y*y>45000 || distance<5)
            {
                return false;
            }
        }
        foreach (Vector2 bush in allBushes)
        {
            float distance = (x - bush.x) * (x - bush.x) + (bush.y - y) * (bush.y - y);
            if (x*x+y*y>45000 || distance<100)
            {
                return false;
            }
        }

        return true;
    }

    public GameObject RandomTree()
    {
        if (Random.value < 0.7f)
        {
            return tree1;
        }
        else
        {
            return tree2;
        }
    }
}
