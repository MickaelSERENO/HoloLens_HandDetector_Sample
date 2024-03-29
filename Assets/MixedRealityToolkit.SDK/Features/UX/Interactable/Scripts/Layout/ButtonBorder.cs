﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SDK.UX.Interactable.Layout
{
    /// <summary>
    /// Use a Unity primitive Cube or cylinder as a border segment relative to the scale of the AnchorTransform
    /// Use with ButtonSize on the component and the Anchor for consistent results
    /// Works best when using with ButtonSize, but not required - See ButtonSize for more info.
    /// </summary>
    [ExecuteInEditMode]
    public class ButtonBorder : MonoBehaviour
    {
        /// <summary>
        /// A scale factor for button layouts, default is based on 2048 pixels to 1 meter.
        /// Similar to values used in designer and 2D art programs and helps create consistency across teams.
        /// 
        /// Use Case:
        /// A designer created a basic button background using a Cube and ButtonSize
        /// Add some more borders, using more cubes or cylinders, that will scale to the edges of the background size
        /// </summary>
        [Tooltip("A pixel to Unity unit conversion, Default: 2048x2048 pixels covers a 1x1 Unity Unit or default primitive size")]
        [SerializeField]
        private float BasePixelScale = 2048;

        /// <summary>
        /// The transform to offset from. 
        /// </summary>
        [Tooltip("The transform this object should be linked and aligned to")]
        [SerializeField]
        private Transform AnchorTransform = null;

        /// <summary>
        /// Width of the border
        /// </summary>
        [Tooltip("Size of the border using pixel values from our design program.")]
        [SerializeField]
        private float Weight = 10;

        /// <summary>
        /// The depth of the border
        /// </summary>
        [Tooltip("Depth of the border using pixel values from our design program.")]
        [SerializeField]
        private float Depth = 20;

        /// <summary>
        /// A vector that sets the border position to the edge of the Anchor and
        /// scales it to match the edge it is assigned to.
        /// Ex: Vector3.right would place the border on the Anchor's right side.
        /// </summary>
        [Tooltip("Where to set this object's center point in relation to the Anchor's center point")]
        [SerializeField]
        private Vector3 Alignment = Vector3.zero;

        /// <summary>
        /// An absolute value to offset the border from the Anchor's edge
        /// </summary>
        [Tooltip("That absolute amount to offset the position")]
        [SerializeField]
        private Vector3 PositionOffset = Vector3.zero;

        /// <summary>
        /// Overlap the edge it is assigned to so there are not gaps in the corners
        /// </summary>
        [Tooltip("Will extend the height or width of the border to create corners.")]
        [SerializeField]
        private bool AddCorner = true;

        /// <summary>
        /// These scales and positions are applied in Unity Editor only while doing layout.
        /// Turn off for responsive UI type results when editing ItemSize during runtime.
        /// Scales will be applied each frame.
        /// </summary>
        [Tooltip("should this only run in Edit mode, to avoid updating as items move?")]
        [SerializeField]
        private bool OnlyInEditMode = true;

        /// <summary>
        /// Set the size and position
        /// </summary>
        private void UpdateSize()
        {
            Vector3 weighDireciton = new Vector3(Mathf.Abs(Alignment.x), Mathf.Abs(Alignment.y), Mathf.Abs(Alignment.z));
            Vector3 scale = weighDireciton * (Weight / BasePixelScale);// Vector3.Scale(Alignment, Scale) + Offset / BasePixelScale;
            float size = ((Weight * 2) / BasePixelScale);
            if (scale.x > scale.y)
            {
                scale.y = AddCorner ? AnchorTransform.localScale.y + size : AnchorTransform.localScale.y;
            }
            else
            {
                scale.x = AddCorner ? AnchorTransform.localScale.x + size : AnchorTransform.localScale.x;
            }
            scale.z = Depth / BasePixelScale;

            transform.localScale = scale;

            Vector3 startPosition = AnchorTransform.localPosition;

            if (AnchorTransform != this.transform)
            {
                startPosition = AnchorTransform.localPosition + (Vector3.Scale(AnchorTransform.localScale * 0.5f, Alignment));
            }

            transform.localPosition = startPosition + (Alignment * Weight * 0.5f / BasePixelScale) + (PositionOffset / BasePixelScale);
        }

        // Update is called once per frame
        void Update()
        {
            if ((Application.isPlaying && !OnlyInEditMode) || (!Application.isPlaying))
            {
                UpdateSize();
            }
        }
    }
}
