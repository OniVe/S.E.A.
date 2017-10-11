<p align="center">
  <a href="https://github.com/OniVe/S.E.A.">
    <img src="https://img.shields.io/github/downloads/onive/s.e.a./latest/total.svg" alt="downloads"/>
  </a>
</p>

# S.E.A.
S.E.A. - Space Engineer Assistant, Workshop mod for the game "Space Engineers" http://steamcommunity.com/sharedfiles/filedetails/?id=680600621

###   Server installation *(Steam client)*:
1.  Download the server **SEA.P** (and game MOD **SEA.GM**)
2.  Unpack the archive file
3.  Go to the Steam client
4.  Menu **"Library"** -> press the right mouse button on the game **"Space Engineers"** -> Properties
5.  In the window **"Space Engineers - Properties"** select the **"Local files"** tab
6.  On the Advanced tab click **"BROWSE LOCAL FILES"**
7.  In the Explorer window, go to the folder **"Bin64"** and copy into it the contents of the directory **"SEA.P"** from the unpacked archive
8.  Close the Explorer window
9.  In the window **"Space Engineers - Properties"** select the **"General"** tab
10. Press the **"SET LAUNCH OPTIONS..."**
11. In the window **"Parameters of run - Space Engineers"** in the input box to insert a line **" -plugin SEA.P.dll"**
12. Press button **"OK"**

###   Server configuration:
1.  Set the port (if necessary) [9000 by default]
 *  Open file **"SAMP.dll.config"** located in the directory of the game **".\Bin64"** via notepad
 *  Change the value to the desired value 9000 on the **<add key = "port" value = "9000" />**
 *  In file **"Utilities\netsh.update.bat"** change the value of 9000, on the desired value in the **"set port = 9000"**
2.  Run **"Utilities\netsh.update.bat"** from administrator

###   The order of updating from the old version:
1.  Delete the directory **"web"** in the root directory of the server **"SEA.P.dll"**
2.  Copy the configuration file **"storage.s3db"** in the directory **".\Bin64"**

-

##   ! Warning !
* If you run the MOD (**SEA.GM**) without connected plugin server (**SEA.P.dll**), it is raise an **error** when you try to save the game. ***(Saving files will not be damaged)***.

