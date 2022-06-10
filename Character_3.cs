
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

    //* ------------------------
    //* Public
    //* ------------------------

    public Rigidbody2D rb;
    public float speed;
    public float JumpTimeLimit;
    public float jumpPower;
    public float gravitationalPull;

    void Start()
    {
        animator = GetComponent<Animator>();
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
            float correctedSpeed = isJumping ? speed / 2 : speed;
            // Debug.Log("movement => " + movement);
            transform.Translate(Vector2.right * movement * Time.deltaTime * correctedSpeed);
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
        if (/**isAnchored &&*/ Input.GetButtonDown("Jump"))
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
        if (Physics.Raycast(transform.position, collisionPoint, out hit, 50))
        {
            transform.rotation = Quaternion.LookRotation(Vector3.Cross(transform.TransformDirection(Vector3.right), hit.normal));
        }
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
