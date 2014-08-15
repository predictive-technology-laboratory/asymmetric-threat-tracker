#!/bin/sh

runTest(){
   
   if [ $? -ne 0 ]
   then   
     exit 1
   fi
}

runTest 
echo "Running GuiTest.dll"
runTest 

exit $?