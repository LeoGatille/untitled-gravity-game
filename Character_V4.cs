
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_4 : MonoBehaviour
{

    //* ------------------------
    //* Private
    //* ------------------------

    private Animator animator;
    private bool isFacingRight = true;
    private bool isJumping = false;
    private float jumpTime = 0;
    private bool isAnchored;
    private Collider2D gravityAnchor;
    private float gameGravity;
    private Transform gravityAnchorParent;

    //* ------------------------
    //* Public
    //* ------------------------

    public Rigidbody2D rb;
    public float speed;
    public float JumpTimeLimit;
    public float jumpPower;
    public float gravitationalPull;
    public float yLimit;
    public LayerMask canHit;

    //* ------------------------
    //* Debug
    //* ------------------------

    public bool useAttraction;
    public bool useRotation;

    void Start()
    {
        animator = GetComponent<Animator>();
        gameGravity = Mathf.Abs(Physics2D.gravity.y);
    }

    void Update()
    {
        Translate(Input.GetAxis("Horizontal"));
        RecordJump();

        if (Input.GetKeyDown(KeyCode.Ampersand))
        {
            rb.velocity = new Vector2(0, 0);
            transform.position = new Vector2(0, 0);
            transform.rotation = new Quaternion(0, 0, 0, 0);
        }


    }

    void FixedUpdate()
    {
        if (isAnchored && !isJumping)
        {
            if (useAttraction)
                PlateformAttraction();
        }

        // if (useRotation) 



    }

    //* ------------------------
    //* Translation
    //* ------------------------

    private void Translate(float movement)
    {
        if (movement != 0)
        {
            float correctedSpeed = isAnchored ? speed / 2 : speed;
            // Debug.Log("movement => " + movement);
            bool isTooFast = (rb.velocity + new Vector2(movement /** * Time.deltaTime * correctedSpeed,*/, 0)).x > speed;
            if (!isTooFast)
            {
                if (isAnchored && !isJumping)
                {
                    transform.Translate(Vector2.right * movement * Time.deltaTime * correctedSpeed);
                    rb.velocity += new Vector2(movement /** * Time.deltaTime * correctedSpeed,*/, 0);
                    transform.Translate(Vector2.right * movement * Time.deltaTime * correctedSpeed);
                }
                else
                {
                    rb.velocity += new Vector2(movement / 10 /** * Time.deltaTime * correctedSpeed,*/, 0);
                }
            }
            animator.SetBool("isWalking", true);

            if (isFacingRight != (movement > 0)) Flip();
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }
    private void Flip()
    {
        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1;
        transform.localScale = currentScale;
        isFacingRight = !isFacingRight;
    }


    //* ------------------------
    //* Jump
    //* ------------------------

    private void RecordJump()
    {
        if (isAnchored && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetButtonDown("Jump")))
        {
            isJumping = true;
            jumpTime = 0;

            //! Legacy animation management
            animator.SetBool("isGrounding", false);
            animator.SetBool("isJumping", isJumping);
        }

        if (isJumping)
        {
            Jump();
        }

        if (Input.GetButtonUp("Jump") | jumpTime > JumpTimeLimit)
        {
            isJumping = false;
            animator.SetBool("isJumping", isJumping);
        }
    }

    private void Jump()
    {
        float movement = Input.GetAxis("Horizontal");
        float correctedJumpPower = movement != 0 ? jumpPower / 1.2f : jumpPower;
        rb.velocity = new Vector2(rb.velocity.x + movement / 2, correctedJumpPower);
        jumpTime += Time.deltaTime;
    }

    //* ------------------------
    //* Physic
    //* ------------------------

    private void PlateformAttraction()
    {
        //- https://stackoverflow.com/questions/56170403/get-y-position-of-box-collider-borders
        var collider = GetComponent<Collider2D>();
        var yHalfExtents = collider.bounds.extents.y;
        var yCenter = collider.bounds.center.y;
        Vector2 bottomCenter = new Vector2(transform.position.x - collider.bounds.extents.x, transform.position.y - collider.bounds.extents.y);

        if (!isAnchored) return;

        Vector2 direction = bottomCenter - gravityAnchor.ClosestPoint(bottomCenter);
        rb.velocity = direction.normalized * gravitationalPull * Time.fixedDeltaTime * -1;
    }


    private Vector3 convertToV3(Vector2 a)
    {
        Vector3 toto = a;
        return a;
    }

    private Vector2 convertToV2(Vector3 a)
    {
        Vector2 toto = a;
        return a;


    }
    //* ------------------------
    //* Collision detection
    //* ------------------------

    
    

    //* ------------------------
    //* Listeners
    //* ------------------------
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("gravityField"))
        {
            gravityAnchor = other.transform.parent.GetComponent<Collider2D>();
            isAnchored = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("gravityField"))
        {
            isAnchored = false;
            gravityAnchorParent = null;
            gravityAnchor = null;
        }
    }


}


