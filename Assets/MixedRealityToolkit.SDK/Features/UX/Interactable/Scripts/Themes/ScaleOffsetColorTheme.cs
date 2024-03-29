﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SDK.UX.Interactable.Themes
{
    public class ScaleOffsetColorTheme : InteractableColorTheme
    {
        protected Vector3 startPosition;
        protected Vector3 startScale;

        public override void Init(GameObject host, InteractableThemePropertySettings settings)
        {
            base.Init(host, settings);
            startPosition = Host.transform.localPosition;
            startScale = Host.transform.localScale;
        }

        public ScaleOffsetColorTheme()
        {
            Types = new Type[] { typeof(Transform), typeof(TextMesh), typeof(TextMesh), typeof(Renderer) };
            Name = "Default: Scale, Offset, Color";
            ThemeProperties = new List<InteractableThemeProperty>();
            ThemeProperties.Add(
                new InteractableThemeProperty()
                {
                    Name = "Scale",
                    Type = InteractableThemePropertyValueTypes.Vector3,
                    Values = new List<InteractableThemePropertyValue>(),
                    Default = new InteractableThemePropertyValue() { Vector3 = Vector3.one }
                });
            ThemeProperties.Add(
                new InteractableThemeProperty()
                {
                    Name = "Offset",
                    Type = InteractableThemePropertyValueTypes.Vector3,
                    Values = new List<InteractableThemePropertyValue>(),
                    Default = new InteractableThemePropertyValue() { Vector3 = Vector3.zero }
                });
            ThemeProperties.Add(
                new InteractableThemeProperty()
                {
                    Name = "Color",
                    Type = InteractableThemePropertyValueTypes.Color,
                    Values = new List<InteractableThemePropertyValue>(),
                    Default = new InteractableThemePropertyValue() { Color = Color.white }
                });
        }

        public override InteractableThemePropertyValue GetProperty(InteractableThemeProperty property)
        {
            InteractableThemePropertyValue start = new InteractableThemePropertyValue();

            switch (property.Name)
            {
                case "Scale":
                    start.Vector3 = Host.transform.localScale;
                    break;
                case "Offset":
                    start.Vector3 = Host.transform.localPosition;
                    break;
                case "Color":
                    start = base.GetProperty(property);
                    break;
                default:
                    break;
            }
            return start;
        }

        public override void SetValue(InteractableThemeProperty property, int index, float percentage)
        {
            switch (property.Name)
            {
                case "Scale":
                    Host.transform.localScale = Vector3.Lerp(property.StartValue.Vector3, Vector3.Scale(startScale, property.Values[index].Vector3), percentage);
                    break;
                case "Offset":
                    Host.transform.localPosition = Vector3.Lerp(property.StartValue.Vector3, startPosition + property.Values[index].Vector3, percentage);
                    break;
                case "Color":
                    base.SetValue(property, index, percentage);
                    break;
                default:
                    break;
            }
        }
    }
}
