using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;
using static System.Runtime.Serialization.ExportOptions;
public class GridGen : MonoBehaviour
{
    [Header ("======Mushroom to Grow======")]
    public GameObject Mushroom;
    
    [Header ("======Debug Options======")]
    public bool grow;
    public bool showDebug;

    [Header("======Mushrooms Options======")]
    public bool cluster;
    [Range(1,5)]public float mushroomGoalHeight = 3;
    [Range(5,30)] public float mushroomWonkiness;
    [Range(0, 1)] public float mushroomGrowthSpeed = .2F;
    
    public float mushroomStackLength = 0.05F;
    public float mushroomSliceAngle = 45;
    public float mushroomStemWidth = .2F;
    
    [Header("======Mushrooms Randomiser======")]
    public bool mushroomWidthRandomiser;
    private List<float> randomWidth;
    
    public bool mushroomHeightRandomiser;
    private List<float> randomHeight;
    
    public List<Vector2> points;
    public List<GameObject> colony;
    public bool showLines;
    public GameObject[,] Grid;
    public Vector2Int gridXY = new Vector2Int(10,10);
    public float radius = 10;
    public int attemptsToSpawn = 30;
    private bool _gridGenerated;

    

    // Start is called before the first frame update
    void OnEnable()
    {
        
        GameObject[,] Grid = new GameObject[gridXY.x, gridXY.y];
        randomWidth = new List<float>();
        randomHeight = new List<float>();
        colony = new List<GameObject>();
        GameObject mushroomContainer = new GameObject
        {
            name = "Mushroom Container",
            transform =
            {
                parent = transform
            }
        };
        GameObject gridContainer = new GameObject
        {
            name = "Grid Container",
            transform =
            {
                parent = transform
            }
        };

        
        
        //CreateGrid();

        /*foreach (var point in Grid)
        {
            Instantiate(Mushroom);
            Mushroom.transform.position = point.transform.position;
        }*/
        int count = 0;
        
        /*for (int i = 0; i < gridXY.x; i++)
        {
            for (int j = 0; j < gridXY.y; j++)
            {
                if (i % 4 == 0 && j % 2 == 0)
                {
                    colony.Add(Mushroom);
                    count++;

                    colony[count - 1].gameObject.transform.position = Grid[i,j].gameObject.transform.position;

                    print("Grid point " + Grid[i,j].gameObject.transform.position);
                }
            }
        }*/
        
        /*colony.Add(Mushroom);
        colony[0].transform.position = Grid[3, 2].transform.position;
        foreach (var mushroom in colony)
        {
            Instantiate(mushroom);
        }*/
        points = GeneratePoints(radius, gridXY, attemptsToSpawn);
        /*foreach (Vector2 validPoint in points)
        {
            count++;
            colony.Add(Mushroom);
            colony[colony.Count - 1].gameObject.transform.position = new Vector3(validPoint.x, 0, validPoint.y);
            
        }*/

        Vector3 clusterStartPoint = new Vector3();


        for (int i = 0; i < points.Count; i++)
        {
            
            randomWidth.Add( Random.Range(.1F, .275F));
            /*
            Mushroom.GetComponentInChildren<BezierCurveGen>().stemWidth = Random.Range(.15F, .3F);
            */
            GameObject mushroom = Instantiate(Mushroom, new Vector3(points[i].x, 0, points[i].y), Mushroom.transform.rotation);
            Transform mushroomStartPoint = mushroom.transform.GetChild(0).transform.GetChild(0).gameObject.transform;
            Transform mushroomMidPoint = mushroom.transform.GetChild(0).transform.GetChild(1).gameObject.transform;
            GameObject mushroomEndPoint = mushroom.transform.GetChild(0).transform.GetChild(2).gameObject;
            if (i == 0)
            {
                clusterStartPoint = mushroomStartPoint.position;
            }
            if (cluster)
            {
                mushroomStartPoint.position = clusterStartPoint;
            }
            
            
            if (mushroomHeightRandomiser)
            {
                randomHeight.Add( Random.Range(mushroomMidPoint.transform.position.y, 5));
                mushroomGoalHeight = randomHeight[i];
                mushroomMidPoint.position = new Vector3(mushroomMidPoint.position.x, mushroomGoalHeight / 2, mushroomMidPoint.position.z);
            }

            if (mushroomHeightRandomiser && mushroomWidthRandomiser)
            {
                var randomisedScale = 1 + (randomHeight[i] * randomWidth[i])/2.5F;
                mushroom.GetComponentInChildren<BezierCurveGen>().capParent.transform.localScale =
                    new Vector3(randomisedScale, randomisedScale, randomisedScale);

            }
            
            
            mushroom.transform.parent = mushroomContainer.transform;
            float angle = Random.value * Mathf.PI * 2;
            //direction of angle to try 
            Vector3 dir = new Vector3(Mathf.Sin(angle), mushroomGoalHeight , Mathf.Cos(angle));
            //point in space to try
            Vector3 candidate = new Vector3(mushroom.transform.position.x + dir.x * Random.Range(radius, 2*radius)/mushroomWonkiness, mushroomGoalHeight, mushroom.transform.position.z + dir.z * Random.Range(radius, 2*radius)/mushroomWonkiness);
            mushroomEndPoint.transform.position = candidate;

            colony.Add(mushroom);
            
        }
        float cellSize = radius/Mathf.Sqrt(2);


        
        
        for (int i = 0; i < gridXY.x; i++)
        {
            for (int j = 0; j < gridXY.y; j++)
            {
                //grid lines may be slightly off because of the way the searching works, starting at 1, instead of 0
                DrawLine("Grid Point " + i.ToString() + "," + j.ToString(), gridContainer.transform, new Vector3(i, 0, j),
                    new Vector3(i + 1,0,j));
                
                if (j < gridXY.y - 1)
                {
                    //var nextPointJ = Grid[i, j + 1].transform.position;
                    DrawLine("Grid Point " + i.ToString() + "," + j.ToString(), gridContainer.transform.GetChild(i), new Vector3(i, 0, j),
                        new Vector3(i, 0, j + 1));
                }
                
            }
        }
        
        /*foreach (var mushroom in colony)
        {
            Instantiate(mushroom);
        }*/
    }

