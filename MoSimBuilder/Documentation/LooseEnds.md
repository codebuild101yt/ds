# Missing Details and Misc.

There are a few features that were not covered. They are mostly the advanced sequencing tools which require more complex robots than reasonably coverable in this guide. Fortunatly, I left several Example robots to be used 
as help when referencing the next set of information.

## Sequencing motion of subsystems
* The final mode for movement is Sequence. This mode is really helpful when something requires multiple steps. it can also allow for toggles on things that require multiple presses.
* The system moves down the available setpoints starting at the top. if a setpoint has a different keybind assinged it is skiped completly. Only setpoints with the same keybind are sequenced together.

![image](https://github.com/user-attachments/assets/778e12c5-9108-40d7-964c-11ff46e0344b)



  ## Game Piece Sequencing.
  * similar to the motion sequences its possible to create sequences of game piece movement. This is done using "presses to transfer" on a stow object
  * When a stow object has a game piece the presses to transfer is the number of times in a row that button must be pressed to transfer to that buttons relevant endpoint. (endpoints can be other stows or outakes)
  * Transfering between two stows does not count towards the presses. so if you transfer from storage a to b by pressing y and motion system b goes to setpoint by pressing y you only need one press as the first of the two is spent transfering from stow a to b.

![image](https://github.com/user-attachments/assets/50e50902-d79a-49fe-b98d-7cbdfa051531)

## Changing fields disclaimer.

* when changing fields you will have to relocate the robot spawn points, these can be handled by simply going to the field scene and opening the game handler dropdown, a child of GameHandler is blueRobotSpawn, this controls the location and angle of your robot spawn.

![image](https://github.com/user-attachments/assets/e5baff41-a3c7-49ce-a7c5-5c2e47b3ad5d)

## Units system
* the units system does not automatically translate. if you change from inch to meter it will not multiply you inputs by 0.0254; 1 inch becomes 1 meter. 

## Unused Generators
* Generate A frame allows you to generate a fairly common structure for decoration or geometry following. 6328 and 6329 example  robots are good examples.

![image](https://github.com/user-attachments/assets/0884e4f9-9a8e-4d7b-9e97-2e86f4d7ba26)

* Generate Turret has also been added. currently these function almost identically to Arms, except they have functional side to side continous aim. 254 is a good example of a use case.

![image](https://github.com/user-attachments/assets/5f51a851-1d1a-4555-9a5b-63575a6bcb6d)


### This is all of the information needed to competently build robots in MoSim Builder Alpha 2+

 ### [FAQ](https://github.com/masonmm3/MoSimBuilder/blob/Stable/Documentation/FAQ.md)
