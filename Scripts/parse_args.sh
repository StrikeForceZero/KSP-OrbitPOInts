#!/bin/bash

for arg in "$@"
do
    case $arg in
        --config=*)
        config="${arg#*=}"
        shift
        ;;
        --solution_dir=*)
        solution_dir="${arg#*=}"
        shift
        ;;
        *)
        echo "Invalid argument: $arg"
        exit 1
        ;;
    esac
done

if [ -z "$config" ] || [ -z "$solution_dir" ]; then
    echo "Error: one or more arguments are empty or not specified!"
    exit 1
fi 

export CONFIG=$config
export SOLUTION_DIR=$solution_dir