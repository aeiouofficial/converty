function(converty_apply_msvc_hardening target)
    if(NOT MSVC)
        return()
    endif()

    target_compile_options(${target} PRIVATE
        /W4
        /WX
        /GS
        /guard:cf
        /sdl
        /permissive-
        /utf-8)

    target_link_options(${target} PRIVATE
        /DYNAMICBASE
        /NXCOMPAT
        /HIGHENTROPYVA
        /guard:cf)
endfunction()
