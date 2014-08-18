#!/bin/sh
mono --runtime=v4.0 packages/NUnit.Runners.2.6.3/tools/nunit-console.exe $1 -noxml -labels -stoponerror -trace=Verbose -out=out.txt -err=err.txt
echo "Done"
exit $?