# Install
To install yo need to have 3 things,

 1. Custom install of mosim builder(not only madtown works by default)
 2. Intermediate layer(robot.py)
 3. elastic

![layers ](https://github.com/codebuild101yt/ds/blob/main/docs/Builder.png)

### Python setup
Setup a folder to put your python script. this project uses [python 3.11](https://www.python.org/downloads/release/python-3110/)
 so download that. note: only the 64 bit version works. 

next you need to install required library's. this project depends heavily on wplib and robotpy.  

    pip install robotpy-wpilib pygame elasticlib wpimath


we will return here later.
### Unity setup
Now we need to download [unity hub](https://unity.com/download).
once that is installed we can get builder setup, open unity and select add and folder.![idiot profing](https://github.com/codebuild101yt/ds/blob/main/docs/Screenshot%202025-12-28%20223718.png)
now go to the downloaded folder and select MoSimBuilder.
it will then promt you to download unity editor version 23.2.18f1 following instructions till done downloading.
### elastic install
now its time to install [elastic](https://github.com/Gold872/elastic_dashboard/releases/tag/v2026.0.0-beta-1). 
once installed go to settings and set robot Ip to `LocalHost` and Ip address mode to `DriverStation`.
![how can anyone not understand that](https://github.com/codebuild101yt/ds/blob/main/docs/Screenshot%202025-12-28%20223718.png)

### final steps
In unity run the script. look in the bottom of your screen and copy that.
![debug log](https://github.com/codebuild101yt/ds/blob/main/docs/Screenshot%202025-12-28%20224436.png)
go to robot.py and look for

    CSV_PATH  =  r"c:\Users\Innov\AppData\LocalLow\GreenMachineStudios\MoSimKrayonCad\robot_pose.csv"
replace the path with the text you just copied.

## Running
to run the software open terminal in the folder with robot.py. run the command `py -3.11-64 -m robotpy sim`. now go to unity and press play.
 

 
