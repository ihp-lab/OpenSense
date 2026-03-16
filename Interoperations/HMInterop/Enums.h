#pragma once

using namespace System;

namespace HMInterop {
    /// <summary>
    /// Chroma subsampling format (maps to ::ChromaFormat in TypeDef.h)
    /// </summary>
    public enum class ChromaFormat : int {
        Chroma400 = 0,  // CHROMA_400
        Chroma420 = 1,  // CHROMA_420
        Chroma422 = 2,  // CHROMA_422
        Chroma444 = 3,  // CHROMA_444
    };

    /// <summary>
    /// HEVC profile (maps to Profile::Name in TypeDef.h)
    /// </summary>
    public enum class ProfileName : int {
        None = 0,               // Profile::NONE
        Main = 1,               // Profile::MAIN
        Main10 = 2,             // Profile::MAIN10
        MainStillPicture = 3,   // Profile::MAINSTILLPICTURE
        MainRExt = 4,           // Profile::MAINREXT
        HighThroughputRExt = 5, // Profile::HIGHTHROUGHPUTREXT
    };

    /// <summary>
    /// HEVC level (maps to Level::Name in TypeDef.h)
    /// </summary>
    public enum class LevelName : int {
        None = 0,
        Level1 = 30,
        Level2 = 60,
        Level2_1 = 63,
        Level3 = 90,
        Level3_1 = 93,
        Level4 = 120,
        Level4_1 = 123,
        Level5 = 150,
        Level5_1 = 153,
        Level5_2 = 156,
        Level6 = 180,
        Level6_1 = 183,
        Level6_2 = 186,
        Level6_3 = 189,
        Level8_5 = 255,
    };

    /// <summary>
    /// HEVC level tier (maps to Level::Tier in TypeDef.h)
    /// </summary>
    public enum class LevelTier : int {
        Main = 0,
        High = 1,
    };

    /// <summary>
    /// Slice type (maps to ::SliceType in TypeDef.h)
    /// </summary>
    public enum class SliceType : int {
        B = 0,  // B_SLICE
        P = 1,  // P_SLICE
        I = 2,  // I_SLICE
    };

    /// <summary>
    /// Decoding refresh type for IRAP picture selection
    /// </summary>
    public enum class DecodingRefreshType : int {
        None = 0,
        CRA = 1,
        IDR = 2,
    };

    /// <summary>
    /// Decoded picture hash type for SEI verification (maps to ::HashType in TypeDef.h)
    /// </summary>
    public enum class HashType : int {
        MD5 = 0,       // HASHTYPE_MD5
        CRC = 1,       // HASHTYPE_CRC
        Checksum = 2,  // HASHTYPE_CHECKSUM
        None = 3,      // HASHTYPE_NONE
    };

    /// <summary>
    /// Motion estimation search method (maps to ::MESearchMethod in TypeDef.h)
    /// </summary>
    public enum class MotionSearchMethod : int {
        Full = 0,             // MESEARCH_FULL
        Diamond = 1,          // MESEARCH_DIAMOND
        Selective = 2,        // MESEARCH_SELECTIVE
        DiamondEnhanced = 3,  // MESEARCH_DIAMOND_ENHANCED
    };

    /// <summary>
    /// Picture component ID (maps to ::ComponentID in TypeDef.h)
    /// </summary>
    public enum class ComponentId : int {
        Y  = 0,  // COMPONENT_Y (luma)
        Cb = 1,  // COMPONENT_Cb (chroma blue)
        Cr = 2,  // COMPONENT_Cr (chroma red)
    };

    /// <summary>
    /// Determines how a Picture disposes its PictureYuv.
    /// </summary>
    public enum class PictureYuvOwnership : int {
        Owned = 0,    // Dispose (destroy the PictureYuv)
        Pooled = 1,   // Return to PictureYuvPool
    };
}
