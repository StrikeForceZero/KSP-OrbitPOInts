#!/bin/bash

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
if [ -z "$CONFIG" ] || [ -z "$SOLUTION_DIR" ]; then  
    source "$DIR/parse_args.sh"
fi


echo "syncing $CONFIG build to repo"
cp $SOLUTION_DIR/OrbitPOInts/bin/$CONFIG/OrbitPOInts.dll $SOLUTION_DIR/GameData/OrbitPOInts/Plugins/
