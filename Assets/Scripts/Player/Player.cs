﻿using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]

public class Player : Caracteristique
{
    /// <summary>
    /// Cette variable est le préfix des controle dans l'InputManager il vaut "J1" ou "J2"
    /// </summary>
    //[SerializeField] 
    public string _prefixController = "J1";
    /// <summary>
    /// Vitesse maximal de l'avatar
    /// </summary>
    public float MaxSpeed = 10.0f;
    /// <summary>
    /// Hauteur maximal de l'avatar
    /// </summary>
    public float JumpHeight = 2.0f;
    /// <summary>
    /// Temps de jump controle (temps pendant lequel tu peux augmenter la hauteur du saut
    /// </summary>
    [SerializeField]
    private float _jumpControlDuration = 0.5f;
    /// <summary>
    /// Acceleration maximale
    /// </summary>
    [SerializeField]
    private float _maxAcceleration = 5.0f;
    /// <summary>
    /// Acceleration maximale en l'air
    /// </summary>
    [SerializeField]
    private float _maxAccelerationAir = 1.0f;
    /// <summary>
    /// Acceleration maximale en l'air
    /// </summary>
    [SerializeField]
    private float _minJumpHeight = 1.0f;
    /// <summary>
    /// Vaut true si et seulement si l'avatar peut sauter
    /// </summary>
    public bool CanJump = true;
    /// <summary>
    /// Variable nécessaire à repérer quand le personnage est au sol
    /// </summary>
    private int grounded = 0;
    /// <summary>
    /// Temps au moment du saut (nécessaire pour jauger un saut)
    /// </summary>
    private float timerJump;
    /// <summary>
    /// Facteur à appliquer à une animation pour qu'elle dure une seconde.
    /// </summary>
    private float _jumpRatio = 15f/30f;

    private Animator anim;
	private Rotation rotor;
    
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
		rotor = GetComponentInChildren<Rotation> ();
    }


    void FixedUpdate()
    {

        var axis = new Vector3(Input.GetAxis(_prefixController+"Horizontal"),0, 0);
		axis += new Vector3(Input.GetAxis(_prefixController + "HorizontalJoystick"),0, 0);


        float h = axis.x;

		anim.SetBool ("Walking", h!=0f);
		if(axis.x!=0)
			rotor.turn (axis.x > 0);

        //Si le personnage touche le sol
        if (grounded>0)
        {
			if(anim.GetBool("Jump")== true)
				anim.SetBool("Jump", false);
			//anim.speed = 1;
            //anim.speed = Mathf.Abs(rigidbody.velocity.x);
            //Si l'input est nul on se laisse aller par l'inertie.
            if (axis.magnitude > 0)
            {
                // Calcule la vitesse à atteindre
                var targetVelocity = transform.TransformDirection(axis);
                targetVelocity *= MaxSpeed;

                ApplySpeedForce(targetVelocity);
            }
            // Jump initial
            if (CanJump && Input.GetButton(_prefixController+"Jump"))
            {
                InitialJump();
				anim.SetBool("Jump", true);
            }
        }
        // si le personnage est en vole
        else
        {
            axis.y = CanJump ? Mathf.Abs(Input.GetAxis(_prefixController + "Jump")) : 0;
            AirControl(axis);
        }

        //grounded = false;
    }
    void OnCollisionEnter(Collision collision)
    {
        Vector3 pos = collision.contacts[0].point;
        bool hitGround = false;
        foreach (ContactPoint contact in collision.contacts)
        {
            // if the collision is against the ground, it's on the x axis, so y and
            // z are the same for all contact points
            if (contact.point.y != pos.y || contact.point.z != pos.z)
            {
                grounded++;
                return;
            }
        }
        if (collision.contacts[0].normal.y > 0.30)
            hitGround = true;
        if (hitGround)
            grounded++;
    }

    void OnCollisionExit(Collision collision)
    {
        Vector3 pos = collision.contacts[0].point;
        foreach (ContactPoint contact in collision.contacts)
        {
            // if the collision is against the ground, it's on the x axis, so y and
            // z are the same for all contact points
            if (contact.point.y != pos.y || contact.point.z != pos.z)
            {
                grounded--;
                return;
            }
        }

        bool hitGround = (Mathf.Abs(collision.contacts[0].normal.y) > 0.10);
        if (hitGround)
            grounded--;
    }
    /// <summary>
    /// Calcule la vitesse de saut initiale 
    /// </summary>
    /// <returns>Retourne la vitesse initiale pour atteindre la hauteur passé en paramètre</returns>
    float CalculateInitialJumpVerticalSpeed(float hauteur)
    {
        return Mathf.Sqrt(2 * hauteur * Physics.gravity.magnitude * rigidbody.mass);
    }
    /// <summary>
    /// Calcule la force qui s'applique pour le air contrôle vertical 
    /// (quand on continue d'appuyer sur jump au début du saut)
    /// </summary>
    /// <returns>Retourn la force calculée</returns>
    float CalculateJumpForce()
    {
        float deltaTime = 1/_jumpControlDuration;//Time.fixedDeltaTime / _jumpControlDuration;
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2 * (JumpHeight -_minJumpHeight) * Physics.gravity.magnitude * deltaTime);
    }

    /// <summary>
    /// Applique les forces necessaire pour atteindre la vitesse targetVelocity
    /// </summary>
    /// <param name="targetVelocity"></param>
    void ApplySpeedForce(Vector3 targetVelocity)
    {
        Vector3 velocity = rigidbody.velocity;
        Vector3 velocityChange = (targetVelocity - velocity);
        var maxVelChan = _maxAcceleration;
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelChan, maxVelChan);
        velocityChange.z = 0;
        velocityChange.y = 0;
        rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
    }
    /// <summary>
    /// Calcule le temps passé en l'air lors d'un saut d'une hauteur de hauteur
    /// </summary>
    /// <param name="hauteur"></param>
    /// <returns>Retourne la valeur calculée</returns>
    float CalculeAirTime(float hauteur)
    {
        var initialJumpSpeed = CalculateInitialJumpVerticalSpeed(hauteur);
        return (2 * initialJumpSpeed / (Physics.gravity.magnitude));
    }
    /// <summary>
    /// Applique les force nécessaire au saut initial
    /// </summary>
    void InitialJump()
    {
        var initialJumpSpeed = CalculateInitialJumpVerticalSpeed(_minJumpHeight);
        rigidbody.velocity = new Vector3(rigidbody.velocity.x, initialJumpSpeed, rigidbody.velocity.z);
        timerJump = Time.time;

        var tempsDeVole = CalculeAirTime(_minJumpHeight);
        //anim.speed = _jumpRatio / tempsDeVole; 
    }
    /// <summary>
    /// Applique les forces de air contrôle en fonction des inputs axis
    /// </summary>
    /// <param name="axis"></param>
    void AirControl(Vector3 axis)
    {
        bool jump = axis.y != 0;
        var velocityChange = _maxAccelerationAir * transform.TransformDirection(axis);
        var t = timerJump + _jumpControlDuration - Time.time;

        if (jump && t > 0)
        {
            velocityChange.y = CalculateJumpForce();
            var timeRatio = t / _jumpControlDuration;
            var tempsDeVole = CalculeAirTime(JumpHeight - timeRatio * (JumpHeight - _minJumpHeight));
            //anim.speed = _jumpRatio / tempsDeVole;

        }
        else // au cas où
            velocityChange.y = 0;

        rigidbody.AddForce(velocityChange, ForceMode.Force);
    }
}
