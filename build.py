#!/usr/bin/env python3

# Usage:
#   build.py [ksp|ksp2] PATH_TO_KSP VERSION
#
# Must be run from the root of the repository.
#
# For example:
#   build.py ksp2 "$HOME/.local/share/Steam/steamapps/common/Kerbal Space Program 2" 0.1.1

import sys
import os
import shutil
import subprocess

assert len(sys.argv) == 4
ksp = sys.argv[1]
ksp_path = sys.argv[2]
ksp_version = sys.argv[3]

assert ksp in ("ksp", "ksp2")
assert os.path.exists(ksp_path)

# Build cilstrip tool
subprocess.check_call(["dotnet", "build", "cilstrip/CILStrip.csproj"])

if os.path.exists("tmp"):
    shutil.rmtree("tmp")

if ksp == "ksp":
    lib_path = os.path.join("KSP_Data", "Managed")
else:
    lib_path = os.path.join("KSP2_x64_Data", "Managed")

root_path = os.path.dirname(os.path.realpath(__file__))
orig_path = os.path.join(ksp_path, lib_path)
out_path = os.path.join("tmp", lib_path)
lib_names = os.listdir(orig_path)

system_lib_names = list(
    filter(lambda x: x.startswith("System.") or x == "mscorlib.dll", lib_names)
)
ksp_lib_names = list(
    filter(lambda x: not (x.startswith("System.") or x == "mscorlib.dll"), lib_names)
)

os.makedirs(out_path)

# Copy system dlls unmodified
for lib in system_lib_names:
    shutil.copy(os.path.join(orig_path, lib), os.path.join(out_path, lib))

# Strip KSP libs
subprocess.check_call(
    [
        os.path.join(root_path, "cilstrip/bin/Debug/net60/CILStrip"),
    ]
    + ksp_lib_names
    + [os.path.abspath(out_path) + "/"],
    cwd=orig_path,
)

# Create archive
subprocess.check_call(
    ["zip", "-r", os.path.join("..", ksp, ksp + "-" + ksp_version + ".zip"), "./"],
    cwd="tmp",
)

shutil.rmtree("tmp")