    /*Vector2 Point(float radius, GameObject[,] Grid, Vector2 cellwidth)
    {
        var cellSize = radius / Mathf.Sqrt(2);
        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPointInCell = new List<Vector2>();
        
        spawnPointInCell.Add(cellwidth/2);
    }*/
    
    

    /*
    private void CreateGrid()
    {
        
        for (int i = 0; i < gridXY.x; i++)
        {
            for (int j = 0; j < gridXY.y; j++)
            {
                //print("Grid Pos: " + i.ToString() + "," + j.ToString());
                Grid[i, j] = new GameObject()
                {
                    name = ("Grid Point " + i.ToString() + "," + j.ToString()),
                    transform =
                    {
                        parent = transform,
                        position = new Vector3(transform.position.x + i , transform.position.y,
                            transform.position.z + j),
                    }
                };
                
                if (j > 0)
                {
                    Grid[i, j].transform.parent = Grid[i, 0].transform;
                }
            }
        }

        if (showLines)
        {
            GenerateLines(Grid);
        }
    }

    private void GenerateLines(GameObject[,] Grid)
    {
        for (int i = 0; i < gridXY.x; i++)
        {
            for (int j = 0; j < gridXY.y; j++)
            {
                //print(Grid[i, j].name);
                var gridPoint = Grid[i, j].transform.position;

                if (i < gridXY.x - 1)
                {
                    var nextPointI = Grid[i + 1, j].transform.position;
                    DrawLine("x", Grid[i, j].transform, new Vector3(gridPoint.x, gridPoint.y, gridPoint.z),
                        new Vector3(nextPointI.x, nextPointI.y, nextPointI.z));
                    if (j < gridXY.y - 1)
                    {
                        var nextPointJ = Grid[i, j + 1].transform.position;
                        DrawLine("y", Grid[i, j].transform, new Vector3(gridPoint.x, gridPoint.y, gridPoint.z),
                            new Vector3(nextPointJ.x, nextPointJ.y, nextPointJ.z));
                    }

                }

                // Final lines
                if (i == gridXY.x - 1 && j < gridXY.y - 1)
                {
                    //print("DING");
                    var nextPointJ = Grid[i, j + 1].transform.position;
                    DrawLine("y", Grid[j, j + 1].transform, new Vector3(gridPoint.x, gridPoint.y, gridPoint.z),
                        new Vector3(nextPointJ.x, nextPointJ.y, nextPointJ.z));
                }
            }
        }
    }
    */

