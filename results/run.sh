#!/bin/bash

mlagents-learn results/configuration.yaml --run-id level1 --force --env results/envs/level1 --no-graphics
mlagents-learn results/configuration.yaml --run-id level2 --force --env results/envs/level2 --no-graphics --initialize-from level1
mlagents-learn results/configuration.yaml --run-id level3 --force --env results/envs/level3 --no-graphics --initialize-from level2
mlagents-learn results/configuration.yaml --run-id level4 --force --env results/envs/level4 --no-graphics --initialize-from level3
mlagents-learn results/configuration.yaml --run-id level5 --force --env results/envs/level5 --no-graphics --initialize-from level4
mlagents-learn results/configuration.yaml --run-id level6 --force --env results/envs/level6 --no-graphics --initialize-from level5
