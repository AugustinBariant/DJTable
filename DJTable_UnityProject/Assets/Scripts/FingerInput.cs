using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerInput {
    public int id { get; }

    /// <summary>
    /// Absolute pixel space position
    /// </summary> 
    public Vector2 position { get; private set; }

    /// <summary>
    /// Relative screen position in range [0,1]
    /// </summary> 
    public Vector2 posRelative { get; private set; }

    public Vector2 velocity { get; private set; }
    public float acceleration { get; private set; }

    public FingerInput(int id, Vector2 position, Vector2 posRelative, Vector2 velocity, float acceleration) {
        this.id = id;
        this.position = position;
        this.posRelative = posRelative;
        this.velocity = velocity;
        this.acceleration = acceleration;
    }

    public void UpdateProps(Vector2 position, Vector2 posRelative, Vector2 velocity, float acceleration) {
        this.position = position;
        this.posRelative = posRelative;
        this.velocity = velocity;
        this.acceleration = acceleration;
    }
}