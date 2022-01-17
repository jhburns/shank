#!/usr/bin/env bash

set -euxo pipefail

cd ./src/
dotnet publish -c Release --verbosity quiet

./test/out/test