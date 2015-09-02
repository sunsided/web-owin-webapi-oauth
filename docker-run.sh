#!/usr/bin/env bash

ME=$(whoami)

docker run --rm -it -p 9000:9000 -p 9001:9001 ${ME}/owin-test
