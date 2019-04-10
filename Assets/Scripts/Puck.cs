using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puck : MonoBehaviour
{
    public Queue<Player> lstPlayerLastTouch;
    public Rigidbody rb;

    Player plrCurrent = null;

    // Start is called before the first frame update
    void Awake()
    {
        lstPlayerLastTouch = new Queue<Player>();
        rb = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider collision)
    {
        Controls c = collision.gameObject.GetComponent<Controls>();
        if (c != null)
        {
            if (plrCurrent != null)
            {
                rb.isKinematic = false;
                transform.parent = null;
                Vector3 v3 = (c.transform.position + plrCurrent.controls.transform.position) / 2;
                rb.velocity = (transform.position - v3) * 40;
                plrCurrent = null;
                rb.isKinematic = false;
                return;

            }

            c.ObtainPuck(this);

            rb.velocity = new Vector3();
            rb.isKinematic = true;

            plrCurrent = c.getPlayer();
            lstPlayerLastTouch.Enqueue(c.getPlayer());
            if (lstPlayerLastTouch.Count > 2)
            {
                lstPlayerLastTouch.Dequeue();
            }
        }
    }
}
