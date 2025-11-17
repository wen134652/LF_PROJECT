using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throwable : MonoBehaviour
{
    Rigidbody2D rb;
    Collider2D col;

    [Header("Fly speed")]
    public float speed = 10.0f;

    [Header("Turn into trigger after land?")]
    public bool turnIntoTrigger = true;

    bool hasLanded = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }
    public void ThrowOut(Vector2 dir)
    {
        if (col)
        {
            col.enabled = true;
        }
        if (true)
        {
            col.isTrigger = false;
            rb.isKinematic = false;
            Vector2 v = dir.normalized * speed;
            rb.velocity = v;
            rb.angularVelocity = Random.Range(1f, 5f);
        }
        
    }
    //落地以后切换状态
    void LandAndSwitch()
    {
        hasLanded = true;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;
        rb.isKinematic = true;
        if (turnIntoTrigger)
        {
            col.isTrigger = true;
        }
        //若落地后可交互，则启动实现了iinteractable的组件
        var all = GetComponents<MonoBehaviour>();
        for (int i = 0; i < all.Length; i++)
        {
            var a = all[i];
            if (a!=null&&a is IInteractable&& a is Behaviour b)
            {
                b.enabled = true;
                break;
            }
        }
        //this.enabled = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 3)
        {
            LandAndSwitch();
        }
    }
}
