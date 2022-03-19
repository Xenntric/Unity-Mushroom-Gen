using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;

[RequireComponent(typeof(MeshFilter))]
public class BezierCurveGen : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] Transform[] controlPoints;
    
    [Range(0, 1)] public float tPoint = 0;
    public float stackLength = 0.05F;
    public float sliceAngle = 45;
    public float cylinderWidth = .2F;
    
    public int stackCount = 0;
    public bool proceed;
    public List<Transform> bezierPointsTransforms;
    Vector3 Getpos(int i) => controlPoints[i].position;
    private PointOrientation _pointOrientation;
    //private GameObject _stem;
    private float _resolutionCalculation;

   public MeshFilter mf;
   public Mesh mesh;

   public List<Vector3> vertexList;
   public List<int> triList;

    
    private void OnEnable()
    {
        bezierPointsTransforms = new List<Transform>();
        _resolutionCalculation = stackLength;
        var stem = new GameObject
        {
            name = "Stem",
            transform =
            {
                parent = transform
            }
        };
        mesh = new Mesh
        {
            name = "Stem"
        };

        mf = new MeshFilter();
        mf = GetComponent<MeshFilter>();
        mf.sharedMesh = mesh;
    }

    public void OnDrawGizmos()
    {
        for (int i = 0; i < controlPoints.Length; i++)
        {
            Handles.color = Color.magenta;
            var handleSize = HandleUtility.GetHandleSize(Getpos(i));
            Handles.DrawSolidDisc(Getpos(i),cam.transform.position,handleSize * .1F);
            Gizmos.DrawWireSphere(Getpos(i),handleSize * .1F);
            //i.GetComponent<SphereCollider>().radius = handleSize * .0075F;
        }
        Handles.DrawBezier(
            Getpos(0), 
            Getpos(2),
            Getpos(0),
            Getpos(1),
            Color.red, EditorGUIUtility.whiteTexture,2f);

        PointOrientation testpoint = GetBezierPoint(tPoint);
        Handles.color = Color.white;
        var testSize = HandleUtility.GetHandleSize(testpoint.pos);
        //Handles.DrawSolidDisc(testpoint.pos,cam.transform.position,testSize * .1F);
        Handles.PositionHandle(testpoint.pos, testpoint.rot);
        float radius = .03F;
        Gizmos.DrawSphere(testpoint.LocaltoWorld(Vector3.right * .2F), radius);
        Gizmos.DrawSphere(testpoint.LocaltoWorld(Vector3.left  * .2F), radius);
        Gizmos.DrawSphere(testpoint.LocaltoWorld(Vector3.down  * .2F), radius);
        Gizmos.DrawSphere(testpoint.LocaltoWorld(Vector3.up    * .2F), radius);

        Gizmos.color = Color.green;
        foreach (var bpt in bezierPointsTransforms)
        {
            Gizmos.DrawSphere(bpt.position, radius/2);
        }
    }

    PointOrientation GetBezierPoint(float t)
    {
        Vector3 p0 = Getpos(0);
        Vector3 p1 = Getpos(1);
        Vector3 p2 = Getpos(2);

        Vector3 a = Vector3.Lerp(p0,p1,t);
        Vector3 b = Vector3.Lerp(p1,p2,t);
        
        Vector3 pos =  Vector3.Lerp(a, b, t);
        Vector3 tangent = (b - a).normalized;

        if (t >= stackLength)
        {
            
            stackLength += _resolutionCalculation;
            stackCount++;
            
            StemPointGen(GetBezierPoint(tPoint));
            //TriangleGen();
        }
        
        return new PointOrientation(pos, tangent);
    }

   

    // Update is called once per frame
    void Update()
    {
        if (proceed && tPoint < 1) 
        {
            //growth speed
            tPoint += .2F * Time.deltaTime;
        }
    }

    void StemPointGen(PointOrientation point)
    {
        float pointsInCircumference = 360 / sliceAngle;

        //Debug.Log("number of points in ring " + pointsInCircumference);
        for (int i = 0; i < pointsInCircumference; i++)
        {
            var tempSliceAngle = sliceAngle * i;
            Vector2 v = new Vector2(cylinderWidth, cylinderWidth);

            Vector3 resultV = Quaternion.Euler(0, 0, tempSliceAngle) * v;
            
            var stemPoint = new GameObject
            {
                transform =
                {
                    position = point.LocaltoWorld(resultV),
                    rotation = point.rot,
                    parent = transform.GetChild(3)
                },
                name = "Point " + bezierPointsTransforms.Count
            };
            bezierPointsTransforms.Add(stemPoint.transform);
        }

        for (int i = 0; i < stackCount; i++)
        {
            TriangleGen(pointsInCircumference);
        }
        
    } 
    private void TriangleGen(float pointsInCircumference)
    {
        if (stackCount <= 1) return;
        var parentOffset = transform.parent.position;
        var pIc = (int) pointsInCircumference;
        var bPt = bezierPointsTransforms;

        for (int i = pIc; i < bPt.Count - 1; i++)
        {
            /*vertexList.Clear();
                triList.Clear();*/
                
            int offset = vertexList.Count;
            Debug.Log("Count: " + i);
                
            /*Vector3 v0 = bPt[j + bPt.Count - (pIc)].transform.position;
                Vector3 v1 = bPt[j + bPt.Count - (pIc * 2)].transform.position;
                Vector3 v2 = bPt[j + bPt.Count - (pIc * 2) + 1].transform.position;
                Vector3 v4 = bPt[j + bPt.Count - (pIc) + 1 ].transform.position;
                
                v0 -= parentOffset;
                v1 -= parentOffset;
                v2 -= parentOffset;

                vertexList.Add(v0);
                vertexList.Add(v1);
                vertexList.Add(v2);

                /*if (!afterFirstTri)
                {
                    triList.Add(0);
                    triList.Add(1);
                    triList.Add(3);
                }
                else
                {
                    triList.Add(j+1);
                    triList.Add(j);
                    triList.Add(j+2);

                }#1#

                triList.Add(j);
                triList.Add(j + 1);
                triList.Add(j + 2);

                /*if (!afterFirstTri)
                {
                    Debug.Log("Ding");
                    triList.Add(0);
                    triList.Add(1);
                    triList.Add(2);
                }
                else
                {
                    Debug.Log("Dong");
                    triList.Add(3);
                    triList.Add(2);
                    triList.Add(4);
                    
                }#1#

                afterFirstTri = true;

                /**
                 * The Triangle Pattern is;
                 * (0,1,3)
                 * (3,2,5)
                 * (5,4,7)
                 * (7,6,9)
                 * (9,8,11)
                 *
                 * so the formula is?????
                 * 
                 *#1#

                //mesh.RecalculateNormals();
                //mf.sharedMesh = mesh;*/
                
            vertexList.AddRange(new []
            {
                bPt[i].transform.position - parentOffset,
                bPt[i+1].transform.position - parentOffset,
                bPt[i-pIc].transform.position - parentOffset,
                bPt[i-pIc + 1].transform.position - parentOffset
            });
            Debug.Log("Vert Pass");
                
            triList.AddRange(new []
            {
                offset+2,
                offset+1,
                offset+3,
                    
                offset,
                offset+1,
                offset+2
            });
            Debug.Log("Tri Pass");

            //Debug.Log(mesh.vertexCount);
                
        }
        /*mesh.vertices = vertexList.ToArray();
        mesh.triangles = triList.ToArray();*/
        //mesh.RecalculateNormals();
        mesh.SetVertices(vertexList);
        mesh.SetTriangles(triList, 0);
        Debug.Log("Set Pass");
    }
}
