if(NOT TARGET AREngineInterop::AREngineInterop)
add_library(AREngineInterop::AREngineInterop SHARED IMPORTED)
set_target_properties(AREngineInterop::AREngineInterop PROPERTIES
    IMPORTED_LOCATION "C:/Users/wemin/.gradle/caches/8.13/transforms/e62b1ec96d69e0ff5fab04822bc047d9/transformed/jetified-AREngineInterop/prefab/modules/AREngineInterop/libs/android.arm64-v8a/libAREngineInterop.so"
    INTERFACE_INCLUDE_DIRECTORIES "C:/Users/wemin/.gradle/caches/8.13/transforms/e62b1ec96d69e0ff5fab04822bc047d9/transformed/jetified-AREngineInterop/prefab/modules/AREngineInterop/include"
    INTERFACE_LINK_LIBRARIES ""
)
endif()

