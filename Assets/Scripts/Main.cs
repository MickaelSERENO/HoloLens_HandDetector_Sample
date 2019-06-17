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

namespace Sereno
{ 
    public class Main : MonoBehaviour
#if ENABLE_WINMD_SUPPORT
        , IHDMediaSinkClbk
#endif
    {

#if ENABLE_WINMD_SUPPORT
        /// <summary>
        /// The hand detector object
        /// </summary>
        private HandDetector.HandDetector m_handDetector = null;

        /// <summary>
        /// The root spatial coordinate system created by Unity
        /// </summary>
        private SpatialCoordinateSystem   m_spatialCoordinateSystem = null;

        /// <summary>
        /// List of hand detected
        /// </summary>
        private List<HandDetected>        m_handsDetected = new List<HandDetected>();

        /// <summary>
        /// The current finger's position
        /// </summary>
        private Vector3 m_fingerPosition = new Vector3(0, 0, 0);

        /// <summary>
        /// The current pointing direction
        /// </summary>
        private Vector3 m_pointingDir = new Vector3(0, 0, 0);
#endif
    
        /// <summary>
        /// The GameObject representing the ray
        /// </summary>
        public GameObject RayObject = null;

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

            lock(this)
            {
                //Update the gameobjects
                RayObject.SetActive(false);
                foreach (HandDetected hd in m_handsDetected)
                {
                    if(hd.IsValid)
                    {
                        Vector3 distBody   = (hd.Position - Camera.main.transform.position);
                        Vector2 distBody2D = new Vector2(distBody.x, distBody.z);
                        if (distBody2D.magnitude > 0.05) //Detection not "on the body"
                        {
                            RayObject.SetActive(true);

                            //Hand position
                            transform.position = hd.Position;

                            //Pointing
                            if(hd.UppestFinger != null)
                            {
                                if (m_pointingDir.x == 0 && m_pointingDir.y == 0 && m_pointingDir.z == 0)
                                {
                                    m_pointingDir = (hd.UppestFinger.Position - hd.Position).normalized;
                                    RayObject.transform.up = m_pointingDir;
                                }
                                else
                                {
                                    m_pointingDir = (1.0f - hd.Smoothness) * (hd.UppestFinger.Position - hd.Position).normalized + hd.Smoothness * m_pointingDir;
                                    RayObject.transform.up = m_pointingDir;
                                }
                                RayObject.transform.localPosition = hd.UppestFinger.Position + RayObject.transform.up * RayObject.transform.localScale.y;
                            }
                            else
                            {
                                if(m_pointingDir.x == 0 && m_pointingDir.y == 0 && m_pointingDir.z == 0)
                                {
                                    m_pointingDir = (hd.Position - hd.WristPosition).normalized;
                                    RayObject.transform.up = m_pointingDir;
                                }
                                else
                                {
                                    m_pointingDir = (1.0f - 0.8f) * (hd.Position - hd.WristPosition).normalized + 0.8f * m_pointingDir;
                                    RayObject.transform.up = m_pointingDir;
                                }
                                RayObject.transform.localPosition = hd.Position + RayObject.transform.up * RayObject.transform.localScale.y;
                            }
                            break;
                        }
                    }
                }

                //Reset the pointing
                if(RayObject.activeSelf == false)
                {
                    m_pointingDir = new Vector3(0, 0, 0);
                }
            }
#endif
        }

