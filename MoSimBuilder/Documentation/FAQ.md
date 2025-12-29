# FAQ
### how do I play
 * at the top of the scene view you will see a play button. click that button to begin playing in the editor
   ![image](https://github.com/user-attachments/assets/c1e5f451-6f25-45d7-9e37-0978e21d68ba)

### Stretching mechanisms.
 * there are two kinds of stretching, one cant be avoided but the other is a simple mistake.
 * The one that can be avoided is a result of losing scale, which happens if you creat a game object as a child of one of the model objects
   
   ![image](https://github.com/user-attachments/assets/d5824021-9716-428c-805b-204b1d9e5f03)
![image](https://github.com/user-attachments/assets/395dc3f9-86fb-40e4-b7ae-6d4a6cad09d4)

 * pictured above is what this looks like.
 * the other kind of stretch is usually charachterized by mechanisms not staying in place when under load, this is an issue with unity.

### My robot Mechanism is not stiff or freaks out
 * This is usually a result of stacked DOF. Unity does not appreceate stacking more than 4 dof. (elevators always count as 2)
 * This also can happen when putting elevators on arms. if the elevators size is larger than that of the arm, or not centered you may have to adjust your weights to keep it from flopping.

### My arm misbehaves and jitters a lot
   * make sure the arm joint only has rotation in ONE axis.
   * If it continues to misbehave make sure the arm is rotating on the X axis not the other axis.

### Can I add a Preload?
   * Technically no, but also yes.
   * if you find the GamePieces Folder you can drag and drop the game peice into the robot prefab of your choice.
   * Now when you spawn that game peice will be spawned as well. spawn it in the right place and hold intake on start and you have a prefab

### How Do I Do Custom Models?
   * Export a complete assembly as Dae (collada) or FBX if you use fusion.
   * if you export to DAE it is recomended to import and re export as fbx using blender for perfomance.
   * drag the model into the imports folder, then drag the model from the imports folder to your prefab heiarchy and you can use it.
   * custom models have no colliders, to add a collider you can either click a part and add a mesh (Making sure to set it to convex mode) or add a new gameObject and add a box collider complex parts should use multiple colliders.

### How do I share my Robot.
   * right click your prefab then select export package, the following will apear
 <img width="359" height="413" alt="image" src="https://github.com/user-attachments/assets/77bfa3e2-f9e2-4513-9926-9ae717ecf656" />
   * deselect everything except for your robot prefab and any custom models, this will keep the size small and avoid changing other peoples installs.

### How do I import a robot.
   * drag and drop the file into the Robots folder with the others

### My drivetrain doesnt work
   * the Generate DriveTrain script is particularly fragile, delete the generated frame, remove the script and re add it.

### How do I do X thats not in the documentation or FAQ
   * If you cant do it by stringing the tools given together no.
   * The power of builder alpha is in the users creativity,

### glitchy game peice behaviour
   * this is caused by an oddity in unity which results in desync.
   * There are no known user side fixes beyond restarting or waitng on it to go away.

### will X bug be patched
   * No, the alpha is no longer recieving updates while the beta is under heavy development,
   * Yes, if you fix it you can and are encouraged to open a pr, the latest public version is open source.


### As a general rule treat it like real life and it should behave
