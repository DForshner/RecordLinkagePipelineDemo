#!/bin/bash

# Exit if simple command fails
set -e

echo "--- build_and_run.bash - Start ---"

echo "----------------------------------------------- Change to solution dir"
cd /vagrant/vagrant/RecordLinkagePipeline
echo pwd

echo "----------------------------------------------- Compile"
xbuild /p:Configuration="DebugMono"

echo "----------------------------------------------- Run"
mono Processor/bin/DebugMono/Processor.exe --config-file ./Resources/config.json --exchange-rate-file ./Resources/exchangeRates.json --camera-training-set-file ./Resources/cameraTrainingSet.txt --accessory-training-set-file ./Resources/accessoryTrainingSet.txt --products-file ./Resources/products.txt --listings-file ./Resources/listings.txt

echo "--- build_and_run.bash - End ---"
