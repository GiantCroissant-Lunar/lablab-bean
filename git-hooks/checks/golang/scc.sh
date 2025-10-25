#!/bin/sh
# Check if scc is in the path and executable
if ! command -v scc >/dev/null 2>&1; then
  echo "scc not found in path. Please install from https://github.com/boyter/scc/"
  exit 1
fi