        /// <summary>
        /// Initialize the hand detector algorithm and launch the hand tracking
        /// </summary>
        /// <returns>The asynchronous task created</returns>
#if ENABLE_WINMD_SUPPORT
        private async Task InitializeHandDetector()
#else
        private void InitializeHandDetector()
    #endif
        {
#if ENABLE_WINMD_SUPPORT
            //Create the HandDetector object
            m_handDetector = await HandDetector.HandDetector.CreateAsync(null);
            await m_handDetector.InitializeAsync(this);
#endif
        }

#if ENABLE_WINMD_SUPPORT
        public void OnHandUpdate(CameraParameter cameraParam, SpatialCoordinateSystem CoordinateSystem, IList<Hand> hands)
        {
            lock(this)
            { 

                if(m_spatialCoordinateSystem != null)
                {
                    //Start a new frame
                    foreach(HandDetected hand in m_handsDetected)
                        hand.NewDetection = true;

                    //For each detected hand
                    foreach(Hand hand in hands)
                    {
                        //Add offsets in the ROI
                        float[] roi = new float[4];
                        roi[0] = hand.WristROIMinX-10;
                        roi[1] = hand.WristROIMinY-10;
                        roi[2] = hand.WristROIMaxX+10;
                        roi[3] = hand.WristROIMaxY+10;

                        //check if we already know it
                        HandDetected handDetected = null;
                        foreach(HandDetected hd in m_handsDetected)
                        {
                            if(!hd.IsDetected && hd.HandCollision(roi))
                            {
                                handDetected = hd;
                                break;
                            }
                        }

                        //If not, this is a new hand!
                        if(handDetected == null)
                        {
                            handDetected = new HandDetected(0.70f);
                            handDetected.NewDetection = true;
                            m_handsDetected.Add(handDetected);
                        }

                        //Compute the hand 3D position in the left-handed coordinate system
                        System.Numerics.Matrix4x4? cameraToWorld = CoordinateSystem.TryGetTransformTo(m_spatialCoordinateSystem).Value;
                        System.Numerics.Matrix4x4 viewToCamera;
                        System.Numerics.Matrix4x4.Invert(cameraParam.CameraViewTransform, out viewToCamera);
                        if (cameraToWorld == null)
                            cameraToWorld = System.Numerics.Matrix4x4.Identity;

                        System.Numerics.Vector4 handVec = System.Numerics.Vector4.Transform(new System.Numerics.Vector4(hand.PalmX, hand.PalmY, hand.PalmZ, 1.0f), viewToCamera);
                        handVec = System.Numerics.Vector4.Transform(handVec, cameraToWorld.Value);
                        Vector3 unityHandVec = new Vector3(handVec.X, handVec.Y, -handVec.Z) / handVec.W;

                        System.Numerics.Vector4 wristVec = System.Numerics.Vector4.Transform(new System.Numerics.Vector4(hand.WristX, hand.WristY, hand.WristZ, 1.0f), viewToCamera);
                        wristVec = System.Numerics.Vector4.Transform(wristVec, cameraToWorld.Value);
                        Vector3 unityWristVec = new Vector3(wristVec.X, wristVec.Y, -wristVec.Z) / wristVec.W;

                        handDetected.PushPosition(unityHandVec, unityWristVec, roi);
                        
                        //Clear fingers information
                        handDetected.Fingers.Clear();
                        handDetected.UppestFinger = null;

                        FingerDetected formerFinger = handDetected.UppestFinger;

                        if (hand.Fingers.Count > 0)
                        {
                            //Conver each fingers detected
                            foreach (Finger f in hand.Fingers)
                            {
                                //Register the finger position
                                System.Numerics.Vector4 fingerVec = System.Numerics.Vector4.Transform(new System.Numerics.Vector4(f.TipX, f.TipY, f.TipZ, 1.0f), viewToCamera);
                                fingerVec = System.Numerics.Vector4.Transform(fingerVec, cameraToWorld.Value);
                                Vector3 unityFingerVec = new Vector3(fingerVec.X, fingerVec.Y, -fingerVec.Z) / fingerVec.W;
                                handDetected.Fingers.Add(new FingerDetected(unityFingerVec));
                            }

                            //Detect the "farthest" finger
                            //float maxSqDist = (new Vector2(hand.Fingers[0].TipX, hand.Fingers[0].TipY) - 
                            //                   new Vector2(hand.PalmX, hand.PalmY)).sqrMagnitude;
                            //    
                            //handDetected.UppestFinger = handDetected.Fingers[0];
                            //
                            //for(int i = 1; i < handDetected.Fingers.Count; i++)
                            //{
                            //    float dist = (new Vector2(hand.Fingers[i].TipX, hand.Fingers[i].TipY) -
                            //                  new Vector2(hand.PalmX, hand.PalmY)).sqrMagnitude;
                            //    if (maxSqDist < dist)
                            //    {
                            //        maxSqDist = dist;
                            //        handDetected.UppestFinger = handDetected.Fingers[i];
                            //    }
                            //}

                            //Detect the uppest finger
                            float minFY = hand.Fingers[0].TipY;
                            handDetected.UppestFinger = handDetected.Fingers[0];
                            
                            for(int i = 1; i < handDetected.Fingers.Count; i++)
                            {
                                if (minFY > hand.Fingers[0].TipY)
                                {
                                    minFY = hand.Fingers[0].TipY;
                                    handDetected.UppestFinger = handDetected.Fingers[i];
                                }
                            }

                            //Apply smoothing
                            if(formerFinger != null)
                                handDetected.UppestFinger.Position = (1.0f - handDetected.Smoothness) * handDetected.UppestFinger.Position + handDetected.Smoothness * formerFinger.Position;

                        }
                    }
                }

                for(int i = 0; i < m_handsDetected.Count; i++)
                {
                    HandDetected hd = m_handsDetected[i];
                    //Handle non detected hands
                    if (!hd.IsDetected)
                    {
                        hd.PushUndetection();

                        //Delete the non valid hands
                        if(!hd.IsValid)
                        {
                            m_handsDetected.RemoveAt(i);
                            i--;
                            continue;
                        }
                    }
                }
            }
        }
#endif
    }
}