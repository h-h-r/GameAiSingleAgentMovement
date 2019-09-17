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



    // Calculate the target to face
    public float Wander(out Vector3 linear)
    {
        // Update the wander orientation
        wanderOrientation += (Random.value - Random.value) * wanderRate;

        // Calculate the combined target orientation
        float orientation = wanderOrientation + agent.orientation;

        // Calculate the center of the wander circle
        Vector3 position = agent.position + wanderOffset * new Vector3(Mathf.Sin(agent.orientation), 0, Mathf.Cos(agent.orientation));
        agent.DrawCircle(position, wanderRadius);

        // Calculate the target location
        position += wanderRadius * new Vector3(Mathf.Sin(orientation), 0, Mathf.Cos(orientation));

        // Work out the direction to target
        Vector3 direction = position - agent.position;

        // Check for a zero direction, and make no change if so ??
        if (direction.magnitude == 0)
        {
            linear = Vector3.zero;
            return 0;
        }

        // Get the naive direction to the target
        float rotation = Mathf.Atan2(direction.x, direction.z) - agent.orientation;

        // Map the result to the (0, 2pi) interval
        while (rotation > Mathf.PI)
        {
            rotation -= 2 * Mathf.PI;
        }
        while (rotation < -Mathf.PI)
        {
            rotation += 2 * Mathf.PI;
        }
        float rotationSize = Mathf.Abs(rotation);

        // Check if we are there, return no steering
        if (rotationSize < targetRadiusA)
        {
            agent.rotation = 0;
        }

        // If we are outside the slowRadius, then use max rotation
        // Otherwise calculate a scaled rotation
        float targetRotation = (rotationSize > slowRadiusA ? maxRotation : maxRotation * rotationSize / slowRadiusA);

        // The final target rotation combines speed (already in the variable) and direction
        targetRotation *= rotation / rotationSize;

        // Acceleration tries to get to the target rotation
        float angular = targetRotation - agent.rotation;
        angular /= timeToTarget;

        // Check if the acceleration is too great
        float angularAcceleration = Mathf.Abs(angular);
        if (angularAcceleration > maxAngularAcceleration)
        {
            angular /= angularAcceleration;
            angular *= maxAngularAcceleration;
        }

        // Now set the linear acceleration to be at full acceleration in the direction of the orientation
        linear = maxAcceleration * new Vector3(Mathf.Sin(agent.orientation), 0, Mathf.Cos(agent.orientation));

        return angular;
    }


    public Vector3 Arrive()
    {
        // Get the direction to the target
        Vector3 direction = target.position - agent.position;
        float distance = direction.magnitude;

        if (distance <= slowRadiusL)
        {
            agent.label.text = "in!!";
            //agent.DestroyPoints();
            agent.DrawCircle(target.position, slowRadiusL);
        }
        else
        {
            agent.DestroyPoints();
        }

        // Check if we are there, return no steering
        if (distance < targetRadiusL)
        {
            agent.velocity = Vector3.zero;
            return Vector3.zero;
        }

        // If we are outside the slowRadius, then go max speed
        // Otherwise calculate a scaled speed
        float targetSpeed = (distance > slowRadiusL ? maxSpeed : maxSpeed * distance / slowRadiusL);




        // The target velocity combines speed and direction
        direction.Normalize();
        direction *= targetSpeed;

        // Acceleration tries to get to the target velocity
        Vector3 steering = (direction - agent.velocity) / timeToTarget;

        // Check if the acceleration is too fast
        if (steering.magnitude > maxAcceleration)
        {
            steering.Normalize();
            steering *= maxAcceleration;
        }

        return steering;
    }


    // Calculate the target to pursue
    public Vector3 Pursue() {
        // Work out the distance to target
        float distance = (target.position - agent.position).magnitude;

        // Work out our current speed
        float speed = agent.velocity.magnitude;

        // Check if speed is too small to give a reasonable prediction time
        float prediction = (speed <= distance / maxPrediction ? maxPrediction : distance / speed);

        agent.DrawCircle(target.position + target.velocity * prediction, 0.3f);
        // Create the structure to hold our output
        Vector3 steering = (target.position + target.velocity * prediction) - agent.position;

        // Give full acceleration along this direction
        steering.Normalize();
        steering *= maxAcceleration;

        return steering;
    }

    // Calculate the target to evade
    public Vector3 Evade() {
        // Work out the distance to target
        float distance = (target.position - agent.position).magnitude;

        // Work out our current speed
        float speed = agent.velocity.magnitude;

        // Check if speed is too small to give a reasonable prediction time
        float prediction = (speed <= distance / maxPrediction ? maxPrediction : distance / speed);

        agent.DrawCircle(target.position + target.velocity * prediction, 0.5f);
        // Create the structure to hold our output
        Vector3 steering = agent.position - (target.position + target.velocity * prediction);

        // Give full acceleration along this direction
        steering.Normalize();
        steering *= maxAcceleration;

        return steering;
    }

 

   
    // Calculate the target to face
    public float Align()
    {

        float rotation = target.orientation - agent.orientation;
   
        // Map the result to the (0, 2pi) interval
        while (rotation > Mathf.PI)
        {
            rotation -= 2 * Mathf.PI;
        }
        while (rotation < -Mathf.PI)
        {
            rotation += 2 * Mathf.PI;
        }
        float rotationSize = Mathf.Abs(rotation);

        // Check if we are there, return no steering
        if (rotationSize < targetRadiusA)
        {
            agent.rotation = 0;
        }

        // If we are outside the slowRadius, then use max rotation
        // Otherwise calculate a scaled rotation
        float targetRotation = (rotationSize > slowRadiusA ? maxRotation : maxRotation * rotationSize / slowRadiusA);

        // The final target rotation combines speed (already in the variable) and direction
        targetRotation *= rotation / rotationSize;

        // Acceleration tries to get to the target rotation
        float angular = targetRotation - agent.rotation;
        angular /= timeToTarget;//speed

        // Check if the acceleration is too great
        float angularAcceleration = Mathf.Abs(angular);
        if (angularAcceleration > maxAngularAcceleration)
        {
            angular /= angularAcceleration;
            angular *= maxAngularAcceleration;
        }

        return angular;
    }


}
