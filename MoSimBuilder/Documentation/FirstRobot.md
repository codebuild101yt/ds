# Creating Your First Robot
### NOTE: Due to the relative complexity of reefscape we will start with examples based on Crescendo.

## Adding a robot
* In the Resources/Robots folder right click the project view.
* Find the Create > menu
* then find the Prefab button in the submenu, click it to create a new prefab.
  <img width="811" height="940" alt="image" src="https://github.com/user-attachments/assets/ad6d0970-9907-4e5d-a1c2-2f8797ad8e89" />
* When you click it should create a new file and allow you to name it, Name it whatever you want as long as it isnt already used. I will name mine 9999 for this example
  ![image](https://github.com/user-attachments/assets/4868b1ca-64b9-44a3-899f-8e5b2914f200)

## Setting up the driveTrain
* double click the prefab you just created.
* Once in it look to the inspector screen, you should see nothing intead of the field scene.
* Make sure to disable auto save which is found in the top right of the scene view to avoid any makor issues. (dont forget to save frequently though)
* To create a basic functional we need to add a GenerateDriveTrain scipt by clicking the top of the heiarchy menu and then looking to the inspector.
  <img width="1920" height="528" alt="image" src="https://github.com/user-attachments/assets/0167693b-0091-4c66-b2bf-b6e5ee11b67c" />
* Next click add Component, this will open the componenet menu. From here we can search for Generate, this will then show several scripts, click on generateDriveTrain to add it to the prefab.
* Several things should change, and you should now see a basic drive train in front of you.
 ![image](https://github.com/user-attachments/assets/9eeae975-c456-4185-944e-90953338829f)
* the inspector will have also changed now featuring the GenerateDriveTrain script, and a number of other componenets which are required for driving.
* Looking at the GerateDriveTrain we just added, it will have a number of parameters for us to fill out, for this example we are going to make the robot frame 27"x27". the wheels 1.5" wide by 4" diameter. A weight of 65lbs, a drive acceleration of 18, and a bumper height of 4 and set the bumper frame offset at the rear to 4". enjoy watching the script change everything for you.
![image](https://github.com/user-attachments/assets/cf7a88cf-8ebf-4d59-9700-4a989ad61be1)



## GamePieces
* right click the 9999 object in the Hierarchy, and select create empty Name it intake.
  <img width="1090" height="532" alt="image" src="https://github.com/user-attachments/assets/31f0eaa8-20d2-4bbc-925c-33f2636d0e28" />
* The blue arrow in the scene view when selecting 9999 indicates the front.
  ![fce78031-84c8-42a7-a3fd-ab431852a162](https://github.com/user-attachments/assets/d09fc19f-4a25-4554-a065-142572a70b4c)
* select the intake object you created and add a generate intake component
* The default size is 0"x0"x0" so we will adjust it to be 27"x6"x4"
* All game peice scripts generate outline boxes to help visualize their location and sizes
* Then move the arrows that are visible when intake is selected to move the object to the rear of the robot and position it to take up the bumper gap
* ![image](https://github.com/user-attachments/assets/2607b233-8431-4fc6-abfd-7e68763ede96)
* next we want to fill out the intake settings. Set the button to Lt and the Intake type to Hold. The intake type controls the behaviour of button actions, set the action delay to 0.15, and click the plus and select note on the intake game pieces so that it can pick up notes.
  
 ![39c41937-5557-42b9-ada6-67ee1d24ff56](https://github.com/user-attachments/assets/c212a419-257c-4736-83dd-c9b6cdf9cb0a)
* Now create another empty object this time named stow.
* add a generate stow component
* Again the default is 0"x0"x0" this time set it to 14"x3"x14", then set the action delay to 0.15
* at the top of the inspector window is the Transform component, all objects have this. it can be used to set percise locations. use it to set the angle to -45,0,0
* Then reposition the stow such that the intake would transfer smoothly into it.
* now return to the Intake object on the generator script is a line called Transfer to Stow, drag the stow object you just made into it. this is the transfer system. it allows you to move objects from an intake to a stow to another stow or outake.
* Now returning to the Stow Object, we want to set the transfer button to Rt, presses to transfer is 0, and transfer type is button.
  ![image](https://github.com/user-attachments/assets/9bddc571-92f2-4dc0-ada5-c8b3f0f8b096)
* now create a outake object with a generateOutake script, Defaults are all 0. The outake is the end of a game pieces travel and as such once it is transfered in the game piece will be rereleased to the world.
* Set the outake size to 14"x3"x2", action delay to 0, OutakeDirection to Forward, speed to 14, and leave the rest at 0.
* Now set the angle to -45 and reposition to the end of the stow. If the dropdowns in the top left of the Scene view are set to Pivot, local, the blue arrow will indicate the direction of travel of the game object.
![image](https://github.com/user-attachments/assets/9ffd63d8-25e5-468e-9c31-95fedd8617bd)

* Now return to the stow object and drag the Outake Object to the endpoint on the generate stow script.
 ![image](https://github.com/user-attachments/assets/50a31568-5bfd-48a2-8d61-8292106d36b1)
* now press the Scenes button in the top left of the scene view just above the pivot drop downs to return to the Field scene. remember to save if it asks.

## Playing with the robot.
* select GameHandler in the Hierarchy and set the Robot Name to 9999
* Below the Robot Spawn controller is the field handler, you may have to click the little arrow next to it to make its settings apear.
* Click the fields setting to open the dropdown and move the OptimizedCrescendo option to the top and the field will swap.
* Click the play button thats above the Scene View.
* The robot should work now, but you may notice something.
* The robot doesnt make shots from the subwoofer.
* Use your previous knowledge to fix that.
* (the fix is to adjust the angle of the stow and outake to -55 then correct their positions.

## Iterating the Design
* The next step is to play with the amp
* To do this we are going to create a new object on the robot named OutakeAmp. we will give it the same position angle and size as the Outake object, delay of 0 and outake direction of forward. Set the Outake Speed to 4.4 and back spin to 4.4
* Now back on stow we are going to add a new endpoint by clicking the plus below the endpoint list. drag outake amp to the new endpoint and the transfer button to Y
* Return and click play and see how it works
![image](https://github.com/user-attachments/assets/b2caebb9-01d2-4073-afc6-b6c7080bfa12)

## Introduction To Heirarchy usage and climbers
* in order to climb we need to understand how the heirarchy works. The heirarchy has two types of object references. Parents, and Childs, a parent is an object that has objects inside its "folder". A child, is the objects inside the folder.
* For instance in the photo below. ArmSec1 is a Child of Arm. ArmSec1 is a parent of ClimbElevator(1), and Stationary is a child of ClimbElevator(1).
  
 ![f220fc6e-64ee-47b2-8858-880c89f74bde](https://github.com/user-attachments/assets/959fa39d-081f-4253-b91f-c81e3c923100)

* So why is this important. well, children of an object will follow their parent. so to get a set of colliders to follow our climb elevator we need to make sure to child it to the correct object on the elevator.
* So create an empty object and add an elevator generator.
* This is by far the most complex generator but is mostly self explanatory. You can set the weight of the non moving part of the elevator, and the weight of the moving part. The only od thing is that if you check stow top, you in essance will visually lose a stage
* Returning to our situation we will set the Height to 15 num of stages to one, and width to 3 so that we can align the hook. Feel free to drop it lower to treat it like a telescopic tube it is not easy to line up child parts if you do though.
* set the stationary weight to 5 and the stage weight to 1.
* set the setpoint to 15.
* Now drag it to one side of the shooter/stow body
* Now, open the climb elevator by clicking the arrow next to it. then stationary > Stage 1. Create an empty object by right clicking stage 1 and name it hook
![44ed85d6-ed1a-44e9-b1fc-8f08076020f7](https://github.com/user-attachments/assets/bf430e82-574e-4a8c-a027-83cfbd5a6447)
* Next add a hook generator to the elevator.
* Set the width to 1, stem Height to 17, bride length to 3 and hook Height to 1.
* Then align it with the cross bridge on the elevator, this is the "stage" it is aligned to middle in the editor for ease of spotting.
![image](https://github.com/user-attachments/assets/183d3edb-a91d-465b-a11c-bc1bcbc67103)
* return to the field scene and test.
* The robot likley did not pull of the ground. this is easy to fix, simply click the climbElevator Object the Ctrl C Ctrl V (copy and paste) the game object and drag to the other side. you now have two identical climbers.
![image](https://github.com/user-attachments/assets/041f6cd3-d868-4591-9821-81f481efe1b3)



# [Next step](https://github.com/masonmm3/MoSimBuilder/blob/Stable/Documentation/SecondRobot.md)



