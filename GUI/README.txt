* The solution comes with the installer project for the ATT. If you're not 
  interested in the installer, it's best to unload the project since
  it takes quite a while to build (right-click the project and select
  "Unload Project").

* Change the configuration from Debug to Release along the top menu in Visual Studio.

* Compile the ATT project as described in README.txt in the ATT project.

* Copy att_config.xml from the ATT project into Config within the GUI project.

* Copy gui_config.xml from the GUI project into Config within the GUI project.

* Right-click on the GUI project and select "Build".

* Edit the two configuration files in Config appropriately for your machine.

* Right-click the GUI project and select "Set as StartUp Project".

* Run the ATT solution. If everything goes okay, it will initialize all of
  the database tables and present the GUI interface.
   