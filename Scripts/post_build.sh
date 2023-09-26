#!/bin/bash

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
source "$DIR/parse_args.sh"
"$DIR/sync_build_to_repo.sh"
"$DIR/install_to_ksp.sh"
"$DIR/bundle_release.sh"
