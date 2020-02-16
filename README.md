# HiveLocalServer

### Instructions:

A.1) Download ```HiveLocalServer``` AND ```Rigs.zip``` file from here:  
**https://github.com/thebitbrine/HiveLocalServer/releases/**  
-**_Note: Download x64 if your OS is 64-bit (most likely), otherwise download the x86 version_**  

A.2) Create a folder called ```HiveLocalServer``` anywhere you like

A.3) Copy-Paste ```HiveLocalServer-win-x64.exe``` to the newly created folder

A.4) Extract ```Rigs.zip``` to the newly created folder  
-**_Note: Make sure ```HiveLocalServer-win-64.exe``` and ```Rigs``` folder are both in the same directory_**  

A.5) Run ```HiveLocalServer``` by Administator

---

B.1) Find your local IP address using ```ipconfig``` command in cmd  
-**_Note: For best results set your local IP to static IP_**  
-**_Note: You will need this in step ```C.2```_**  

B.2) Open port 80 by setting an ```Inbound Rule``` in Windows Firewall

---

C.1) Go to you HiveOS setup and open ```rig.conf``` file

C.2) Set HIVE_HOST_URL to ```http://YourLocalIP:80``` replacing ```YourLocalIP``` with the IP form step ```B.1```  
-**_Note: Line has to look like this: ```HIVE_HOST_URL=http://192.168.1.23:80```_**  

C.3) Set both ```RIG_ID``` and ```WORKER_NAME``` to name of the rig  
-**_Note: It is important to set the both to exacly the same thing (case-sensitive)_**  
-**_Note: It can be an alphanumerical name (i.e EthRig01)_**  
-**_Note: It can't contain any special characters (i.e. ! _ - +)_**  

C.4) Set ```RIG_PASSWD``` to ```1```

C.5) Set ```WD_ENABLED``` to ```0```

C.6) Save the file and reboot you HiveOS setup

**NOTE: Upon HiveOS startup you should see the device in HiveLocalServer with status: ```Hello``` in yellow text
If that is not the case check your configs and try running ```hello``` command in HiveOS**

---

D.1) After getting the ```hello``` from HiveOS there should be a new folder under ```Rigs``` directory
that contains configs of your setup  
-**_Note: The new folder name will be the one that you set in ```rig.conf```_**  

D.2) If you change anything from that folder make sure to run ```hello``` command
in HiveOS and then reboot the system for changes to take place

D.3) You must keep ```HiveLocalServer``` running all the time, because HiveOS needs to always check the server in order to run.  
-**_Note: If HiveOS is unable to connect to ```HiveLocalServer``` it will stop mining after 2-3 minutes_**  


#### Happy mining!