/**
// - Collision detection tuto script
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryController : MonoBehaviour
{
    [Header("Line renderer veriables")]
    public LineRenderer line;
    [Range(2, 30)]
    public int resolution;

    [Header("Formula variables")]
    public Vector2 velocity;
    public float yLimit;
    private float g;

    [Header("Linecast variables")]
    [Range(2, 30)]
    public int linecastResolution;
    public LayerMask canHit;

    private void Start()
    {
        g = Mathf.Abs(Physics2D.gravity.y);
    }

    private void Update()
    {
        RenderArc();
    }

    private void RenderArc()
    {
        line.positionCount = resolution + 1;
        line.SetPositions(CalculateLineArray());
    }

    private Vector3[] CalculateLineArray()
    {
        //* Génere un liste de Vecor2
        Vector3[] lineArray = new Vector3[resolution + 1];

        //* Détermine le temps minimum passé sur X
        var lowestTimeValue = MaxTimeX() / resolution;

        for (int i = 0; i < lineArray.Length; i++)
        {
            //* Détermine la position de la ligne en fonction du minimum fois son index
            var t = lowestTimeValue * i;
            //* Récupère le point<Vector2> de la ligne actuelle en lui passant la valeur du temps passé sur X
            lineArray[i] = CalculateLinePoint(t);
        }

        return lineArray;
    }

    private Vector2 HitPosition()
    {
        //* Détermine le temps minimum passé  sur Y
        var lowestTimeValue = MaxTimeY() / linecastResolution;

        for (int i = 0; i < linecastResolution + 1; i++)
        {
            //* Détermine le temp passé sur Y pour cette itération
            var t = lowestTimeValue * i;
            //* Détermine le temp passé sur Y pour la prochaine itération
            var tt = lowestTimeValue * (i + 1);

            //* Check si un impact va se produir entre cette itération et la prochaine
            var hit = Physics2D.Linecast(CalculateLinePoint(t), CalculateLinePoint(tt), canHit);

            if (hit)
            {
                //* Si oui le point d'impact est renvoyé
                return hit.point;
            }
        }

        //* Si non le dernier point que peut atteindre le rb sans collision est retourné
        return CalculateLinePoint(MaxTimeY());
    }

    private Vector3 CalculateLinePoint(float t)
    {
        //* Le déplacement sur x est égale à la vélocité sur X fois le temps passé
        float x = velocity.x * t;

        //* Le déplacement sur y est égale à la vélocité sur Y fois le temps passé
        //* Moins la valeur de la gravité fois le temps passé au carré (FOR REASONS...)
        //* Divisé par 2 (FOR REASONS...) 
        float y = (velocity.y * t) - (g * Mathf.Pow(t, 2) / 2);

        //* Retourne le Vector2 issue du cumule de la position Actuel du RB + les facteurs de déplacement
        return new Vector3(x + transform.position.x, y + transform.position.y);
    }

    private float MaxTimeY()
    {
        var v = velocity.y;
        var vv = v * v;

        var t = (v + Mathf.Sqrt(vv + 2 * g * (transform.position.y - yLimit))) / g;
        return t;
    }

    private float MaxTimeX()
    {
        //* Récup la vélocité X du rb
        var x = velocity.x;
        if (x == 0) //* Ne peut être égale à 0 (FOR REASON...)
        {
            velocity.x = 000.1f;
            x = velocity.x;
        }
        //* Récup la valeur du temps (le maxTimeX ne peut être supérieur à HitPosition qui retourne le point d'impact si la valeur max sur X ne peut être atteinte)
        var t = (HitPosition().x - transform.position.x) / x;
        return t;
    }
}


*/