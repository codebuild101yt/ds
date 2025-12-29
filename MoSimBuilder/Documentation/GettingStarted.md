# Getting Started

## Downloading
  * unity is roughly 5.21 gb
  * Mo sim builder is roughly 1.65 gb
  * these are estimates and untested

### The first step is going to be to download unity hub
  * [Download Here](https://unity.com/download)
  * once installed open the app.
  * use a personal liscence
  * DO NOT install editor versions during setup
### The Second Step is to download the MoSimBuilder source. This can be done more than one way
  * simple install
    * install from the main page using Code -> Download Zip
    * go to your downloads folder and unzip to a location
  *  _advanced install_  (optionally fork first)
     * Open the github Desktop app
     * File -> clone Repository -> fill out fields
### The final Step is to let unity hub install the correct version of unity for you
  * in the unity hub app click the Add button in the top right
    <img width="772" height="582" alt="image" src="https://github.com/user-attachments/assets/770bfcd8-a7a9-49fd-877f-ed4e6ccb9a3e" />
  * next add project from disk
  * double click your project folder to open the outer layer. then select the folder with the name MoSimBuilder - V0.0.....
  * if you selected properly is will ask you if you want to install the correct version of unity, click yes.
  * It will then ask you ask you about adding modules, the default is all that you need checked.
    ![image](https://github.com/user-attachments/assets/cad4705a-0795-4613-ba4c-2c0d5f1c7224)
  * you can now open the project once the download is complete. NOTE: The editor takes a long time to download.

### Updating
 * if you used the advanced install simply fetch from origin then pull the origin on the github desktop app
 * if you used the simple install you will need to start from scratch.

## Getting Started

### Familiarizing
* once the project is open you will see below.
  <img width="1911" height="1014" alt="image" src="https://github.com/user-attachments/assets/26b95c99-6aba-4704-86a2-31255e5ba393" />
* looking to the bottom of the screen you will see the photo below. This is a photo of what is called the _Project Window_ this is where files and scenes are stored.
  ![c43656f0-f2d7-4133-ae5d-ecb374d28579](https://github.com/user-attachments/assets/666f7452-7d31-4656-8ca6-d95f3a99b3ac)
* on the Left Side of the project window is the file browser, scroll and then double click the scenes folder to make yours look like mine.
* in the middle of the above photo is the file viewer. Double click the file named Field, this is a scene, open the field viewer.
* This scene is where all the magic happens all of the individual componenets come together to create the visualization of designs.
* Looking up to the center of the screen we are greeted by the Hierarchy on the left, and the Scene/Game View in the middle with the Play button above the scene view.
![4eb4c310-33f1-4662-9b2c-350a3a5cac3f](https://github.com/user-attachments/assets/f023d61d-2e1f-44bb-9a21-2bd49921e62f)
* In the Hierarchy we see a couple of things. GameHandler, GameManagement, DsColliders, AimFinder.
   * The Game Handler is where the field robots, and spawn points are set
   * The Game management houses ui and other static elements
   * DsColliders houses the non field side colliders for the driver stations
   * AimFinder is a blank object that can be used to find the cordinates of an aiming point.
* The Scene/Game View is where you actually work
* The Play, Pause and Step buttons located above the Scene view are what spawns robots and initializes gameplay
* This brings us to the final thing, the Inspector window. When you select an object using either scene view or the Hierarchy this window on the right side of the screen will populate with the "componenets" on the object.
   * When you select the GameHandler object in the Heirarchy using a left click the inspector menu will populate as it is below
![f35c028a-07b7-4fee-ae88-200d59513959](https://github.com/user-attachments/assets/8b3ea1da-bfae-4a9b-ab5e-cb95bacafb5d)
   * This is where the bulk of the changes will occur.
   * The first "Componenet" of interest is the RobotSpawnController, on it we have
      * The robot name which will reference a prefab file in the Resources->Robots project folder.
      * Camera mode which selects which camera it is
      * alliance selects the alliance.
   * The second "componenet" is the FieldLoader. This is where you select your active field. The top field in the list is the one you will see before you. dragging a lower one to the top will result in the relevant field being selected
* finally we return to the Project view and open the Resources Folder, then the Robots folder and we can see all of the included robots
  ![90746f51-7cff-4cb2-bb4e-3bd8c9a5f2f9](https://github.com/user-attachments/assets/6d3a6089-86a3-4efd-a17d-ba0c6aa215a7)
 (photo was taken prior to completing this guide)
   * there is no requirement that names be numbers.
   * The file name is what you add to the Robot Spawn Controller in order to load the robot.
   * To play you select the Play button on the top of the scene view.
   * Controls? whatever you make them.
 ## Controls Disclaimer.
 The controls are made for xbox controller. The following is the Keyboard Translation. (Controller:Keyboard)
 * A : E
 * X : Q
 * Y: 1
 * B : 3
 * Lt : Shift
 * Rt : Space
 * Rb : K
 * Lb : I
 * Dpad Up: T
 * Dpad Left : F
 * Dpad Right: H
 * Dpad Down :G

# [First Robot](https://github.com/masonmm3/MoSimBuilder/blob/Stable/Documentation/FirstRobot.md)
## Blue text in large font indicates a link to the next step in the documentation. Click the blue words to continue learning about Builders inner workings
