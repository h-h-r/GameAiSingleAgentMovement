using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the place to put all of the various steering behavior methods we're going
/// to be using. Probably best to put them all here, not in NPCController.
/// </summary>

public class SteeringBehavior : MonoBehaviour
{

    // The agent at hand here, and whatever target it is dealing with
    public NPCController agent;
    public NPCController target;

    // Below are a bunch of variable declarations that will be used for the next few
    // assignments. Only a few of them are needed for the first assignment.

    // For pursue and evade functions
    public float maxPrediction;
    public float maxAcceleration;

    // For arrive function
    public float maxSpeed;
    public float targetRadiusL;
    public float slowRadiusL;
    public float timeToTarget;

    // For Face function
    public float maxRotation;
    public float maxAngularAcceleration;
    public float targetRadiusA;
    public float slowRadiusA;

    // For wander function
    public float wanderOffset;
    public float wanderRadius;
    public float wanderRate;
    private float wanderOrientation;

    // Holds the path to follow
    public GameObject[] Path;
    public int current = 0;

    protected void Start() {
        agent = GetComponent<NPCController>();
        //painter = GetComponent<NPCController>();
        wanderOrientation = agent.orientation;
    }


    public Vector3 Seek() {
        Vector3 linear_acc = target.position - agent.position; //seek direction vector

        //clip to max linear acceleration
        if (linear_acc.magnitude > this.maxAcceleration){
            linear_acc = linear_acc.normalized * maxAcceleration;
        }

        //clip to max speed is handled in the UpdateMovement in NPCController.cs 
        //angular acceleration will be handled by face()  

        return linear_acc;  //returns the linear acc 
    }

    public Vector3 Flee()
    {
        Vector3 linear_acc = agent.position - target.position;

        //clip to max linear acceleration
        if (linear_acc.magnitude > this.maxAcceleration)
        {
            linear_acc = linear_acc.normalized * maxAcceleration;
        }

        //clip to max speed is handled in the UpdateMovement in NPCController.cs 
        //angular acceleration will be handled by face()

        return linear_acc; 
    }

    // Calculate the angular acceleration required to rotate to target
    public float Face()
    {

        Vector3 direction = target.position - agent.position;

        // Check for a zero direction, and make no change if so
        if (direction.magnitude ==0)
        {
            return 0;
        }

        // Get anount of angle need to rotate
        float rotationAmount = Mathf.Atan2(direction.x, direction.z) - agent.orientation;
        //agent.orientaion range [-inf,inf]

        // clip to (-pi, pi) interval
        while (rotationAmount > Mathf.PI)
        {
            rotationAmount -= 2 * Mathf.PI;
        }
        while (rotationAmount < -Mathf.PI)
        {
            rotationAmount += 2 * Mathf.PI;
        }

        // if already facing target, set angular speed to zero
        if (Mathf.Abs(rotationAmount) < targetRadiusA)
        {
            agent.rotation = 0;
        }

        // greater than slowRadius => clip to max rotation speed
        // less than slowRadius => clip to scaled rotation speed 
        float rotationSpeed = (rotationAmount > slowRadiusA ? maxRotation : maxRotation * Mathf.Abs(rotationAmount) / slowRadiusA);

        // get the correct rotation direction
        rotationSpeed *= rotationAmount / Mathf.Abs(rotationAmount);

        // calculate the rotation acceleration
        float angular_acc= rotationSpeed - agent.rotation;
        angular_acc /= timeToTarget;

        // clip to max angular acc if needed
        if ( Mathf.Abs(angular_acc)> maxAngularAcceleration)
        {
            angular_acc /= Mathf.Abs(angular_acc);
            angular_acc *= maxAngularAcceleration;
        }

        return angular_acc;
    }

    // Calculate the angular acceleration required to rotate to target
    public float FaceAway()
    {
        Vector3 direction =  agent.position - target.position; //only diff with face

        // Check for a zero direction, and make no change if so
        if (direction.magnitude == 0)
        {
            return 0;
        }

        // Get anount of angle need to rotate
        float rotationAmount = Mathf.Atan2(direction.x, direction.z) - agent.orientation;
        //agent.orientaion range [-inf,inf]

        // clip to (-pi, pi) interval
        while (rotationAmount > Mathf.PI)
        {
            rotationAmount -= 2 * Mathf.PI;
        }
        while (rotationAmount < -Mathf.PI)
        {
            rotationAmount += 2 * Mathf.PI;
        }

        // if already facing target, set angular speed to zero
        if (Mathf.Abs(rotationAmount) < targetRadiusA)
        {
            agent.rotation = 0;
        }

        // greater than slowRadius => clip to max rotation speed
        // less than slowRadius => clip to scaled rotation speed 
        float rotationSpeed = (rotationAmount > slowRadiusA ? maxRotation : maxRotation * Mathf.Abs(rotationAmount) / slowRadiusA);

        // get the correct rotation direction
        rotationSpeed *= rotationAmount / Mathf.Abs(rotationAmount);

        // calculate the rotation acceleration
        float angular_acc = rotationSpeed - agent.rotation;
        angular_acc /= timeToTarget;

        // clip to max angular acc if needed
        if (Mathf.Abs(angular_acc) > maxAngularAcceleration)
        {
            angular_acc /= Mathf.Abs(angular_acc);
            angular_acc *= maxAngularAcceleration;
        }

        return angular_acc;
    }



