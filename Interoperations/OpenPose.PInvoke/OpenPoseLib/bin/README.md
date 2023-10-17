# CPU_ONLY or CUDA ?

Now the default back-end of OpenPose is CUDA.

You can switch to CPU_ONLY version by renaming `x64_CPU` to `x64`, and then rebuild everything.

However, we choose CUDA as our default back-end because of following reasons:

+ A bug in CPU_ONLY version.
OpenPose depends on (CAFFE)[https://github.com/BVLC/caffe], and it uses a very old CAFFE version in default which have a bug.
The bug makes the program crush (netCaffe.cpp:176 DevidedByZero) with a probability when we reboot the OpenPose.
Also, CAFFE is out of official maintenance for years, we do not want to introduce more issues by rebuild the latest commit, because we do not know the OpenPose's CAFFE build settings.
+ CPU version is really slow, without applicability.