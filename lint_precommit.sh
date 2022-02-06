#!/usr/bin/env bash

set -euxo pipefail

PATH=$PATH:~/.local/bin

yamllint -c .yamllint.yaml .
mdformat .

cd ./src/

dotnet tool run fantomas --recurse ./

dotnet tool run dotnet-fsharplint lint ./core/core.fsproj
dotnet tool run dotnet-fsharplint lint ./test/test.fsproj