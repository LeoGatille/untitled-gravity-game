
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_3 : MonoBehaviour
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

    void Start()
    {
        animator = GetComponent<Animator>();
        gameGravity = Mathf.Abs(Physics2D.gravity.y);
    }

    void Update()
    {

        Translate(Input.GetAxis("Horizontal"));
        RecordJump();

        if (isAnchored && !isJumping)
        {
            PlateformAttraction();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            rb.velocity = new Vector2(0, 0);
            transform.position = new Vector2(0, 0);
            transform.rotation = new Quaternion(0, 0, 0, 0);
        }
    }

    void FixedUpdate()
    {
        DisplayCollisions();
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
            if (Mathf.Abs(rb.velocity.x) < speed)
            {
                rb.velocity += new Vector2(movement /** * Time.deltaTime * correctedSpeed,*/, 0);
                if (isAnchored)
                {
                    transform.Translate(Vector2.right * (movement / 10) * Time.deltaTime * correctedSpeed);
                }
            }
            else
            {
                Debug.Log("Too much velocity");
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
        rb.velocity = new Vector2(rb.velocity.x, jumpPower);
        jumpTime += Time.deltaTime;
    }

    //* ------------------------
    //* Physic
    //* ------------------------

    private void PlateformAttraction()
    {
        if (!isAnchored) return;
        Vector2 v2Positon = transform.position;
        Vector2 direction = v2Positon - gravityAnchor.ClosestPoint(transform.position);
        rb.velocity = direction.normalized * gravitationalPull * Time.fixedDeltaTime * -1;
    }
    private void MatchPlateformFloorAngle(Vector2 collisionPoint)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, collisionPoint, out hit, 1))
        {
            transform.rotation = Quaternion.LookRotation(Vector3.Cross(transform.TransformDirection(Vector3.right), hit.normal));
        }
    }


    //* ------------------------
    //* Collision detection
    //* ------------------------

    // private void DispayCollisionPoint(Vector2 origin)
    // {
    //     Vector2[] trajectories = CalluateFuturePosions(origin);
    //     Debug.DrawLine(origin, trajectories[trajectories.Length - 1]);
    // }

    private Vector2[] CalluateFuturePosions(Vector2 origin)
    {
        Vector2[] positions = new Vector2[31];

        float lowestTimeValue = MaxTimeX(origin) / 30;

        for (int i = 0; i < positions.Length; i++)
        {
            float currentTimeValue = lowestTimeValue * i;
            positions[i] = CalculatePositionPoint(currentTimeValue, origin);
        }

        return positions;
    }

    private Vector2 CalculatePositionPoint(float time, Vector2 origin)
    {
        float x = rb.velocity.x * time;
        float y = (rb.velocity.y * time) - (gameGravity * Mathf.Pow(time, 2) / 2);

        return new Vector2(x + origin.x, y + origin.y);
    }

    private Vector2 HitPosition(Vector2 origin)
    {
        float lowestTimeValue = MaxTimeY(origin) / 15;

        for (int i = 1; i < 16; i++)
        {
            float currentIterationTime = lowestTimeValue * i;
            float nextIterationTime = lowestTimeValue * (i + 1);

            RaycastHit2D hit = Physics2D.Linecast(CalculatePositionPoint(currentIterationTime, origin), CalculatePositionPoint(nextIterationTime, origin), canHit);

            if (hit)
            {
                return hit.point;
            }
        }

        return CalculatePositionPoint(MaxTimeY(origin), origin);
    }

    private float MaxTimeY(Vector2 origin)
    {
        float v = rb.velocity.y;
        float vv = v * v;

        return (v + Mathf.Sqrt(vv + 2 * gameGravity * (origin.y - -8))) / gameGravity;
    }
    private float MaxTimeX(Vector2 origin)
    {
        var x = rb.velocity.x;
        return (HitPosition(origin).x - origin.x) / x;
    }

    private void DisplayCollisions()
    {
        var boxBounds = GetComponent<Collider2D>().bounds;

        var topLeft = new Vector2(boxBounds.center.x - boxBounds.extents.x, boxBounds.center.y + boxBounds.extents.y);
        var centerLeft = new Vector2(boxBounds.center.x - boxBounds.extents.x, boxBounds.center.y);
        var bottomLeft = new Vector2(boxBounds.center.x - boxBounds.extents.x, boxBounds.center.y - boxBounds.extents.y);

        var topRight = new Vector2(boxBounds.center.x + boxBounds.extents.x, boxBounds.center.y + boxBounds.extents.y);
        var centerRight = new Vector2(boxBounds.center.x + boxBounds.extents.x, boxBounds.center.y);
        var bottomRight = new Vector2(boxBounds.center.x + boxBounds.extents.x, boxBounds.center.y - boxBounds.extents.y);

        var collisionListeners = new Vector2[] {
            topLeft,
            centerLeft,
            bottomLeft,
            topRight,
            centerRight,
            bottomRight
        };

        // var collisionListeners = new Vector2[] {
        //     new Vector2(boxBounds.center.x, boxBounds.center.y + boxBounds.extents.y),
        //     boxBounds.center,
        //     new Vector2(boxBounds.center.x, boxBounds.center.y - boxBounds.extents.y),
        // };

        int closestHitRayIndex = -1;
        for (int i = 0; i < collisionListeners.Length; i++)
        {
            print("i => " + i);
            if (closestHitRayIndex == -1)
            {
                closestHitRayIndex = i;
            }
            else
            {
                var lastOrigin = collisionListeners[closestHitRayIndex];
                var lastHitPoint = HitPosition(lastOrigin);
                var lastDistance = Vector2.Distance(lastOrigin, lastHitPoint);

                var origin = collisionListeners[i];
                var nextHitPoint = HitPosition(origin);
                var distance = Vector2.Distance(origin, nextHitPoint);
                if (lastDistance > distance)
                {
                    closestHitRayIndex = i;
                }
            }
        }

        for (int i = 0; i < collisionListeners.Length; i++)
        {
            var color = i == closestHitRayIndex ? Color.red : Color.black;
            Debug.DrawLine(collisionListeners[i], HitPosition(collisionListeners[i]), color);
        }


        // Debug.DrawLine(top, trajectories[trajectories.Length - 1]);
        // Debug.DrawLine(boxBounds.center, trajectories[trajectories.Length - 1]);
        // Debug.DrawLine(bottom, trajectories[trajectories.Length - 1]);

        // DispayCollisionPoint(top);
        // DispayCollisionPoint(boxBounds.center);
        // DispayCollisionPoint(bottom);
    }












    // private void DispayCollisionPoint(Vector2 collisionPoint, Color color)
    // {
    //     Debug.DrawLine(transform.position, collisionPoint, color);
    // }
    // private void PredictCollision()
    // {   //- https://www.youtube.com/watch?v=ITsynQy5APg
    //     var time = maxTimeY();
    //     float x = rb.velocity.x * time;
    //     float y = (rb.velocity.y * time) - (gameGravity * Mathf.Pow(time, 2) / 2);

    //     Vector2 collisionPoint = new Vector2(x + transform.position.x, y + transform.position.y);
    //     // Debug.DrawLine(transform.position, collisionPoint, Color.green);

    //     // float x = transform.position.x + rb.velocity.x * Time.fixedDeltaTime;
    //     // float y = transform.position.y + rb.velocity.y * Time.fixedDeltaTime - ((-9 * Time.fixedDeltaTime * Time.fixedDeltaTime) / 2);
    //     // Debug.DrawLine(transform.position, new Vector2(x, y), Color.green);
    //     // Debug.DrawLine(transform.position, new Vector2(0, 0), Color.black);
    // }

    // private Vector2[] RayCastCharacterPath()
    // {
    //     Vector2[] steps = new Vector2[31];
    //     for (int i = 0; i < steps.Length; i++)
    //     {
    //         // RaycastHit2D hit = Physics2D.Linecast()
    //     }
    // }

    // private float maxTimeY()
    // {
    //     float velocityYPow2 = Mathf.Pow(rb.velocity.y, 2);
    //     return (rb.velocity.y + Mathf.Sqrt(velocityYPow2 + 2 * gameGravity * (transform.position.y - -8 /**yLimit*/))) / gameGravity;
    // }

    // private float maxTimeX() {
    //     var x = rb.velocity.x;

    //     var t = (PredictCollision().x - transform.position.x) / x;
    // }

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