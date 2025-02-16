using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CapsuleCollider))]

public class Zombie : MonoBehaviour
{
	// Settings
	public float damage = 25f;
	private float runSpeed = 3;
	private float walkSpeed = 0.5f;
	private float fieldOfView = 90f;
	private float viewDistance = 10f;
	private float bodyRemovalTime = 5f;
	private float playerSearchInterval = 1f;
	private float minChase = 5f;
	private float maxChase = 10f;
	private float minWander = 15f;
	private float maxWander = 25f;
	private float minSoundInterval = 3f;
	private float maxSoundInterval = 15f;
	private float wanderRadius = 15f;
	private float attackRange = 1f;

	public Transform zombieHead = null; //Pivot point to use as reference, use it as if it were the eyes of the zombie.

	public AudioClip[] idleAudioClips;
	public AudioClip[] chaseAudioClips;
	public AudioClip[] deathAudioClips;

	private bool playerChase = false, wandering = false, eatingBody = false, playedChaseSound = false;
	/*
			 * Will be true when the zombie is chasing the player, then the code will randomize when will stop doing it.
			*/
	private float lastCheck = -1f, lastChaseInterval = -1f, lastWander = -1f, lastSound = -1f;
	/*
			*Last time we checked the player's position.
			*Last or next time we chased the player before losing him.
			*Last time we set a wander position
			*/
	private NavMeshAgent agent;//This zombie's nav mesh agent
	private Animator Anim;//This zombie's animator
	private Health health;
	private Player player = null; //Player position/transform
	private Vector3 lastKnownPos = Vector3.zero;//Last known player position
	private AudioSource audioSource;
	private ZombieAnimKeys animKeys;

	private void Awake()
	{
		animKeys = new ZombieAnimKeys();
		player = Player.instance;

		agent = GetComponent<NavMeshAgent>();
		Anim = GetComponent<Animator>();
		health = GetComponent<Health>();
		audioSource = GetComponent<AudioSource>();

		health.OnDeath.AddListener(OnDeath);
		health.OnTakeDamage.AddListener(ChasePlayer);

		//SET THE MAIN VALUES
		agent.speed = walkSpeed;
		agent.acceleration = runSpeed * 40;
		agent.angularSpeed = 999;
		ResetZombie();
	}

	private void Update()
	{
		/* IN CASE YOU CHANGE YOUR PLAYER GAMEOBJECT OR THE ZOMBIE IS SPAWNINg */
		if (Anim.GetCurrentAnimatorStateInfo(0).IsName("Spawn"))
			return;

		/* ACTUAL ZOMBIE CODE */

		if (Time.time > lastCheck)
		{//SEARCH FOR THE PLAYER AFTER INTERVAL
			CheckView();
			lastCheck = Time.time + playerSearchInterval;
		}

		if (Time.time > lastSound)
		{
			PlayRandomSound();
			lastSound = Time.time + Random.Range(minSoundInterval, maxSoundInterval);
		}

		if (!eatingBody)
		{
			/* PLAYER SEARCH ALGORITHMS */
			if (playerChase && Anim.GetBool(animKeys._isChasing))
			{
				if (Time.time > lastChaseInterval) GotoLastKnown();
				else ChasePlayer();
			}

			//SET THE ATTACK AND RESET IT
			AnimatorStateInfo state = Anim.GetCurrentAnimatorStateInfo(0);
			if (!state.IsName(animKeys._attackState) && playerChase && MeleeDistance())
			{//READY TO ATTACK!
				Anim.SetTrigger(animKeys._attack);
				Anim.SetBool(animKeys._isIdle, false);
				Anim.SetBool(animKeys._isChasing, false);
				Anim.SetBool(animKeys._isWandering, false);

				playerChase = false;
			}
			if (state.IsName(animKeys._attackState) && state.normalizedTime > 0.90f)
			{
				ChasePlayer();
			}
		}
		else
		{
			/* EAT BODIE ALGORITHMS */
			if (ReachedPathEnd())
			{//This means we are in the right position to eat the bodie.
				StartEating();
			}
			else
			{
				FollowBody();//In case or explosions or stuff like that.
			}
		}

		//MAKE THE ZOMBIE WANDER AROUND THE MAP
		if (wandering)
		{
			if (ReachedPathEnd()) ResetZombie();
			if (Time.time > lastWander) GetNewWanderPos();
		}

	}

	//Just to make this code prettier, simplify with functions...
	private void CheckView()
	{
		Vector3 checkPosition = player.transform.position - zombieHead.position;
		float distanceToPlayer = checkPosition.magnitude; // Get distance to player

		// In FOV and close enough
		if (Vector3.Angle(checkPosition, zombieHead.forward) < fieldOfView && distanceToPlayer < viewDistance)
		{
			ChasePlayer();
			lastChaseInterval = Time.time + Random.Range(minChase, maxChase);
		}
		// If really close
		else if (MeleeDistance())
		{
			ChasePlayer();
			lastChaseInterval = Time.time + Random.Range(minChase, maxChase);
		}
	}

	private void GotoLastKnown()
	{
		Anim.SetBool(animKeys._isChasing, false);
		Anim.SetBool(animKeys._isWandering, true);

		Anim.SetBool(animKeys._isIdle, false);
		playerChase = true;
		agent.SetDestination(lastKnownPos);
		agent.isStopped = false;
		agent.speed = walkSpeed;
		wandering = true;
		eatingBody = false;
	}

