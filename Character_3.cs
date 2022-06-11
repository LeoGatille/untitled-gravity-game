
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

    void Start()
    {
        animator = GetComponent<Animator>();
        gameGravity = Mathf.Abs(Physics2D.gravity.y);
    }

    void Update()
    {

        Translate(Input.GetAxis("Horizontal"));
        RecordJump();
        PredictCollision();

        if (isAnchored && !isJumping)
        {
            PlateformAttraction();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            transform.rotation = new Quaternion(0, 0, 0, 0);
        }
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
                    transform.Translate(Vector2.right * movement * Time.deltaTime * correctedSpeed);
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
        if (isAnchored && Input.GetButtonDown("Jump"))
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
    private void PredictCollision()
    {   //! https://www.youtube.com/watch?v=ITsynQy5APg
        var time = maxTimeY();
        float x = rb.velocity.x * time;
        float y = (rb.velocity.y * time) - (gameGravity * Mathf.Pow(time, 2) / 2);

        Vector2 collisionPoint = new Vector2(x + transform.position.x, y + transform.position.y);
        Debug.DrawLine(transform.position, collisionPoint, Color.green);

        // float x = transform.position.x + rb.velocity.x * Time.fixedDeltaTime;
        // float y = transform.position.y + rb.velocity.y * Time.fixedDeltaTime - ((-9 * Time.fixedDeltaTime * Time.fixedDeltaTime) / 2);
        // Debug.DrawLine(transform.position, new Vector2(x, y), Color.green);
        Debug.DrawLine(transform.position, new Vector2(0, 0), Color.black);
    }

    private float maxTimeY()
    {
        float velocityYPow2 = Mathf.Pow(rb.velocity.y, 2);
        return (rb.velocity.y + Mathf.Sqrt(velocityYPow2 + 2 * gameGravity * (transform.position.y - -8 /**yLimit*/))) / gameGravity;
    }

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
