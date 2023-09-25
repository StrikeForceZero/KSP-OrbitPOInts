#!/bin/bash

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
if [ -z "$CONFIG" ] || [ -z "$SOLUTION_DIR" ]; then
    source "$DIR/parse_args.sh"
fi


echo "copying mod to ksp install"
cp -R $SOLUTION_DIR/GameData/OrbitPOInts/ "$HOME/.local/share/Steam/steamapps/common/Kerbal Space Program/GameData/"
