using System;
using System.Timers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySpriteCutter;


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
    private Transform gravityAnchorParent;
    private Rigidbody2D rb;
    private Vector2 gravityForce;
    private Vector2[] cutPositions;
    private Vector2 direction;

    private bool lockCutter;

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
    public Transform dashReadyParticules;

    //* ------------------------
    //* Debug
    //* ------------------------

    public bool useAttraction;
    public bool useRotation;

    public float detectionDistance;
#nullable enable

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        gameGravity = Mathf.Abs(Physics2D.gravity.y);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }
        if (isAnchored)
        {
            DebugCutDirection();
        }

        ShowDirection();
        RecordDash();
        RecordJump();
        CalculateFuturePosions(transform.position);

        if (Input.GetKeyDown(KeyCode.B))
        {
            rb.velocity = Vector3.zero;
            transform.position = new Vector2(0, 0);
            transform.rotation = new Quaternion(0, 0, 0, 0);
        }
        if (useRotation && !isDashing)
        {
            CastCollider();
        }
        GetNextCollisionPoint();
    }

    void FixedUpdate()
    {

        if (isDashing)
        {
            if (canDash)
            {
                Dash();
                dashTime += Time.deltaTime;
            }
            else
            {
                slowDownDash();
            }
        }
        else
        {
            Translate(Input.GetAxis("Horizontal"));
        }

        if (isJumping)
        {
            jumpTime += Time.deltaTime;
            Jump();
        }

        if (!isAnchored && !isJumping)
        {
            rb.AddForce(new Vector2(0, -20));
        }
    }


    //* ------------------------
    //* Translation
    //* ------------------------
    private Vector2 GetInputDirection()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        // return Quaternion.Euler(0, 0, 180) * new Vector2(x, y);
        // return Quaternion.Euler(0, 0, 180) * new Vector2(x, y);

        return Vector3.zero - Quaternion.Euler(0, 0, 180) * new Vector2(x, y); // - VectorConvertor.v3ToV2(transform.position);
    }
    private void ShowDirection()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        Vector2 direction = new Vector2(x, y);

        //* Juste have to shot a ray & make sure the arrow won't hit the current gravityAncor
        // Debug.DrawLine(transform.position, VectorConvertor.v3ToV2(transform.position) - realDirection, Color.red);
        // Debug.DrawRay(transform.position, direction * 100, Color.cyan);
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
    //* $ Dash
    //* ------------------------
    private bool canDash = true;
    private bool isDashing = false;
    private float dashTime = 0;
    private float dashRecovetyTimer = 0;
    private Vector2 dashDirection;

    public float maxDashTime;
    public float maxDashSpeed;
    public float dashSpeed;
    public float maxdashRecoveryTime;
    public float dashCutLength;

    private void RecordDash()
    {
        if (canDash)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                GetComponent<Collider2D>().enabled = false;
                if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
                {
                    dashDirection = GetInputDirection().normalized;
                }
                else
                {
                    // ? Should it always be the same direction ?
                    // ? Facing direction seems a bit thought to understand in game so small
                    dashDirection = isFacingRight ? Vector2.right : Vector2.left;
                }
                // rb.mass = 500;
                rb.velocity = Vector3.zero;
                isDashing = true;
                DashCut();
            }
        }
        else
        {
            if (isDashing)
            {
                if (rb.velocity.magnitude <= maxSpeed)
                {
                    // rb.mass = 1;
                    isDashing = false;
                    dashTime = 0;
                }
            }
            else
            {
                dashRecovetyTimer += Time.deltaTime;
                if (dashRecovetyTimer >= maxdashRecoveryTime)
                {
                    canDash = true;
                    dashRecovetyTimer = 0;
                }
            }
        }

        if (dashTime >= maxDashTime)
        {
            canDash = false;
            GetComponent<Collider2D>().enabled = true;
        }
    }

    private void Dash()
    {

        // Debug.DrawLine(transform.position, dashDirection)


        // Vector2 relativeDashDirection = dashDirection - Vector2.zero;

        Vector3 a = Quaternion.Euler(0, 0, 15) * dashDirection * 10;
        Vector3 b = Quaternion.Euler(0, 0, -15) * dashDirection * 10;

        Debug.DrawLine(transform.position, a + transform.position, Color.red);
        Debug.DrawLine(transform.position, b + transform.position, Color.red);


        if (rb.velocity.magnitude < maxDashSpeed)
        {
            rb.AddForce(dashDirection * (dashSpeed * (rb.mass / 2)), ForceMode2D.Impulse);
        }
        if (canDash)
        {
            // DashCut();
            // DashPush();
        }
    }

    private void DashCut()
    {

        // Vector3 cutterA = Quaternion.Euler(0, 0, 15) * dashDirection * 10;
        // Vector3 cutterB = Quaternion.Euler(0, 0, -15) * dashDirection * 10;






        // CutStuff(dashDirection, dashCutLength);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dashDirection, 50, canHit);

        if (hit.collider != null)
        {
            List<SpriteCutterOutput> cutItems = CutStuff(dashDirection, dashCutLength);
            foreach (SpriteCutterOutput item in cutItems)
            {

                GameObject[] pieces = new GameObject[] { item.firstSideGameObject, item.secondSideGameObject };
                Vector2 pos1 = pieces[0].transform.GetComponent<Collider2D>().ClosestPoint(transform.position);
                Vector2 pos2 = pieces[1].transform.GetComponent<Collider2D>().ClosestPoint(transform.position);
                // Vector2 explosionOrigin = pos1 + (pos2 - pos1);

                // Debug.DrawRay(pos1, pos2 - pos1, Color.red, 5f);

                // pieces[1].GetComponent<Rigidbody2D>().AddForce(hit.point * 100000);

                Debug.DrawRay(transform.position, pos1 - VectorConvertor.v3ToV2(transform.position), Color.green, 6f);
                Debug.DrawRay(transform.position, pos2 - VectorConvertor.v3ToV2(transform.position), Color.blue, 3f);

                // print("Pos1 : " + pos1 + " Pos2 : " + pos2);

                foreach (GameObject piece in pieces)
                {
                    // if (piece.GetComponent<BoxCollider2D>().bounds.extents.x <= 10) //! Need more thinking
                    // {
                    //     Destroy(piece);
                    //     Debug.LogWarning("Destroy piece");
                    // }
                    Rigidbody2D pieceRb = piece.GetComponent<Rigidbody2D>();

                    pieceRb.AddExplosionForce(explosionForce: 10000, hit.point, 50, 0.0f, ForceMode2D.Force);
                    // pieceRb.AddForce(hit.point * 100000);

                    // ExplosionForce.AddExplosionForce(pieceRb, explosionForce: 100000, hit.point, 10, 0.0f, ForceMode2D.Force);
                    // pieceRb.AddForce((piece.transform.position - transform.position) * 100000);
                }
            }
        }
        else
        {
            Debug.LogWarning("DashCut.hit.collider = null");
        }
    }


    // private void AddExplosionForce(Rigidbody2D rb, float explosionForce, Vector2 explosionPosition, float explosionRadius, float upwardsModifier = 0.0F, ForceMode2D mode = ForceMode2D.Force)
    // {
    //     var explosionDir = rb.position - explosionPosition;
    //     var explosionDistance = explosionDir.magnitude;

    //     // Normalize without computing magnitude again
    //     if (upwardsModifier == 0)
    //         explosionDir /= explosionDistance;
    //     else
    //     {
    //         // From Rigidbody.AddExplosionForce doc:
    //         // If you pass a non-zero value for the upwardsModifier parameter, the direction
    //         // will be modified by subtracting that value from the Y component of the centre point.
    //         explosionDir.y += upwardsModifier;
    //         explosionDir.Normalize();
    //     }

    //     rb.AddForce(Mathf.Lerp(0, explosionForce, (1 - explosionDistance)) * explosionDir, mode);
    // }

    private void DashPush()
    {

        Debug.DrawRay(transform.position, dashDirection, Color.cyan);
        List<Rigidbody2D> rbAffectedByExplosion = new List<Rigidbody2D>();
        Vector2 explosionOrigin;

        Vector2 size = GetComponent<BoxCollider2D>().size;
        RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, size * 2, Quaternion.Angle(Quaternion.identity, transform.rotation), dashDirection, dashDirection.magnitude, canHit);
        foreach (RaycastHit2D boxHit in hits)
        {
            rbAffectedByExplosion.Add(boxHit.transform.gameObject.GetComponent<Rigidbody2D>());
        }


        RaycastHit2D hit = Physics2D.Raycast(transform.position, dashDirection, dashDirection.magnitude, canHit);
        if (hit.collider != null)
        {
            explosionOrigin = hit.point;
            foreach (Rigidbody2D explodedRb in rbAffectedByExplosion)
            {
                // AddExplosionForce(explodedRb, 100000, explosionOrigin, 10, 0.0f, ForceMode2D.Impulse);
            }
        }
        else
        {
            Debug.LogWarning("DashPush.hit.collider == null");
        }

        //! Legacy (addForce to both cut pieces instead of explosionForce)
        // Vector2 size = GetComponent<BoxCollider2D>().size;
        // Vector2 nextFramePosition = CalculatePositionPoint(MaxTimeY(transform.position) / 2, transform.position);
        // float distance = Vector3.Distance(nextFramePosition, transform.position);
        // RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, size * 2, Quaternion.Angle(Quaternion.identity, transform.rotation), dashDirection, distance, canHit);

        // foreach (RaycastHit2D hit in hits)
        // {
        //     Vector2 collisionPoint = hit.point;
        //     Rigidbody2D hitRb = hit.transform.gameObject.GetComponent<Rigidbody2D>();
        //     hitRb.AddForce((collisionPoint - VectorConvertor.v3ToV2(transform.position)) * 1000, ForceMode2D.Impulse);
        // }
    }

    private void slowDownDash()
    {
        if (rb.velocity.magnitude > maxSpeed)
        {
            print("slow down");
            rb.AddForce(dashDirection * -500);

        }
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

        if ((Input.GetButtonUp("Jump") && (jumpTime > 0 && jumpTime < JumpTimeLimit / 2)) || jumpTime > JumpTimeLimit)
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
        var realJumpPower = jumpTime == 0 ? jumpPower * 2 : jumpPower;
        Vector2 force = new Vector2(0, isVYValid ? realJumpPower : 0);

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

        RaycastHit2D hit = Physics2D.BoxCast(transform.position, size, Quaternion.Angle(Quaternion.identity, transform.rotation), nextFramePosition - VectorConvertor.v3ToV2(transform.position), detectionDistance, canHit);
        RaycastHit2D feetHit = Physics2D.BoxCast(transform.position, size, Quaternion.Angle(Quaternion.identity, transform.rotation), Vector3.zero - transform.up, 5, canHit);
        if (hit.collider != null)
        {
            MatchPlateformFloorAngleV2(hit);
        }

        if (feetHit.collider != null)
        {
            cutPositions = new Vector2[] { feetHit.point, feetHit.normal };
            isAnchored = true;
            if (!isJumping)
            {
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
            // Debug.DrawLine(i == 0 ? transform.position : positions[i - 1], positions[i]);
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







        // for (int i = 0; i < collisionListeners.Length; i++)
        // {
        //     var color = i == closestHitRayIndex ? Color.red : Color.black;
        //     Debug.DrawLine(collisionListeners[i], HitPosition(collisionListeners[i]), color);
        // }

        return collisionListeners[closestHitRayIndex];




        // DispayCollisionPoint(top);
        // DispayCollisionPoint(boxBounds.center);
        // DispayCollisionPoint(bottom);
    }



    //* ------------------------
    //* Cut
    //* ------------------------

#nullable enable
    private List<SpriteCutterOutput> CutStuff(Vector2 cutDirection, float cutLength)
    {
        lockCutter = true;
        Vector2 posV2 = VectorConvertor.v3ToV2(transform.position);
        // Vector2 cutDirection = Vector2.zero - normal;
        Vector2 cutLimit = posV2 + (posV2 + cutDirection + cutDirection - posV2) * cutLength;

        RaycastHit2D[] hits = Physics2D.LinecastAll(transform.position, cutLimit, canHit);
        List<SpriteCutterOutput> outputs = new List<SpriteCutterOutput>();
        foreach (RaycastHit2D hit in hits)
        {
            GameObject go = hit.transform.gameObject;
            Rigidbody2D goRb = go.GetComponent<Rigidbody2D>();
            // goRb.gravityScale = 2;
            // goRb.mass = 0.01f;
            // goRb.AddForce(cutLimit * 10);
            // SpriteCutterOutput output =
            outputs.Add(SpriteCutter.Cut(new SpriteCutterInput()
            {
                lineStart = transform.position,
                lineEnd = cutLimit,
                gameObject = go,
                gameObjectCreationMode = SpriteCutterInput.GameObjectCreationMode.CUT_OFF_COPY, //! Wut ?  
            })
            );


            // List<GameObject> pieces = new List<GameObject>();
            // pieces.Add(output.firstSideGameObject);
            // pieces.Add(output.secondSideGameObject);

            // foreach (GameObject piece in pieces)
            // {
            //     Rigidbody2D pRb = piece.GetComponent<Rigidbody2D>();
            //     pRb.mass = 50;
            //     pRb.AddForce(Vector2.zero * 10);
            // }


            // lockCutter = false;
            // var dashTimer = new System.Timers.Timer();
            // dashTimer.Interval = 1000;
            // dashTimer.Elapsed += UnlockCutter;
            // dashTimer.AutoReset = false;
            // dashTimer.Enabled = true;

        }
        return outputs;
    }

    private void UnlockCutter(object? source, ElapsedEventArgs e)
    {
        lockCutter = false;
    }

    private void DebugCutDirection()
    {
        List<GameObject> gameObjectToCut = new List<GameObject>();
        Vector2 origin = cutPositions[0];
        Vector2 normal = cutPositions[1];
        Vector2 cutDirection = Vector2.zero - normal;
        Vector2 posV2 = VectorConvertor.v3ToV2(transform.position);



        // Debug.DrawLine(origin, origin + cutDirection, Color.red);

        // Normal from postion through obj
        // Debug.DrawLine(transform.position, posV2 + (posV2 + cutDirection + cutDirection - posV2) * 5, Color.blue);

        // Debug.DrawLine(Vector3.zero, origin + cutDirection, Color.magenta);
    }
}