    void DrawLine(string vector, Transform parent, Vector3 start, Vector3 end)
    {
        Color color = Color.red;
        GameObject myLine = new GameObject
        {
            transform =
            {
                parent = parent,
                position = start
            },
            name = vector
        };
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.SetColors(color, color);
        lr.SetWidth(0.1f, 0.1f);
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    // Update is called once per frame
    void Update()
    {
        
        foreach (var mushroom in colony)
        {
            var mushroomScript = mushroom.GetComponentInChildren<BezierCurveGen>();

            if (mushroomWidthRandomiser)
            {
                mushroomScript.stemWidth = randomWidth[colony.IndexOf(mushroom)];
            }
            else
            {
                mushroomScript.stemWidth = mushroomStemWidth;
            }

            mushroomScript.generate = grow;
            mushroomScript.showPoints = showDebug;
            mushroomScript.growthSpeed = mushroomGrowthSpeed;
            mushroomScript.stackLength = mushroomStackLength;
            mushroomScript.sliceAngle = mushroomSliceAngle;
        }
       
    }
    
    public static List<Vector2> GeneratePoints(float radius, Vector2 totalGridSize, int numSamplesBeforeRejection = 30) 
    {
        //define cell size so only one mushroom can occupy a cell
        float cellSize = radius/Mathf.Sqrt(2);
        
        //create grid with max number of cells, number of cells is determined by max number divided by cell widths
        int[,] grid = new int[Mathf.CeilToInt(totalGridSize.x/cellSize), Mathf.CeilToInt(totalGridSize.y/cellSize)];
        
        //points stores confirmed points the shrooms can spawn at
        List<Vector2> points = new List<Vector2>();
        //Stores current spawn points to check and branch off from., if no possible points left, bin point.
        List<Vector2> spawnPoints = new List<Vector2>();
        
        //begin from middle of grid
        spawnPoints.Add(totalGridSize/2);
        
        //loop will run until all spawn points have been tested
        while (spawnPoints.Count > 0) 
        {
            //pick random spawn point to branch from
            int spawnIndex = Random.Range(0,spawnPoints.Count);
            //get position of that point
            Vector2 spawnCentre = spawnPoints[spawnIndex];
            bool candidateAccepted = false;

            //pick random direction and length (within max cell width) and check if its within
            //max accepted radius, if so; bin candidate. Otherwise place and move on.
            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
                //random angle to try
                float angle = Random.value * Mathf.PI * 2;
                //direction of angle to try 
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                //point in space to try
                Vector2 candidate = spawnCentre + dir * Random.Range(radius, 2*radius);
                if (IsValid(candidate, totalGridSize, cellSize, radius, points, grid)) 
                {
                    //if there is no point there yet, add to valid points list, and spawnPoints,
                    //so it can branch off again
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    
                    //grid index is now occupied
                    grid[(int)(candidate.x/cellSize),(int)(candidate.y/cellSize)] = points.Count;
                    candidateAccepted = true;
                    break;
                }
            }
            
            if (!candidateAccepted) 
            {
                //point has no more possible points to branch to, remove spawn point
                spawnPoints.RemoveAt(spawnIndex);
            }
        }
        
        return points;
    }

    private void OnDrawGizmos()
    {
    }
    
    static bool IsValid(Vector2 candidate, Vector2 totalGridSize, float cellSize, float radius, List<Vector2> points, int[,] grid) {
        //if candidate point is within grid
        if (candidate.x >= 0 && candidate.x < totalGridSize.x && 
            candidate.y >= 0 && candidate.y < totalGridSize.y) 
        {
            
            int cellX = (int)(candidate.x/cellSize);
            int cellY = (int)(candidate.y/cellSize);
            
            //searches the 5x5 blocks around point, if its trying to access cell that doesnt exist, use last valid cell
            int searchStartX = Mathf.Max(0,cellX -2);
            int searchEndX = Mathf.Min(cellX+2,grid.GetLength(0)-1);
            int searchStartY = Mathf.Max(0,cellY -2);
            int searchEndY = Mathf.Min(cellY+2,grid.GetLength(1)-1);

            
            for (int x = searchStartX; x <= searchEndX; x++) 
            {
                for (int y = searchStartY; y <= searchEndY; y++) 
                {
                    int pointIndex = grid[x,y]-1;
                    if (pointIndex != -1) {
                        float sqrDst = (candidate - points[pointIndex]).sqrMagnitude;
                        if (sqrDst < radius*radius) 
                        {
                            return false;
                        }
                        
                    }
                }
            }
            return true;
        }
        return false;
    }
}
