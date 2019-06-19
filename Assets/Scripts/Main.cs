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
    {

#if ENABLE_WINMD_SUPPORT
        /// <summary>
        /// The current pointing direction
        /// </summary>
        private Vector3 m_pointingDir = new Vector3(0, 0, 0);

        /// <summary>
        /// The root spatial coordinate system created by Unity
        /// </summary>
        private SpatialCoordinateSystem m_spatialCoordinateSystem = null;
#endif

        private HandDetectorProvider m_hdProvider = new HandDetectorProvider();

        /// <summary>
        /// The GameObject representing the ray
        /// </summary>
        public GameObject RayObject = null;

        /// <summary>
        /// The GameObject permitting to anchor a position
        /// </summary>
        public GameObject PosObject = null;

        // Start is called before the first frame update
        void Start()
        {
            m_hdProvider.Smoothness = 0.75f;
            m_hdProvider.InitializeHandDetector();
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
                m_hdProvider.SetSpatialCoordinateSystem(m_spatialCoordinateSystem);
            }

            lock(m_hdProvider)
            {
                //Update the gameobjects
                RayObject.SetActive(false);
                foreach (HandDetected hd in m_hdProvider.HandsDetected)
                {
                    if(hd.IsValid)
                    {
                        Vector3 distBody   = (hd.Position - Camera.main.transform.position);
                        Vector2 distBody2D = new Vector2(distBody.x, distBody.z);
                        if (distBody2D.magnitude > 0.1) //Discard detection that may be the chest "on the body"
                        {
                            RayObject.SetActive(true);

                            //Hand position
                            transform.position = hd.Position;

                            //Pointing
                            Vector3 anchorPoint = Camera.main.transform.position + new Vector3(0, -0.25f, 0);
                            Vector3 pointDir = (hd.Position - anchorPoint).normalized;
                            if (m_pointingDir.x == 0 && m_pointingDir.y == 0 && m_pointingDir.z == 0)
                            {
                                m_pointingDir = pointDir;
                                RayObject.transform.up = m_pointingDir;
                            }
                            else
                            {
                                m_pointingDir = (1.0f - 0.8f) * pointDir + 0.8f * m_pointingDir; //Apply a strong "smoothing"
                                RayObject.transform.up = m_pointingDir;
                            }
                            RayObject.transform.localPosition = anchorPoint + RayObject.transform.up * RayObject.transform.localScale.y;
                                
                            //Select a position!
                            PosObject.transform.localPosition = anchorPoint + m_pointingDir * (20.0f * Math.Max((anchorPoint - hd.Position).magnitude, 0.3f) - 6.0f);
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
    }
}