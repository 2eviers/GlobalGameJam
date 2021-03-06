﻿using UnityEngine;
using System.Collections;

public class Door : Mechanism {

    public bool Upward;
    public float Speed;

    private float _initialPosition;
    private Vector3 _direction;
    private float _height;


    void Start()
    {
        _initialPosition = this.transform.position.y;
    }

    // Lift the door
    protected override void runMechanism()
    {
     

        bool b;
        if (Upward)
            b = this.transform.position.y >= _initialPosition + GetComponent<MeshCollider>().bounds.size.y; 
        else
            b = this.transform.position.y <= _initialPosition - GetComponent<MeshCollider>().bounds.size.y;
        if (b)
            return;
        else
        {
            _direction = new Vector3(0, (Upward ? 1 : -1), 0);
            transform.Translate(_direction * Speed * Time.deltaTime);
        }
    }

    // Close the door
    protected override void backToDefaultPosition()
    {
        if (this.transform.position.y != _initialPosition)
        {
            bool b;
            if (Upward)
                b = this.transform.position.y <= _initialPosition - GetComponent<MeshCollider>().bounds.size.y;
            else
                b = this.transform.position.y >= _initialPosition + GetComponent<MeshCollider>().bounds.size.y;
            if (b)
                return;
            else
            {
                _direction = new Vector3(0, (Upward ? -1 : 1), 0);
                transform.Translate(_direction * Speed * Time.deltaTime);
            }
        }
    }
}
