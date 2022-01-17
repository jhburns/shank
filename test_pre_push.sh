#!/usr/bin/env bash

set -euxo pipefail

cd ./src/
dotnet publish --verbosity quiet

dotnet ./test/out/test.dll