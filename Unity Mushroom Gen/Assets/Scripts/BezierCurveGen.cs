using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(MeshFilter))]
public class BezierCurveGen : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] Transform[] controlPoints;
    [Range(0, 1)] public float tTest = 0;
    public float bendResolution = 0.05F;
    public float cylinderResolution = 0.45F;
    public float cylinderWidth = .2F;
    
    //[Range(2, 20)] public int resolution = 2;
    public bool proceed;
    public List<Transform> bezierPointsTransforms;
    Vector3 Getpos(int i) => controlPoints[i].position;

    private PointOrientation _pointOrientation;
    private int _count;
    private GameObject _stem;
    private float _resolutionCalculation;
    
    private void OnEnable()
    {
        bezierPointsTransforms = new List<Transform>();
        _resolutionCalculation = bendResolution;
        var _stem = new GameObject
        {
            name = "Stem"
        };
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

        PointOrientation testpoint = GetBezierPoint(tTest);
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
        foreach (var BPT in bezierPointsTransforms)
        {
            Gizmos.DrawSphere(BPT.position, radius/2);
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

        if (t >= bendResolution)
        {
            bendResolution += _resolutionCalculation;
            _count++;
            
            Debug.Log("current count: " + _count);
            
            /*GameObject tempGameObject = new GameObject
            {
                transform =
                {
                    position = GetBezierPoint(tTest).pos,
                    rotation = GetBezierPoint(tTest).rot,
                    parent = GameObject.Find("Stem").transform
                },
                name = "Stem Point " + (_count - 1)
            };
            
            bezierPointsTransforms.Add(tempGameObject.transform);*/
            StemPointGen(GetBezierPoint(tTest));
        }
        
        return new PointOrientation(pos, tangent);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (proceed && tTest < 1) 
        {
            tTest += .2F * Time.deltaTime;
        }
    }

    void StemPointGen(PointOrientation point)
    {
        /*var StemPointUp = new GameObject
        {
            transform =
            {
                position = point.LocaltoWorld(Vector3.up * .2F),
                rotation = point.rot,
                parent = GameObject.Find("Stem").transform
            }
        };
        bezierPointsTransforms.Add(StemPointUp.transform);
        var StemPointRight = new GameObject
        {
            transform =
            {
                position = point.LocaltoWorld(Vector3.right * .2F),
                rotation = point.rot,
                parent = GameObject.Find("Stem").transform
            }
        };
        bezierPointsTransforms.Add(StemPointRight.transform);
        var StemPointDown = new GameObject
        {
            transform =
            {
                position = point.LocaltoWorld(Vector3.down * .2F),
                rotation = point.rot,
                parent = GameObject.Find("Stem").transform
            }
        };
        bezierPointsTransforms.Add(StemPointDown.transform);
        var StemPointLeft = new GameObject
        {
            transform =
            {
                position = point.LocaltoWorld(Vector3.left * .2F),
                rotation = point.rot,
                parent = GameObject.Find("Stem").transform
            }
        };
        bezierPointsTransforms.Add(StemPointLeft.transform);*/

        float cylinderResolutionAngle;
        
        for (int i = 0; i < (360 / cylinderResolution); i++)
        {
            cylinderResolutionAngle = cylinderResolution * i;
            Vector2 v = new Vector2(cylinderWidth, cylinderWidth);

            Vector3 resultV = Quaternion.Euler(0, 0, cylinderResolutionAngle) * v;
            
            var StemPoint = new GameObject
            {
                transform =
                {
                    position = point.LocaltoWorld(resultV),
                    rotation = point.rot,
                    parent = GameObject.Find("Stem").transform
                }
            };
            bezierPointsTransforms.Add(StemPoint.transform);
        }
        
        
    }
}
