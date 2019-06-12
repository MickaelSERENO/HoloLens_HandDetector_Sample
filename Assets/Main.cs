using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

#if ENABLE_WINMD_SUPPORT
using Sereno.HandDetector;
using HandDetector_Native;
using Windows.Perception.Spatial;
#endif

public class Main : MonoBehaviour
#if ENABLE_WINMD_SUPPORT
    , IHDMediaSinkClbk
#endif
{

#if ENABLE_WINMD_SUPPORT
    private HandDetector m_handDetector = null;
    private SpatialCoordinateSystem m_spatialCoordinateSystem = null;
#endif

    private Vector3 m_handPos = new Vector3(0, 0, 0);

    // Start is called before the first frame update
    void Start()
    { 
        InitializeHandDetector();
    }

    // Update is called once per frame
    void Update()
    {
#if ENABLE_WINMD_SUPPORT
        if(m_spatialCoordinateSystem == null)
        {
            //Get the Spatial Coordinate System pointer
            IntPtr spatialCoordinateSystemPtr = UnityEngine.XR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr();
            m_spatialCoordinateSystem = Marshal.GetObjectForIUnknown(spatialCoordinateSystemPtr) as SpatialCoordinateSystem;
        }

#endif
        lock(this)
        { 
            transform.position = m_handPos;
        }
    }

#if ENABLE_WINMD_SUPPORT
    private async Task InitializeHandDetector()
#else
    private void InitializeHandDetector()
#endif
    {
#if ENABLE_WINMD_SUPPORT
        //Create the HandDetector object
        m_handDetector = await HandDetector.CreateAsync(null);
        await m_handDetector.InitializeAsync(this);
#endif
    }

#if ENABLE_WINMD_SUPPORT
    public void OnHandUpdate(CameraParameter cameraParam, SpatialCoordinateSystem CoordinateSystem, IList<Hand> hands)
    {
        //Get the origin spatial coordinate
        SpatialCoordinateSystem originSpatialCoordinate = null;
        lock(this)
            originSpatialCoordinate = m_spatialCoordinateSystem;

        if(hands.Count > 0 && originSpatialCoordinate != null)
        {     
            //Find the hands in the right-handed coordinate system
            System.Numerics.Matrix4x4? cameraToWorld = CoordinateSystem.TryGetTransformTo(originSpatialCoordinate).Value;
            System.Numerics.Matrix4x4  viewToCamera;
            System.Numerics.Matrix4x4.Invert(cameraParam.CameraViewTransform, out viewToCamera);
            if(cameraToWorld == null)
                cameraToWorld = System.Numerics.Matrix4x4.Identity;
            
            System.Numerics.Vector4 handVec = System.Numerics.Vector4.Transform(new System.Numerics.Vector4(hands[0].PalmX, hands[0].PalmY, hands[0].PalmZ, 1.0f), viewToCamera);
            handVec = System.Numerics.Vector4.Transform(handVec, cameraToWorld.Value);

            //Convert the hand position in the left-handed coordinate system
            lock(this)
            {
                m_handPos = new Vector3(handVec.X, handVec.Y, -handVec.Z) / (handVec.W);
            }
        }
    }
#endif
}
