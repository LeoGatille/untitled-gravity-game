using System;
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
    private Rigidbody2D rb;
    private Vector2 gravityForce;

    private Vector2 cutDirection;


    //* ------------------------
    //* Public
    //* ------------------------

    public float speed;
    public float JumpTimeLimit;
    public float maxJumpVelocity;
    public float gravitationalPull;
    public float yLimit;
    public LayerMask canHit;
    public float moveSpeed;
    public float maxSpeed;
    public float jumpPower;

    //* ------------------------
    //* Debug
    //* ------------------------

    public bool useAttraction;
    public bool useRotation;

    public float detectionDistance;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        gameGravity = Mathf.Abs(Physics2D.gravity.y);
    }

    void Update()
    {


        if (isAnchored && Input.GetKeyDown(KeyCode.P))
        {
            CutStuff();
            print("Key down: P");
        }

        Debug.DrawRay(transform.up, Vector3.zero - transform.up);
        ShowDirection();
        RecordJump();
        CalculateFuturePosions(transform.position);

        if (Input.GetKeyDown(KeyCode.B))
        {
            rb.velocity = Vector3.zero;
            transform.position = new Vector2(0, 0);
            transform.rotation = new Quaternion(0, 0, 0, 0);
        }
        if (useRotation)
        {
            CastCollider();
        }
        GetNextCollisionPoint();
        //! Need to add force at the end of all computation to remove parasite forces 
        //! sutch as the gravityPull falsing the jump's velocity computation 
        //! (actually their no way to see any difference beteween gravityForce or a Translation froce)
    }

    void FixedUpdate()
    {
        Translate(Input.GetAxis("Horizontal"));


        if (isJumping)
        {
            jumpTime += Time.deltaTime;
            Jump();
        }

        // if (isAnchored && !isJumping)
        // {
        //     // rb.gravityScale = 0;
        //     if (useAttraction)
        //         PlateformAttraction();
        // }
        // else
        // {
        //     // rb.gravityScale = 1;
        // }
        if (!isAnchored && !isJumping)
        {
            rb.AddForce(new Vector2(0, -20));
        }
        // MatchPlateformFloorAngle();





    }


    //* ------------------------
    //* Translation
    //* ------------------------
    private Vector2 GetInputDirection()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        return Quaternion.Euler(0, 0, 180) * new Vector2(x, y);
    }
    private void ShowDirection()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        Vector2 realDirection = Quaternion.Euler(0, 0, 180) * new Vector2(x, y);

        //* Juste have to shot a ray & make sure the arrow won't hit the current gravityAncor
        Debug.DrawLine(transform.position, VectrorConvertor.v3ToV2(transform.position) - realDirection, Color.green);
    }

    private void Translate(float movement)
    {

        if (movement != 0)
        {
            bool goesRight = movement > 0;
            float velocityModifier = isAnchored ? 2 : 1;
            Vector2 velocityTmp = rb.velocity;
            Vector2 velocityToAdd = new Vector2(movement * moveSpeed / velocityModifier, 0);

            bool isUnderVelocityLimits = goesRight ? velocityTmp.x + velocityToAdd.x <= maxSpeed : velocityTmp.x + velocityToAdd.x >= maxSpeed * -1;

            if (isUnderVelocityLimits)
            {
                rb.AddForce(velocityToAdd, ForceMode2D.Impulse);
                animator.SetBool("isWalking", true);
            }

            if (isFacingRight != goesRight)
            {
                Flip();
            }
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
        if (isAnchored && Input.GetButton("Jump"))
        {
            isJumping = true;
            jumpTime = 0;

            //! Legacy animation management
            animator.SetBool("isGrounding", false);
            animator.SetBool("isJumping", isJumping);
        }

        if (Input.GetButtonUp("Jump") | jumpTime > JumpTimeLimit)
        {
            isJumping = false;
            animator.SetBool("isJumping", isJumping);
        }
    }

    private void Jump()
    {
        Func<float, float, bool> isVelocityValid = (velocity, limit) => velocity > 0 ? velocity <= limit : velocity >= limit * -1;

        Vector2 direction = GetInputDirection();
        Vector2 velocity = rb.velocity;
        float maxVelocityXToAdd = velocity.x - direction.x;
        float maxVelocityYToAdd = velocity.y - direction.x + jumpPower;

        // bool isVXValid = isVelocityValid(velocity.x + maxVelocityXToAdd, maxSpeed);
        // bool isVYValid = isVelocityValid(velocity.y + maxVelocityYToAdd, maxJumpVelocity);

        // bool isVXValid = isVelocityValid(velocity.x + maxVelocityXToAdd, maxSpeed);
        bool isVYValid = isVelocityValid(velocity.y + jumpPower, maxJumpVelocity);



        //! Nice concept to always reach the max but needs more thinking
        // float velocityXToAdd = isVXValid ? maxVelocityXToAdd : maxVelocityXToAdd - velocity.x;
        // float velocityYToAdd = isVYValid ? maxVelocityYToAdd : maxVelocityYToAdd - velocity.y;

        // print("X valid : " + isVXValid + " Y valid : " + isVYValid);

        Vector2 force = new Vector2(0, isVYValid ? jumpPower : 0);

        // if (isAnchored)
        // {
        //     //     rb.AddForce(gravityForce * -1, ForceMode2D.Force); //* Cancel Gravity pull parasites forces 
        //     rb.velocity = Vector3.zero;
        // }


        rb.AddForce(force, ForceMode2D.Impulse);
    }

    //* ------------------------
    //* Physic
    //* ------------------------

    private void PlateformAttractionV2(RaycastHit2D hit)
    {
        var direction = Quaternion.Euler(0, 0, 180) * (hit.point - hit.normal);
        rb.AddForce(hit.normal * gravitationalPull * -1, ForceMode2D.Force);
    }

    private void MatchPlateformFloorAngleV2(RaycastHit2D hit)
    {
        Vector3 direction = new Vector3(hit.point.x - hit.normal.x, hit.point.y - hit.normal.y, 0);

        Vector3 toto = new Vector3(transform.up.x, transform.up.y, 0);
        Vector3 tata = new Vector3(hit.normal.x, hit.normal.y, 0);

        var rotation = Quaternion.FromToRotation(toto, tata) * transform.rotation;


        // int frameRate = Application.targetFrameRate;

        var distance = Vector3.Distance(hit.point, transform.position); //Vector3.Distance(hit.collider.bounds.center, hit.point) + Vector3.Distance(hit.collider.bounds.center, transform.position);

        var delta = distance / Vector3.Distance(transform.position, rb.velocity);


        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, delta);


        // transform.rotation = Quaternion.FromToRotation(toto, tata) * transform.rotation;
    }

    //* ------------------------
    //* Collision detection
    //* ------------------------
    private void CastCollider()
    {
        Vector2 size = GetComponent<BoxCollider2D>().size;
        Vector2 nextFramePosition = CalculatePositionPoint(MaxTimeY(transform.position) / 2, transform.position);
        float distance = Vector2.Distance(transform.position, nextFramePosition);

        RaycastHit2D hit = Physics2D.BoxCast(transform.position, size, Quaternion.Angle(Quaternion.identity, transform.rotation), nextFramePosition - VectrorConvertor.v3ToV2(transform.position), detectionDistance, canHit);
        RaycastHit2D feetHit = Physics2D.BoxCast(transform.position, size, Quaternion.Angle(Quaternion.identity, transform.rotation), Vector3.zero - transform.up, 5, canHit);
        if (hit.collider != null)
        {
            MatchPlateformFloorAngleV2(hit);
        }

        if (feetHit.collider != null)
        {
            cutDirection = feetHit.normal;
            isAnchored = true;
            if (!isJumping)
            {
                cutDirection = Vector2.zero;
                PlateformAttractionV2(feetHit);
            }
        }
        else
        {
            isAnchored = false;
        }
    }

    private Vector2 GetNextFramePosition(Vector2 origin)
    {
        float timeValue = MaxTimeX(origin) / 30 * 2;
        return CalculatePositionPoint(timeValue, origin);
    }

    private Vector2[] CalculateFuturePosions(Vector2 origin)
    {
        Vector2[] positions = new Vector2[31];

        float lowestTimeValue = MaxTimeX(origin) / 30;

        for (int i = 0; i < positions.Length; i++)
        {
            float currentTimeValue = lowestTimeValue * i;
            positions[i] = CalculatePositionPoint(currentTimeValue, origin);
            Debug.DrawLine(i == 0 ? transform.position : positions[i - 1], positions[i]);
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
        float lowestTimeValue = MaxTimeY(origin) / 40;

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

    private Vector2[] GetCollisionPrediction()
    {
        var boxBounds = GetComponent<Collider2D>().bounds;

        var topLeft = new Vector2(boxBounds.center.x - boxBounds.extents.x, boxBounds.center.y + boxBounds.extents.y);
        var centerLeft = new Vector2(boxBounds.center.x - boxBounds.extents.x, boxBounds.center.y);
        var bottomLeft = new Vector2(boxBounds.center.x - boxBounds.extents.x, boxBounds.center.y - boxBounds.extents.y);

        var topRight = new Vector2(boxBounds.center.x + boxBounds.extents.x, boxBounds.center.y + boxBounds.extents.y);
        var centerRight = new Vector2(boxBounds.center.x + boxBounds.extents.x, boxBounds.center.y);
        var bottomRight = new Vector2(boxBounds.center.x + boxBounds.extents.x, boxBounds.center.y - boxBounds.extents.y);


        //- https://gamedev.stackexchange.com/questions/128833/how-can-i-get-a-box-colliders-corners-vertices-positions


        return new Vector2[] {
            topLeft,
            centerLeft,
            bottomLeft,
            topRight,
            centerRight,
            bottomRight
        };

    }

    //! Rename this since it returns the closet point from the collision
    private Vector2 GetNextCollisionPoint()
    {
        Vector2[] collisionListeners = GetCollisionPrediction();

        int closestHitRayIndex = -1;
        for (int i = 0; i < collisionListeners.Length; i++)
        {
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
        return collisionListeners[closestHitRayIndex];
    }

    //* ------------------------
    //* Collision detection
    //* ------------------------

    private void CutStuff()
    {
        Debug.DrawRay(transform.position, cutDirection, Color.red);
        Debug.DrawLine(transform.position, cutDirection * 5, Color.cyan);
    }










}
