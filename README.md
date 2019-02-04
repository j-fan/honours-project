<img src="https://j-fan.github.io/portfolio/img/algo1.jpg" max-width="800px" />
                                                                  
<h2>Description</h2>

This project contains the code used for my Media Arts honours artwork. 

Algorhythmic (2018) is a interactive audiovisual experience featuring generative graphics created with Unity and OpenCV. The program tracks people in a space and represents that movement in virtual space filled with responsive particle systems and objects. The animation is then augmented by the speed and frequencies in the chosen song.

<h2>Documentation </h2>

Video documentation : https://www.youtube.com/watch?v=heBQwd_XMw4

Portfolio : https://j-fan.github.io/portfolio/

<h2>Requirements</h2>

As a disclaimer, I am only a student with limited C++ and C# experience so your mileage may vary.

Unity 2017

A depth camera that uses openNI (orbbec astra in this case)

OpenCV (custom build with OPENNI2 and opencv world flags, intructions found in "resources/ORBSLAM2_Install_Guide_Win10.pdf" but ignore the orbslam install)

Openni 2 (can be found on Orbbec website: https://orbbec3d.com/develop/, download openni sdk)

OSCPack (needs to be built in orbbec-blob/oscpack/bin or change the vs project paths accordingly)

Environment variables needed:

Path: add path to opencv\bin and openni\tools to this

OPENCV_DIR: add path to opencv folder

<h2> Credits </h2>

Keijiro Takahashi for unity post-processing effects

Mirza Beig for particle plexus code

UnityOSC https://github.com/thomasfredericks/UnityOSC

OSCPack http://www.rossbencina.com/code/oscpack

Salocinx's Orbbec+OpenCV+OpenNI guide https://stackoverflow.com/questions/48835876/how-to-use-orbbec-astra-depth-sensor-with-opencv
