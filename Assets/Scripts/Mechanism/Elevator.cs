﻿using UnityEngine;
using System.Collections;

public class Elevator : Mechanism {

    public GameObject Platform;
    public GameObject Axis;
    public float Speed;

    private int _upward;
    private float _originalHeight;

    void Start()
    {
        _upward = 1;
        _originalHeight = Platform.transform.position.y;
    }

    // Go up / down if the platform is on the bottom / top, and loop
    protected override void runMechanism()
    {
        float height = Axis.transform.localScale.y;

        // If it's at the top, _upward = false (going down)
        if (Platform.transform.position.y - Axis.transform.position.y >= height)
            _upward = -1;
        else if (Platform.transform.position.y - Axis.transform.position.y <= -height)
            _upward = 1;

        Vector3 dir = new Vector3(0, 1, 0);
        Platform.GetComponent<Rigidbody>().MovePosition(Platform.GetComponent<Rigidbody>().position + dir * Speed * _upward * Time.fixedDeltaTime);
       // rigidbody.transform.Translate(dir * Speed * _upward * Time.fixedDeltaTime);

       // Platform.transform.Translate(dir * Speed * _upward * Time.deltaTime);
    }

    // Go back to original position
    protected override void backToDefaultPosition()
    {
        Vector3 dir = new Vector3(0, 1, 0);
        if (Mathf.Abs(Platform.transform.position.y - _originalHeight) < .1)
            return;
        if (Platform.transform.position.y > _originalHeight)
            _upward = -1;
        else if (Platform.transform.position.y < _originalHeight)
            _upward = 1;
        Platform.GetComponent<Rigidbody>().MovePosition(Platform.GetComponent<Rigidbody>().position + dir * Speed * _upward * Time.fixedDeltaTime);
        //rigidbody.transform.Translate(dir * Speed * _upward * Time.fixedDeltaTime);

      // Platform.transform.Translate(dir * Speed * _upward * Time.deltaTime);
    }
}
