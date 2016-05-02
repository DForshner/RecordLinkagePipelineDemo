#!/bin/bash

# Exit if simple command fails
set -e

echo "\e[92m --- build_and_run.bash - Start --- \e[0m"

echo -e "\e[32m ----------------------------------------------- Change to solution dir \e[0m"
cd RecordLinkagePipeline
echo pwd

echo -e "\e[32m ----------------------------------------------- Compile  \e[0m"
xbuild /p:Configuration="DebugMono"

echo -e "\e[32m ----------------------------------------------- Run \e[0m"
mono Processor/bin/DebugMono/Processor.exe

echo "\e[92m --- build_and_run.bash - End --- \e[0m"