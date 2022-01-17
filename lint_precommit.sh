#!/usr/bin/env bash

set -euxo pipefail

PATH=$PATH:~/.local/bin

yamllint -c .yamllint.yaml .
mdformat --check .

cd ./src/

dotnet tool run fantomas --recurse ./
dotnet tool run dotnet-fsharplint lint ./core/core.fsproj ./test/test.fsproj