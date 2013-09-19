1) Compile the ATT project as described in README.txt in the ATT project.

2) Right-click on the GUI project, select "Properties", and go to 
   "Reference Paths". Add the output directory into which you copied
   the Public libraries when building the ATT project.

3) Copy att_config.xml from the ATT project into Config within the GUI project.
   After copying, select the file and, within the Properties window, set 
   "Copy to Output Directory" to "Copy always" if it isn't already set.

4) Copy gui_config.xml from the GUI project into Config within the GUI project.
   After copying, select the file and, within the Properties window, set 
   "Copy to Output Directory" to "Copy always" if it isn't already set.

5) Right-click on the GUI project and select "Build".

6) Edit the two configuration files in Config appropriately for your machine.

7) Right-click the GUI project and select "Set as StartUp Project".

8) Run the ATT solution. If everything goes okay, it will initialize all of
   the database tables and present the GUI interface.
   