	private void ChasePlayer()
	{
		if (!playedChaseSound)
			PlayChaseSound();

		Anim.SetBool(animKeys._isChasing, true);
		Anim.SetBool(animKeys._isWandering, false);

		Anim.SetBool(animKeys._isIdle, false);
		playerChase = true;
		agent.SetDestination(player.transform.position);
		lastKnownPos = player.transform.position;
		agent.speed = runSpeed;
		agent.isStopped = false;
		agent.speed = runSpeed;
		wandering = false;
		eatingBody = false;
	}

	private void StopChase()
	{
		Anim.SetBool(animKeys._isChasing, false);
		Anim.SetBool(animKeys._isWandering, false);

		Anim.SetBool(animKeys._isIdle, true);
		playerChase = false;
		agent.isStopped = true;
		wandering = false;
		eatingBody = false;
	}

	private void ResetZombie()
	{
		Anim.SetBool(animKeys._isIdle, true);
		Anim.SetBool(animKeys._isChasing, false);
		Anim.SetBool(animKeys._isEating, false);
		Anim.SetBool(animKeys._isWandering, false);

		playerChase = false;
		agent.isStopped = true;
		agent.speed = walkSpeed;
		wandering = true;
		eatingBody = false;
		playedChaseSound = false;

		audioSource.Stop();
	}

	private void StartEating()
	{
		Anim.SetBool(animKeys._isChasing, false);

		Anim.SetBool(animKeys._isIdle, false);
		Anim.SetBool(animKeys._isEating, true);
		Anim.SetBool(animKeys._isWandering, false);

		playerChase = false;
		agent.SetDestination(player.transform.position);//Just to keep track of it, ignore this.
		agent.isStopped = true;//Won't actually follow the bodie, let's store that for later.
		agent.speed = walkSpeed;
		wandering = false;
		eatingBody = false;
	}

	private void FollowBody()
	{
		Anim.SetBool(animKeys._isChasing, true);

		Anim.SetBool(animKeys._isIdle, false);
		Anim.SetBool(animKeys._isEating, false);
		Anim.SetBool(animKeys._isWandering, false);
		playerChase = false;
		agent.SetDestination(player.transform.position);//In this case "player" will be a dead bodie.
		agent.isStopped = false;//Lets follow it, to prevent mistakes.
		agent.speed = runSpeed;
		wandering = false;
		eatingBody = false;
		//SNDSource.Stop();
	}

	private void GetNewWanderPos()
	{
		// Get random new position
		Vector3 randomDir = Random.insideUnitSphere * wanderRadius + transform.position;
		NavMeshHit hit;
		NavMesh.SamplePosition(randomDir, out hit, wanderRadius, 1);
		Vector3 targetPos = hit.position;

		// Do stuff
		Anim.SetBool(animKeys._isIdle, false);
		Anim.SetBool(animKeys._isWandering, true);

		playerChase = false;
		agent.SetDestination(targetPos);
		agent.isStopped = false;
		agent.speed = walkSpeed;
		lastWander = Time.time + Random.Range(minWander, maxWander);
	}

	private void PlayRandomSound()
	{
		if (playerChase || audioSource.isPlaying) return;

		audioSource.clip = idleAudioClips[Random.Range(0, idleAudioClips.Length)];
		audioSource.Play();
	}

	private void PlayChaseSound()
	{
		audioSource.clip = chaseAudioClips[Random.Range(0, chaseAudioClips.Length)];
		audioSource.Play();

		playedChaseSound = true;
	}

	//ONLY WHEN THE PLAYER AND DEAD BODY VARIABLE ARE NULL
	private void DoWanderFunctions()
	{
		if (wandering)
		{
			if (ReachedPathEnd()) ResetZombie();
			if (Time.time > lastWander) GetNewWanderPos();
		}
	}

	public void CheckHitPlayer()
	{
		if (!MeleeDistance())//DON'T DO ANYTHING IF WE ARE NOT AT A MELEE DISTANCE TO THE PLAYER
			return;

		/*
					 * HERE YOU SET DAMAGE TO PLAYER YOU MUST DO THE CODE BY YOURSELF
					 * REMEMBER THE PLAYER VARIABLE IS ALREADY SET
					*/
		agent.updateRotation = false;
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(agent.destination - transform.position, transform.up), 7.0f * Time.deltaTime);
		agent.updateRotation = true;
		player.health.TakeDamage(damage);
	}

	private void OnDeath()
	{
		GameManager.instance.SpawnAmmoCrate(transform.position);

		audioSource.clip = deathAudioClips[Random.Range(0, deathAudioClips.Length)];
		audioSource.Play();

		Anim.SetTrigger(Random.value > 0.5f ? animKeys._die1 : animKeys._die2);

		agent.enabled = false;
		GetComponent<Collider>().enabled = false;
		Destroy(gameObject, bodyRemovalTime);
		enabled = false;

	}

	//Check if agent reached the player
	private bool ReachedPathEnd()
	{
		if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
		{
			if (!agent.hasPath || agent.velocity.sqrMagnitude == 0.0f)
			{
				return true;
			}
			return true;
		}
		return false;
	}

	private bool MeleeDistance()
	{
		Vector3 offset = player.transform.position - transform.position;
		return offset.sqrMagnitude < attackRange * attackRange;
	}
}

[System.Serializable]
public class ZombieAnimKeys
{
	// Triggers
	public string _attack = "attack";
	public string _die1 = "die1";
	public string _die2 = "die2";

	// Bools
	public string _isIdle = "isIdle";
	public string _isChasing = "isChasing";
	public string _isWandering = "isWandering";
	public string _isEating = "isEating";

	// Anim States
	public string _attackState = "Z_Attack";
}