    // wander returns the angular_acc(account for face direction) 
    public float Wander()
//    public float Wander(out Vector3 linear)
    {
        // adjust the initial wanderOrientation with a small random angle
        wanderOrientation += (Random.value - Random.value) * wanderRate;

        // Calculate the combined target orientation
        float orientation = wanderOrientation + agent.orientation;


        // the wander circle center position
        Vector3 position = agent.position + wanderOffset * new Vector3(Mathf.Sin(agent.orientation), 0, Mathf.Cos(agent.orientation));
        agent.DrawCircle(position, wanderRadius);

        // Calculate the wander target 
        position += wanderRadius * new Vector3(Mathf.Sin(orientation), 0, Mathf.Cos(orientation));

        // direction to wander target
        Vector3 direction = position - agent.position;

        // Get the naive direction to the target
        float rotation = Mathf.Atan2(direction.x, direction.z) - agent.orientation;

        //clip to [-pi,pi]
        while (rotation > Mathf.PI)
        {
            rotation -= 2 * Mathf.PI;
        }
        while (rotation < -Mathf.PI)
        {
            rotation += 2 * Mathf.PI;
        }
        float rotationSize = Mathf.Abs(rotation);

        // within targetRadius -> set roration speed to 0
        if (rotationSize < targetRadiusA)
        {
            agent.rotation = 0;
        }

        //calculate desire rotation speed
        float rotationSpeed = (rotationSize > slowRadiusA ? maxRotation : maxRotation * rotationSize / slowRadiusA);

        // apply direction
        rotationSpeed *= rotation / rotationSize;

        // Acceleration tries to get to the target rotation
        float angular_acc = rotationSpeed - agent.rotation;
        angular_acc /= timeToTarget;//angular acc

        // clip angular_acc
        if (Mathf.Abs(angular_acc) > maxAngularAcceleration)
        {
            angular_acc /= Mathf.Abs(angular_acc);
            angular_acc *= maxAngularAcceleration;
        }

        return angular_acc;

    }



    public Vector3 Arrive()
    {
        Vector3 direction = target.position - agent.position;

        if (direction.magnitude <= slowRadiusL)
        {
            agent.label.text = "dynamic arrive\n<In slowRadiusL>";
            //agent.DestroyPoints();
            agent.DrawCircle(target.position, slowRadiusL);
        }
        else
        {
            agent.DestroyPoints();
        }

        // stop if arrive (in target radius) and return zero linear_acc
        if (direction.magnitude < targetRadiusL)
        {
            agent.label.text = "dynamic arrive\n<In targetRadiusL>";
            agent.velocity = Vector3.zero;
            return Vector3.zero;
        }

        //calculate appropriate speed
        float speed = (direction.magnitude > slowRadiusL? maxSpeed : maxSpeed* direction.magnitude / slowRadiusL);
        
        //apply direction
        direction.Normalize();
        Vector3 velocity = direction * speed;

        // calculate linear_acc
        Vector3 linear_acc = (velocity - agent.velocity) / timeToTarget;

        // clip linear_acc
        if (linear_acc.magnitude > maxAcceleration)
        {
            linear_acc.Normalize();
            linear_acc *= maxAcceleration;
        }

        return linear_acc;
    }


    public Vector3 Pursue() {
        float distance = (target.position - agent.position).magnitude;

        // speed scalar
        float speed = agent.velocity.magnitude;

        // if speed small-> use bigger predictionTime
        float predictTime = (speed <= distance / maxPrediction ? maxPrediction : distance / speed);

        //draw prediction circle
        agent.DrawCircle(target.position + target.velocity * predictTime, 0.3f);

        //direction to prediction point
        Vector3 linear_acc = (target.position + target.velocity * predictTime) - agent.position;

        //clip to map linear acc
        linear_acc.Normalize();
        linear_acc *= maxAcceleration;

        return linear_acc;
    }

    public Vector3 Evade() {
        float distance = (target.position - agent.position).magnitude;

        // speed scalar
        float speed = agent.velocity.magnitude;

        // if speed small-> use bigger predictionTime
        float prediction = (speed <= distance / maxPrediction ? maxPrediction : distance / speed);

        //draw prediction circle
        agent.DrawCircle(target.position + target.velocity * prediction, 0.5f);

        //direction to evade prediction point
        Vector3 linear_acc = agent.position - (target.position + target.velocity * prediction);

        //clip to map linear acc
        linear_acc.Normalize();
        linear_acc *= maxAcceleration;

        return linear_acc;
    }

 
    public float Align()
    {

        float rotation = target.orientation - agent.orientation;

        // clip to (-pi, pi) interval
        while (rotation > Mathf.PI)
        {
            rotation -= 2 * Mathf.PI;
        }
        while (rotation < -Mathf.PI)
        {
            rotation += 2 * Mathf.PI;
        }
        float rotationSize = Mathf.Abs(rotation);

        //within targetRadiusA
        if (rotationSize < targetRadiusA)
        {
            agent.rotation = 0;
        }

        //calculate desire rotation speed
        float rotationSpeed = (rotationSize > slowRadiusA ? maxRotation : maxRotation * rotationSize / slowRadiusA);

        // apply direction
        rotationSpeed *= rotation / rotationSize;

        // Acceleration tries to get to the target rotation
        float angular_acc = rotationSpeed - agent.rotation;
        angular_acc /= timeToTarget;//angular acc

        // clip angular_acc
        if (Mathf.Abs(angular_acc) > maxAngularAcceleration)
        {
            angular_acc /= Mathf.Abs(angular_acc);
            angular_acc *= maxAngularAcceleration;
        }

        return angular_acc;
    }


}
