using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDamageAnim : MonoBehaviour
{
    private Animator anim;
    [SerializeField] private float turnSpeed;

    [SerializeField] private List<EyeLaser> laserEffects;
    [SerializeField] private Vector3 laserTargetOffset = new Vector3(0, 10, 0);

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        Transform boss = ((BossGameController)GameController.Instance).spawnedBoss?.transform;

        Vector3 targetDir = new Vector3(0, 0, -1);
        if (boss && anim.GetBool("Attacking")) 
        {
            targetDir = (boss.position - transform.position).normalized;
            targetDir.y = 0;

            laserEffects.ForEach(e => {
                e.line.SetPosition(0, e.effect.transform.position);
                e.line.SetPosition(1, boss.position + laserTargetOffset);
            });
        }

        transform.forward = Vector3.RotateTowards(transform.forward, targetDir, turnSpeed * Mathf.Deg2Rad * Time.deltaTime, 0);
    }

    [ServerCallback]
    public void SetAttacking(bool attacking)
    {
        anim.SetBool("Attacking", attacking);
    }

    public void SetLasers(int lasers) { SetLasers(lasers == 1); }
    public void SetLasers(bool doLasers)
    {
        laserEffects.ForEach(e => {
            e.effect.gameObject.SetActive(doLasers);
            e.line.enabled = doLasers;
        });
    }
}

[System.Serializable]
public struct EyeLaser
{
    public Transform eye;
    public LineRenderer line;
    public ParticleSystem effect;
}