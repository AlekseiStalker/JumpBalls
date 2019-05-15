﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BallMaterial
{
    Standard, Bouncing
}

public enum BallLayer
{
    Solid, Flying
}

public class Ball : MonoBehaviour
{ 
    public PhysicsMaterial2D bouncingMat;
    public PhysicsMaterial2D solidMat; 

    private Collider2D _collider2D;
    private Rigidbody2D _rb2D;
    private SpriteRenderer _sprite;
    private TrailRenderer _trail; 

    private void Awake()
    { 
        _rb2D = GetComponent<Rigidbody2D>();
        _collider2D = GetComponent<Collider2D>();
        _sprite = GetComponentInChildren<SpriteRenderer>();
        _trail = GetComponent<TrailRenderer>();
    }

    private void Start()
    {  
        SetColliderMaterial(BallMaterial.Standard);
        SetRigidbodyType(RigidbodyType2D.Dynamic);
        ActiveTrail(false); 
    } 

    public void UpdateColor(Color color)
    {
        Color = color;

        Points = IsBig ? Points + 2 : Points + 1;
    }

    public void UpdateScaleSize(Vector3 newScale, float tailWidth)
    {
        if (IsBig)
        {
            Debug.Log("Ball already big...");
            return;
        }
        ScaleSize = newScale;
        TailWidth = tailWidth;

        IsBig = true;

        Points = Points + 2;
    }
     
    private void ActiveTrail(bool value)
    {
        _trail.enabled = value;
    }

    private void SetColliderMaterial(BallMaterial ballMaterial)
    {
        switch (ballMaterial)
        {
            case BallMaterial.Standard:
                _collider2D.sharedMaterial = solidMat;
                break;
            case BallMaterial.Bouncing:
                _collider2D.sharedMaterial = bouncingMat;
                break;
            default:
                break;
        }
    }

    private void SetRigidbodyType(RigidbodyType2D value)
    {
        _rb2D.bodyType = value;
    }

    private void SetLayer(BallLayer ballLayer)
    {
        switch (ballLayer)
        {
            case BallLayer.Solid:
                this.gameObject.layer = LayerMask.NameToLayer("SolidBall");
                break;
            case BallLayer.Flying:
                this.gameObject.layer = LayerMask.NameToLayer("FlyingBall");
                break;
            default:
                break;
        }
    }
       
    public void Fly(Quaternion rotation, float impulseForce,
                    BallMaterial ballMaterial, BallLayer ballLayer, bool trail = true)  
    { 
        if (_rb2D.bodyType == RigidbodyType2D.Static)
        { 
            SetRigidbodyType(RigidbodyType2D.Dynamic); 
        }
        SetColliderMaterial(ballMaterial);
        SetLayer(ballLayer);
        ActiveTrail(trail);

        this.gameObject.transform.rotation = rotation; 

        _rb2D.AddForce(this.transform.up * impulseForce, ForceMode2D.Impulse);
    } 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Corridor_DOWN")
        { 
            SetRigidbodyType(RigidbodyType2D.Static);
            ActiveTrail(false);

            GameManager.Instance.BallsManager.GoCorridorDown(this);
        }
        else if (other.tag == "Corridor_UP")
        {
            SetRigidbodyType(RigidbodyType2D.Static);
            ActiveTrail(false);

            GameManager.Instance.BallsManager.GoCorridorUp(this);
        }
        else if (other.tag == "BallCatcher")
        {
            SetColliderMaterial(BallMaterial.Standard); 
        } 
        else if (other.tag == "OutOfGameZone")
        {
            SetOutOFGameState(); 

            GameManager.Instance.BallsManager.BallOutOfGame(this.gameObject);   
            Destroy(this.gameObject);

            Debug.Log("Ball out of game");
        } 
    }

    public void SetOutOFGameState()
    {
        SetColliderMaterial(BallMaterial.Standard);
        SetLayer(BallLayer.Solid);
        ActiveTrail(false);

        InGame = false;
    }

    public void SetInGameState()
    {
        SetLayer(BallLayer.Flying);
        ActiveTrail(true);
         
        InGame = true;
    }

    public Color Color
    {
        get { return _sprite.color; }
        set
        {
            _sprite.color = value;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(value, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.5f, 0.0f), new GradientAlphaKey(0.01f, 1.0f) }
            );
             
            _trail.colorGradient = gradient;
        }
    } 

    public Vector3 ScaleSize
    {
        get { return transform.localScale; }
        set
        {
            transform.localScale = value;
        }
    }
    public float TailWidth
    {
        get { return _trail.startWidth; }
        set
        {
            _trail.startWidth = value;
            _trail.endWidth = 0;
        }
    }
     
    public bool InGame { get; set; } = true;
    public int Points { get; set; } = 1;
    public bool IsBig { get; set; } = false; 
    public bool ExtraBallBonus { get; set; }
}
