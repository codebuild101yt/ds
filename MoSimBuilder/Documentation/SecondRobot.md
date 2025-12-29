# Second Robot

## Applying Heirarchy and The Game Piece system

* we will be making a robot inspired by the Ri3d team cranbery alarm. This allows us to explore Heirarchy and the game piece system.
* Start by creating a prefab and robot with a 28by28 frame, and bumper height set to 2.5 so that it is just off the ground. I will name the prefab 9998.
* Create an empty object and name it Intake.
* Add an Arm Generator to it.
* Set the Rotation of the Intake Object to -90,0,0
* Set Height to 12, leave width at 14.
* Set stow angle to -60
* Set limits of -65 and 125. Limits are not required and can be left at 0,0 (min, max). These simply prevent the arm from traveling the fastest path if that path is through another system.
    * NOTE: LIMITS are extremely unstabel and ofter dont work (this is an alpha afterall)
* Set Control type to Hold, set the setpoint to 120, button to Lt,
* Move it to the rear of the robot.
* Set up and Intake, Stow, and Outake, on the arm all as childs of ArmSec1. Intake on Lt, outake on Rt, outake speed of 5, direction forward.
* Save and test, dont forget to change the robot Name in the spawn controller.
![image](https://github.com/user-attachments/assets/d7c44442-c9d4-4aff-8cf1-b227876b8933)


* as part of the attempts to make this stable and easy to use, there is no intercollision with internal robot parts. so pay atention and feel free to adjust the intake so that it doesnt collide with the robot frame/bumper.
* Now, create another intake, Stow and Outake, on the main object. this will be the shooter. a stow angle of -53 works.
* Set the stow size to 14,2,5, set Intake and Outake same as the intake.
* Now set the Intake Type on the Intake to always. and the Transfer type on Stow to instant, and the outake speed to 15
* We are creating two independant systems not because we have to but because it is a better representation. By modeling the handof as opposed to simply transfering in between the two stows.
![image](https://github.com/user-attachments/assets/12e77a56-40ae-4d18-bd72-1d52a689e008)

* This is the entire robot. feel free to play around. Im sure if you play long enough with the intake dimensions and settings it could amp.

## [Third Robot](https://github.com/masonmm3/MoSimBuilder/blob/Stable/Documentation/ThirdRobot.md)
