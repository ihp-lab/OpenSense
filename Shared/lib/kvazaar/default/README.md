Kvazaar HEVC Encoder.
We're using a modified version of Kvazaar (https://github.com/KanaLab/kvazaar) to enable 16-bit encoding.

# Version

- **Upstream**: `https://github.com/ultravideo/kvazaar` commit `4b8a5e0b` (after v2.3.1)
- **Fork**: `https://github.com/KanaLab/kvazaar` commit `5a3c3412`

# Fork Changes

- `29eae6fe` Set 16 bit depth
- `5a3c3412` Remove assembler listing output to fix LTCG linking in downstream projects

# Known Issues

- **Profile signaling incomplete** (`src/encoder_state-bitstream.c`, function `encoder_state_write_bitstream_PTL`): Kvazaar hardcodes profile_idc as `bitdepth == 8 ? 1 (Main) : 2 (Main 10)`. No support for Main 12, Monochrome, Monochrome 12/16, or any Range Extensions profiles. When encoding 16-bit monochrome depth/IR streams, the correct profile should be Monochrome 16 (Range Extensions), but the bitstream is incorrectly marked as Main 10. Fix requires refactoring PTL writing to select profile_idc and constraint flags based on actual bit depth and chroma format.

# Build Steps

+ Clone the fork repository
+ Open `build/kvazaar_VS2015.sln`
+ Retarget to the latest SDK and Toolkit
+ Build `Debug` and `Release`
+ Copy artifacts