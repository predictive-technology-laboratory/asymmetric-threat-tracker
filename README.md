# Operating System Compatibility
* All instructions below apply to Windows XP and 7.
* Windows 8 should also work but hasn't been tested.
* Running on Linux via Mono should work (must compile from source -- see below). 

# Prerequisites
* Install [​PostgreSQL](http://www.enterprisedb.com/products-services-training/pgdownload) 9.2 or later. After installing PostgreSQL, use Stack Builder to install PostGIS 2.0 or later. PostGIS is located under the Spatial Extensions tab. Keep note of the file paths to "shp2pgsql.exe" and "pgsql2shp.exe".
* Install [​R](http://www.r-project.org) 2.15.0 or later. Make sure the directory to which you install R packages is writable. Keep note of the file path to "R.exe".
* Install ​[Java](https://www.java.com/en/download) 1.6 or later. Keep note of the file path to "java.exe". 
* Install the most current version of ​[LibLinear](http://www.csie.ntu.edu.tw/~cjlin/liblinear). Keep note of the file paths to "train.exe" and predict.exe".
* Install the most current version of [​SvmRank](http://www.cs.cornell.edu/people/tj/svm_light/svm_rank.html). Keep note of the file paths to "svm_rank_learn.exe" and "svm_rank_classify.exe".

# Installation
There are two installation choices: binary installer and compilation from source. Unless you are interested in modifying the ATT or understanding the nitty gritty of how it works, you will probably want to use the binary installer.

## Binary Installer
Download and run the installer for the version you would like
* [1.3](https://github.com/MatthewGerber/asymmetric-threat-tracker/releases/download/v1.3/setup.exe)

After you run the installer, edit the configuration files in the Config sub-directory of the installation directory. Use values appropriate for your machine. Then run the ATT. If everything is installed/configured correctly, the system will start.

## Compilation from Source
* Install a working version of Microsoft Visual Studio that is capable of running C# applications. ​[Visual Studio Express 2013](http://www.visualstudio.com/en-US/products/visual-studio-express-vs) is sufficient and free.
* Optional
  * Go to Tools -> Extensions and Updates, then search for and install the License Header Manager. This is used to apply license text to each source code file.
  * Install the [InstallShield Limited Edition for Visual Studio](http://learn.flexerasoftware.com/content/IS-EVAL-InstallShield-Limited-Edition-Visual-Studio) add-on if you want to build the installer package.
* Obtain the source code
  * Download the source archive for the version of interest
    * 1.3 ([zip](https://github.com/MatthewGerber/asymmetric-threat-tracker/archive/v1.3.zip),  [tar.gz](https://github.com/MatthewGerber/asymmetric-threat-tracker/archive/v1.3.tar.gz))
  * To get the most recent version, clone the [ATT GitHub repository](https://github.com/MatthewGerber/asymmetric-threat-tracker). In your clone command, pass "--recursive" to retrieve necessary sub-projects. For example:
    ```
    git clone --recursive git@github.com:MatthewGerber/asymmetric-threat-tracker.git
    ```
* If you're going to build the installer or cut releases, edit "Installer\Installer.isl" replacing "C:\Users\matt\Documents\GitHub\asymmetric-threat-tracker" (or whatever appears) with the appropriate path to your local repository. This assumes that you have installed the InstallShield add-on as described above. If you're not going to work with the installer, skip this step.
* Open the ATT solution in Visual Studio by double-clicking the "ATT.sln" file.
* If you're not going to build the installer, unload the Installer project.
* Along the top menu in Visual Studio, change the configuration of the solution from "Debug" to "Release". 
* Right-click the ATT project and select "Build".
* Copy "att_config.xml" from the ATT project into the Config folder within the GUI project. After copying, select the copied file and, within the properties window, set "Copy to Output Directory" to "Copy Always".
* Copy "gui_config.xml" from the GUI project into the Config folder within the GUI project. After copying, select the copied file and, within the properties window, set "Copy to Output Directory" to "Copy Always".
* Edit the "att_config.xml" and "gui_config.xml" files in Config appropriately for your machine. Consult the documentation within the config files for detailed instructions.
* Right-click the GUI project and select "Build".
* Right-click the GUI project and select "Set as Start Up Project".
* Run the ATT solution. If everything goes okay, it will initialize all of the database tables and present the GUI interface.
