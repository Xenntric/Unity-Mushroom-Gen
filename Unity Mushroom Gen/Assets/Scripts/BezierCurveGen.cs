using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using UnityEngine.XR;

[RequireComponent(typeof(MeshFilter))]
public class BezierCurveGen : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] Transform[] controlPoints;
    /*[Space (5)]*/
    [Header("======Mushroom Caps======")] 
    [SerializeField] public List<GameObject> stages;
    private GameObject _capParent;
    private List<GameObject> _loadedCaps;
    
    [SerializeField] public List<float> capStagePercentage;

    [Header ("======Debug Bools======")]
    public bool generate;
    public bool showPoints;
    
    [Header ("======Stem Options======")]
    [Range(0, 1)] public float growthSpeed = .2F;
    public float stackLength = 0.05F;
    public float sliceAngle = 45;
    public float stemWidth = .2F;


    [Header ("======Stem Values======")]
    [Range(0, 1)] public float tPoint = 0;
    public int stackCount = 0;
    
    [Header ("======Stem Gen Lists======")]
    public List<Transform> bezierPointsTransforms;
    public List<Vector3> vertexList;
    public List<int> triList;

    Vector3 Getpos(int i) => controlPoints[i].position;
    private PointOrientation _pointOrientation;
    private float _stemLengthCalculation = 1;
    private float _resolutionCalculation;
    private MeshFilter _mf;
    private Mesh _mesh;

    public PointOrientation testpoint;

    private Vector3 parentOffset;
    private void OnEnable()
    {
        parentOffset = transform.parent.position;
        bezierPointsTransforms = new List<Transform>();
        _loadedCaps = new List<GameObject>();
        _resolutionCalculation = stackLength;
        _stemLengthCalculation = stackLength;
        
        var stem = new GameObject
        {
            name = "Stem",
            transform =
            {
                parent = transform
            }
        };
        var caps = new GameObject
        {
            name = "Caps",
            transform =
            {
                parent = transform
            }
        };
        _mesh = new Mesh
        {
            name = "Stem"
        };

        _mf = new MeshFilter();
        _mf = GetComponent<MeshFilter>();
        _mf.sharedMesh = _mesh;
        _mesh.indexFormat = IndexFormat.UInt32;

        if (stages.Capacity > 0)
        {
            foreach (var t in stages)
            {
                Instantiate(t, caps.transform).SetActive(false);
            }

            _capParent = caps;
            //currentCap = Instantiate(currentCap, caps.transform);
        }

        for (int i = 0; i < stages.Count; i++)
        { 
            _loadedCaps.Add(caps.transform.GetChild(i).gameObject);
        }
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
       
        if (!showPoints) return;
        Handles.DrawBezier(
            Getpos(0), 
            Getpos(2),
            Getpos(0),
            Getpos(1),
            Color.red, EditorGUIUtility.whiteTexture,2f);
        
        Handles.color = Color.white;
        var testSize = HandleUtility.GetHandleSize(testpoint.pos);
        //Handles.DrawSolidDisc(testpoint.pos,cam.transform.position,testSize * .1F);
        Handles.PositionHandle(testpoint.pos, testpoint.rot);
        float radius = .03F;
        
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(testpoint.LocaltoWorld(Vector3.zero), radius);
        
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

        if (t >= _stemLengthCalculation)
        {
            _stemLengthCalculation += _resolutionCalculation;
            stackCount++;
            
            StemPointGen(GetBezierPoint(tPoint));
        }
        
        return new PointOrientation(pos, tangent);
    }
    
    // Update is called once per frame
    void Update()
    {
        testpoint = GetBezierPoint(tPoint);

        
        /*if (stages.Count > 0)
        {
            testRot += rotspeed * Time.deltaTime;
            currentCap.transform.eulerAngles = new Vector3(testRot,0,0);
            Debug.Log("rot " + testRot);
        }*/

        if (generate && tPoint < 1) 
        {
            tPoint += growthSpeed * Time.deltaTime;
        }
    }

    void StemPointGen(PointOrientation point)
    {
        float pointsInCircumference = 360 / sliceAngle;
        
        for (int i = 0; i < pointsInCircumference; i++)
        {
            var tempSliceAngle = sliceAngle * i;
            Vector2 v = new Vector2(stemWidth, stemWidth);

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
        TriangleGen(pointsInCircumference);
        CapUpdate();
    }
    private void TriangleGen(float pointsInCircumference)
    {
        if (stackCount <= 1) return;
        
        var pIc = (int) pointsInCircumference;
        var bPt = bezierPointsTransforms;

        for (int i = pIc; i < bPt.Count - 1; i++)
        {
            int offset = vertexList.Count;

            vertexList.AddRange(new []
            {
                bPt[i].transform.position - parentOffset,
                bPt[i+1].transform.position - parentOffset,
                bPt[i-pIc].transform.position - parentOffset,
                bPt[i-pIc + 1].transform.position - parentOffset
            });

            triList.AddRange(new []
            {
                offset+2,
                offset+1,
                offset,

                offset+3,
                offset+1,
                offset+2,
            });
        }
        _mesh.SetVertices(vertexList);
        _mesh.SetTriangles(triList, 0);
    }

    private void CapUpdate()
    {
        _capParent.transform.position = testpoint.pos;
        _capParent.transform.rotation = testpoint.rot;

        if (capStagePercentage.Count > 0)
        {
            if (tPoint > 0)
            {
                _loadedCaps[0].SetActive(true);
            }
            if (tPoint > capStagePercentage[0])
            {
                _loadedCaps[0].SetActive(false);
                _loadedCaps[1].SetActive(true);
            }
        }
        
    }
}
