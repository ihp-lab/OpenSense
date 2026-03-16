Minimp4 header-only library.
We're using a modified version of minimp4 (https://github.com/KanaLab/minimp4) to circumvent its limitations.

# Changes

- HEVC demuxer: added BOX_hvc1/hev1 VisualSampleEntry parsing and BOX_hvcC HEVCDecoderConfigurationRecord parsing
- HEVC muxer: fixed numOfVPS bug (vpps -> vvps), added VPS/SPS parsing for hvcC header (profile/tier/level/chroma/bitdepth)
- HEVC muxer: added NAL caching mechanism to bundle non-VCL NALs (AUD, SEI) with VCL NALs into single samples
- Composition offset: added ctts box writing (version 1, signed offsets) and signed arithmetic in ctts reading
- Demuxer: accept ctts box version 1 (max_version 0 -> 1 in g_fullbox)
- MP4E_put_sample: added composition_offset parameter
- mp4_h26x_write_nal: added composition_offset parameter
- Minor fixes: strlen cast warning, file_size signed comparison
