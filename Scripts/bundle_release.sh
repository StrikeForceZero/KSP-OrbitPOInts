#1/bin/bash 

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
if [ -z "$CONFIG" ] || [ -z "$SOLUTION_DIR" ]; then
    source "$DIR/parse_args.sh"
fi

cd $SOLUTION_DIR

echo "bundling release"
rm OrbitPOInts.zip
zip -r OrbitPOInts.zip GameData
