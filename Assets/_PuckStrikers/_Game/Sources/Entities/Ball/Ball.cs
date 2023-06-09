using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;
using Masomo.ArenaStrikers.Config;
using System.ComponentModel;

[RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
   
    public class Ball : MonoBehaviour
    {

        [SerializeField] Transform Player;
        [SerializeField] Transform playerBallPosition;
        [SerializeField] BallConfig config;
        [SerializeField] ParticleSystem SpawnParticle;
        [SerializeField] ParticleSystem GoalParticle;
        [SerializeField] ParticleSystem PowerShotParticle;
        [SerializeField] Transform BallSpawnPoint;
        [SerializeField] PowerShot powershot;
        public bool StickPlayer;
        private bool RespawnBall;
        private Vector3 previousLocation;
        private Rigidbody _rigidbody;
        private SphereCollider _collider;
        private float _maxSpeed;
        private float _friction;
        private float _bounciness;
        private float _mass;
        private float _radius;
        private Vector3 _velocity;
        private readonly Vector3 _zeroVector = new Vector3(0, 0, 0);
        private const float SquareMagnitudeEpsilon = .1f;
        private const string ReflectableTag = "Reflectable";
        private Vector3 lastvelocity;
        private   PhysicMaterial BallPyhsic;
     
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<SphereCollider>();
            BallPyhsic = _collider.material;
            Initialize(config);
        }

       public void Initialize(BallConfig config)
        {
            _maxSpeed = config.MaxSpeed;
            _friction = config.Friction;
            _bounciness = config.Bounciness;
            _mass = config.Mass;
            _radius = config.Radius;
            _rigidbody.mass = _mass;
            _collider.radius = _radius;
        }
        
        private void FixedUpdate()
        {
            if (CheckBallPos())
            {
                this.gameObject.transform.position = Vector3.zero;
            }
        }
        private void Update()
        {
      
            _rigidbody.velocity = _rigidbody.velocity / SROptions.Current.Friction;
            if (powershot.GetCanUse())
            {
                PowerShotParticle.gameObject.SetActive(true);
            }
            else if(PowerShotParticle.gameObject.activeSelf && !powershot.GetCanUse())
            {
                PowerShotParticle.gameObject.SetActive(false);
            }

    
        if (StickPlayer)
        {
            Dribble();
        }
            _rigidbody.velocity = Vector3.ClampMagnitude(_rigidbody.velocity, _maxSpeed);
    }

    private void LateUpdate()
    {
        
        BallPyhsic.bounciness = SROptions.Current.bounciness;
  
        BallPyhsic.dynamicFriction = SROptions.Current.Friction;
        _maxSpeed = SROptions.Current.MaxSpeed;
        _rigidbody.mass = SROptions.Current.Mass;

        lastvelocity = _rigidbody.velocity;
    }
    public void Dribble()
    {

        /*Vector2 currentLocation = new Vector2(transform.position.x, transform.position.z);
        float speed = Vector2.Distance(currentLocation, previousLocation) / Time.deltaTime;
        transform.Rotate(new Vector3(Player.right.x, 0, Player.right.z), speed, Space.World);
        previousLocation = currentLocation;*/
        transform.position = playerBallPosition.position;

    }
    public void SetPlayer(Transform go)
    {
        Player = go;
      
    }
    public GameObject GetPlayer()
    {
        if(Player == null)
        {
            return null;
        }
        else
        {

            return Player.gameObject;
        }

    }
    public void SetPlayerBallPosition(Transform pos)
    {
        playerBallPosition = pos;
       
    }
    public Transform GetBallLocation()
    {
        return playerBallPosition;
    }
    private bool CheckBallPos()
    {
        bool respawnBall = false;
        if (this.gameObject.transform.position.z < -215f || this.gameObject.transform.position.z > 215f || this.gameObject.transform.position.x > 330f || this.gameObject.transform.position.x < -330f)
        {
            respawnBall = true;
        }
        return respawnBall;
        
    }
    public IEnumerator Show()
        {
            this.GetComponent<Rigidbody>().velocity = Vector3.zero;
            this.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            this.transform.position = BallSpawnPoint.position;
            SpawnParticle.Play();
            yield return new WaitForSeconds(SpawnParticle.main.duration);
            gameObject.SetActive(true);
        }
    public IEnumerator Hide( GameObject goal,float wait)
        {
            this.GetComponent<MeshRenderer>().enabled = false;
            yield return new WaitForSeconds(wait);
            this.GetComponent<MeshRenderer>().enabled = true;
            GameManager.instance.Score(goal);
            gameObject.SetActive(false);
        }  
    private void Reflect(Collision collision)
    {
        var normal = collision.contacts[0].normal;
     
        _velocity = Vector3.Reflect(lastvelocity, normal)*_bounciness;
       
        _velocity.y = 0f;
        _rigidbody.velocity = _velocity;
        _rigidbody.angularVelocity = _zeroVector;

    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (other.gameObject.CompareTag("GoalLine"))
        {
      
            GoalParticle.transform.position = transform.position;
            GoalParticle.Play();
            StartCoroutine(Hide(other.gameObject,GoalParticle.main.duration));
        
        }
        else if (other.gameObject.CompareTag("Player"))
        {
            SetPlayer(other.gameObject.transform);
            SetPlayerBallPosition(other.GetComponent<PlayerController>().BallLocation);
            GameManager.instance.shoottakenfalse();
            StickPlayer = true;
        }
        else if (other.gameObject.CompareTag("enemy"))
        {
            SetPlayer(other.gameObject.transform);
            SetPlayerBallPosition(other.GetComponent<Enemy>().BallLocation);
            GameManager.instance.shoottakenfalse();
            StickPlayer = true;

        }
        else if (other.gameObject.CompareTag("OwnGoalLine"))
        {
            _rigidbody.velocity = Vector3.zero;
            GoalParticle.transform.position = transform.position;
            GoalParticle.Play();
            StartCoroutine(Hide(other.gameObject, GoalParticle.main.duration));
        }
    }
    private void OnTriggerStay(Collider other)
    {
       
        if (!StickPlayer)
        {
            if (other.gameObject.CompareTag("Player") )
            {
                SetPlayer(other.gameObject.transform);
                SetPlayerBallPosition(other.GetComponent<PlayerController>().BallLocation);
                GameManager.instance.shoottakenfalse();
                StickPlayer = true;

            }
            else if( other.gameObject.CompareTag("enemy"))
            {
                SetPlayer(other.gameObject.transform);
                SetPlayerBallPosition(other.GetComponent<Enemy>().BallLocation);
                GameManager.instance.shoottakenfalse();
                StickPlayer = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("enemy"))
        {
            SetPlayer(null);
            SetPlayerBallPosition(null);
            GameManager.instance.shoottakentrue();
            StickPlayer = false;

        }
 
    }
    private void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.CompareTag("Wall"))
        {
           

            Reflect(collision);
         
            
         
        }
        if (collision.gameObject.CompareTag("GoalLine"))
        {

            GoalParticle.transform.position = transform.position;
            GoalParticle.Play();
            StartCoroutine(Hide(collision.gameObject, GoalParticle.main.duration));
        }
        else if (collision.gameObject.CompareTag("OwnGoalLine"))
        {
            _rigidbody.velocity = Vector3.zero;
            GoalParticle.transform.position = transform.position;
            GoalParticle.Play();
            StartCoroutine(Hide(collision.gameObject, GoalParticle.main.duration));
        }
    }
}

public partial class SROptions
{
    private float _Bounciness = 0.9f;
    private float _Friction = 1.001f;
    private float _MaxSpeed = 1000f;
    private float  _Mass= 0.1f;
   
   
    [Category("Bounciness")]
    [Increment(0.050f)]
    public float bounciness
    {
        get { return _Bounciness; }
        set { _Bounciness = value; }
    }

    [Category("Friction")]
    [Increment(0.001f)]

    public float Friction
    {
        get { return _Friction; }
        set { _Friction = value; }
    }
    [Category("MaxSpeed")]
    [Increment(1.0f)]
    public float MaxSpeed
    {
        get { return _MaxSpeed; }
        set { _MaxSpeed = value; }
    }
    [Category("Mass")]
    [Increment(0.10f)]
    public float Mass
    {
        get { return _Mass; }
        set { _Mass = value; }
    }

}

