﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License

using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.UI.Interaction
{
    /// <summary>
    /// Definition class for an animation target, utilized in the State Visualizer component.
    /// </summary>
    [Serializable]
    public class AnimationTarget
    {
        [SerializeField]
        [Tooltip("The target game object for animations.")]
        private GameObject target;

        /// <summary>
        /// The target game object for animations.
        /// </summary>
        public GameObject Target
        {
            get => target;
            set
            {
                if (IsTargetObjectValid(value))
                {
                    target = value;
                }
                else
                {
                    target = null;
                    Debug.LogError("The Target property can only be a child of this game object, the target was set back to null");
                }
            }
        }

        [SerializeReference]
        [Tooltip("List of animatable properties for the target game object.  Scale and material color are examples of animatable properties.")]
        private List<IStateAnimatableProperty> stateAnimatableProperties = new List<IStateAnimatableProperty>();

        /// <summary>
        /// List of animatable properties for the target game object.  Scale and material color are examples of animatable properties.
        /// </summary>
        public List<IStateAnimatableProperty> StateAnimatableProperties
        {
            get => stateAnimatableProperties;
            internal set => stateAnimatableProperties = value;
        }

        public void SetKeyFrames(AnimationClip animationClip)
        {
            foreach (var animatableProperty in StateAnimatableProperties)
            {
                animatableProperty.Target = Target;
                animatableProperty.SetKeyFrames(animationClip);
            }
        }

        public void RemoveKeyFrames(string animatablePropertyName, AnimationClip animationClip)
        {
            IStateAnimatableProperty animatableProperty = GetAnimatableProperty(animatablePropertyName);

            if (animatableProperty != null)
            {
                animatableProperty.RemoveKeyFrames(animationClip);
            }
        }

        private IStateAnimatableProperty GetAnimatableProperty(string animatablePropertyName)
        {
            return StateAnimatableProperties.Find((prop) => prop.AnimatablePropertyName == animatablePropertyName);
        }

        private bool IsTargetObjectValid(GameObject target)
        {
            Transform startTransform = target.transform;
            Transform initialTransform = target.transform;

            // If this game object has the State Visualizer attached 
            if (target.GetComponent<StateVisualizer>() != null)
            {
                return true;
            }

            // If the current object is a root and does not have a parent 
            if (startTransform.parent != null)
            {
                // Traverse parents until the State Visualizer is found to determine if the current target is a valid child object
                while (startTransform.parent != initialTransform)
                {
                    if (startTransform.GetComponent<StateVisualizer>() != null)
                    {
                        return true;
                    }

                    startTransform = startTransform.parent;
                }
            }

            return false;
        }

        internal void CreateAnimatablePropertyInstance(string animatablePropertyName, string stateName)
        {
            StateAnimatableProperty animatableProperty;

            // Find matching event configuration by state name
            var animatablePropertyTypes = TypeCacheUtility.GetSubClasses<StateAnimatableProperty>();
            Type animatablePropertyType = animatablePropertyTypes.Find((type) => type.Name.StartsWith(animatablePropertyName));

            if (animatablePropertyType != null)
            {
                // If a state has an associated event configuration class, then create an instance with the matching type
                animatableProperty = Activator.CreateInstance(animatablePropertyType) as StateAnimatableProperty;
            }
            else
            {
                animatableProperty = null;
                Debug.Log("The animatableProperty property name given does not have a matching configuration type");
            }

            animatableProperty.StateName = stateName;
            animatableProperty.Target = Target;

            StateAnimatableProperties.Add(animatableProperty);
        }
    }
}
