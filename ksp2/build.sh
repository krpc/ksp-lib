#!/bin/bash
# Usage:
#   ksp2/build.sh PATH_TO_KSP2 VERSION
#
# Must be run from the root of the repository.
# Password for encrypting the resulting zip file is read from a file named password.txt in the
# root of the repository.
#
# For example:
#   ksp2/build.sh "$HOME/.local/share/Steam/steamapps/common/Kerbal Space Program 2" 0.1.1

set -ev

KSP2_DIR=$1
NAME="ksp2-$2"
PASSWORD=$(cat password.txt | tr -d '\n')

# Build cilstrip tool
pushd cilstrip
dotnet build CILStrip.csproj
popd

pushd ksp2

# Set up directories
rm -rf tmp
mkdir tmp
mkdir -p tmp/stripped
mkdir -p tmp/KSP2_x64_Data/Managed
ln -s "$KSP2_DIR/KSP2_x64_Data/Managed" tmp/ksp2

# Copy unmodified assemblies
cp tmp/ksp2/mscorlib.dll tmp/KSP2_x64_Data/Managed/
cp tmp/ksp2/System*.dll tmp/KSP2_x64_Data/Managed/

# Strip assemblies
cp tmp/ksp2/*.dll tmp/stripped/
rm tmp/stripped/mscorlib.dll tmp/stripped/System*.dll
pushd tmp/stripped
../../../cilstrip/bin/Debug/net60/CILStrip \
  *.dll \
  ../KSP2_x64_Data/Managed/
popd

# Build archive
chmod 664 tmp/KSP2_x64_Data/Managed/*
rm -f ${NAME}.zip
pushd tmp
zip --encrypt --password $PASSWORD ../${NAME}.zip -r KSP2_x64_Data
popd

# Clean up
rm -rf tmp

popd
