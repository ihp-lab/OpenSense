# Introduction

Official .NET implementation of our WACV 2024 Application Track paper: LibreFace: An Open-Source Toolkit for Deep Facial Expression Analysis.

# Input Format

RGB 24bpp 224×224 image. BGR image will produce incorrect result.

# Face Alignment

A sample facial image alignment C# code can be found at [here](https://github.com/ihp-lab/OpenSense/blob/master/Components/LibreFace/FaceImageAligner.cs).

# Additional Dependencies

A specific ONNX runtime is not included as a dependency of this package.
Please use one of the ONNX runtime packages listed in [this page](https://onnxruntime.ai/docs/get-started/with-csharp.html#builds).

# License

Our code is distributed under the USC research license. See `LICENSE.txt` file for more information.

# Citation

```
@misc{chang2023libreface,
      title={LibreFace: An Open-Source Toolkit for Deep Facial Expression Analysis}, 
      author={Di Chang and Yufeng Yin and Zongjian Li and Minh Tran and Mohammad Soleymani},
      year={2023},
      eprint={2308.10713},
      archivePrefix={arXiv},
      primaryClass={cs.CV}
}
```

