Unity-Logger
============

A logging framework designed for educational games built in the Unity game engine environment.

###  Inclusion in game files
Importing the .unitypackage file will export the scripts to their relevant locations. 
Alternatively the raw scripts can be added directly into the Assets directory of any Unity project.


### Implementation
The Logger class uses a singleton implementation, with the main logger being accessed at Logger.Instance.

Every Log must begin with a call to Logger.SessionStart() before further logging calls can be made. 

The SessionManager and LoggableObject scripts are not necessary for the system to function but can be useful templates for how to iinstrument Logger calls.
