#!/usr/bin/env bash

ME=$(whoami)

docker build -t ${ME}/owin-test .
