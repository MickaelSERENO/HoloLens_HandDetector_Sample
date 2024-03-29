﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SDK.UX.Interactable.Themes
{
    public class InteractableOffsetTheme : InteractableThemeBase
    {
        private Vector3 startPosition;

        public InteractableOffsetTheme()
        {
            Types = new Type[] { typeof(Transform) };
            Name = "Offset Theme";
            ThemeProperties.Add(
                new InteractableThemeProperty()
                {
                    Name = "Offset",
                    Type = InteractableThemePropertyValueTypes.Vector3,
                    Values = new List<InteractableThemePropertyValue>(),
                    Default = new InteractableThemePropertyValue() { Vector3 = Vector3.zero }
                });
        }

        public override void Init(GameObject host, InteractableThemePropertySettings settings)
        {
            base.Init(host, settings);
            startPosition = Host.transform.localPosition;
        }

        public override InteractableThemePropertyValue GetProperty(InteractableThemeProperty property)
        {
            InteractableThemePropertyValue start = new InteractableThemePropertyValue();
            start.Vector3 = Host.transform.localPosition;
            return start;
        }

        public override void SetValue(InteractableThemeProperty property, int index, float percentage)
        {
            Host.transform.localPosition = Vector3.Lerp(property.StartValue.Vector3, startPosition + property.Values[index].Vector3, percentage);
        }
    }
}
