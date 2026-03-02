if(NOT TARGET EasyAR::EasyAR)
add_library(EasyAR::EasyAR SHARED IMPORTED)
set_target_properties(EasyAR::EasyAR PROPERTIES
    IMPORTED_LOCATION "C:/Users/wemin/.gradle/caches/8.13/transforms/011a0f025fee05e07b9c6003f7d7ee0d/transformed/jetified-EasyAR/prefab/modules/EasyAR/libs/android.arm64-v8a/libEasyAR.so"
    INTERFACE_INCLUDE_DIRECTORIES "C:/Users/wemin/.gradle/caches/8.13/transforms/011a0f025fee05e07b9c6003f7d7ee0d/transformed/jetified-EasyAR/prefab/modules/EasyAR/include"
    INTERFACE_LINK_LIBRARIES ""
)
endif()

