using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private int speed = 5;
    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator animator;

    private bool hasXParam;
    private bool hasYParam;
    private bool hasSpeedParam;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        hasXParam = HasParameter("X");
        hasYParam = HasParameter("Y");
        hasSpeedParam = HasParameter("Speed");
    }

    private void OnMovement(InputValue value)
    {
        movement = value.Get<Vector2>();
        if(movement.x != 0 || movement.y != 0)
        {
            if (hasXParam && animator != null) animator.SetFloat("X", movement.x);
            if (hasYParam && animator != null) animator.SetFloat("Y", movement.y);
        }
        if (hasSpeedParam && animator != null) animator.SetFloat("Speed", movement.magnitude);
    }

    private void FixedUpdate()
    {   
        if(PauseController.isGamePaused)
        {
            rb.linearVelocity = Vector2.zero;
            if (hasSpeedParam && animator != null) animator.SetFloat("Speed", 0f);
            return;
        }
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
        if (hasSpeedParam && animator != null) animator.SetFloat("Speed", movement.magnitude);
    }

    private bool HasParameter(string paramName)
    {
        if (animator == null) return false;
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }
}
