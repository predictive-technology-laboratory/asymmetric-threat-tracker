#!/bin/sh
mono --runtime=v4.0 packages/NUnit.Runners.2.6.1/tools/nunit-console.exe -trace=Verbose -noxml -nodots -labels -stoponerror $1
exit $?