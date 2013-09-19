1) Compile the ATT project as described in README.txt in the ATT project.

2) Copy att_config.xml from the ATT project into Config within the GUI project.
   After copying, select the file and, within the Properties window, set 
   "Copy to Output Directory" to "Copy always" if it isn't already set.

3) Copy gui_config.xml from the GUI project into Config within the GUI project.
   After copying, select the file and, within the Properties window, set 
   "Copy to Output Directory" to "Copy always" if it isn't already set.

4) Right-click on the GUI project and select "Build".

5) Edit the two configuration files in Config appropriately for your machine.

6) Right-click the GUI project and select "Set as StartUp Project".

7) Run the ATT solution. If everything goes okay, it will initialize all of
   the database tables and present the GUI interface.
   