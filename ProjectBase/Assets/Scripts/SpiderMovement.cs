using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderMovement : MonoBehaviour
{
    public bool IsGrounded;
    [SerializeField] private float _speed = 40f;
    [SerializeField] private float _gravity = 20f;
    //place these four coordinates by their name (fl = front left etc.)
    //put y a little above the object origin (like ~0.05f, but make sure smaller than ground check distance)
    [SerializeField] private Transform _rayObjfl;
    [SerializeField] private Transform _rayObjfr;
    [SerializeField] private Transform _rayObjbl;
    [SerializeField] private Transform _rayObjbr;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundCheckDistance = 0.1f;
    private RaycastHit[] hitResultfl;
    private RaycastHit[] hitResultfr;
    private RaycastHit[] hitResultbl;
    private RaycastHit[] hitResultbr;

    private void Update()
    {
        //give the spider boid behavior to avoid walls and obstacles while moving towards player
    }

    private void FixedUpdate()
    {
        
    }

    private void DetectSurface()
    {

        //use average of front r-l and back r-l to get a right vector and same for front and store it for later time for smooth transition
        int hitsfl = Physics.RaycastNonAlloc(_rayObjfl.position, -transform.up, hitResultfl, _groundCheckDistance, _groundLayer);
        int hitsfr = Physics.RaycastNonAlloc(_rayObjfr.position, -transform.up, hitResultfr, _groundCheckDistance, _groundLayer);
        int hitsbr = Physics.RaycastNonAlloc(_rayObjbr.position, -transform.up, hitResultbr, _groundCheckDistance, _groundLayer);
        int hitsbl = Physics.RaycastNonAlloc(_rayObjbl.position, -transform.up, hitResultbl, _groundCheckDistance, _groundLayer);
        Vector3 rightVec = transform.right;
        Vector3 forwardVec = transform.forward;
        if (hitsbl >= 1 && hitsfl >= 1 && hitsbr >= 1 && hitsfr >= 1)
        {
            rightVec = ((hitResultfr[0].point - hitResultfl[0].point).normalized + (hitResultbr[0].point - hitResultbl[0].point).normalized) / 2;
            forwardVec = ((hitResultfr[0].point - hitResultbr[0].point).normalized + (hitResultfl[0].point - hitResultbl[0].point).normalized) / 2;
            //these are the target forward/right vectors that would go into the spider SetLookRotation
        }
        //check grounding but using transform downward vector
        IsGrounded = (hitsbl >= 1 || hitsbr >= 1 || hitsfl >= 1 || hitsfr >= 1);
    }

   
}
