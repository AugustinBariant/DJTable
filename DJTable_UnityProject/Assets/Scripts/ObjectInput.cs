using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectInput
{
    // Temporary ID for the surface object
    // we need this to keep track if it's active
    public int id { get; }

    // The actual marker id
    public int tagValue { get; }

    /// <summary>
    /// Absolute pixel space position
    /// </summary> 
    public Vector2 position { get; private set; }

    /// <summary>
    /// Relative screen position in range [0,1]
    /// </summary> 
    public Vector2 posRelative { get; private set; }

    /// <summary>
    /// Orientation in radians
    /// </summary> 
    public float orientation { get; private set; }
    public Vector2 velocity { get; private set; }
    public float acceleration { get; private set; }

    public float angularVelocity { get; private set; }

    public float angularAcceleration { get; private set; }

    const float PI2 = 2 * Mathf.PI;

    public ObjectInput(int id, int tagValue, Vector2 position, Vector2 posRelative, float orientation, Vector2 velocity, float acceleration, float angularVelocity, float angularAcceleration)
    {
        this.id = id;
        this.tagValue = tagValue;
        this.position = position;
        this.posRelative = posRelative;
        this.orientation = NormalizeOrientation(orientation);
        this.velocity = velocity;
        this.acceleration = acceleration;
        this.angularVelocity = angularVelocity;
        this.angularAcceleration = angularAcceleration;
    }

    public void UpdateProps(Vector2 position, Vector2 posRelative, float orientation, Vector2 velocity, float acceleration, float angularVelocity, float angularAcceleration) {
        this.position = position;
        this.posRelative = posRelative;
        this.orientation = NormalizeOrientation(orientation);
        this.velocity = velocity;
        this.acceleration = acceleration;
        this.angularVelocity = angularVelocity;
        this.angularAcceleration = angularAcceleration;
    }

    /// <summary>
    /// Use for development only
    /// </summary>
    /// <param name="orientation"></param>
    public void UpdateOrientation(float orientation)
    {
        this.orientation = NormalizeOrientation(orientation);
    }

    /// <summary>
    /// Normalizes orientation value to [0,2pi) range
    /// </summary>
    /// <param name="orientation"></param>
    /// <returns></returns>
    private float NormalizeOrientation(float orientation)
    {
        float normalized = orientation % PI2;
        if (normalized < 0f)
        {
            normalized += PI2;
        }
        return normalized;
    }
}