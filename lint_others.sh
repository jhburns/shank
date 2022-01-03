#!/usr/bin/env bash

set -euxo pipefail

PATH=$PATH:~/.local/bin

yamllint .
mdformat --check .